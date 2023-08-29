using RVA.EventBusAbstractions;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVA.RabbitMQ.Extensions;

namespace RVA.RabbitMQ
{
    public class RabbitMQPublisher<TQueueSettings> : IEventBusPublisher<TQueueSettings>
        where TQueueSettings : QueueSettings
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQPublisher<TQueueSettings>> _logger;
        private readonly TQueueSettings _queueSettings;

        public RabbitMQPublisher(
            ConnectionFactory connectionFactory,
            ILogger<RabbitMQPublisher<TQueueSettings>> logger,
            IOptions<TQueueSettings> options)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            _queueSettings = options.Value;
        }

        public void Publish<T>(T message) where T : class
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                channel.QueueDeclare(_queueSettings);

                _logger.LogDebug($"Serializing {message.GetType()}");

                string jsonValue = JsonSerializer.Serialize(message);

                _logger.LogDebug($"Serialized value: {jsonValue}");

                var body = Encoding.UTF8.GetBytes(jsonValue);

                channel.BasicPublish(exchange: _queueSettings.Exchange,
                                     routingKey: _queueSettings.Name,
                                     basicProperties: null,
                                     body: body);

                _logger.LogDebug($"The message successfuly sent to {_queueSettings.Name}");
            }
        }
    }
}
