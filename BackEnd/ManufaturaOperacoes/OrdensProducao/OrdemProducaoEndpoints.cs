using GenesisAltivonSys.BackEnd.CadastrosCorporativos.ProdutosServicos;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.BackEnd.ManufaturaOperacoes.OrdensProducao;

public static class OrdemProducaoEndpoints
{
    public static void MapearEndpointsManufaturaMes(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/mes/ordens-producao")
            .RequireAuthorization(PoliticasAutorizacao.OperacaoComercial);

        grupo.MapGet("/", async (ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var ordens = await db.Set<OrdemProducaoRegistro>()
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new RespostaOrdemProducao(
                    o.Id,
                    o.TenantId,
                    o.CodigoOp,
                    o.ProdutoAcabadoId,
                    o.LinhaProducao,
                    o.StatusOp,
                    o.QuantidadePlanejada,
                    o.QuantidadeProduzida,
                    o.IniciadaEm,
                    o.ConcluidaEm,
                    o.CreatedAt))
                .ToListAsync(ct);

            return Results.Ok(ordens);
        });

        grupo.MapPost("/", async (RequisicaoCriarOrdemProducao req, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            if (req.ProdutoAcabadoId == Guid.Empty || req.QuantidadePlanejada <= 0)
            {
                return Results.BadRequest(new
                {
                    erro = "ProdutoAcabadoId e QuantidadePlanejada (maior que zero) sao obrigatorios."
                });
            }

            var produtoExiste = await db.Set<ProdutoRegistro>()
                .AsNoTracking()
                .AnyAsync(p => p.Id == req.ProdutoAcabadoId, ct);

            if (!produtoExiste)
            {
                return Results.BadRequest(new { erro = "Produto acabado nao encontrado no catalogo do tenant." });
            }

            var sufixo = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var novaOp = new OrdemProducaoRegistro
            {
                CodigoOp = $"OP-{DateTime.UtcNow:yyyyMMdd}-{sufixo}",
                ProdutoAcabadoId = req.ProdutoAcabadoId,
                LinhaProducao = string.IsNullOrWhiteSpace(req.LinhaProducao)
                    ? "LINHA-01"
                    : req.LinhaProducao.Trim().ToUpperInvariant(),
                StatusOp = "PLANEJADA",
                QuantidadePlanejada = req.QuantidadePlanejada,
                QuantidadeProduzida = 0
            };

            db.Set<OrdemProducaoRegistro>().Add(novaOp);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/mes/ordens-producao/{novaOp.Id}", new RespostaOrdemProducao(
                novaOp.Id,
                novaOp.TenantId,
                novaOp.CodigoOp,
                novaOp.ProdutoAcabadoId,
                novaOp.LinhaProducao,
                novaOp.StatusOp,
                novaOp.QuantidadePlanejada,
                novaOp.QuantidadeProduzida,
                novaOp.IniciadaEm,
                novaOp.ConcluidaEm,
                novaOp.CreatedAt));
        });

        grupo.MapPost("/{id:guid}/iniciar", async (Guid id, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var op = await db.ObterComBloqueioExclusivoAsync<OrdemProducaoRegistro>(id, ct);
            if (op is null || op.StatusOp != "PLANEJADA")
            {
                return Results.BadRequest(new { erro = "Ordem de producao nao encontrada ou nao esta PLANEJADA." });
            }

            op.StatusOp = "EM_PRODUCAO";
            op.IniciadaEm = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            return Results.Ok(new RespostaOrdemProducao(
                op.Id,
                op.TenantId,
                op.CodigoOp,
                op.ProdutoAcabadoId,
                op.LinhaProducao,
                op.StatusOp,
                op.QuantidadePlanejada,
                op.QuantidadeProduzida,
                op.IniciadaEm,
                op.ConcluidaEm,
                op.CreatedAt));
        });

        grupo.MapPost("/{id:guid}/concluir", async (
            Guid id,
            RequisicaoConcluirOrdemProducao req,
            ContextoBancoRelacional db,
            CancellationToken ct) =>
        {
            if (req.QuantidadeProduzida <= 0)
            {
                return Results.BadRequest(new { erro = "QuantidadeProduzida deve ser maior que zero." });
            }

            var op = await db.ObterComBloqueioExclusivoAsync<OrdemProducaoRegistro>(id, ct);
            if (op is null || (op.StatusOp != "PLANEJADA" && op.StatusOp != "EM_PRODUCAO"))
            {
                return Results.BadRequest(new { erro = "Ordem de producao invalida, ja concluida ou cancelada." });
            }

            var produtoAcabado = await db.ObterComBloqueioExclusivoAsync<ProdutoRegistro>(op.ProdutoAcabadoId, ct);
            if (produtoAcabado is null)
            {
                return Results.BadRequest(new { erro = "Produto acabado vinculado nao encontrado." });
            }

            op.StatusOp = "CONCLUIDA";
            op.QuantidadeProduzida = req.QuantidadeProduzida;
            op.IniciadaEm ??= DateTime.UtcNow;
            op.ConcluidaEm = DateTime.UtcNow;

            // Credita a produção finalizada diretamente no saldo físico do produto acabado
            produtoAcabado.EstoqueAtual += req.QuantidadeProduzida;

            await db.SaveChangesAsync(ct);

            return Results.Ok(new RespostaOrdemProducao(
                op.Id,
                op.TenantId,
                op.CodigoOp,
                op.ProdutoAcabadoId,
                op.LinhaProducao,
                op.StatusOp,
                op.QuantidadePlanejada,
                op.QuantidadeProduzida,
                op.IniciadaEm,
                op.ConcluidaEm,
                op.CreatedAt));
        });
    }
}