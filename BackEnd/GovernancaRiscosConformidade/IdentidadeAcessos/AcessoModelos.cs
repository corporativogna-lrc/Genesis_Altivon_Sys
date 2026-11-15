namespace GenesisAltivonSys.BackEnd.GovernancaRiscosConformidade.IdentidadeAcessos;

public sealed record RequisicaoBootstrapAdmin(
    Guid TenantId,
    string NomeCompleto,
    string Email,
    string Senha);

public sealed record RequisicaoLogin(
    Guid TenantId,
    string Email,
    string Senha,
    string? CodigoTotpMfa);

public sealed record RequisicaoRefreshToken(
    Guid TenantId,
    string RefreshToken);

public sealed record RequisicaoConfirmarMfa(
    string CodigoTotp);

public sealed record RequisicaoCriarUsuarioRbac(
    string NomeCompleto,
    string Email,
    string SenhaInicial,
    string PerfilRbac);

public sealed record RespostaSessaoAutenticada(
    Guid UsuarioId,
    Guid TenantId,
    string NomeCompleto,
    string Email,
    string PerfilRbac,
    bool MfaHabilitado,
    string AccessToken,
    DateTime AccessTokenExpiraEmUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiraEmUtc);

public sealed record RespostaUsuarioRbac(
    Guid Id,
    Guid TenantId,
    string NomeCompleto,
    string Email,
    string PerfilRbac,
    bool MfaHabilitado,
    bool Ativo,
    DateTime CreatedAt,
    ulong RowVersion);