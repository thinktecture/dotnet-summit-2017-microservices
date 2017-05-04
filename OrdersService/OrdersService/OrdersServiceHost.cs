using System;
using System.Threading.Tasks;
using AutoMapper;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Nanophone.Core;
using Nanophone.RegistryHost.ConsulRegistry;
using OrdersService.Discovery;
using OrdersService.Hubs;
using OrdersService.Properties;
using QueuingMessages;
using Order = OrdersService.DTOs.Order;
using OrderItem = OrdersService.DTOs.OrderItem;

namespace OrdersService
{
    class OrdersServiceHost
    {
        private static ServiceRegistry _serviceRegistry;
        private static RegistryInformation _registryInformation;
        private static IDisposable _server;
        private static IBus _bus;

        public void Start()
        {
            var baseUrl = new Uri("http://localhost:7777");
            var healthUrl = new Uri("http://localhost:7777/api/health/ping");

            WireAppDomainHandlers();
            InitializeMapper();
            SetupQueues();
            ListenOnQueues();

            var registryHost = new ConsulRegistryHost();
            _serviceRegistry = new ServiceRegistry(registryHost);

            Task.Run(async () =>
            {
                _registryInformation = await _serviceRegistry.AddTenantAsync(new CustomWebApiRegistryTenant(baseUrl), "orders", "0.0.2", healthUrl);

                _server = WebApp.Start<Startup>(baseUrl.ToString());
                Console.WriteLine("Orders Service running - listening at {0} ...", baseUrl);
            }).GetAwaiter().GetResult();
        }

        public void Stop()
        {
            _serviceRegistry.DeregisterServiceAsync(_registryInformation.Id).Wait();

            _bus?.Dispose();
            _server?.Dispose();
        }

        private static void SetupQueues()
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

        private static void WireAppDomainHandlers()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
            currentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private static void InitializeMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<OrderItem, QueuingMessages.OrderItem>();
                cfg.CreateMap<Order, QueuingMessages.Order>()
                    .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));
            });
            Mapper.AssertConfigurationIsValid();
        }

        private static async void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (_registryInformation != null)
            {
                await _serviceRegistry.DeregisterServiceAsync(_registryInformation.Id);
            }
        }

        private static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_registryInformation != null)
            {
                await _serviceRegistry.DeregisterServiceAsync(_registryInformation.Id);
            }
        }
    }
}