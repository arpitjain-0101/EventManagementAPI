using EventManagement.Api.Data;
using EventManagement.Api.Models;

namespace EventManagement.Api.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IEventRepository _events;
    private readonly IRegistrationRepository _regs;

    public RegistrationService(IEventRepository events, IRegistrationRepository regs)
    {
        _events = events;
        _regs = regs;
    }

    public async Task<IReadOnlyList<RegistrationUser>?> GetUsersAsync(Guid eventId)
    {
        var evt = await _events.GetByIdAsync(eventId);
        if (evt is null) return null;
        return await _regs.GetUsersAsync(eventId);
    }

    public async Task<RegistrationResult> RegisterAsync(Guid eventId, string userId, string name, string email, DateTimeOffset nowUtc)
    {
        var evt = await _events.GetByIdAsync(eventId);
        if (evt is null) return new RegistrationResult(false, "Event not found.");
        if (evt.Date <= nowUtc) return new RegistrationResult(false, "Cannot register for past events.");
        if (await _regs.ExistsAsync(eventId, userId)) return new RegistrationResult(false, "User already registered for this event.");
        if (await _regs.CountAsync(eventId) >= evt.MaxCapacity) return new RegistrationResult(false, "Event capacity exceeded.");

        await _regs.AddAsync(eventId, userId, name, email);
        return new RegistrationResult(true);
    }

    public async Task<RegistrationResult> UnregisterAsync(Guid eventId, string userId)
    {
        var evt = await _events.GetByIdAsync(eventId);
        if (evt is null) return new RegistrationResult(false, "Event not found.");

        var removed = await _regs.RemoveAsync(eventId, userId);
        return removed ? new RegistrationResult(true) : new RegistrationResult(false, "User was not registered.");
    }
}
