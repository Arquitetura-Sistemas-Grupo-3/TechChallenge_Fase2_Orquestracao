namespace Contracts.Events;

public record UserCreatedEvent(
    Guid UserId,
    string Name,
    string Email,
    DateTime CreatedAt);
