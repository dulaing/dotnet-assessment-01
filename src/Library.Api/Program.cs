using Library.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// builder.Configuration is appsettings already loaded and parsed
builder.Services.AddInfrastructure(builder.Configuration);

var app =  builder.Build();

app.Run();