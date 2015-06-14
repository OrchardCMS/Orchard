using System.Collections.Generic;

namespace IDeliverable.Licensing.Service.Services
{
    internal class OrderInfo
    {
        public OrderInfo(int orderId, IEnumerable<string> hostnames)
        {
            OrderId = orderId;
            Hostnames = hostnames;
        }

        public int OrderId { get; }
        public IEnumerable<string> Hostnames { get; }
    }
}