using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.Testes.Suporte;

public sealed class ProvedorContextoTenantSimulado : IProvedorContextoTenant
{
    public Guid TenantIdAtual { get; set; }
    public string UsuarioIdAtual { get; set; } = "TESTE_AUTOMATIZADO_USR";
    public string? IpOrigemAtual { get; set; } = "127.0.0.1";
}

public static class ContextoTesteFactory
{
    public static (ContextoBancoRelacional Contexto, ProvedorContextoTenantSimulado ProvedorTenant) CriarContextoIsolado(
        Guid tenantInicial,
        string? nomeBancoMemoria = null)
    {
        var dbName = nomeBancoMemoria ?? $"GenesisAltivonTestDb_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<ContextoBancoRelacional>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var provedor = new ProvedorContextoTenantSimulado
        {
            TenantIdAtual = tenantInicial
        };

        var contexto = new ContextoBancoRelacional(options, provedor);
        return (contexto, provedor);
    }
}