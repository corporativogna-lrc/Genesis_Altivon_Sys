using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.BackEnd.CadastrosCorporativos.Clientes;

public static class ClienteEndpoints
{
    public static void MapearEndpointsClientes(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/cadastros/clientes").RequireAuthorization(PoliticasAutorizacao.OperacaoComercial);

        grupo.MapGet("/", async (ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var clientes = await db.Set<ClienteRegistro>()
                .AsNoTracking()
                .OrderBy(c => c.RazaoSocialNome)
                .Select(c => new RespostaCliente(
                    c.Id,
                    c.TenantId,
                    c.RazaoSocialNome,
                    c.NomeFantasia,
                    c.TipoPessoa,
                    c.CpfCnpj,
                    c.Email,
                    c.Telefone,
                    c.Ativo,
                    c.CreatedAt))
                .ToListAsync(ct);

            return Results.Ok(clientes);
        });

        grupo.MapPost("/", async (
            RequisicaoCriarCliente req,
            ContextoBancoRelacional db,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.RazaoSocialNome) || string.IsNullOrWhiteSpace(req.CpfCnpj))
            {
                return Results.BadRequest(new { erro = "RazaoSocialNome e CpfCnpj sao obrigatorios." });
            }

            var novoCliente = new ClienteRegistro
            {
                RazaoSocialNome = req.RazaoSocialNome.Trim(),
                NomeFantasia = req.NomeFantasia?.Trim(),
                TipoPessoa = req.TipoPessoa.ToUpperInvariant() == "PF" ? "PF" : "PJ",
                CpfCnpj = req.CpfCnpj.Trim(),
                Email = req.Email?.Trim(),
                Telefone = req.Telefone?.Trim(),
                Ativo = true
            };

            db.Set<ClienteRegistro>().Add(novoCliente);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/cadastros/clientes/{novoCliente.Id}", new RespostaCliente(
                novoCliente.Id,
                novoCliente.TenantId,
                novoCliente.RazaoSocialNome,
                novoCliente.NomeFantasia,
                novoCliente.TipoPessoa,
                novoCliente.CpfCnpj,
                novoCliente.Email,
                novoCliente.Telefone,
                novoCliente.Ativo,
                novoCliente.CreatedAt));
        });
    }
}