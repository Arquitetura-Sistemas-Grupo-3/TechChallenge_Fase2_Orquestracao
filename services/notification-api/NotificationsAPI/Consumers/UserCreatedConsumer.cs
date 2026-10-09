using Contracts.Events;
using MassTransit;

namespace NotificationsAPI.Consumers;

public class UserCreatedConsumer(ILogger<UserCreatedConsumer> logger)
    : IConsumer<UserCreatedEvent>
{
    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "[EMAIL SIMULADO - Boas-vindas] Para: {Email} | Assunto: Bem-vindo(a) à FIAP Cloud Games, {Name}! | UserId: {UserId}",
            message.Email,
            message.Name,
            message.UserId);

        return Task.CompletedTask;
    }
}
