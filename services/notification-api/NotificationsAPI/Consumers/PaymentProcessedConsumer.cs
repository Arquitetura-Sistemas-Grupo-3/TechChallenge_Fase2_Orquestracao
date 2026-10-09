using Contracts.Events;
using MassTransit;

namespace NotificationsAPI.Consumers;

public class PaymentProcessedConsumer(ILogger<PaymentProcessedConsumer> logger)
    : IConsumer<PaymentProcessedEvent>
{
    public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var message = context.Message;

        if (message.Status != PaymentStatus.Approved)
        {
            logger.LogInformation(
                "Pagamento rejeitado para OrderId {OrderId} (UserId {UserId}) — nenhum e-mail enviado.",
                message.OrderId,
                message.UserId);
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "[EMAIL SIMULADO - Confirmação de Compra] Para: {Email} | Assunto: Compra confirmada — {GameName} | OrderId: {OrderId} | Valor: R$ {Price:F2}",
            message.UserEmail,
            message.GameName,
            message.OrderId,
            message.Price);

        return Task.CompletedTask;
    }
}
