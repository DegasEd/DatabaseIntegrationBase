using System.Data.SqlTypes;

var builder = DistributedApplication.CreateBuilder(args);

var mysql = builder.AddMySql("mysql")
                   .WithPhpMyAdmin();
                   

var mysqldb = mysql.AddDatabase("moviedb");
               
            

var apiService = builder.AddProject<Projects.DatabaseIntegrationBase_ApiService>("apiservice")
                        .WithReference(mysqldb)
                        .WaitFor(mysqldb);

builder.AddProject<Projects.DatabaseIntegrationBase_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
