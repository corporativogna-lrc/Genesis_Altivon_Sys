using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;

namespace GenesisAltivonSys.BackEnd.CadastrosCorporativos.Clientes;

[Table("cad_clientes")]
public sealed class ClienteRegistro : SysAuditableEntity
{
    [Required]
    [MaxLength(200)]
    [Column("razao_social_nome")]
    public string RazaoSocialNome { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column("nome_fantasia")]
    public string? NomeFantasia { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("tipo_pessoa")]
    public string TipoPessoa { get; set; } = "PJ"; // PJ ou PF

    [Required]
    [MaxLength(18)]
    [Column("cpf_cnpj")]
    public string CpfCnpj { get; set; } = string.Empty;

    [MaxLength(180)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(30)]
    [Column("telefone")]
    public string? Telefone { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;
}

public sealed record RequisicaoCriarCliente(
    string RazaoSocialNome,
    string? NomeFantasia,
    string TipoPessoa,
    string CpfCnpj,
    string? Email,
    string? Telefone);

public sealed record RespostaCliente(
    Guid Id,
    Guid TenantId,
    string RazaoSocialNome,
    string? NomeFantasia,
    string TipoPessoa,
    string CpfCnpj,
    string? Email,
    string? Telefone,
    bool Ativo,
    DateTime CreatedAt);