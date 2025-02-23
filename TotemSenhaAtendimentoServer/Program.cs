using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TotemSenhaAtendimentoServer.Application.Senhas.Services;
using TotemSenhaAtendimentoServer.Domain.Bases;
using TotemSenhaAtendimentoServer.Domain.Senhas.Services;
using TotemSenhaAtendimentoServer.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Configuração de Controllers, Autenticação e Swagger
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS para permitir requisições de qualquer origem
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<RabbitMqService>();

builder.Services.AddScoped<ISenhaService, SenhaService>();


var baseServiceType = typeof(IServiceBase);

var assemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName!.StartsWith("TotemSenhaAtendimentoServer"))
    .ToList();

var implementationTypes = assemblies
    .SelectMany(a => a.GetTypes())
    .Where(t => !t.IsInterface && !t.IsAbstract && baseServiceType.IsAssignableFrom(t))
    .ToList();

foreach (var type in implementationTypes)
{
    var interfaces = type.GetInterfaces();
    foreach (var @interface in interfaces)
    {
        builder.Services.AddScoped(@interface, type);
        Console.WriteLine($"✅ Registrado: {@interface.Name} -> {type.Name}");
    }
}

// Se não encontrou nenhum serviço, avisar no console
if (implementationTypes.Count == 0)
{
    Console.WriteLine("❌ Nenhum serviço foi registrado automaticamente.");
}

// Criar a aplicação
var app = builder.Build();

app.UseCors("AllowAllOrigins");

// Ativar Swagger para documentação da API
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();
