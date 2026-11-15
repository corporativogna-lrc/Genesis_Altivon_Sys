using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.BackEnd.OperacaoPDV.VendaBalcao;

public static class PdvEndpoints
{
    public static void MapearEndpointsPdv(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/pdv/vendas").RequireAuthorization(PoliticasAutorizacao.OperacaoComercial);

        // 1. Iniciar nova venda no balcão
        grupo.MapPost("/", async (RequisicaoIniciarVenda req, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var novaVenda = new PdvVendaRegistro
            {
                ClienteId = req.ClienteId,
                Status = "ABERTA",
                ValorTotal = 0
            };

            db.Set<PdvVendaRegistro>().Add(novaVenda);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/pdv/vendas/{novaVenda.Id}", 
                new RespostaVendaPdv(novaVenda.Id, novaVenda.TenantId, novaVenda.Status, novaVenda.ValorTotal, novaVenda.CreatedAt));
        });

        // 2. Adicionar item e atualizar valor total
        grupo.MapPost("/{vendaId:guid}/itens", async (Guid vendaId, RequisicaoAdicionarItem req, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            // Trava Pessimista (FOR UPDATE) para evitar concorrência no fechamento da venda
            var venda = await db.ObterComBloqueioExclusivoAsync<PdvVendaRegistro>(vendaId, ct);
            
            if (venda is null || venda.Status != "ABERTA")
                return Results.BadRequest(new { erro = "Venda nao encontrada ou ja finalizada/cancelada." });

            if (req.Quantidade <= 0)
                return Results.BadRequest(new { erro = "A quantidade deve ser maior que zero." });

            var produto = await db.Set<GenesisAltivonSys.BackEnd.CadastrosCorporativos.ProdutosServicos.ProdutoRegistro>()
                .FirstOrDefaultAsync(p => p.Id == req.ProdutoId, ct);

            if (produto is null || !produto.Ativo)
                return Results.BadRequest(new { erro = "Produto nao encontrado ou inativo." });

            var subtotal = produto.PrecoVenda * req.Quantidade;

            var novoItem = new PdvItemRegistro
            {
                VendaId = venda.Id,
                ProdutoId = produto.Id,
                Quantidade = req.Quantidade,
                PrecoUnitario = produto.PrecoVenda,
                Subtotal = subtotal
            };

            db.Set<PdvItemRegistro>().Add(novoItem);
            
            venda.ValorTotal += subtotal;
            
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { item = novoItem.Id, subtotalAdicionado = subtotal, novoTotalVenda = venda.ValorTotal });
        });

        // 3. Finalizar Venda e baixar estoque
        grupo.MapPost("/{vendaId:guid}/finalizar", async (Guid vendaId, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var venda = await db.ObterComBloqueioExclusivoAsync<PdvVendaRegistro>(vendaId, ct);
            if (venda is null || venda.Status != "ABERTA")
                return Results.BadRequest(new { erro = "Venda nao encontrada ou nao esta aberta." });

            var itens = await db.Set<PdvItemRegistro>().Where(i => i.VendaId == vendaId).ToListAsync(ct);
            if (itens.Count == 0)
                return Results.BadRequest(new { erro = "Nao e possivel finalizar uma venda sem itens." });

            // Baixa de estoque
            foreach (var item in itens)
            {
                var produto = await db.ObterComBloqueioExclusivoAsync<GenesisAltivonSys.BackEnd.CadastrosCorporativos.ProdutosServicos.ProdutoRegistro>(item.ProdutoId, ct);
                if (produto != null)
                {
                    produto.EstoqueAtual -= item.Quantidade;
                }
            }

            venda.Status = "FINALIZADA";
            await db.SaveChangesAsync(ct);

            return Results.Ok(new RespostaVendaPdv(venda.Id, venda.TenantId, venda.Status, venda.ValorTotal, venda.CreatedAt));
        });
    }
}