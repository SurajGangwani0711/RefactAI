using Microsoft.OpenApi.Models;
using RefactAI.Common.Runner;
using RefactAI.Common.AI;
using RefactAI.Refactor;
using RefactAI.Orleans.Grains;
using RefactAI.Orleans.Contracts;
using Orleans.Configuration;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------
// 1. Add Services
// ----------------------------------------------------------
builder.Services.AddControllers();


// Orleans Client
builder.Host.UseOrleansClient(client =>
{
    client.UseStaticClustering(options =>
    {
        options.Gateways.Add(
            new Uri("gwy.tcp://127.0.0.1:30000")
        );
    });

    client.Configure<ClusterOptions>(opts =>
    {
        opts.ClusterId = "default";
        opts.ServiceId = "default";
    });
});


// Dependency Injection
builder.Services.AddSingleton<IDotnetRunner, DotnetRunner>();
builder.Services.AddSingleton<IRefactorService, RefactorService>();
builder.Services.AddHttpClient<IGitHubService, GitHubService>();
builder.Services.AddSingleton<IGitHubService, GitHubService>(); // or via AddHttpClient as above
builder.Services.AddSingleton<IOllamaService, OllamaService>();


// CORS (optional)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RefactAI API",
        Version = "v1",
        Description = "API for running repository refactoring tasks"
    });
});

// ----------------------------------------------------------
// 2. Build App
// ----------------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------------
// 3. Middleware
// ----------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RefactAI API v1");
        options.RoutePrefix = string.Empty;  // Swagger at root
    });
}

app.UseCors("AllowAll");
app.UseRouting();
app.MapControllers();

app.Run();
