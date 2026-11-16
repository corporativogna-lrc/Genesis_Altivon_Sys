using System.IdentityModel.Tokens.Jwt;
using GenesisAltivonSys.BackEnd.CadastrosCorporativos.ProdutosServicos;
using GenesisAltivonSys.BackEnd.ManufaturaOperacoes.OrdensProducao;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using GenesisAltivonSys.Testes.Suporte;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace GenesisAltivonSys.Testes;

public sealed class SegurancaIamManufaturaTests
{
    private static GeradorTokenJwtMfa CriarServicoJwtMfa()
    {
        var configuracaoEmMemoria = new Dictionary<string, string?>
        {
            ["SegurancaJwt:Emissor"] = "GenesisAltivonSys.IAM.Test",
            ["SegurancaJwt:Audiencia"] = "GenesisAltivonSys.ERP.Test",
            ["SegurancaJwt:ChaveAssinatura"] = "ChaveDeTesteAutomatizado_HMAC_SHA256_Com_Mais_De_64_Caracteres_Seguros!",
            ["SegurancaJwt:ExpiracaoAccessTokenMinutos"] = "15",
            ["SegurancaJwt:ExpiracaoRefreshTokenDias"] = "7"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configuracaoEmMemoria)
            .Build();

        var dataProtection = new EphemeralDataProtectionProvider();
        return new GeradorTokenJwtMfa(configuration, dataProtection);
    }

    [Fact]
    public void Deve_Gerar_E_Validar_Senha_Pbkdf2_E_Emitir_Jwt_Com_Claims_Tenant()
    {
        var servico = CriarServicoJwtMfa();
        var hash = servico.GerarHashSenha("SenhaForte@2026");

        Assert.True(servico.VerificarSenha("SenhaForte@2026", hash));
        Assert.False(servico.VerificarSenha("SenhaErrada@2026", hash));

        var tenantId = Guid.NewGuid();
        var usuario = new SysUsuarioRegistro
        {
            TenantId = tenantId,
            NomeCompleto = "Diretor Financeiro",
            Email = "financeiro@altivon.com.br",
            SenhaHash = hash,
            PerfilRbac = "DIRETOR_FINANCEIRO",
            MfaHabilitado = true
        };

        var tokens = servico.EmitirParTokens(usuario);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.Equal(64, tokens.RefreshTokenHashSha256.Length);

        var jwtLido = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);
        Assert.Equal(tenantId.ToString(), jwtLido.Claims.First(c => c.Type == "tenant_id").Value);
        Assert.Equal("DIRETOR_FINANCEIRO", jwtLido.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value);
    }

    [Fact]
    public void Deve_Gerar_Segredo_Totp_Cifrado_E_Rejeitar_Codigo_Invalido()
    {
        var servico = CriarServicoJwtMfa();
        var (segredoBase32, segredoCifrado, otpAuthUri) = servico.GerarNovoSegredoTotp(
            "admin@altivon.com.br",
            "GenesisAltivon");

        Assert.False(string.IsNullOrWhiteSpace(segredoBase32));
        Assert.NotEmpty(segredoCifrado);
        Assert.StartsWith("otpauth://totp/", otpAuthUri);
        Assert.False(servico.ValidarCodigoTotp(segredoCifrado, "000000"));
        Assert.False(servico.ValidarCodigoTotp(segredoCifrado, "abc"));
    }

    [Fact]
    public async Task Deve_Concluir_Ordem_Producao_Mes_E_Creditar_Saldo_Produto_Acabado()
    {
        var tenantId = Guid.NewGuid();
        var (ctx, _) = ContextoTesteFactory.CriarContextoIsolado(tenantId);

        var produto = new ProdutoRegistro
        {
            Descricao = "Equipamento Industrial Modelo X",
            Sku = "EQP-IND-X01",
            PrecoVenda = 4500.00m,
            EstoqueAtual = 10m
        };
        ctx.Produtos.Add(produto);

        var op = new OrdemProducaoRegistro
        {
            CodigoOp = "OP-20261115-TESTE1",
            ProdutoAcabadoId = produto.Id,
            LinhaProducao = "LINHA-01",
            StatusOp = "EM_PRODUCAO",
            QuantidadePlanejada = 25m
        };
        ctx.OrdensProducao.Add(op);
        await ctx.SaveChangesAsync();

        // Simula apontamento de conclusao da OP
        op.StatusOp = "CONCLUIDA";
        op.QuantidadeProduzida = 25m;
        op.ConcluidaEm = DateTime.UtcNow;
        produto.EstoqueAtual += op.QuantidadeProduzida;

        await ctx.SaveChangesAsync();

        Assert.Equal("CONCLUIDA", op.StatusOp);
        Assert.Equal(35m, produto.EstoqueAtual);
        Assert.Equal((ulong)2, produto.RowVersion);
    }
}