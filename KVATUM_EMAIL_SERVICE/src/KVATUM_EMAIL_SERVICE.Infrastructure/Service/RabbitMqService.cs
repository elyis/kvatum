using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using KVATUM_EMAIL_SERVICE.Core.Entities;
using KVATUM_EMAIL_SERVICE.Core.IService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KVATUM_EMAIL_SERVICE.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly IEmailService _emailService;
        private readonly string _hostname;
        private readonly string _sendConfirmationCodeQueue;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            IEmailService emailService,
            string hostname,
            string userName,
            string password,
            string sendConfirmationCodeQueue)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;
            _serviceFactory = serviceFactory;
            _emailService = emailService;
            _sendConfirmationCodeQueue = sendConfirmationCodeQueue;

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

            DeclareQueue(_sendConfirmationCodeQueue);
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

            ConsumeQueue(_sendConfirmationCodeQueue, HandleSendConfirmationCodeAsync);

            await Task.CompletedTask;
        }

        private async Task HandleSendConfirmationCodeAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RabbitMqService>>();


            var sendConfirmationCodeBody = JsonSerializer.Deserialize<SendConfirmationCodeBody>(message);
            if (sendConfirmationCodeBody == null)
                return;

            var subject = "Confirmation code";
            var body = $"Your confirmation code is {sendConfirmationCodeBody.Code}";
            await _emailService.SendMessage(sendConfirmationCodeBody.Email, subject, body);
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}