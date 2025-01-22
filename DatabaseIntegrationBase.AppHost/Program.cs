var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
                .WithLifetime(ContainerLifetime.Persistent);
                 
var sqldb = sql.AddDatabase("sqldb", "master");


var apiService = builder.AddProject<Projects.DatabaseIntegrationBase_ApiService>("apiservice")
                        .WithReference(sqldb)
                        .WaitFor(sqldb);

builder.AddProject<Projects.DatabaseIntegrationBase_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
