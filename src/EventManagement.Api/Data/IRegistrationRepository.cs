namespace EventManagement.Api.Data;

using EventManagement.Api.Models;

public interface IRegistrationRepository
{
    Task<bool> AddAsync(Guid eventId, string userId, string name, string email);
    Task<bool> RemoveAsync(Guid eventId, string userId);
    Task<bool> ExistsAsync(Guid eventId, string userId);
    Task<int> CountAsync(Guid eventId);
    Task<IReadOnlyList<RegistrationUser>> GetUsersAsync(Guid eventId);
    Task ClearAsync(Guid eventId);
}
