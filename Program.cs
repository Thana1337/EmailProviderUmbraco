using Azure.Communication.Email;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton(s =>
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureCommunicationServiceConnectionString");
            return new EmailClient(connectionString);
        });

    })
    .Build();

host.Run();
