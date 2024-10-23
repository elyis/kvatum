using System.Text;
using System.Text.Json;
using KVATUM_CHATFLOW_SERVICE.Core.Entities.Events;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KVATUM_CHATFLOW_SERVICE.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly string _hostname;
        private readonly string _updateHubIconQueue;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            string hostname,
            string userName,
            string password,
            string updateHubIconQueue)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;
            _serviceFactory = serviceFactory;

            _updateHubIconQueue = updateHubIconQueue;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _userName,
                Password = _password,
                DispatchConsumersAsync = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            DeclareQueue(_updateHubIconQueue);
        }

        private void DeclareQueue(string queueName)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        private void ConsumeQueue(string queueName, Func<string, Task> handler)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await handler(message);
            };
            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            ConsumeQueue(_updateHubIconQueue, HandleUpdateHubIconMessageAsync);

            await Task.CompletedTask;
        }

        private async Task HandleUpdateHubIconMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RabbitMqService>>();
            logger.LogInformation($"Message is {message}");
            var hubRepository = scope.ServiceProvider.GetRequiredService<IHubRepository>();
            var updateHubIconBody = JsonSerializer.Deserialize<HubIconUploadEvent>(message);
            logger.LogInformation($"Serialize result: {updateHubIconBody}");
            if (updateHubIconBody == null)
                return;

            await hubRepository.UpdateHubIconAsync(updateHubIconBody.HubId, updateHubIconBody.FileName);
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}