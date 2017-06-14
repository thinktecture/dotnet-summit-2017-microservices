using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using AutoMapper;
using EasyNetQ;
using Microsoft.AspNet.SignalR;
using OrdersService.Hubs;
using OrdersService.Properties;
using QueuingMessages;
using Order = OrdersService.DTOs.Order;

namespace OrdersService.Controllers
{
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private static readonly ConcurrentDictionary<Guid, Order> Datastore;

        static OrdersController()
        {
            Datastore = new ConcurrentDictionary<Guid, Order>();
        }

        [HttpGet]
        [Route]
        public List<Order> GetOrders()
        {
            return Datastore.Values.OrderByDescending(o => o.Created).ToList();
        }

        [HttpPost]
        [Route]
        public void AddNewOrder(Order newOrder)
        {
            var orderId = Guid.NewGuid();
            newOrder.Id = orderId;

            Datastore.TryAdd(orderId, newOrder);

            using (var bus = RabbitHutch.CreateBus(Settings.Default.RabbitMqConnectionString))
            {
                var identity = User.Identity as ClaimsIdentity;
                var subjectId = identity?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                // TODO: re-think data/message shapes, coupling, and mapping <- pragmatic?
                var message = new NewOrderMessage
                {
                    UserId = subjectId,
                    Order = Mapper.Map<QueuingMessages.Order>(newOrder)
                };

                bus.Publish(message);

                GlobalHost.ConnectionManager.GetHubContext<OrdersHub>()
                   .Clients.Group(message.UserId)
                   .orderCreated();
            }
        }
    }
}