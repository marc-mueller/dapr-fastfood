namespace OrderPlacement.Services;

public enum OrderEventRoutingTarget
{
    OrderProcessingServiceActor,
    OrderProcessingServiceState,
    OrderProcessingServiceWorkflow
}