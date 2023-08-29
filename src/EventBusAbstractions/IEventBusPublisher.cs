namespace RVA.EventBusAbstractions
{
    public interface IEventBusPublisher<TQueueSettings>
        where TQueueSettings : QueueSettings
    {
        void Publish<T>(T message)
            where T : class;
    }
}
