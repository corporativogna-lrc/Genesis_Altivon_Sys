using GenesisAltivonSys.BackEnd.CadastrosCorporativos.Clientes;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Testes.Suporte;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.Testes;

public sealed class GovernancaMultiTenantAuditoriaTests
{
    [Fact]
    public async Task Deve_Isolar_Leitura_Entre_Tenants_Diferentes()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var nomeBanco = $"BancoIsolamento_{Guid.NewGuid():N}";

        // 1. Grava cliente no Tenant A
        var (ctxA, _) = ContextoTesteFactory.CriarContextoIsolado(tenantA, nomeBanco);
        ctxA.Clientes.Add(new ClienteRegistro
        {
            RazaoSocialNome = "Empresa Alfa Ltda",
            TipoPessoa = "PJ",
            CpfCnpj = "11222333000181"
        });
        await ctxA.SaveChangesAsync();

        // 2. Consulta no contexto autenticado como Tenant B (mesmo banco)
        var (ctxB, _) = ContextoTesteFactory.CriarContextoIsolado(tenantB, nomeBanco);
        var clientesVisiveisNoTenantB = await ctxB.Clientes.ToListAsync();

        Assert.Empty(clientesVisiveisNoTenantB);
    }

    [Fact]
    public async Task Deve_Bloquear_Gravacao_Com_TenantId_Forjado()
    {
        var tenantSessao = Guid.NewGuid();
        var tenantAlvoForjado = Guid.NewGuid();

        var (ctx, _) = ContextoTesteFactory.CriarContextoIsolado(tenantSessao);

        ctx.Clientes.Add(new ClienteRegistro
        {
            TenantId = tenantAlvoForjado,
            RazaoSocialNome = "Tentativa Invasao Tenant",
            TipoPessoa = "PJ",
            CpfCnpj = "99888777000166"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => ctx.SaveChangesAsync());
    }

    [Fact]
    public async Task Deve_Converter_Exclusao_Em_SoftDelete_E_Incrementar_RowVersion_Com_Auditoria()
    {
        var tenantId = Guid.NewGuid();
        var (ctx, _) = ContextoTesteFactory.CriarContextoIsolado(tenantId);

        var cliente = new ClienteRegistro
        {
            RazaoSocialNome = "Cliente Auditoria Teste",
            TipoPessoa = "PF",
            CpfCnpj = "12345678901"
        };

        ctx.Clientes.Add(cliente);
        await ctx.SaveChangesAsync();

        Assert.Equal((ulong)1, cliente.RowVersion);
        Assert.False(cliente.IsDeleted);

        // Executa exclusao logica (Remove -> interceptado para SOFT_DELETE)
        ctx.Clientes.Remove(cliente);
        await ctx.SaveChangesAsync();

        // Nao deve mais aparecer em consultas normais
        var consultaAtivos = await ctx.Clientes.ToListAsync();
        Assert.Empty(consultaAtivos);

        // Deve existir fisicamente marcado como IsDeleted = true e RowVersion = 2
        var registroBruto = await ctx.Clientes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == cliente.Id);

        Assert.NotNull(registroBruto);
        Assert.True(registroBruto!.IsDeleted);
        Assert.Equal((ulong)2, registroBruto.RowVersion);

        // Valida geracao de trilha imutavel em SysAuditLogRegistro
        var logs = await ctx.AuditLogs
            .Where(l => l.EntidadeId == cliente.Id.ToString())
            .OrderBy(l => l.Id)
            .ToListAsync();

        Assert.Equal(2, logs.Count);
        Assert.Equal("INSERT", logs[0].Acao);
        Assert.Equal("SOFT_DELETE", logs[1].Acao);
    }

    [Fact]
    public async Task Deve_Redigir_Campos_Sensiveis_Na_Trilha_De_Auditoria()
    {
        var tenantId = Guid.NewGuid();
        var (ctx, _) = ContextoTesteFactory.CriarContextoIsolado(tenantId);

        var usuario = new SysUsuarioRegistro
        {
            NomeCompleto = "Operador Teste",
            Email = "operador@altivon.com.br",
            SenhaHash = "PBKDF2$120000$SALTSECRETO$HASHSECRETO",
            PerfilRbac = "OPERADOR_PDV",
            RefreshTokenHash = "hash_refresh_super_secreto"
        };

        ctx.Usuarios.Add(usuario);
        await ctx.SaveChangesAsync();

        var log = await ctx.AuditLogs.FirstAsync(l => l.EntidadeId == usuario.Id.ToString());

        Assert.NotNull(log.DadosNovos);
        Assert.DoesNotContain("HASHSECRETO", log.DadosNovos);
        Assert.DoesNotContain("hash_refresh_super_secreto", log.DadosNovos);
        Assert.Contains("***REDIGIDO***", log.DadosNovos);
    }
}