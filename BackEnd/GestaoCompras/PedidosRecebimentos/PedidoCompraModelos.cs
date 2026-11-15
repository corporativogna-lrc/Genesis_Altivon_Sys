using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;

namespace GenesisAltivonSys.BackEnd.GestaoCompras.PedidosRecebimentos;

[Table("scm_pedidos")]
public sealed class PedidoCompraRegistro : SysAuditableEntity
{
    [Required]
    [Column("fornecedor_id")]
    public Guid FornecedorId { get; set; }

    [Required]
    [MaxLength(30)]
    [Column("status")]
    public string Status { get; set; } = "RASCUNHO"; // RASCUNHO, APROVADO, RECEBIDO, CANCELADO

    [Column("valor_total", TypeName = "decimal(18,4)")]
    public decimal ValorTotal { get; set; }
}

[Table("scm_pedido_itens")]
public sealed class PedidoCompraItemRegistro : SysAuditableEntity
{
    [Required]
    [Column("pedido_id")]
    public Guid PedidoId { get; set; }

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

public sealed record RequisicaoCriarPedidoCompra(Guid FornecedorId);
public sealed record RequisicaoAdicionarItemPedido(Guid ProdutoId, decimal Quantidade, decimal PrecoUnitario);
public sealed record RespostaPedidoCompra(Guid Id, Guid TenantId, Guid FornecedorId, string Status, decimal ValorTotal, DateTime CreatedAt);