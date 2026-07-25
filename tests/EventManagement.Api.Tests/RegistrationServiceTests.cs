using EventManagement.Api.Data;
using EventManagement.Api.Models;
using EventManagement.Api.Services;

namespace EventManagement.Api.Tests;

public class RegistrationServiceTests
{
    [Fact]
    public async Task Register_Fails_ForPastEvent()
    {
        var id = Guid.NewGuid();
        var service = new RegistrationService(
        new FakeEventRepo(new EventEntity { Id = id, Title = "T", Description = "D", Date = DateTimeOffset.UtcNow.AddDays(-1), MaxCapacity = 10 }),
        new FakeRegRepo());

        var result = await service.RegisterAsync(id, "user1", "User One", "user1@example.com", DateTimeOffset.UtcNow);

        Assert.False(result.Success);
        Assert.Equal("Cannot register for past events.", result.Error);
    }

    [Fact]
    public async Task Register_Fails_Duplicate()
    {
        var id = Guid.NewGuid();
        var reg = new FakeRegRepo();
        await reg.AddAsync(id, "user1", "User One", "user1@example.com");

        var service = new RegistrationService(
        new FakeEventRepo(new EventEntity { Id = id, Title = "T", Description = "D", Date = DateTimeOffset.UtcNow.AddDays(1), MaxCapacity = 10 }),
        reg);

        var result = await service.RegisterAsync(id, "user1", "User One", "user1@example.com", DateTimeOffset.UtcNow);

        Assert.False(result.Success);
        Assert.Equal("User already registered for this event.", result.Error);
    }

    [Fact]
    public async Task Register_Fails_Capacity()
    {
        var id = Guid.NewGuid();
        var reg = new FakeRegRepo();
        await reg.AddAsync(id, "user1", "User One", "user1@example.com");

        var service = new RegistrationService(
        new FakeEventRepo(new EventEntity { Id = id, Title = "T", Description = "D", Date = DateTimeOffset.UtcNow.AddDays(1), MaxCapacity = 1 }),
        reg);

        var result = await service.RegisterAsync(id, "user2", "User Two", "user2@example.com", DateTimeOffset.UtcNow);

        Assert.False(result.Success);
        Assert.Equal("Event capacity exceeded.", result.Error);
    }

    [Fact]
    public async Task Register_Succeeds_Valid()
    {
        var id = Guid.NewGuid();
        var reg = new FakeRegRepo();

        var service = new RegistrationService(
        new FakeEventRepo(new EventEntity { Id = id, Title = "T", Description = "D", Date = DateTimeOffset.UtcNow.AddDays(1), MaxCapacity = 2 }),
        reg);

        var result = await service.RegisterAsync(id, "user2", "User Two", "user2@example.com", DateTimeOffset.UtcNow);

        Assert.True(result.Success);
    }

    private class FakeEventRepo : IEventRepository
    {
        private readonly Dictionary<Guid, EventEntity> _events = new();
        public FakeEventRepo(EventEntity seed) => _events[seed.Id] = seed;
        public Task CreateAsync(EventEntity entity) { _events[entity.Id] = entity; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id) { _events.Remove(id); return Task.CompletedTask; }
        public Task<IReadOnlyList<EventEntity>> GetAllAsync() => Task.FromResult((IReadOnlyList<EventEntity>)_events.Values.ToList());
        public Task<EventEntity?> GetByIdAsync(Guid id) => Task.FromResult(_events.TryGetValue(id, out var e) ? e : null);
        public Task UpdateAsync(EventEntity entity) { _events[entity.Id] = entity; return Task.CompletedTask; }
    }

    private class FakeRegRepo : IRegistrationRepository
    {
        private readonly Dictionary<Guid, Dictionary<string, (string Name, string Email)>> _regs = new();

        public Task<bool> AddAsync(Guid eventId, string userId, string name, string email)
        {
            if (!_regs.ContainsKey(eventId)) _regs[eventId] = new Dictionary<string, (string Name, string Email)>();
            if (_regs[eventId].ContainsKey(userId)) return Task.FromResult(false);

            _regs[eventId][userId] = (name, email);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAsync(Guid eventId, string userId)
            => Task.FromResult(_regs.TryGetValue(eventId, out var users) && users.Remove(userId));

        public Task<bool> ExistsAsync(Guid eventId, string userId)
            => Task.FromResult(_regs.TryGetValue(eventId, out var users) && users.ContainsKey(userId));

        public Task<int> CountAsync(Guid eventId)
            => Task.FromResult(_regs.TryGetValue(eventId, out var users) ? users.Count : 0);

        public Task<IReadOnlyList<RegistrationUser>> GetUsersAsync(Guid eventId)
            => Task.FromResult((IReadOnlyList<RegistrationUser>)(_regs.TryGetValue(eventId, out var users)
                ? users.Select(kvp => new RegistrationUser(kvp.Key, kvp.Value.Name, kvp.Value.Email)).ToList()
                : new List<RegistrationUser>()));

        public Task ClearAsync(Guid eventId) { _regs.Remove(eventId); return Task.CompletedTask; }
    }
}
