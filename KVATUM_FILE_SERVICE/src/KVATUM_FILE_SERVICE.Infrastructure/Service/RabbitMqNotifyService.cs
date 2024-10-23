using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using KVATUM_FILE_SERVICE.Core.Enums;
using KVATUM_FILE_SERVICE.Core.IService;

namespace KVATUM_FILE_SERVICE.Infrastructure.Service
{
    public class RabbitMqNotifyService : INotifyService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        private readonly string _profileImageQueue;
        private readonly string _hubIconQueue;

        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password,
            string profileImageQueue,
            string hubIconQueue)
        {
            _hostname = hostname;
            _username = username;
            _password = password;

            _profileImageQueue = profileImageQueue;
            _hubIconQueue = hubIconQueue;
        }

        public void Publish<T>(T message, ContentUploaded content)
        {
            string queueName = GetQueueName(content);

            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
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

        private string GetQueueName(ContentUploaded content)
        {
            return content switch
            {
                ContentUploaded.ProfileImage => _profileImageQueue,
                ContentUploaded.HubIcon => _hubIconQueue,
            };
        }
    }
}