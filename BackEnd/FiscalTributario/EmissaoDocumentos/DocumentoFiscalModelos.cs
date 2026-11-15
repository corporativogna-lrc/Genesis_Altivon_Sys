using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;

namespace GenesisAltivonSys.BackEnd.FiscalTributario.EmissaoDocumentos;

[Table("fis_documentos")]
public sealed class DocumentoFiscalRegistro : SysAuditableEntity
{
    [Required]
    [MaxLength(10)]
    [Column("modelo")]
    public string Modelo { get; set; } = "65"; // 55 para NFe, 65 para NFCe

    [Required]
    [MaxLength(20)]
    [Column("status_sefaz")]
    public string StatusSefaz { get; set; } = "PENDENTE"; // PENDENTE, AUTORIZADA, REJEITADA, CANCELADA

    [Column("numero")]
    public int Numero { get; set; }

    [Column("serie")]
    public int Serie { get; set; } = 1;

    [Column("valor_total_nota", TypeName = "decimal(18,4)")]
    public decimal ValorTotalNota { get; set; }

    [MaxLength(44)]
    [Column("chave_acesso")]
    public string? ChaveAcesso { get; set; }

    [Column("protocolo_autorizacao")]
    public string? ProtocoloAutorizacao { get; set; }
}

public sealed record RequisicaoEmitirNota(
    string Modelo,
    int Numero,
    int Serie,
    decimal ValorTotalNota);

public sealed record RespostaDocumentoFiscal(
    Guid Id,
    Guid TenantId,
    string Modelo,
    string StatusSefaz,
    int Numero,
    int Serie,
    decimal ValorTotalNota,
    string? ChaveAcesso,
    DateTime CreatedAt);