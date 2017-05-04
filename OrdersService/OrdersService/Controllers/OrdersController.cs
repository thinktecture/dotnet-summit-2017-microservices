using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using OrdersService.DTOs;

namespace OrdersService.Controllers
{
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private static readonly ConcurrentDictionary<Guid, Order> Database;

        static OrdersController()
        {
            Database= new ConcurrentDictionary<Guid, Order>();
        }

        [HttpGet]
        [Route]
        public List<Order> GetOrders()
        {
            return Database.Values.ToList();
        }

        [HttpPost]
        [Route]
        public void AddNewOrder(Order newOrder)
        {
            var orderId = Guid.NewGuid();
            newOrder.Id = orderId;

            Database.TryAdd(orderId, newOrder);
        }
    }
}
