using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using KVATUM_STREAMING_SERVICE.Core.Entities.Cache;
using KVATUM_STREAMING_SERVICE.Core.Entities.Event;
using KVATUM_STREAMING_SERVICE.Core.Entities.Response;
using KVATUM_STREAMING_SERVICE.Core.Enums;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KVATUM_STREAMING_SERVICE.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly ICacheService _cacheService;
        private readonly IMainConnectionService _mainConnectionService;
        private readonly string _cacheAccountKeyPrefix = "account";
        private readonly string _hostname;
        private readonly string _cachedAccountUpdateQueue;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            IMainConnectionService mainConnectionService,
            ICacheService cacheService,
            string hostname,
            string userName,
            string password,
            string cachedAccountUpdateQueue)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;
            _serviceFactory = serviceFactory;
            _mainConnectionService = mainConnectionService;
            _cacheService = cacheService;

            _cachedAccountUpdateQueue = cachedAccountUpdateQueue;

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

            DeclareQueue(_cachedAccountUpdateQueue);
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

            ConsumeQueue(_cachedAccountUpdateQueue, HandleCachedAccountUpdateMessageAsync);

            await Task.CompletedTask;
        }

        private async Task HandleCachedAccountUpdateMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var cachedAccountUpdateEvent = JsonSerializer.Deserialize<CachedAccountUpdateEvent>(message);
            if (cachedAccountUpdateEvent == null || cachedAccountUpdateEvent.Id == Guid.Empty)
                return;

            var cacheKey = $"{_cacheAccountKeyPrefix}:{cachedAccountUpdateEvent.Id}";
            var account = await _cacheService.GetFromCacheAsync<CachedAccount>(cacheKey);
            if (account != null)
            {
                _mainConnectionService.UpdateAccountProfile(cachedAccountUpdateEvent.Id, account.ToAccountProfileBody());
                var currentRoomId = _mainConnectionService.GetCurrentRoomId(cachedAccountUpdateEvent.Id);
                if (currentRoomId != null)
                {
                    var updateAccountProfileBody = new UpdateAccountProfileBody
                    {
                        EventType = MessageEvent.UpdateAccountProfile,
                        AccountProfile = account.ToAccountProfileBody()
                    };

                    var serializedUpdateAccountProfileBody = JsonSerializer.Serialize(updateAccountProfileBody);
                    await _mainConnectionService.SendMessageToAllExceptSender(cachedAccountUpdateEvent.Id, WebSocketMessageType.Text, Encoding.UTF8.GetBytes(serializedUpdateAccountProfileBody));
                }
            }
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}