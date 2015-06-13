using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDeliverable.Licensing.Service.Services
{
    public class OrderInfo
    {
        public OrderInfo(int orderId, IEnumerable<string> hostnames)
        {
            OrderId = orderId;
            Hostnames = hostnames;
        }

        public int OrderId { get; private set; }
        public IEnumerable<string> Hostnames { get; private set; }
    }
}