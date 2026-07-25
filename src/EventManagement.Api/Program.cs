using EventManagement.Api.Data;
using EventManagement.Api.Services;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var redisHost = builder.Configuration["Redis:Host"];
var redisPort = builder.Configuration.GetValue<int?>("Redis:Port");
var redisUser = builder.Configuration["Redis:User"];
var redisPassword = builder.Configuration["Redis:Password"];

if (string.IsNullOrWhiteSpace(redisHost) || redisPort is null or <= 0)
{
    throw new InvalidOperationException("Redis Host/Port configuration is missing or invalid.");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        new ConfigurationOptions
        {
            EndPoints = { { redisHost, redisPort.Value } },
            User = redisUser,
            Password = redisPassword
        }));

builder.Services.AddScoped<IEventRepository, RedisEventRepository>();
builder.Services.AddScoped<IRegistrationRepository, RedisRegistrationRepository>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("frontend");
app.MapControllers();

app.Run();
