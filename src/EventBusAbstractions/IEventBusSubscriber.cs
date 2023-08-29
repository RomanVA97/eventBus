namespace RVA.EventBusAbstractions
{
    public interface IEventBusSubscriber<TMessage>
        where TMessage : class
    {
        /// <summary>
        /// Process queue message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Result of the operation</returns>
        public ValueTask<bool> Handle(TMessage message);
    }
}
