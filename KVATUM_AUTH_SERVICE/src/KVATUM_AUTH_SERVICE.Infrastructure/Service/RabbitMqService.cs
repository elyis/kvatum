using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KVATUM_AUTH_SERVICE.Core.Entities.Request;
using KVATUM_AUTH_SERVICE.Core.IRepository;
using KVATUM_AUTH_SERVICE.Core.IService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KVATUM_AUTH_SERVICE.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly string _hostname;
        private readonly string _updateProfileQueue;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            string hostname,
            string userName,
            string password,
            string updateProfileQueue)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;
            _serviceFactory = serviceFactory;

            _updateProfileQueue = updateProfileQueue;

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

            DeclareQueue(_updateProfileQueue);
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

            ConsumeQueue(_updateProfileQueue, HandleUpdateProfileMessageAsync);

            await Task.CompletedTask;
        }

        private async Task HandleUpdateProfileMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var updateProfileBody = JsonSerializer.Deserialize<UpdateProfileBody>(message);
            if (updateProfileBody == null)
                return;

            var account = await accountRepository.UpdateProfileIconAsync(updateProfileBody.AccountId, updateProfileBody.FileName);
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}