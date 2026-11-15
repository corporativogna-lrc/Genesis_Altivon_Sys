using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace GenesisAltivonSys.BackEnd.FiscalTributario.EmissaoDocumentos;

public static class NfeNfceEndpoints
{
    public static void MapearEndpointsFiscal(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/v1/fiscal/documentos").RequireAuthorization(PoliticasAutorizacao.OperacaoComercial);

        grupo.MapGet("/", async (ContextoBancoRelacional db, CancellationToken ct) =>
        {
            var notas = await db.Set<DocumentoFiscalRegistro>()
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new RespostaDocumentoFiscal(
                    n.Id,
                    n.TenantId,
                    n.Modelo,
                    n.StatusSefaz,
                    n.Numero,
                    n.Serie,
                    n.ValorTotalNota,
                    n.ChaveAcesso,
                    n.CreatedAt))
                .ToListAsync(ct);

            return Results.Ok(notas);
        });

        grupo.MapPost("/", async (RequisicaoEmitirNota req, ContextoBancoRelacional db, CancellationToken ct) =>
        {
            if (req.ValorTotalNota <= 0 || req.Numero <= 0)
            {
                return Results.BadRequest(new { erro = "Numero e ValorTotalNota devem ser maiores que zero." });
            }

            var novaNota = new DocumentoFiscalRegistro
            {
                Modelo = req.Modelo.Trim() == "55" ? "55" : "65",
                Numero = req.Numero,
                Serie = req.Serie,
                ValorTotalNota = req.ValorTotalNota,
                StatusSefaz = "AUTORIZADA", // Simulação de autorização síncrona padrão
                ChaveAcesso = $"352611000000000000{req.Modelo}{req.Serie:D3}{req.Numero:D9}1",
                ProtocoloAutorizacao = "135260000000001"
            };

            db.Set<DocumentoFiscalRegistro>().Add(novaNota);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/fiscal/documentos/{novaNota.Id}", new RespostaDocumentoFiscal(
                novaNota.Id,
                novaNota.TenantId,
                novaNota.Modelo,
                novaNota.StatusSefaz,
                novaNota.Numero,
                novaNota.Serie,
                novaNota.ValorTotalNota,
                novaNota.ChaveAcesso,
                novaNota.CreatedAt));
        });
    }
}