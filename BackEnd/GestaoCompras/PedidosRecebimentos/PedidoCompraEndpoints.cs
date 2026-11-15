using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;
using GenesisAltivonSys.BackEnd.CadastrosCorporativos.ProdutosServicos;

namespace GenesisAltivonSys.BackEnd.GestaoCompras.PedidosRecebimentos;

public static class PedidoCompraEndpoints
{
    public static void MapearEndpointsCompras(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/scm/pedidos").RequireAuthorization(PoliticasAutorizacao.OperacaoComercial);

        // 1. Criar cabeçalho do pedido
        grupo.MapPost("/", async (RequisicaoCriarPedidoCompra req, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            if (req.FornecedorId == Guid.Empty)
                return Results.BadRequest(new { erro = "FornecedorId é obrigatório." });

            var novoPedido = new PedidoCompraRegistro
            {
                FornecedorId = req.FornecedorId,
                Status = "RASCUNHO",
                ValorTotal = 0
            };

            db.Set<PedidoCompraRegistro>().Add(novoPedido);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/scm/pedidos/{novoPedido.Id}", 
                new RespostaPedidoCompra(novoPedido.Id, novoPedido.TenantId, novoPedido.FornecedorId, novoPedido.Status, novoPedido.ValorTotal, novoPedido.CreatedAt));
        });

        // 2. Adicionar itens ao pedido
        grupo.MapPost("/{pedidoId:guid}/itens", async (Guid pedidoId, RequisicaoAdicionarItemPedido req, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var pedido = await db.ObterComBloqueioExclusivoAsync<PedidoCompraRegistro>(pedidoId, ct);
            if (pedido is null || pedido.Status != "RASCUNHO")
                return Results.BadRequest(new { erro = "Pedido nao encontrado ou nao esta mais em rascunho." });

            if (req.Quantidade <= 0 || req.PrecoUnitario <= 0)
                return Results.BadRequest(new { erro = "Quantidade e PrecoUnitario devem ser maiores que zero." });

            var produto = await db.Set<ProdutoRegistro>().FirstOrDefaultAsync(p => p.Id == req.ProdutoId, ct);
            if (produto is null)
                return Results.BadRequest(new { erro = "Produto nao encontrado." });

            var subtotal = req.Quantidade * req.PrecoUnitario;

            var novoItem = new PedidoCompraItemRegistro
            {
                PedidoId = pedido.Id,
                ProdutoId = produto.Id,
                Quantidade = req.Quantidade,
                PrecoUnitario = req.PrecoUnitario,
                Subtotal = subtotal
            };

            db.Set<PedidoCompraItemRegistro>().Add(novoItem);
            pedido.ValorTotal += subtotal;

            await db.SaveChangesAsync(ct);

            return Results.Ok(new { item = novoItem.Id, subtotalAdicionado = subtotal, novoTotalPedido = pedido.ValorTotal });
        });

        // 3. Aprovar Pedido
        grupo.MapPost("/{pedidoId:guid}/aprovar", async (Guid pedidoId, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var pedido = await db.ObterComBloqueioExclusivoAsync<PedidoCompraRegistro>(pedidoId, ct);
            if (pedido is null || pedido.Status != "RASCUNHO")
                return Results.BadRequest(new { erro = "Apenas pedidos em RASCUNHO podem ser aprovados." });

            var possuiItens = await db.Set<PedidoCompraItemRegistro>().AnyAsync(i => i.PedidoId == pedidoId, ct);
            if (!possuiItens)
                return Results.BadRequest(new { erro = "Nao e possivel aprovar um pedido vazio." });

            pedido.Status = "APROVADO";
            await db.SaveChangesAsync(ct);

            return Results.Ok(new RespostaPedidoCompra(pedido.Id, pedido.TenantId, pedido.FornecedorId, pedido.Status, pedido.ValorTotal, pedido.CreatedAt));
        });
    }
}