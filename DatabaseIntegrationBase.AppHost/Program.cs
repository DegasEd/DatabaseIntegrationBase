var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.DatabaseIntegrationBase_ApiService>("apiservice");

builder.AddProject<Projects.DatabaseIntegrationBase_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
