using CompaniesHouseResilience.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostingContext, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .ConfigureOptions(hostingContext.Configuration)
            .AddServices(hostingContext.Configuration)
            .AddHttpClients(hostingContext.Configuration);
    })
    .Build();

await host.RunAsync();
