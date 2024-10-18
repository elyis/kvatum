using System.Text;
using System.Text.Json;
using KVATUM_EMAIL_SERVICE.Core;
using KVATUM_EMAIL_SERVICE.Core.IService;
using RabbitMQ.Client;

namespace KVATUM_EMAIL_SERVICE.Infrastructure.Service
{
    public class RabbitMqNotifyService : INotifyService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;


        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password)
        {
            _hostname = hostname;
            _username = username;
            _password = password;
        }

        public void Publish<T>(T message, PublishEvent eventType)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var queueName = GetQueueName(eventType);
            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: properties,
                                 body: body);
        }

        private string GetQueueName(PublishEvent eventType)
        {
            return eventType switch
            {
                _ => throw new ArgumentException("Invalid event type")
            };
        }
    }
}