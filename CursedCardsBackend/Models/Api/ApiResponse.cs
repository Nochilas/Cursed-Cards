namespace CursedCardsBackend.Models;

public record class ApiResponse<TResponse>(
    TResponse? Response = null,
    bool HasError = false,
    string? ErrorMessage = null)
    where TResponse : class;