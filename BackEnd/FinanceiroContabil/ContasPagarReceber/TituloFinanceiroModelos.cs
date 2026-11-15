using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;

namespace GenesisAltivonSys.BackEnd.FinanceiroContabil.ContasPagarReceber;

[Table("fin_titulos")]
public sealed class TituloFinanceiroRegistro : SysAuditableEntity
{
    [Required]
    [MaxLength(20)]
    [Column("tipo_titulo")]
    public string TipoTitulo { get; set; } = "RECEBER"; // RECEBER ou PAGAR

    [Column("pessoa_id")]
    public Guid? PessoaId { get; set; } // ClienteId ou FornecedorId

    [Required]
    [MaxLength(200)]
    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "ABERTO"; // ABERTO, PAGO, CANCELADO, ATRASADO

    [Column("valor_original", TypeName = "decimal(18,4)")]
    public decimal ValorOriginal { get; set; }

    [Column("valor_pago", TypeName = "decimal(18,4)")]
    public decimal ValorPago { get; set; }

    [Column("data_vencimento", TypeName = "date")]
    public DateTime DataVencimento { get; set; }

    [Column("data_pagamento", TypeName = "date")]
    public DateTime? DataPagamento { get; set; }
}

public sealed record RequisicaoCriarTitulo(
    string TipoTitulo,
    Guid? PessoaId,
    string Descricao,
    decimal ValorOriginal,
    DateTime DataVencimento);

public sealed record RequisicaoBaixarTitulo(
    decimal ValorPago,
    DateTime DataPagamento);

public sealed record RespostaTituloFinanceiro(
    Guid Id,
    Guid TenantId,
    string TipoTitulo,
    string Descricao,
    string Status,
    decimal ValorOriginal,
    decimal ValorPago,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    DateTime CreatedAt);