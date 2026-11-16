using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using GenesisAltivonSys.BackEnd.CadastrosCorporativos.Clientes;
using GenesisAltivonSys.BackEnd.CadastrosCorporativos.ProdutosServicos;
using GenesisAltivonSys.BackEnd.OperacaoPDV.VendaBalcao;
using GenesisAltivonSys.BackEnd.FinanceiroContabil.ContasPagarReceber;
using GenesisAltivonSys.BackEnd.EstoqueLogistica.ControleSaldosWMS;
using GenesisAltivonSys.BackEnd.EstoqueLogistica.Transporte;
using GenesisAltivonSys.BackEnd.GestaoCompras.PedidosRecebimentos;
using GenesisAltivonSys.BackEnd.FiscalTributario.EmissaoDocumentos;
using GenesisAltivonSys.BackEnd.ManufaturaOperacoes.OrdensProducao;

namespace GenesisAltivonSys.Infraestrutura.BancoDeDados;

public interface IEntidadeMultiTenant
{
    Guid TenantId { get; set; }
}

public interface IEntidadeAuditavel
{
    bool IsDeleted { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    string CreatedBy { get; set; }
    string UpdatedBy { get; set; }
    ulong RowVersion { get; set; }
}

public abstract class SysAuditableEntity : IEntidadeMultiTenant, IEntidadeAuditavel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Required]
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(100)]
    [Column("created_by")]
    public string CreatedBy { get; set; } = "SYSTEM";

    [Required]
    [MaxLength(100)]
    [Column("updated_by")]
    public string UpdatedBy { get; set; } = "SYSTEM";

    [ConcurrencyCheck]
    [Column("row_version")]
    public ulong RowVersion { get; set; } = 1;
}

[Table("sys_tenants")]
public sealed class SysTenantRegistro : SysAuditableEntity
{
    [Required]
    [MaxLength(200)]
    [Column("razao_social")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("nome_fantasia")]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required]
    [MaxLength(18)]
    [Column("cnpj")]
    public string Cnpj { get; set; } = string.Empty;

    [Column("regime_tributario")]
    public byte RegimeTributario { get; set; } = 1;

    [Column("ativo")]
    public bool Ativo { get; set; } = true;
}

[Table("sys_usuarios")]
public sealed class SysUsuarioRegistro : SysAuditableEntity
{
    [Required]
    [MaxLength(180)]
    [Column("nome_completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("senha_hash")]
    public string SenhaHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    [Column("perfil_rbac")]
    public string PerfilRbac { get; set; } = "OPERADOR";

    [Column("mfa_habilitado")]
    public bool MfaHabilitado { get; set; }

    [Column("mfa_segredo_cifrado")]
    public byte[]? MfaSegredoCifrado { get; set; }

    [MaxLength(64)]
    [Column("refresh_token_hash")]
    public string? RefreshTokenHash { get; set; }

    [Column("refresh_token_expira_em")]
    public DateTime? RefreshTokenExpiraEm { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;
}

[Table("sys_audit_logs")]
public sealed class SysAuditLogRegistro
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public ulong Id { get; set; }

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(120)]
    [Column("entidade")]
    public string Entidade { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    [Column("entidade_id")]
    public string EntidadeId { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    [Column("acao")]
    public string Acao { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("usuario_id")]
    public string UsuarioId { get; set; } = string.Empty;

    [MaxLength(45)]
    [Column("ip_origem")]
    public string? IpOrigem { get; set; }

    [Column("dados_anteriores")]
    public string? DadosAnteriores { get; set; }

    [Column("dados_novos")]
    public string? DadosNovos { get; set; }

    [Column("registrado_em")]
    public DateTime RegistradoEm { get; set; } = DateTime.UtcNow;
}

public interface IProvedorContextoTenant
{
    Guid TenantIdAtual { get; }
    string UsuarioIdAtual { get; }
    string? IpOrigemAtual { get; }
}

public sealed class ProvedorContextoTenantHttp : IProvedorContextoTenant
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProvedorContextoTenantHttp(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantIdAtual
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null) return Guid.Empty;

            var claimTenant = httpContext.User.FindFirst("tenant_id")?.Value;
            if (Guid.TryParse(claimTenant, out var tenantClaimId) && tenantClaimId != Guid.Empty)
            {
                return tenantClaimId;
            }

            if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenant) &&
                Guid.TryParse(headerTenant.FirstOrDefault(), out var tenantHeaderId) &&
                tenantHeaderId != Guid.Empty)
            {
                return tenantHeaderId;
            }

            return Guid.Empty;
        }
    }

    public string UsuarioIdAtual
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null) return "SYSTEM_WORKER";

            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? httpContext.User.FindFirst("sub")?.Value;

            return string.IsNullOrWhiteSpace(userId) ? "ANONIMO_API" : userId;
        }
    }

    public string? IpOrigemAtual =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}

