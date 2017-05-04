using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EasyNetQ;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Nanophone.Core;
using Nanophone.RegistryHost.ConsulRegistry;
using OrdersService.Discovery;
using OrdersService.Hubs;
using OrdersService.Properties;
using QueuingMessages;
using RabbitMQ.Client;
using Order = OrdersService.DTOs.Order;
using OrderItem = OrdersService.DTOs.OrderItem;

namespace OrdersService
{
    class OrdersServiceHost
    {
        private Uri _baseUrl = new Uri("http://localhost:7777");
        private Uri _healthUrl = new Uri("http://localhost:7777/api/health/ping");

        private static ServiceRegistry _serviceRegistry;
        private static RegistryInformation _registryInformation;
        private static IDisposable _server;
        private static IBus _bus;

        public void Start()
        {
            InitializeMapper();
            SetupQueues();
            ListenOnQueues();

            var registryHost = new ConsulRegistryHost();
            _serviceRegistry = new ServiceRegistry(registryHost);

            Task.Run(async () =>
            {
                _registryInformation = await _serviceRegistry.AddTenantAsync(
                    new CustomWebApiRegistryTenant(_baseUrl), "orders-dns", "1.0.0", _healthUrl);

                _server = WebApp.Start<Startup>(_baseUrl.ToString());
                Console.WriteLine("Orders Service running - listening at {0} ...", _baseUrl);
            }).GetAwaiter().GetResult();

        }

        private void ListenOnQueues()
        {
            _bus = RabbitHutch.CreateBus(Settings.Default.RabbitMqConnectionString);

            // TODO think about async subscribing
            _bus.Subscribe<ShippingCreatedMessage>("shipping", msg =>
            {
                Console.WriteLine("#Shipping created: " + msg.Created + " for " + msg.OrderId);

                GlobalHost.ConnectionManager.GetHubContext<OrdersHub>()
                 .Clients.Group(msg.UserId)
                 .shippingCreated(msg.OrderId);
            });

        }

        private void InitializeMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<OrderItem, QueuingMessages.OrderItem>();
                cfg.CreateMap<Order, QueuingMessages.Order>()
                  .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));
            });
            Mapper.AssertConfigurationIsValid();

        }

        private void SetupQueues()
        {
            using (var advancedBus = RabbitHutch.CreateBus(Settings.Default.RabbitMqConnectionString).Advanced)
            {
                var newOrderQueue = advancedBus.QueueDeclare("QueuingMessages.NewOrderMessage:QueuingMessages_shipping");
                var newOrderExchange = advancedBus.ExchangeDeclare("QueuingMessages.NewOrderMessage:QueuingMessages", ExchangeType.Topic);
                advancedBus.Bind(newOrderExchange, newOrderQueue, String.Empty);

                var shippingCreatedQueue = advancedBus.QueueDeclare("QueuingMessages.ShippingCreatedMessage:QueuingMessages_shipping");
                var shippingCreatedExchange = advancedBus.ExchangeDeclare("QueuingMessages.ShippingCreatedMessage:QueuingMessages", ExchangeType.Topic);
                advancedBus.Bind(shippingCreatedExchange, shippingCreatedQueue, String.Empty);
            }

        }

        public void Stop()
        {
            _serviceRegistry.DeregisterServiceAsync(_registryInformation.Id).Wait();

            _bus?.Dispose();
            _server?.Dispose();
        }
    }
}
