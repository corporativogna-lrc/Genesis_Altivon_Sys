using System.Text;
using GenesisAltivonSys.BackEnd.GovernancaRiscosConformidade.IdentidadeAcessos;
using GenesisAltivonSys.BackEnd.CadastrosCorporativos.Clientes;
using GenesisAltivonSys.BackEnd.CadastrosCorporativos.ProdutosServicos;
using GenesisAltivonSys.BackEnd.OperacaoPDV.VendaBalcao;
using GenesisAltivonSys.BackEnd.FinanceiroContabil.ContasPagarReceber;
using GenesisAltivonSys.BackEnd.EstoqueLogistica.ControleSaldosWMS;
using GenesisAltivonSys.BackEnd.EstoqueLogistica.Transporte;
using GenesisAltivonSys.BackEnd.GestaoCompras.PedidosRecebimentos;
using GenesisAltivonSys.BackEnd.FiscalTributario.EmissaoDocumentos;
using GenesisAltivonSys.BackEnd.ManufaturaOperacoes.OrdensProducao;
using GenesisAltivonSys.Infraestrutura.BancoDeDados;
using GenesisAltivonSys.Infraestrutura.Seguranca;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("configuracoes.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration["BancoDeDados:ConnectionString"];
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falha critica de inicializacao: 'BancoDeDados:ConnectionString' ausente em configuracoes.json.");
}

var emissorJwt = builder.Configuration["SegurancaJwt:Emissor"]
    ?? throw new InvalidOperationException("SegurancaJwt:Emissor ausente em configuracoes.json.");
var audienciaJwt = builder.Configuration["SegurancaJwt:Audiencia"]
    ?? throw new InvalidOperationException("SegurancaJwt:Audiencia ausente em configuracoes.json.");
var chaveAssinaturaJwt = builder.Configuration["SegurancaJwt:ChaveAssinatura"]
    ?? throw new InvalidOperationException("SegurancaJwt:ChaveAssinatura ausente em configuracoes.json.");
var dirChavesDpapi = builder.Configuration["SegurancaJwt:DiretorioChavesDataProtection"]
    ?? @"D:\Genesis_Altivon_Sys\StorageDados\chaves-dpapi";

Directory.CreateDirectory(dirChavesDpapi);

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dirChavesDpapi))
    .SetApplicationName("GenesisAltivonSys");

var versaoMariaDb = new MariaDbServerVersion(new Version(10, 4, 32));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IProvedorContextoTenant, ProvedorContextoTenantHttp>();

builder.Services.AddDbContext<ContextoBancoRelacional>(options =>
{
    options.UseMySql(connectionString, versaoMariaDb, mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
        mySqlOptions.CommandTimeout(30);
    });
});

builder.Services.AddScoped<GerenciadorEsquemaUnificado>();
builder.Services.AddSingleton<GeradorTokenJwtMfa>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = emissorJwt,
            ValidateAudience = true,
            ValidAudience = audienciaJwt,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveAssinaturaJwt)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization(PoliticasAutorizacao.ConfigurarPoliticas);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var gerenciadorEsquema = scope.ServiceProvider.GetRequiredService<GerenciadorEsquemaUnificado>();
    await gerenciadorEsquema.ValidarIntegridadeEsquemaAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new {
    sistema = "Genesis_Altivon_Sys",
    versao = "1.1.5",
    status = "ONLINE",
    documentacao = "Utilize /api/v1/sistema/saude para verificar a integridade."
}));

app.MapGet("/api/v1/sistema/saude", async (
    GerenciadorEsquemaUnificado gerenciadorEsquema,
    CancellationToken cancellationToken) =>
{
    var diagnostico = await gerenciadorEsquema.ValidarIntegridadeEsquemaAsync(cancellationToken);
    return Results.Ok(new
    {
        sistema = "Genesis_Altivon_Sys",
        versao = "1.1.5",
        status = "OPERACIONAL",
        bancoDeDados = diagnostico
    });
});

app.MapearEndpointsUsuarioRbac();
app.MapearEndpointsClientes();
app.MapearEndpointsProdutos();
app.MapearEndpointsPdv();
app.MapearEndpointsFinanceiro();
app.MapearEndpointsEstoqueWms();
app.MapearEndpointsCompras();
app.MapearEndpointsFiscal();
app.MapearEndpointsTransporte();
app.MapearEndpointsManufaturaMes();

app.Run();