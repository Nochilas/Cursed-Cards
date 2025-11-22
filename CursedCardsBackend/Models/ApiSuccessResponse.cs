namespace CursedCardsBackend.Models;

public record class ApiSuccessResponse<TData>(TData? Response);