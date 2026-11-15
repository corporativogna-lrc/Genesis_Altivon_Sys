using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;

namespace GenesisAltivonSys.BackEnd.OperacaoPDV.VendaBalcao;

[Table("pdv_vendas")]
public sealed class PdvVendaRegistro : SysAuditableEntity
{
    [Column("cliente_id")]
    public Guid? ClienteId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "ABERTA"; // ABERTA, FINALIZADA, CANCELADA

    [Column("valor_total", TypeName = "decimal(18,4)")]
    public decimal ValorTotal { get; set; }
}

[Table("pdv_itens")]
public sealed class PdvItemRegistro : SysAuditableEntity
{
    [Required]
    [Column("venda_id")]
    public Guid VendaId { get; set; }

    [Required]
    [Column("produto_id")]
    public Guid ProdutoId { get; set; }

    [Column("quantidade", TypeName = "decimal(18,4)")]
    public decimal Quantidade { get; set; }

    [Column("preco_unitario", TypeName = "decimal(18,4)")]
    public decimal PrecoUnitario { get; set; }

    [Column("subtotal", TypeName = "decimal(18,4)")]
    public decimal Subtotal { get; set; }
}

public sealed record RequisicaoIniciarVenda(Guid? ClienteId);
public sealed record RequisicaoAdicionarItem(Guid ProdutoId, decimal Quantidade);
public sealed record RespostaVendaPdv(Guid Id, Guid TenantId, string Status, decimal ValorTotal, DateTime CreatedAt);