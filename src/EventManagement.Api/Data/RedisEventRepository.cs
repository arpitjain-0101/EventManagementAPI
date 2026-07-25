 using EventManagement.Api.Models;
 using StackExchange.Redis;

namespace EventManagement.Api.Data;

public class RedisEventRepository : IEventRepository
{
    private readonly IDatabase _db;
    private const string AllEventsKey = "events:all";

    public RedisEventRepository(IConnectionMultiplexer mux) => _db = mux.GetDatabase();

    public async Task CreateAsync(EventEntity entity)
    {
        await _db.HashSetAsync(EventKey(entity.Id), ToHashEntries(entity));
        await _db.SetAddAsync(AllEventsKey, entity.Id.ToString());
    }

    public async Task DeleteAsync(Guid id)
    {
        await _db.KeyDeleteAsync(EventKey(id));
        await _db.SetRemoveAsync(AllEventsKey, id.ToString());
    }

    public async Task<IReadOnlyList<EventEntity>> GetAllAsync()
    {
        var ids = await _db.SetMembersAsync(AllEventsKey);
        var events = new List<EventEntity>();

        foreach (var redisVal in ids)
        {
            if (Guid.TryParse(redisVal.ToString(), out var id))
            {
                var evt = await GetByIdAsync(id);
                if (evt is not null) events.Add(evt);
            }
        }

        return events.OrderBy(x => x.Date).ToList();
    }

    public async Task<EventEntity?> GetByIdAsync(Guid id)
    {
        var entries = await _db.HashGetAllAsync(EventKey(id));
        if (entries.Length == 0) return null;

        var map = entries.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
        return new EventEntity
        {
            Id = Guid.Parse(map["id"]),
            Title = map["title"],
            Description = map["description"],
            Date = DateTimeOffset.Parse(map["date"]),
            MaxCapacity = int.Parse(map["maxCapacity"])
        };
    }

    public Task UpdateAsync(EventEntity entity) =>
    _db.HashSetAsync(EventKey(entity.Id), ToHashEntries(entity));

    private static string EventKey(Guid id) => $"event:{id}";

    private static HashEntry[] ToHashEntries(EventEntity e) =>
    [
    new HashEntry("id", e.Id.ToString()),
 new HashEntry("title", e.Title),
 new HashEntry("description", e.Description),
 new HashEntry("date", e.Date.ToString("O")),
 new HashEntry("maxCapacity", e.MaxCapacity)
    ];
}
