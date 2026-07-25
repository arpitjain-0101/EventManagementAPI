namespace EventManagement.Api.Services;

using EventManagement.Api.Models;

public interface IRegistrationService
{
    Task<RegistrationResult> RegisterAsync(Guid eventId, string userId, string name, string email, DateTimeOffset nowUtc);
    Task<RegistrationResult> UnregisterAsync(Guid eventId, string userId);
    Task<IReadOnlyList<RegistrationUser>?> GetUsersAsync(Guid eventId);
}
