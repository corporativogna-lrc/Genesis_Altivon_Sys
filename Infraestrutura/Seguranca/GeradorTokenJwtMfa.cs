using System.Buffers.Binary;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;

namespace GenesisAltivonSys.Infraestrutura.Seguranca;

public sealed record ParTokensSessao(
    string AccessToken,
    DateTime AccessTokenExpiraEmUtc,
    string RefreshTokenPlano,
    string RefreshTokenHashSha256,
    DateTime RefreshTokenExpiraEmUtc);

public sealed class GeradorTokenJwtMfa
{
    private const string PropositoProtecaoMfa = "GenesisAltivonSys.IAM.MfaTotpSecret.v1";
    private const string AlfabetoBase32 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    private readonly IConfiguration _configuration;
    private readonly IDataProtector _protectorMfa;

    public GeradorTokenJwtMfa(
        IConfiguration configuration,
        IDataProtectionProvider dataProtectionProvider)
    {
        _configuration = configuration;
        _protectorMfa = dataProtectionProvider.CreateProtector(PropositoProtecaoMfa);
    }

    public string GerarHashSenha(string senhaPlana)
    {
        if (string.IsNullOrWhiteSpace(senhaPlana) || senhaPlana.Length < 8)
        {
            throw new ArgumentException("A senha deve conter no minimo 8 caracteres.");
        }

        Span<byte> salt = stackalloc byte[16];
        RandomNumberGenerator.Fill(salt);

        const int iteracoes = 120_000;
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(senhaPlana),
            salt,
            iteracoes,
            HashAlgorithmName.SHA256,
            32);

        return $"PBKDF2${iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerificarSenha(string senhaPlana, string hashArmazenado)
    {
        if (string.IsNullOrWhiteSpace(senhaPlana) || string.IsNullOrWhiteSpace(hashArmazenado))
        {
            return false;
        }

        var partes = hashArmazenado.Split('$');
        if (partes.Length != 4 || partes[0] != "PBKDF2" || !int.TryParse(partes[1], out var iteracoes))
        {
            return false;
        }

        var salt = Convert.FromBase64String(partes[2]);
        var hashEsperado = Convert.FromBase64String(partes[3]);

        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(senhaPlana),
            salt,
            iteracoes,
            HashAlgorithmName.SHA256,
            hashEsperado.Length);

        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }

    public ParTokensSessao EmitirParTokens(SysUsuarioRegistro usuario)
    {
        var emissor = _configuration["SegurancaJwt:Emissor"]
            ?? throw new InvalidOperationException("SegurancaJwt:Emissor nao configurado.");
        var audiencia = _configuration["SegurancaJwt:Audiencia"]
            ?? throw new InvalidOperationException("SegurancaJwt:Audiencia nao configurado.");
        var chave = _configuration["SegurancaJwt:ChaveAssinatura"]
            ?? throw new InvalidOperationException("SegurancaJwt:ChaveAssinatura nao configurado.");

        var minutosAccess = int.TryParse(_configuration["SegurancaJwt:ExpiracaoAccessTokenMinutos"], out var m) ? m : 15;
        var diasRefresh = int.TryParse(_configuration["SegurancaJwt:ExpiracaoRefreshTokenDias"], out var d) ? d : 7;

        var agoraUtc = DateTime.UtcNow;
        var expiraAccess = agoraUtc.AddMinutes(minutosAccess);
        var expiraRefresh = agoraUtc.AddDays(diasRefresh);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.NomeCompleto),
            new("tenant_id", usuario.TenantId.ToString()),
            new(ClaimTypes.Role, usuario.PerfilRbac),
            new("mfa_verificado", usuario.MfaHabilitado ? "true" : "false"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: emissor,
            audience: audiencia,
            claims: claims,
            notBefore: agoraUtc,
            expires: expiraAccess,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        Span<byte> bytesRefresh = stackalloc byte[48];
        RandomNumberGenerator.Fill(bytesRefresh);
        var refreshTokenPlano = Convert.ToBase64String(bytesRefresh)
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');

        var refreshHash = CalcularSha256Hex(refreshTokenPlano);

        return new ParTokensSessao(
            AccessToken: accessToken,
            AccessTokenExpiraEmUtc: expiraAccess,
            RefreshTokenPlano: refreshTokenPlano,
            RefreshTokenHashSha256: refreshHash,
            RefreshTokenExpiraEmUtc: expiraRefresh);
    }

    public string CalcularSha256Hex(string valor)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(valor));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public (string SegredoBase32, byte[] SegredoCifrado, string OtpAuthUri) GerarNovoSegredoTotp(
        string emailUsuario,
        string nomeEmpresa)
    {
        Span<byte> segredoBytes = stackalloc byte[20];
        RandomNumberGenerator.Fill(segredoBytes);

        var segredoBase32 = CodificarBase32(segredoBytes);
        var segredoCifrado = _protectorMfa.Protect(segredoBytes.ToArray());

        var issuerCodificado = Uri.EscapeDataString(nomeEmpresa);
        var contaCodificada = Uri.EscapeDataString(emailUsuario);
        var otpAuthUri = $"otpauth://totp/{issuerCodificado}:{contaCodificada}?secret={segredoBase32}&issuer={issuerCodificado}&digits=6&period=30";

        return (segredoBase32, segredoCifrado, otpAuthUri);
    }

    public bool ValidarCodigoTotp(byte[] segredoCifrado, string codigoInformado)
    {
        if (segredoCifrado is null || segredoCifrado.Length == 0 || string.IsNullOrWhiteSpace(codigoInformado))
        {
            return false;
        }

        var codigoLimpo = codigoInformado.Trim();
        if (codigoLimpo.Length != 6 || !codigoLimpo.All(char.IsDigit))
        {
            return false;
        }

        byte[] segredoBytes;
        try
        {
            segredoBytes = _protectorMfa.Unprotect(segredoCifrado);
        }
        catch (CryptographicException)
        {
            return false;
        }

        var passoAtual = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30L;

        for (long deslocamento = -1; deslocamento <= 1; deslocamento++)
        {
            var codigoEsperado = ComputarCodigoTotp(segredoBytes, passoAtual + deslocamento);
            if (CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(codigoEsperado),
                Encoding.ASCII.GetBytes(codigoLimpo)))
            {
                return true;
            }
        }

        return false;
    }

    private static string ComputarCodigoTotp(byte[] chaveSecreta, long contador)
    {
        Span<byte> bufferContador = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(bufferContador, contador);

        var hash = HMACSHA1.HashData(chaveSecreta, bufferContador);
        var offset = hash[^1] & 0x0F;

        var codigoBinario =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        var otp = codigoBinario % 1_000_000;
        return otp.ToString("D6");
    }

    private static string CodificarBase32(ReadOnlySpan<byte> dados)
    {
        var resultado = new StringBuilder((dados.Length * 8 + 4) / 5);
        int buffer = dados[0];
        int proximoByte = 1;
        int bitsRestantes = 8;

        while (bitsRestantes > 0 || proximoByte < dados.Length)
        {
            if (bitsRestantes < 5)
            {
                if (proximoByte < dados.Length)
                {
                    buffer <<= 8;
                    buffer |= dados[proximoByte++] & 0xFF;
                    bitsRestantes += 8;
                }
                else
                {
                    int pad = 5 - bitsRestantes;
                    buffer <<= pad;
                    bitsRestantes += pad;
                }
            }

            int indice = (buffer >> (bitsRestantes - 5)) & 0x1F;
            bitsRestantes -= 5;
            resultado.Append(AlfabetoBase32[indice]);
        }

        return resultado.ToString();
    }
}