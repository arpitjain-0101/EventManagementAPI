using EventManagement.Api.Contracts;

namespace EventManagement.Api.Services;

public interface IEventService
{
    Task<IReadOnlyList<EventResponse>> GetAllAsync();
    Task<EventResponse?> GetByIdAsync(Guid id);
    Task<EventResponse> CreateAsync(CreateEventRequest request);
    Task<EventResponse?> UpdateAsync(Guid id, UpdateEventRequest request);
    Task<bool> DeleteAsync(Guid id);
}
