using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;

namespace GenesisAltivonSys.BackEnd.EstoqueLogistica.ControleSaldosWMS;

[Table("wms_movimentacoes")]
public sealed class WmsMovimentacaoRegistro : SysAuditableEntity
{
    [Required]
    [Column("produto_id")]
    public Guid ProdutoId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("tipo_movimento")]
    public string TipoMovimento { get; set; } = "ENTRADA"; // ENTRADA, SAIDA, AJUSTE

    [Column("quantidade", TypeName = "decimal(18,4)")]
    public decimal Quantidade { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("observacao")]
    public string Observacao { get; set; } = string.Empty;
}

public sealed record RequisicaoMovimentarEstoque(
    Guid ProdutoId,
    string TipoMovimento,
    decimal Quantidade,
    string Observacao);

public sealed record RespostaMovimentacaoWms(
    Guid Id,
    Guid TenantId,
    Guid ProdutoId,
    string TipoMovimento,
    decimal Quantidade,
    string Observacao,
    DateTime CreatedAt);