namespace Contracts.Events;

public record PaymentProcessedEvent(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    string GameName,
    string UserEmail,
    decimal Price,
    PaymentStatus Status,
    DateTime ProcessedAt);

public enum PaymentStatus
{
    Approved,
    Rejected
}
