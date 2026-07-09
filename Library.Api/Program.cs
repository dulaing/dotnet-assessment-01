using Library.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapBookEndpoints();
app.MapMemberEndpoints();
app.MapBorrowingEndpoints();

app.Run();
