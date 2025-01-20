var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSql integration
var postgres = builder.AddPostgres("pg")
               .WithPgAdmin()
               .WithBindMount("./Service.API/Seed", "/docker-entrypoint-initdb.d")
               .AddDatabase("postgres");

var apiService = builder.AddProject<Projects.DatabaseIntegrationBase_ApiService>("apiservice")
                        .WithReference(postgres)
                        .WaitFor(postgres);

builder.AddProject<Projects.DatabaseIntegrationBase_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
