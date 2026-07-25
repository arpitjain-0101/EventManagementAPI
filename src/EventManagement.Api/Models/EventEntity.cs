namespace EventManagement.Api.Models;

public class EventEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public int MaxCapacity { get; set; }
}
