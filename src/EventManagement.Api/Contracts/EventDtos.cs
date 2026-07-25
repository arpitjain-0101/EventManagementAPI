namespace EventManagement.Api.Contracts;

public record CreateEventRequest(string Title, string Description, DateTimeOffset Date, int MaxCapacity);
public record UpdateEventRequest(string Title, string Description, DateTimeOffset Date, int MaxCapacity);
public record RegisterRequest(string UserId, string Name, string Email);

public record EventResponse(
     Guid Id,
     string Title,
     string Description,
     DateTimeOffset Date,
     int MaxCapacity,
     int CurrentRegistrations
 );
