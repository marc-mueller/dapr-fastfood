namespace FastFood.Common;

public static class FastFoodConstants
{
    public const string PubSubName = "pubsub";
    public const string StateStoreName = "statestore";
    public const string SecretStore = "fastfood-secrets";

    public static class EventNames
    {
        public const string OrderCreated = "ordercreated";
        public const string OrderUpdated = "orderupdated";
        public const string OrderProcessingUpdated = "orderprocessingupdated";
        public const string OrderConfirmed = "orderconfirmed";
        public const string OrderPaid = "orderpaid";
        public const string KitchenItemFinished = "kitchenitemfinished";
        public const string KitchenOrderStartProcessing = "kitchenorderstartprocessing";
        public const string OrderPrepared = "orderprepared";
        public const string OrderClosed = "orderclosed";
        public const string DeadLetterOrderPaid = "deadletter_orderpaid";
        public const string DeadLetterKitchenOrderStartProcessing = "deadletter_kitchenorderstartprocessing";
        public const string DeadLetterKitchenItemFinished = "deadletter_kitchenitemfinished";
    }

    public static class Services
    {
        public const string FinanceService = "financeservice";
        public const string OrderService = "orderservice";
        public const string KitchenService = "kitchenservice";
    }
}