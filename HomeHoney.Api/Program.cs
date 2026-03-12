using HomeHoney.Api.Endpoints;
using HomeHoney.Api.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();
builder.Services.AddBackendServices(builder.Configuration);

var app = builder.Build();

app.Services.ValidateBackendStorageConfiguration();
await app.Services.EnsureMongoIndexesAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthEndpoints();
app.MapAdminEndpoints();
app.MapDocumentsEndpoints();
app.MapCollaborationEndpoints();
app.MapReminderEndpoints();
app.MapSearchEndpoints();
app.MapPreferenceEndpoints();

app.Run();

public partial class Program;
