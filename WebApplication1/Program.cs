using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication1.Application;
using WebApplication1.Data;
using WebApplication1.Data.Configurations;
using WebApplication1.Infra.Interceptors;
using WebApplication1.Infra.Mensageria;
using WebApplication1.Mappings;
using WebApplication1.Services;

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

builder.Services.AddDbContext<AppDbContext>(options =>
    
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).AddInterceptors(
        new AuditoriaInterceptor(),
        new SoftDeleteInterceptor()));

builder.Services.AddSingleton<IPublicadorDeEventos>(sp =>
{
    var connectionString = builder.Configuration["RabbitMq:ConnectionString"]!;
    return PublicadorRabbitMq.CriarAsync(connectionString).GetAwaiter().GetResult();
});
    


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

builder.Services.AddScoped<IPacienteApiService, PacienteApiService>();
builder.Services.AddScoped<IProfissionalApiService, ProfissionalApiService>();
builder.Services.AddScoped<IConsultaApiService, ConsultaApiService>();
builder.Services.AddScoped<IConvenioApiService, ConvenioApiService>();
builder.Services.AddScoped<ITipoProfissionalApiService, TipoProfissionalApiService>();

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