using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.BackEnd.CadastrosCorporativos.ProdutosServicos;

public static class ProdutoEndpoints
{
    public static void MapearEndpointsProdutos(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/cadastros/produtos").RequireAuthorization(PoliticasAutorizacao.OperacaoComercial);

        grupo.MapGet("/", async (ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var produtos = await db.Set<ProdutoRegistro>()
                .AsNoTracking()
                .OrderBy(p => p.Descricao)
                .Select(p => new RespostaProduto(
                    p.Id,
                    p.TenantId,
                    p.Descricao,
                    p.Sku,
                    p.GtinEan,
                    p.PrecoVenda,
                    p.EstoqueAtual,
                    p.Ativo,
                    p.CreatedAt))
                .ToListAsync(ct);

            return Results.Ok(produtos);
        });

        grupo.MapPost("/", async (
            RequisicaoCriarProduto req,
            ContextoBancoRelacional db,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Descricao) || string.IsNullOrWhiteSpace(req.Sku))
            {
                return Results.BadRequest(new { erro = "Descricao e Sku sao obrigatorios." });
            }

            var novoProduto = new ProdutoRegistro
            {
                Descricao = req.Descricao.Trim(),
                Sku = req.Sku.Trim().ToUpperInvariant(),
                GtinEan = req.GtinEan?.Trim(),
                PrecoVenda = req.PrecoVenda,
                EstoqueAtual = req.EstoqueAtual,
                Ativo = true
            };

            db.Set<ProdutoRegistro>().Add(novoProduto);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/cadastros/produtos/{novoProduto.Id}", new RespostaProduto(
                novoProduto.Id,
                novoProduto.TenantId,
                novoProduto.Descricao,
                novoProduto.Sku,
                novoProduto.GtinEan,
                novoProduto.PrecoVenda,
                novoProduto.EstoqueAtual,
                novoProduto.Ativo,
                novoProduto.CreatedAt));
        });
    }
}