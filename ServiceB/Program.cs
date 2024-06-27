using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Setting role name and role instance
// Create a dictionary of resource attributes.
var resourceAttributes = new Dictionary<string, object> {
    { "service.name", "ServiceB" }};

// Configure the OpenTelemetry tracer provider to add the resource attributes to all traces.
builder.Services.ConfigureOpenTelemetryTracerProvider((sp, builder) => 
    builder.ConfigureResource(resourceBuilder => 
        resourceBuilder.AddAttributes(resourceAttributes)));

var connectionString = "<Application Insight Connection String>";

// Add the OpenTelemetry telemetry service to the application.
// This service will collect and send telemetry data to Azure Monitor.
builder.Services.AddOpenTelemetry().UseAzureMonitor(options => {
    options.ConnectionString = connectionString;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/weatherforecast/fromservicea", async () =>
{
    HttpClient httpClient = new();
    var response = await httpClient.GetAsync("http://localhost:5183/weatherforecast");
    var content = await response.Content.ReadAsStringAsync();
    return content;
})
.WithName("GetWeatherForecastFromServiceB")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
