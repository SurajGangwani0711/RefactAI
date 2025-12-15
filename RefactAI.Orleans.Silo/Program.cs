using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using RefactAI.Orleans.Grains;
using RefactAI.Common.Runner;
using RefactAI.Common.AI;
using RefactAI.Refactor;
using RefactAI.Common.AI;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // HttpClient MUST be registered for GitHubService to work
        services.AddHttpClient();

        // GitHub service (needs HttpClient)
        services.AddHttpClient<IGitHubService, GitHubService>();
        services.AddSingleton<IGitHubService, GitHubService>();
        services.AddSingleton<IOllamaService, OllamaService>();


        // Other services used by grains
        services.AddSingleton<IDotnetRunner, DotnetRunner>();
        services.AddSingleton<IRefactorService, RefactorService>();
    })
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering(
            siloPort: 11111,
            gatewayPort: 30000);

        silo.Configure<ClusterOptions>(opts =>
        {
            opts.ClusterId = "default";
            opts.ServiceId = "default";
        });
    });

await builder.Build().RunAsync();