public class ContextoBancoRelacional : DbContext
{
    private readonly IProvedorContextoTenant _provedorTenant;

    public ContextoBancoRelacional(
        DbContextOptions<ContextoBancoRelacional> options,
        IProvedorContextoTenant provedorTenant)
        : base(options)
    {
        _provedorTenant = provedorTenant;
    }

    public Guid TenantIdSessao => _provedorTenant.TenantIdAtual;

    // Fase I e II - Governança e IAM
    public DbSet<SysTenantRegistro> Tenants => Set<SysTenantRegistro>();
    public DbSet<SysUsuarioRegistro> Usuarios => Set<SysUsuarioRegistro>();
    public DbSet<SysAuditLogRegistro> AuditLogs => Set<SysAuditLogRegistro>();

    // Fase III - Master Data
    public DbSet<ClienteRegistro> Clientes => Set<ClienteRegistro>();
    public DbSet<ProdutoRegistro> Produtos => Set<ProdutoRegistro>();

    // Fase IV - Operação PDV
    public DbSet<PdvVendaRegistro> PdvVendas => Set<PdvVendaRegistro>();
    public DbSet<PdvItemRegistro> PdvItens => Set<PdvItemRegistro>();

    // Fase V - Financeiro Contábil
    public DbSet<TituloFinanceiroRegistro> TitulosFinanceiros => Set<TituloFinanceiroRegistro>();

    // Fase VI - Controle de Estoque (WMS)
    public DbSet<WmsMovimentacaoRegistro> WmsMovimentacoes => Set<WmsMovimentacaoRegistro>();

    // Fase VII - Gestão de Compras (SCM)
    public DbSet<PedidoCompraRegistro> PedidosCompra => Set<PedidoCompraRegistro>();
    public DbSet<PedidoCompraItemRegistro> PedidoItens => Set<PedidoCompraItemRegistro>();

    // Fase VIII - Fiscal e Tributário
    public DbSet<DocumentoFiscalRegistro> DocumentosFiscais => Set<DocumentoFiscalRegistro>();

    // Fase IX - Logística e Transporte (TMS)
    public DbSet<EntregaTransporteRegistro> EntregasTransporte => Set<EntregaTransporteRegistro>();

    // Fase X - Manufatura e Operações (MES)
    public DbSet<OrdemProducaoRegistro> OrdensProducao => Set<OrdemProducaoRegistro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SysTenantRegistro>(entity =>
        {
            entity.HasIndex(e => e.Cnpj).IsUnique();
            entity.HasIndex(e => e.TenantId);
        });

