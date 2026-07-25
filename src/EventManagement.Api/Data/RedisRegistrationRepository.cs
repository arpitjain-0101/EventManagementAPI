using System.Text.Json;
using EventManagement.Api.Models;
using StackExchange.Redis;

namespace EventManagement.Api.Data;

public class RedisRegistrationRepository : IRegistrationRepository
{
    private readonly IDatabase _db;
    public RedisRegistrationRepository(IConnectionMultiplexer mux) => _db = mux.GetDatabase();

    public async Task<bool> AddAsync(Guid eventId, string userId, string name, string email)
    {
        var added = await _db.SetAddAsync(UsersKey(eventId), userId);
        if (!added) return false;

        var payload = JsonSerializer.Serialize(new RegistrationUser(userId, name, email));
        await _db.HashSetAsync(ProfilesKey(eventId), userId, payload);
        return true;
    }

    public async Task<bool> RemoveAsync(Guid eventId, string userId)
    {
        var removed = await _db.SetRemoveAsync(UsersKey(eventId), userId);
        await _db.HashDeleteAsync(ProfilesKey(eventId), userId);
        return removed;
    }

    public Task<bool> ExistsAsync(Guid eventId, string userId) =>
    _db.SetContainsAsync(UsersKey(eventId), userId);

    public async Task<int> CountAsync(Guid eventId) =>
    (int)await _db.SetLengthAsync(UsersKey(eventId));

    public async Task<IReadOnlyList<RegistrationUser>> GetUsersAsync(Guid eventId)
    {
        var profiles = await _db.HashGetAllAsync(ProfilesKey(eventId));
        var byUserId = new Dictionary<string, RegistrationUser>(StringComparer.Ordinal);

        foreach (var entry in profiles)
        {
            var userId = entry.Name.ToString();
            if (string.IsNullOrWhiteSpace(userId)) continue;

            var raw = entry.Value.ToString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<RegistrationUser>(raw);
                    if (parsed is not null)
                    {
                        byUserId[userId] = parsed with { UserId = userId };
                        continue;
                    }
                }
                catch
                {
                }
            }

            byUserId[userId] = new RegistrationUser(userId, string.Empty, string.Empty);
        }

        var ids = await _db.SetMembersAsync(UsersKey(eventId));
        foreach (var id in ids.Select(v => v.ToString()).Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            if (!byUserId.ContainsKey(id!))
            {
                byUserId[id!] = new RegistrationUser(id!, string.Empty, string.Empty);
            }
        }

        return byUserId.Values.OrderBy(u => u.UserId, StringComparer.Ordinal).ToList();
    }

    public Task ClearAsync(Guid eventId) => _db.KeyDeleteAsync(new RedisKey[] { UsersKey(eventId), ProfilesKey(eventId) });

    private static string UsersKey(Guid eventId) => $"event:{eventId}:registrations";
    private static string ProfilesKey(Guid eventId) => $"event:{eventId}:registrations:profiles";
}
