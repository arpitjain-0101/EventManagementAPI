using EventManagement.Api.Contracts;
using EventManagement.Api.Data;
using EventManagement.Api.Models;

namespace EventManagement.Api.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _events;
    private readonly IRegistrationRepository _regs;

    public EventService(IEventRepository events, IRegistrationRepository regs)
    {
        _events = events;
        _regs = regs;
    }

    public async Task<EventResponse> CreateAsync(CreateEventRequest request)
    {
        var entity = new EventEntity
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Date = request.Date,
            MaxCapacity = request.MaxCapacity
        };
        await _events.CreateAsync(entity);
        return await MapAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _events.GetByIdAsync(id);
        if (existing is null) return false;

        await _events.DeleteAsync(id);
        await _regs.ClearAsync(id);
        return true;
    }

    public async Task<IReadOnlyList<EventResponse>> GetAllAsync()
    {
        var items = await _events.GetAllAsync();
        var list = new List<EventResponse>();
        foreach (var item in items) list.Add(await MapAsync(item));
        return list;
    }

    public async Task<EventResponse?> GetByIdAsync(Guid id)
    {
        var item = await _events.GetByIdAsync(id);
        return item is null ? null : await MapAsync(item);
    }

    public async Task<EventResponse?> UpdateAsync(Guid id, UpdateEventRequest request)
    {
        var existing = await _events.GetByIdAsync(id);
        if (existing is null) return null;

        existing.Title = request.Title.Trim();
        existing.Description = request.Description.Trim();
        existing.Date = request.Date;
        existing.MaxCapacity = request.MaxCapacity;

        await _events.UpdateAsync(existing);
        return await MapAsync(existing);
    }

    private async Task<EventResponse> MapAsync(EventEntity e)
    {
        var count = await _regs.CountAsync(e.Id);
        return new EventResponse(e.Id, e.Title, e.Description, e.Date, e.MaxCapacity, count);
    }
}
