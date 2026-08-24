var builder = DistributedApplication.CreateBuilder(args);

// postgres stays in docker-compose, so the apphost only orchestrates the api
builder.AddProject<Projects.Library_Api>("library-api");

builder.Build().Run();