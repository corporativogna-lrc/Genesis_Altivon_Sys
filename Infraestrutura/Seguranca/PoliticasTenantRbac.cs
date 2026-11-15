using Microsoft.AspNetCore.Authorization;

namespace GenesisAltivonSys.Infraestrutura.Seguranca;

public static class PerfisRbac
{
    public const string AdminMaster = "ADMIN_MASTER";
    public const string DiretorFinanceiro = "DIRETOR_FINANCEIRO";
    public const string GerenteOperacional = "GERENTE_OPERACIONAL";
    public const string OperadorPdv = "OPERADOR_PDV";
    public const string AuditorGrc = "AUDITOR_GRC";

    public static readonly IReadOnlySet<string> TodosPerfisValidos = new HashSet<string>(StringComparer.Ordinal)
    {
        AdminMaster,
        DiretorFinanceiro,
        GerenteOperacional,
        OperadorPdv,
        AuditorGrc
    };
}

public static class PoliticasAutorizacao
{
    public const string ApenasAdminMaster = "PoliticaAdminMaster";
    public const string GestaoFinanceira = "PoliticaGestaoFinanceira";
    public const string OperacaoComercial = "PoliticaOperacaoComercial";
    public const string AuditoriaLeitura = "PoliticaAuditoriaLeitura";

    public static void ConfigurarPoliticas(AuthorizationOptions options)
    {
        options.AddPolicy(ApenasAdminMaster, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim("tenant_id")
                  .RequireRole(PerfisRbac.AdminMaster));

        options.AddPolicy(GestaoFinanceira, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim("tenant_id")
                  .RequireRole(PerfisRbac.AdminMaster, PerfisRbac.DiretorFinanceiro));

        options.AddPolicy(OperacaoComercial, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim("tenant_id")
                  .RequireRole(
                      PerfisRbac.AdminMaster,
                      PerfisRbac.DiretorFinanceiro,
                      PerfisRbac.GerenteOperacional,
                      PerfisRbac.OperadorPdv));

        options.AddPolicy(AuditoriaLeitura, policy =>
            policy.RequireAuthenticatedUser()
                  .RequireClaim("tenant_id")
                  .RequireRole(PerfisRbac.AdminMaster, PerfisRbac.AuditorGrc));
    }
}