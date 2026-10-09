using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OdontoPrime.Application;
using OdontoPrime.Data;
using OdontoPrime.Data.Configurations;
using OdontoPrime.Infra.Interceptors;
using OdontoPrime.Infra.Jobs;
using OdontoPrime.Infra.Mensageria;
using OdontoPrime.Infra.Mensageria.Outbox;
using OdontoPrime.Mappings;
using OdontoPrime.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);
builder.Services.AddScoped<ConsultaService>();
builder.Services.AddScoped<IOutbox,OutboxEfCore>();    // "quem pedir IOutbox, recebe OutboxEfCore"
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<RelogioDaClinica>();
builder.Services.AddSingleton<PoliticaDeLembrete>();

builder.Services.AddDbContext<AppDbContext>(options =>
    
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).AddInterceptors(
        new AuditoriaInterceptor(),
        new SoftDeleteInterceptor()));

// background services
builder.Services.AddHostedService<LembreteScheduler>();
builder.Services.AddHostedService<RelayDaOutbox>();
builder.Services.AddHostedService<ManutencaoDaOutbox>();


// Add services to the container.
builder.Services.AddRazorPages();

// HttpClient usado pelas Razor Pages para consumir a própria API (mesmo processo).
// O BaseAddress é montado a partir da requisição atual, então funciona em qualquer porta/host/ambiente.
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("Api", (sp, client) =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext!;
    client.BaseAddress = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}");
});

builder.Services.AddScoped<IPacienteApiService, PacienteApiClient>();
builder.Services.AddScoped<IProfissionalApiService, ProfissionalApiClient>();
builder.Services.AddScoped<IConsultaApiService, ConsultaApiClient>();
builder.Services.AddScoped<IConvenioApiService, ConvenioApiClient>();
builder.Services.AddScoped<ITipoProfissionalApiService, TipoProfissionalApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();