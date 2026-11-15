using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.BackEnd.FinanceiroContabil.ContasPagarReceber;

public static class TituloFinanceiroEndpoints
{
    public static void MapearEndpointsFinanceiro(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/fico/titulos").RequireAuthorization(PoliticasAutorizacao.GestaoFinanceira);

        grupo.MapGet("/", async (ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var titulos = await db.Set<TituloFinanceiroRegistro>()
                .AsNoTracking()
                .OrderBy(t => t.DataVencimento)
                .Select(t => new RespostaTituloFinanceiro(
                    t.Id,
                    t.TenantId,
                    t.TipoTitulo,
                    t.Descricao,
                    t.Status,
                    t.ValorOriginal,
                    t.ValorPago,
                    t.DataVencimento,
                    t.DataPagamento,
                    t.CreatedAt))
                .ToListAsync(ct);

            return Results.Ok(titulos);
        });

        grupo.MapPost("/", async (RequisicaoCriarTitulo req, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Descricao) || req.ValorOriginal <= 0)
            {
                return Results.BadRequest(new { erro = "Descricao e ValorOriginal (maior que zero) sao obrigatorios." });
            }

            var tipo = req.TipoTitulo.ToUpperInvariant() == "PAGAR" ? "PAGAR" : "RECEBER";
            var statusInicial = req.DataVencimento.Date < DateTime.UtcNow.Date ? "ATRASADO" : "ABERTO";

            var novoTitulo = new TituloFinanceiroRegistro
            {
                TipoTitulo = tipo,
                PessoaId = req.PessoaId,
                Descricao = req.Descricao.Trim(),
                Status = statusInicial,
                ValorOriginal = req.ValorOriginal,
                ValorPago = 0,
                DataVencimento = req.DataVencimento.Date
            };

            db.Set<TituloFinanceiroRegistro>().Add(novoTitulo);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/fico/titulos/{novoTitulo.Id}", new RespostaTituloFinanceiro(
                novoTitulo.Id,
                novoTitulo.TenantId,
                novoTitulo.TipoTitulo,
                novoTitulo.Descricao,
                novoTitulo.Status,
                novoTitulo.ValorOriginal,
                novoTitulo.ValorPago,
                novoTitulo.DataVencimento,
                novoTitulo.DataPagamento,
                novoTitulo.CreatedAt));
        });

        grupo.MapPost("/{id:guid}/baixar", async (Guid id, RequisicaoBaixarTitulo req, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var titulo = await db.ObterComBloqueioExclusivoAsync<TituloFinanceiroRegistro>(id, ct);
            
            if (titulo is null || titulo.Status == "PAGO" || titulo.Status == "CANCELADO")
                return Results.BadRequest(new { erro = "Titulo nao encontrado, ja pago ou cancelado." });

            if (req.ValorPago <= 0)
                return Results.BadRequest(new { erro = "ValorPago deve ser maior que zero." });

            titulo.ValorPago = req.ValorPago;
            titulo.DataPagamento = req.DataPagamento.Date;
            
            // Lógica simples de baixa (pode ser expandida para pagamentos parciais depois)
            if (titulo.ValorPago >= titulo.ValorOriginal)
            {
                titulo.Status = "PAGO";
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new RespostaTituloFinanceiro(
                titulo.Id,
                titulo.TenantId,
                titulo.TipoTitulo,
                titulo.Descricao,
                titulo.Status,
                titulo.ValorOriginal,
                titulo.ValorPago,
                titulo.DataVencimento,
                titulo.DataPagamento,
                titulo.CreatedAt));
        });
    }
}