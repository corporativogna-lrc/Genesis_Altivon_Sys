using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;
using GenesisAltivonSys.BackEnd.CadastrosCorporativos.ProdutosServicos;

namespace GenesisAltivonSys.BackEnd.EstoqueLogistica.ControleSaldosWMS;

public static class EstoqueEndpoints
{
    public static void MapearEndpointsEstoqueWms(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/wms/movimentacoes").RequireAuthorization(PoliticasAutorizacao.OperacaoComercial);

        grupo.MapPost("/", async (
            RequisicaoMovimentarEstoque req,
            ContextoBancoRelacional db,
            CancellationToken ct) =>
        {
            if (req.Quantidade <= 0)
                return Results.BadRequest(new { erro = "A quantidade movimentada deve ser maior que zero." });

            var tipo = req.TipoMovimento.ToUpperInvariant();
            if (tipo != "ENTRADA" && tipo != "SAIDA" && tipo != "AJUSTE")
                return Results.BadRequest(new { erro = "TipoMovimento invalido. Use ENTRADA, SAIDA ou AJUSTE." });

            var produto = await db.ObterComBloqueioExclusivoAsync<ProdutoRegistro>(req.ProdutoId, ct);
            if (produto is null)
                return Results.BadRequest(new { erro = "Produto nao encontrado no estoque." });

            if (tipo == "SAIDA" && produto.EstoqueAtual < req.Quantidade)
                return Results.BadRequest(new { erro = "Saldo insuficiente para a saida solicitada." });

            var movimentacao = new WmsMovimentacaoRegistro
            {
                ProdutoId = produto.Id,
                TipoMovimento = tipo,
                Quantidade = req.Quantidade,
                Observacao = req.Observacao.Trim()
            };

            db.Set<WmsMovimentacaoRegistro>().Add(movimentacao);

            if (tipo == "ENTRADA" || tipo == "AJUSTE")
                produto.EstoqueAtual += req.Quantidade;
            else if (tipo == "SAIDA")
                produto.EstoqueAtual -= req.Quantidade;

            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/wms/movimentacoes/{movimentacao.Id}", new RespostaMovimentacaoWms(
                movimentacao.Id,
                movimentacao.TenantId,
                movimentacao.ProdutoId,
                movimentacao.TipoMovimento,
                movimentacao.Quantidade,
                movimentacao.Observacao,
                movimentacao.CreatedAt));
        });

        grupo.MapGet("/produto/{produtoId:guid}", async (Guid produtoId, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var historico = await db.Set<WmsMovimentacaoRegistro>()
                .AsNoTracking()
                .Where(m => m.ProdutoId == produtoId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new RespostaMovimentacaoWms(
                    m.Id,
                    m.TenantId,
                    m.ProdutoId,
                    m.TipoMovimento,
                    m.Quantidade,
                    m.Observacao,
                    m.CreatedAt))
                .ToListAsync(ct);

            return Results.Ok(historico);
        });
    }
}