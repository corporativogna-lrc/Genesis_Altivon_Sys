using System.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.Infraestrutura.BancoDeDados;

public sealed record ResultadoDiagnosticoBanco(
    bool Conectado,
    string Servidor,
    int Porta,
    string NomeBanco,
    string VersaoMotor,
    long LatenciaMs,
    IReadOnlyList<string> TabelasGovernancaValidadas,
    int TotalTenantsAtivos);

public sealed class GerenciadorEsquemaUnificado
{
    private static readonly string[] TabelasObrigatorias =
    [
        "sys_tenants",
        "sys_usuarios",
        "sys_audit_logs"
    ];

    private readonly ContextoBancoRelacional _contexto;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GerenciadorEsquemaUnificado> _logger;

    public GerenciadorEsquemaUnificado(
        ContextoBancoRelacional contexto,
        IConfiguration configuration,
        ILogger<GerenciadorEsquemaUnificado> logger)
    {
        _contexto = contexto;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ResultadoDiagnosticoBanco> ValidarIntegridadeEsquemaAsync(
        CancellationToken cancellationToken = default)
    {
        var cronometro = Stopwatch.StartNew();
        var servidor = _configuration["BancoDeDados:Servidor"] ?? "127.0.0.1";
        var porta = int.TryParse(_configuration["BancoDeDados:Porta"], out var p) ? p : 3309;
        var nomeBanco = _configuration["BancoDeDados:NomeBanco"] ?? "GenesisAltivonDb";

        var conexao = _contexto.Database.GetDbConnection();
        var fechadaOriginalmente = conexao.State != ConnectionState.Open;

        if (fechadaOriginalmente)
        {
            await conexao.OpenAsync(cancellationToken);
        }

        try
        {
            var versaoMotor = conexao.ServerVersion;
            var tabelasEncontradas = new List<string>();

            foreach (var tabela in TabelasObrigatorias)
            {
                await using var cmd = conexao.CreateCommand();
                cmd.CommandText = """
                    SELECT COUNT(*)
                    FROM information_schema.tables
                    WHERE table_schema = @schema
                      AND table_name = @tabela;
                    """;

                var paramSchema = cmd.CreateParameter();
                paramSchema.ParameterName = "@schema";
                paramSchema.Value = nomeBanco;
                cmd.Parameters.Add(paramSchema);

                var paramTabela = cmd.CreateParameter();
                paramTabela.ParameterName = "@tabela";
                paramTabela.Value = tabela;
                cmd.Parameters.Add(paramTabela);

                var existe = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)) > 0;
                if (!existe)
                {
                    throw new InvalidOperationException(
                        $"Tabela canonica obrigatoria '{tabela}' ausente no banco '{nomeBanco}'. Execute iniciar-infra.bat.");
                }

                tabelasEncontradas.Add(tabela);
            }

            await using var cmdTenants = conexao.CreateCommand();
            cmdTenants.CommandText = "SELECT COUNT(*) FROM sys_tenants WHERE is_deleted = 0 AND ativo = 1;";
            var totalTenantsAtivos = Convert.ToInt32(await cmdTenants.ExecuteScalarAsync(cancellationToken));

            if (totalTenantsAtivos == 0)
            {
                throw new InvalidOperationException(
                    "Nenhum tenant ativo encontrado em sys_tenants. Verifique o bootstrap de init_genesis_altivon_db.sql.");
            }

            cronometro.Stop();

            _logger.LogInformation(
                "Esquema canonico {Banco} validado em {Servidor}:{Porta} ({Versao}) - {Latencia}ms - {Tenants} tenant(s) ativo(s).",
                nomeBanco,
                servidor,
                porta,
                versaoMotor,
                cronometro.ElapsedMilliseconds,
                totalTenantsAtivos);

            return new ResultadoDiagnosticoBanco(
                Conectado: true,
                Servidor: servidor,
                Porta: porta,
                NomeBanco: nomeBanco,
                VersaoMotor: versaoMotor,
                LatenciaMs: cronometro.ElapsedMilliseconds,
                TabelasGovernancaValidadas: tabelasEncontradas,
                TotalTenantsAtivos: totalTenantsAtivos);
        }
        finally
        {
            if (fechadaOriginalmente && conexao.State == ConnectionState.Open)
            {
                await conexao.CloseAsync();
            }
        }
    }
}