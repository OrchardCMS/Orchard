using System.Collections.Generic;

namespace IDeliverable.Licensing.Service.Services
{
    internal class OrderInfo
    {
        public OrderInfo(int orderId, OrderStatus status, IEnumerable<string> hostnames)
        {
            OrderId = orderId;
            Hostnames = hostnames;
            Status = status;
        }

        public int OrderId { get; }
        public IEnumerable<string> Hostnames { get; }
        public OrderStatus Status { get; }
    }
}