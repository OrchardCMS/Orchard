using System.Collections.Generic;

namespace IDeliverable.Licensing.Service.Services
{
    internal class OrderInfo
    {
        public OrderInfo(int orderId, OrderStatus status, bool isProductAccessAllowed, int? subscriptionId, IEnumerable<string> hostnames)
        {
            OrderId = orderId;
            Hostnames = hostnames;
            Status = status;
            IsProductAccessAllowed = isProductAccessAllowed;
            SubscriptionId = subscriptionId;
        }

        public int OrderId { get; }
        public IEnumerable<string> Hostnames { get; }
        public OrderStatus Status { get; }
        public bool IsProductAccessAllowed { get; }
        public int? SubscriptionId { get; }
    }
}