using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.BackEnd.EstoqueLogistica.Transporte;

public static class TransporteEndpoints
{
    private static readonly HashSet<string> StatusPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "AGUARDANDO_COLETA",
        "EM_TRANSITO",
        "ENTREGUE",
        "OCORRENCIA"
    };

    public static void MapearEndpointsTransporte(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/logistica/entregas")
            .RequireAuthorization(PoliticasAutorizacao.OperacaoComercial);

        grupo.MapGet("/", async (ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var entregas = await db.Set<EntregaTransporteRegistro>()
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new RespostaEntrega(
                    e.Id,
                    e.TenantId,
                    e.PedidoVendaId,
                    e.Transportadora,
                    e.StatusEntrega,
                    e.CodigoRastreio,
                    e.ValorFrete,
                    e.CreatedAt))
                .ToListAsync(ct);

            return Results.Ok(entregas);
        });

        grupo.MapPost("/", async (RequisicaoCriarEntrega req, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            if (req.PedidoVendaId == Guid.Empty || string.IsNullOrWhiteSpace(req.Transportadora))
            {
                return Results.BadRequest(new { erro = "PedidoVendaId e Transportadora sao obrigatorios." });
            }

            if (req.ValorFrete < 0)
            {
                return Results.BadRequest(new { erro = "ValorFrete nao pode ser negativo." });
            }

            var sufixoRastreio = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var novaEntrega = new EntregaTransporteRegistro
            {
                PedidoVendaId = req.PedidoVendaId,
                Transportadora = req.Transportadora.Trim(),
                StatusEntrega = "AGUARDANDO_COLETA",
                CodigoRastreio = $"ALT-{DateTime.UtcNow:yyyyMMdd}-{sufixoRastreio}",
                ValorFrete = req.ValorFrete
            };

            db.Set<EntregaTransporteRegistro>().Add(novaEntrega);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/logistica/entregas/{novaEntrega.Id}", new RespostaEntrega(
                novaEntrega.Id,
                novaEntrega.TenantId,
                novaEntrega.PedidoVendaId,
                novaEntrega.Transportadora,
                novaEntrega.StatusEntrega,
                novaEntrega.CodigoRastreio,
                novaEntrega.ValorFrete,
                novaEntrega.CreatedAt));
        });

        grupo.MapPost("/{id:guid}/status", async (
            Guid id,
            RequisicaoAtualizarStatusEntrega req,
            ContextoBancoRelacional db,
            CancellationToken ct) =>
        {
            var novoStatus = req.StatusEntrega?.Trim().ToUpperInvariant() ?? string.Empty;
            if (!StatusPermitidos.Contains(novoStatus))
            {
                return Results.BadRequest(new
                {
                    erro = "StatusEntrega invalido. Utilize AGUARDANDO_COLETA, EM_TRANSITO, ENTREGUE ou OCORRENCIA."
                });
            }

            var entrega = await db.ObterComBloqueioExclusivoAsync<EntregaTransporteRegistro>(id, ct);
            if (entrega is null)
            {
                return Results.BadRequest(new { erro = "Ordem de entrega nao encontrada para o tenant atual." });
            }

            entrega.StatusEntrega = novoStatus;
            if (!string.IsNullOrWhiteSpace(req.CodigoRastreio))
            {
                entrega.CodigoRastreio = req.CodigoRastreio.Trim();
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new RespostaEntrega(
                entrega.Id,
                entrega.TenantId,
                entrega.PedidoVendaId,
                entrega.Transportadora,
                entrega.StatusEntrega,
                entrega.CodigoRastreio,
                entrega.ValorFrete,
                entrega.CreatedAt));
        });
    }
}