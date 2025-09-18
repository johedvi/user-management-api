using UserManagementApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServices();

var app = builder.Build();

app.UseMiddlewares();

// Register API endpoints
app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapUserEndpoints();


app.Run();

public partial class Program { }
