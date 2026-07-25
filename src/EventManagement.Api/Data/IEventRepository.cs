 using EventManagement.Api.Models;

namespace EventManagement.Api.Data;

public interface IEventRepository
{
    Task<EventEntity?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<EventEntity>> GetAllAsync();
    Task CreateAsync(EventEntity entity);
    Task UpdateAsync(EventEntity entity);
    Task DeleteAsync(Guid id);
}
