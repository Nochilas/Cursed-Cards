using System.Text.Json;
using CursedCardsBackend.Managers;
using CursedCardsBackend.Services;
using CursedCardsBackend.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton(new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    })
    .AddSingleton<GameManager>()
    .AddSingleton<GameService>();

var app = builder.Build();

// Serve static files
app.UseDefaultFiles();
app.UseStaticFiles();
app.ConfigureEndpoints();

// Starts the app
app.Run();