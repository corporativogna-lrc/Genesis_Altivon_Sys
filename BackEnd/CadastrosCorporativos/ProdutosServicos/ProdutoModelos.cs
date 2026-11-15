using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;

namespace GenesisAltivonSys.BackEnd.CadastrosCorporativos.ProdutosServicos;

[Table("cad_produtos")]
public sealed class ProdutoRegistro : SysAuditableEntity
{
    [Required]
    [MaxLength(150)]
    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    [Column("sku")]
    public string Sku { get; set; } = string.Empty;

    [MaxLength(14)]
    [Column("gtin_ean")]
    public string? GtinEan { get; set; }

    [Column("preco_venda", TypeName = "decimal(18,4)")]
    public decimal PrecoVenda { get; set; }

    [Column("estoque_atual", TypeName = "decimal(18,4)")]
    public decimal EstoqueAtual { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;
}

public sealed record RequisicaoCriarProduto(
    string Descricao,
    string Sku,
    string? GtinEan,
    decimal PrecoVenda,
    decimal EstoqueAtual);

public sealed record RespostaProduto(
    Guid Id,
    Guid TenantId,
    string Descricao,
    string Sku,
    string? GtinEan,
    decimal PrecoVenda,
    decimal EstoqueAtual,
    bool Ativo,
    DateTime CreatedAt);