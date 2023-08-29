using RVA.EventBusAbstractions;
using RabbitMQ.Client;

namespace RVA.RabbitMQ.Extensions
{
    internal static class IModelExtensions
    {
        internal static QueueDeclareOk QueueDeclare(this IModel model, QueueSettings queueSettings)
        {
            return model.QueueDeclare(queue: queueSettings.Name,
                                      durable: queueSettings.Durable,
                                      exclusive: queueSettings.Exclusive,
                                      autoDelete: queueSettings.AutoDelete,
                                      arguments: queueSettings.Arguments);
        }
    }
}
