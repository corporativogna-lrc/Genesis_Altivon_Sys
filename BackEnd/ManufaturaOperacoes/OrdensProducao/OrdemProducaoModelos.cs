using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;

namespace GenesisAltivonSys.BackEnd.ManufaturaOperacoes.OrdensProducao;

[Table("mes_ordens_producao")]
public sealed class OrdemProducaoRegistro : SysAuditableEntity
{
    [Required]
    [MaxLength(40)]
    [Column("codigo_op")]
    public string CodigoOp { get; set; } = string.Empty;

    [Required]
    [Column("produto_acabado_id")]
    public Guid ProdutoAcabadoId { get; set; }

    [MaxLength(100)]
    [Column("linha_producao")]
    public string LinhaProducao { get; set; } = "LINHA-01";

    [Required]
    [MaxLength(25)]
    [Column("status_op")]
    public string StatusOp { get; set; } = "PLANEJADA"; // PLANEJADA, EM_PRODUCAO, CONCLUIDA, CANCELADA

    [Column("quantidade_planejada", TypeName = "decimal(18,4)")]
    public decimal QuantidadePlanejada { get; set; }

    [Column("quantidade_produzida", TypeName = "decimal(18,4)")]
    public decimal QuantidadeProduzida { get; set; }

    [Column("iniciada_em")]
    public DateTime? IniciadaEm { get; set; }

    [Column("concluida_em")]
    public DateTime? ConcluidaEm { get; set; }
}

public sealed record RequisicaoCriarOrdemProducao(
    Guid ProdutoAcabadoId,
    string? LinhaProducao,
    decimal QuantidadePlanejada);

public sealed record RequisicaoConcluirOrdemProducao(
    decimal QuantidadeProduzida);

public sealed record RespostaOrdemProducao(
    Guid Id,
    Guid TenantId,
    string CodigoOp,
    Guid ProdutoAcabadoId,
    string LinhaProducao,
    string StatusOp,
    decimal QuantidadePlanejada,
    decimal QuantidadeProduzida,
    DateTime? IniciadaEm,
    DateTime? ConcluidaEm,
    DateTime CreatedAt);