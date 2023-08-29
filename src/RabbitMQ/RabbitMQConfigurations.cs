using RVA.EventBusAbstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RVA.RabbitMQ.Configurations;
using RVA.RabbitMQ.Extensions;
using System.Text;
using System.Text.Json;

namespace RVA.RabbitMQ
{
    public static class RabbitMQConfigurations
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
        {
            services.AddSingleton(x =>
            {
                var settings = x.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                return new ConnectionFactory
                {
                    HostName = settings.HostName,
                    UserName = settings.UserName,
                    Password = settings.Password,
                    VirtualHost = settings.VirtualHost,
                    ContinuationTimeout = TimeSpan.FromSeconds(settings.ContinuationTimeout),
                    RequestedHeartbeat = TimeSpan.FromMilliseconds(settings.RequestedHeartbeat),
                    DispatchConsumersAsync = true,
                };
            });
            
            services.AddTransient(typeof(IEventBusPublisher<>), typeof(RabbitMQPublisher<>));

            return services;
        }

        public static IApplicationBuilder AddRabbitMQSubscriber<TSubscriber, TMessage, TQueueSettings>(
            this IApplicationBuilder applicationBuilder)
            where TSubscriber : IEventBusSubscriber<TMessage>
            where TMessage : class
            where TQueueSettings : QueueSettings
        {
            var queueSettings = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<TQueueSettings>>().Value;
            var factory = applicationBuilder.ApplicationServices.GetRequiredService<ConnectionFactory>();

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queueSettings);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, argumens) =>
            {
                var logger = applicationBuilder.ApplicationServices.GetRequiredService<ILogger<TSubscriber>>();

                try
                {
                    logger.LogDebug($"Received a message with delivery tag: {argumens.DeliveryTag}. Extracting data...");
                    var body = argumens.Body.ToArray();
                    var jsonValue = Encoding.UTF8.GetString(body);
                    logger.LogDebug($"Json serialized value: {jsonValue}. Deserializing to {typeof(TMessage)}");
                    var message = JsonSerializer.Deserialize<TMessage>(jsonValue);
                    logger.LogDebug($"Deserialization completed successfuly! Start processing...");

                    var handler = applicationBuilder.ApplicationServices.GetRequiredService<TSubscriber>();

                    if (await handler.Handle(message))
                    {
                        channel.BasicAck(argumens.DeliveryTag, false);
                    }
                    else
                    {
                        channel.BasicNack(argumens.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error of message processing. Message: {ex.Message}, Inner: {ex.InnerException?.Message}");

                    channel.BasicNack(argumens.DeliveryTag, false, true);
                }
            };

            channel.BasicConsume(queue: queueSettings.Name,
                                 autoAck: false,
                                 consumer: consumer);

            return applicationBuilder;
        }
    }
}
