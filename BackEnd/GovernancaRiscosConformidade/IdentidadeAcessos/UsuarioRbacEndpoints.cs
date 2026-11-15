using System.Security.Claims;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.BackEnd.GovernancaRiscosConformidade.IdentidadeAcessos;

public static class UsuarioRbacEndpoints
{
    public static void MapearEndpointsUsuarioRbac(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/iam");

        grupo.MapPost("/bootstrap-admin", async (
            RequisicaoBootstrapAdmin req,
            ContextoBancoRelacional db,
            GeradorTokenJwtMfa seguranca,
            CancellationToken ct) =>
        {
            if (req.TenantId == Guid.Empty ||
                string.IsNullOrWhiteSpace(req.NomeCompleto) ||
                string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Senha))
            {
                return Results.BadRequest(new { erro = "TenantId, NomeCompleto, Email e Senha sao obrigatorios." });
            }

            var tenantExiste = await db.Tenants
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Id == req.TenantId && !t.IsDeleted && t.Ativo, ct);

            if (!tenantExiste)
            {
                return Results.NotFound(new { erro = "Tenant informado nao existe ou esta inativo." });
            }

            var jaExisteAdmin = await db.Usuarios
                .IgnoreQueryFilters()
                .AnyAsync(u => u.TenantId == req.TenantId && !u.IsDeleted && u.PerfilRbac == PerfisRbac.AdminMaster, ct);

            if (jaExisteAdmin)
            {
                return Results.Conflict(new { erro = "Este Tenant ja possui um ADMIN_MASTER ativo. Utilize o login administrativo." });
            }

            var emailNormalizado = req.Email.Trim().ToLowerInvariant();
            var novoAdmin = new SysUsuarioRegistro
            {
                TenantId = req.TenantId,
                NomeCompleto = req.NomeCompleto.Trim(),
                Email = emailNormalizado,
                SenhaHash = seguranca.GerarHashSenha(req.Senha),
                PerfilRbac = PerfisRbac.AdminMaster,
                MfaHabilitado = false,
                Ativo = true
            };

            db.Usuarios.Add(novoAdmin);
            await db.SaveChangesAsync(ct);

            var salvo = await db.Usuarios
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstAsync(u => u.Id == novoAdmin.Id && u.TenantId == req.TenantId, ct);

            return Results.Created($"/api/v1/iam/usuarios/{salvo.Id}", new RespostaUsuarioRbac(
                salvo.Id,
                salvo.TenantId,
                salvo.NomeCompleto,
                salvo.Email,
                salvo.PerfilRbac,
                salvo.MfaHabilitado,
                salvo.Ativo,
                salvo.CreatedAt,
                salvo.RowVersion));
        });

        grupo.MapPost("/login", async (
            RequisicaoLogin req,
            ContextoBancoRelacional db,
            GeradorTokenJwtMfa seguranca,
            CancellationToken ct) =>
        {
            if (req.TenantId == Guid.Empty || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Senha))
            {
                return Results.BadRequest(new { erro = "TenantId, Email e Senha sao obrigatorios." });
            }

            var emailNormalizado = req.Email.Trim().ToLowerInvariant();
            var usuario = await db.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u =>
                    u.TenantId == req.TenantId &&
                    u.Email == emailNormalizado &&
                    !u.IsDeleted &&
                    u.Ativo, ct);

            if (usuario is null || !seguranca.VerificarSenha(req.Senha, usuario.SenhaHash))
            {
                return Results.Unauthorized();
            }

            if (usuario.MfaHabilitado)
            {
                if (string.IsNullOrWhiteSpace(req.CodigoTotpMfa))
                {
                    return Results.Json(new
                    {
                        mfaRequerido = true,
                        mensagem = "Informe o codigo TOTP de 6 digitos do autenticador."
                    }, statusCode: StatusCodes.Status428PreconditionRequired);
                }

                if (usuario.MfaSegredoCifrado is null || !seguranca.ValidarCodigoTotp(usuario.MfaSegredoCifrado, req.CodigoTotpMfa))
                {
                    return Results.Unauthorized();
                }
            }

            var tokens = seguranca.EmitirParTokens(usuario);
            usuario.RefreshTokenHash = tokens.RefreshTokenHashSha256;
            usuario.RefreshTokenExpiraEm = tokens.RefreshTokenExpiraEmUtc;

            await db.SaveChangesAsync(ct);

            return Results.Ok(new RespostaSessaoAutenticada(
                usuario.Id,
                usuario.TenantId,
                usuario.NomeCompleto,
                usuario.Email,
                usuario.PerfilRbac,
                usuario.MfaHabilitado,
                tokens.AccessToken,
                tokens.AccessTokenExpiraEmUtc,
                tokens.RefreshTokenPlano,
                tokens.RefreshTokenExpiraEmUtc));
        });

        grupo.MapPost("/refresh", async (
            RequisicaoRefreshToken req,
            ContextoBancoRelacional db,
            GeradorTokenJwtMfa seguranca,
            CancellationToken ct) =>
        {
            if (req.TenantId == Guid.Empty || string.IsNullOrWhiteSpace(req.RefreshToken))
            {
                return Results.BadRequest(new { erro = "TenantId e RefreshToken sao obrigatorios." });
            }

            var hashRecebido = seguranca.CalcularSha256Hex(req.RefreshToken.Trim());
            var usuario = await db.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u =>
                    u.TenantId == req.TenantId &&
                    u.RefreshTokenHash == hashRecebido &&
                    !u.IsDeleted &&
                    u.Ativo, ct);

            if (usuario is null ||
                usuario.RefreshTokenExpiraEm is null ||
                usuario.RefreshTokenExpiraEm <= DateTime.UtcNow)
            {
                return Results.Unauthorized();
            }

            var novosTokens = seguranca.EmitirParTokens(usuario);
            usuario.RefreshTokenHash = novosTokens.RefreshTokenHashSha256;
            usuario.RefreshTokenExpiraEm = novosTokens.RefreshTokenExpiraEmUtc;

            await db.SaveChangesAsync(ct);

            return Results.Ok(new RespostaSessaoAutenticada(
                usuario.Id,
                usuario.TenantId,
                usuario.NomeCompleto,
                usuario.Email,
                usuario.PerfilRbac,
                usuario.MfaHabilitado,
                novosTokens.AccessToken,
                novosTokens.AccessTokenExpiraEmUtc,
                novosTokens.RefreshTokenPlano,
                novosTokens.RefreshTokenExpiraEmUtc));
        });

        grupo.MapPost("/mfa/iniciar", async (
            ClaimsPrincipal user,
            ContextoBancoRelacional db,
            GeradorTokenJwtMfa seguranca,
            CancellationToken ct) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (usuario is null)
            {
                return Results.NotFound(new { erro = "Usuario nao localizado no tenant atual." });
            }

            var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == usuario.TenantId, ct);
            var nomeEmpresa = tenant?.NomeFantasia ?? "GenesisAltivonSys";

            var (segredoBase32, segredoCifrado, otpAuthUri) = seguranca.GerarNovoSegredoTotp(usuario.Email, nomeEmpresa);
            usuario.MfaSegredoCifrado = segredoCifrado;
            usuario.MfaHabilitado = false;

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                chaveManualBase32 = segredoBase32,
                otpAuthUri,
                instrucao = "Leia a URI no Google Authenticator / Microsoft Authenticator e confirme em /api/v1/iam/mfa/confirmar."
            });
        }).RequireAuthorization();

        grupo.MapPost("/mfa/confirmar", async (
            RequisicaoConfirmarMfa req,
            ClaimsPrincipal user,
            ContextoBancoRelacional db,
            GeradorTokenJwtMfa seguranca,
            CancellationToken ct) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Results.Unauthorized();
            }

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (usuario is null || usuario.MfaSegredoCifrado is null)
            {
                return Results.BadRequest(new { erro = "Inicialize a configuracao MFA antes de confirmar." });
            }

            if (!seguranca.ValidarCodigoTotp(usuario.MfaSegredoCifrado, req.CodigoTotp))
            {
                return Results.BadRequest(new { erro = "Codigo TOTP invalido ou expirado." });
            }

            usuario.MfaHabilitado = true;
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { mfaHabilitado = true, mensagem = "Autenticacao em dois fatores (TOTP) ativada com sucesso." });
        }).RequireAuthorization();

        grupo.MapGet("/usuarios", async (
            ContextoBancoRelacional db,
            CancellationToken ct) =>
        {
            var usuarios = await db.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.NomeCompleto)
                .Select(u => new RespostaUsuarioRbac(
                    u.Id,
                    u.TenantId,
                    u.NomeCompleto,
                    u.Email,
                    u.PerfilRbac,
                    u.MfaHabilitado,
                    u.Ativo,
                    u.CreatedAt,
                    u.RowVersion))
                .ToListAsync(ct);

            return Results.Ok(usuarios);
        }).RequireAuthorization(PoliticasAutorizacao.ApenasAdminMaster);

        grupo.MapPost("/usuarios", async (
            RequisicaoCriarUsuarioRbac req,
            ContextoBancoRelacional db,
            GeradorTokenJwtMfa seguranca,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.NomeCompleto) ||
                string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.SenhaInicial) ||
                !PerfisRbac.TodosPerfisValidos.Contains(req.PerfilRbac))
            {
                return Results.BadRequest(new
                {
                    erro = "Dados invalidos ou PerfilRbac nao permitido.",
                    perfisPermitidos = PerfisRbac.TodosPerfisValidos
                });
            }

            var emailNormalizado = req.Email.Trim().ToLowerInvariant();
            var duplicado = await db.Usuarios.AnyAsync(u => u.Email == emailNormalizado, ct);
            if (duplicado)
            {
                return Results.Conflict(new { erro = "Ja existe um usuario com este e-mail no Tenant atual." });
            }

            var novoUsuario = new SysUsuarioRegistro
            {
                NomeCompleto = req.NomeCompleto.Trim(),
                Email = emailNormalizado,
                SenhaHash = seguranca.GerarHashSenha(req.SenhaInicial),
                PerfilRbac = req.PerfilRbac,
                MfaHabilitado = false,
                Ativo = true
            };

            db.Usuarios.Add(novoUsuario);
            await db.SaveChangesAsync(ct);

            var releitura = await db.Usuarios
                .AsNoTracking()
                .FirstAsync(u => u.Id == novoUsuario.Id, ct);

            return Results.Created($"/api/v1/iam/usuarios/{releitura.Id}", new RespostaUsuarioRbac(
                releitura.Id,
                releitura.TenantId,
                releitura.NomeCompleto,
                releitura.Email,
                releitura.PerfilRbac,
                releitura.MfaHabilitado,
                releitura.Ativo,
                releitura.CreatedAt,
                releitura.RowVersion));
        }).RequireAuthorization(PoliticasAutorizacao.ApenasAdminMaster);
    }
}