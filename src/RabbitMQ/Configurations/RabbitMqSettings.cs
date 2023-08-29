namespace RVA.RabbitMQ.Configurations
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string VirtualHost { get; set; }

        /// <summary>
        /// Value in seconds
        /// </summary>
        public int ContinuationTimeout { get; set; } = 5;

        /// <summary>
        /// Value in milliseconds
        /// </summary>
        public int RequestedHeartbeat { get; set; } = 60;
    }
}