        modelBuilder.Entity<SysUsuarioRegistro>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.HasIndex(e => e.TenantId);
        });

        modelBuilder.Entity<SysAuditLogRegistro>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Entidade, e.EntidadeId });
            entity.HasIndex(e => e.RegistradoEm);
        });

        AplicarFiltroGlobalMultiTenantESoftDelete(modelBuilder);
    }

    private void AplicarFiltroGlobalMultiTenantESoftDelete(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var implementaTenant = typeof(IEntidadeMultiTenant).IsAssignableFrom(clrType);
            var implementaAudit = typeof(IEntidadeAuditavel).IsAssignableFrom(clrType);

            if (!implementaTenant && !implementaAudit) continue;

            var parameter = Expression.Parameter(clrType, "e");
            Expression? filtroCombinado = null;

            if (implementaAudit)
            {
                var propIsDeleted = Expression.Property(parameter, nameof(IEntidadeAuditavel.IsDeleted));
                var notDeleted = Expression.Equal(propIsDeleted, Expression.Constant(false));
                filtroCombinado = notDeleted;
            }

            if (implementaTenant)
            {
                var propTenantId = Expression.Property(parameter, nameof(IEntidadeMultiTenant.TenantId));
                var propSessaoTenantId = Expression.Property(Expression.Constant(this), nameof(TenantIdSessao));
                var matchTenant = Expression.Equal(propTenantId, propSessaoTenantId);
                filtroCombinado = filtroCombinado is null
                    ? matchTenant
                    : Expression.AndAlso(filtroCombinado, matchTenant);
            }

            if (filtroCombinado is not null)
            {
                var lambda = Expression.Lambda(filtroCombinado, parameter);
                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
        }
    }

    public async Task<TEntidade?> ObterComBloqueioExclusivoAsync<TEntidade>(
        Guid id,
        CancellationToken cancellationToken = default)
        where TEntidade : SysAuditableEntity
    {
        if (TenantIdSessao == Guid.Empty)
        {
            throw new InvalidOperationException("Operacao com bloqueio FOR UPDATE exige TenantId ativo na sessao.");
        }

        var entityType = Model.FindEntityType(typeof(TEntidade))
            ?? throw new InvalidOperationException($"Entidade {typeof(TEntidade).Name} nao mapeada no contexto.");

        var tableName = entityType.GetTableName()
            ?? throw new InvalidOperationException($"Tabela nao definida para {typeof(TEntidade).Name}.");

        var sql = $"SELECT * FROM `{tableName}` WHERE `id` = {{0}} AND `tenant_id` = {{1}} AND `is_deleted` = 0 LIMIT 1 FOR UPDATE";

        return await Set<TEntidade>()
            .FromSqlRaw(sql, id, TenantIdSessao)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AplicarGovernancaAuditoriaETenant();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        AplicarGovernancaAuditoriaETenant();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AplicarGovernancaAuditoriaETenant()
    {
        var agora = DateTime.UtcNow;
        var usuarioAtual = _provedorTenant.UsuarioIdAtual;
        var tenantSessao = _provedorTenant.TenantIdAtual;
        var ipOrigem = _provedorTenant.IpOrigemAtual;
        var logsPendentes = new List<SysAuditLogRegistro>();

        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is SysAuditLogRegistro || entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
            {
                continue;
            }

            var tenantRegistro = tenantSessao;

            if (entry.Entity is IEntidadeMultiTenant entidadeTenant)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entidadeTenant.TenantId == Guid.Empty)
                    {
                        if (tenantSessao == Guid.Empty)
                        {
                            throw new UnauthorizedAccessException("Insercao bloqueada: TenantId nao informado na entidade nem na sessao.");
                        }
                        entidadeTenant.TenantId = tenantSessao;
                    }
                    else if (tenantSessao != Guid.Empty && entidadeTenant.TenantId != tenantSessao)
                    {
                        throw new UnauthorizedAccessException("Violacao de isolamento Multi-Tenant: tentativa de gravar em outro TenantId.");
                    }
                }
                else if (tenantSessao != Guid.Empty && entidadeTenant.TenantId != tenantSessao)
                {
                    throw new UnauthorizedAccessException("Violacao de isolamento Multi-Tenant: tentativa de alterar registro de outro TenantId.");
                }

                tenantRegistro = entidadeTenant.TenantId;
            }

            var acaoAuditoria = entry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "SOFT_DELETE",
                _ => "UNKNOWN"
            };

            if (entry.Entity is IEntidadeAuditavel entidadeAuditavel)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entidadeAuditavel.IsDeleted = false;
                        entidadeAuditavel.CreatedAt = agora;
                        entidadeAuditavel.UpdatedAt = agora;
                        entidadeAuditavel.CreatedBy = usuarioAtual;
                        entidadeAuditavel.UpdatedBy = usuarioAtual;
                        entidadeAuditavel.RowVersion = 1;
                        break;

                    case EntityState.Modified:
                        entry.Property(nameof(IEntidadeAuditavel.CreatedAt)).IsModified = false;
                        entry.Property(nameof(IEntidadeAuditavel.CreatedBy)).IsModified = false;
                        entidadeAuditavel.UpdatedAt = agora;
                        entidadeAuditavel.UpdatedBy = usuarioAtual;
                        entidadeAuditavel.RowVersion += 1;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Property(nameof(IEntidadeAuditavel.CreatedAt)).IsModified = false;
                        entry.Property(nameof(IEntidadeAuditavel.CreatedBy)).IsModified = false;
                        entidadeAuditavel.IsDeleted = true;
                        entidadeAuditavel.UpdatedAt = agora;
                        entidadeAuditavel.UpdatedBy = usuarioAtual;
                        entidadeAuditavel.RowVersion += 1;
                        break;
                }
            }

            var entidadeId = entry.Properties
                .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?
                .CurrentValue?.ToString() ?? "SEM_ID";

            logsPendentes.Add(new SysAuditLogRegistro
            {
                TenantId = tenantRegistro,
                Entidade = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                EntidadeId = entidadeId,
                Acao = acaoAuditoria,
                UsuarioId = usuarioAtual,
                IpOrigem = ipOrigem,
                DadosAnteriores = acaoAuditoria == "INSERT" ? null : SerializarValores(entry, original: true),
                DadosNovos = SerializarValores(entry, original: false),
                RegistradoEm = agora
            });
        }

        if (logsPendentes.Count > 0)
        {
            AuditLogs.AddRange(logsPendentes);
        }
    }

    private static string SerializarValores(EntityEntry entry, bool original)
    {
        var mapa = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            var nomeColuna = prop.Metadata.GetColumnName();
            if (nomeColuna.Contains("senha", StringComparison.OrdinalIgnoreCase) ||
                nomeColuna.Contains("segredo", StringComparison.OrdinalIgnoreCase) ||
                nomeColuna.Contains("token", StringComparison.OrdinalIgnoreCase))
            {
                mapa[nomeColuna] = "***REDIGIDO***";
                continue;
            }

            mapa[nomeColuna] = original ? prop.OriginalValue : prop.CurrentValue;
        }

        return JsonSerializer.Serialize(mapa);
    }
}