using CursedCardsBackend.Constants;
using CursedCardsBackend.Endpoints;
using CursedCardsBackend.Managers;
using CursedCardsBackend.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton(new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    })
    .AddSingleton<GameManager>()
    .AddSingleton<GameService>()
    .AddSingleton<GameNotifier>()
    .AddSignalR();

var app = builder.Build();

// Serve static files
app.UseDefaultFiles();
app.UseStaticFiles();
await app.ConfigureEndpointsAsync();

// SignalR
app.MapHub<GameHub>(CursedCardsConstants.GAME_HUB_ENDPOINT);

// Starts the app
app.Run();