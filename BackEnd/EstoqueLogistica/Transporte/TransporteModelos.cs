using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;

namespace GenesisAltivonSys.BackEnd.EstoqueLogistica.Transporte;

[Table("log_entregas")]
public sealed class EntregaTransporteRegistro : SysAuditableEntity
{
    [Required]
    [Column("pedido_venda_id")]
    public Guid PedidoVendaId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("transportadora")]
    public string Transportadora { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    [Column("status_entrega")]
    public string StatusEntrega { get; set; } = "AGUARDANDO_COLETA"; // AGUARDANDO_COLETA, EM_TRANSITO, ENTREGUE, OCORRENCIA

    [MaxLength(100)]
    [Column("codigo_rastreio")]
    public string? CodigoRastreio { get; set; }

    [Column("valor_frete", TypeName = "decimal(18,4)")]
    public decimal ValorFrete { get; set; }
}

public sealed record RequisicaoCriarEntrega(
    Guid PedidoVendaId,
    string Transportadora,
    decimal ValorFrete);

public sealed record RequisicaoAtualizarStatusEntrega(
    string StatusEntrega,
    string? CodigoRastreio);

public sealed record RespostaEntrega(
    Guid Id,
    Guid TenantId,
    Guid PedidoVendaId,
    string Transportadora,
    string StatusEntrega,
    string? CodigoRastreio,
    decimal ValorFrete,
    DateTime CreatedAt);