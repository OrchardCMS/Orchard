namespace IDeliverable.Licensing.Service.Services {
    internal enum OrderStatus
    {
        Unknown,
        Initial,
        PaymentStarted,
        PaymentPending,
        Failed,
        Complete,
        Chargeback,
        Refunded,
        InDispute,
        Free,
        Imported,
        FraudReview,
        SubscriptionSetup,
        SubscriptionActive,
        SubscriptionComplete,
        SubscriptionCancelled,
    }
}