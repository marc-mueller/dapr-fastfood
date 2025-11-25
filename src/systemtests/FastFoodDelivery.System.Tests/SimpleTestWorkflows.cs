using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OrderService.Common.Dtos;
using Xunit;

namespace FastFoodDelivery.System.Tests
{
    public class SimpleTestWorkflows
    {
        private readonly HttpClient _client = new();
        private readonly string _orderServiceUrlState = "http://localhost:8601/api/orderstate";
        private readonly string _orderServiceUrlActor = "http://localhost:8601/api/orderactor";
        private readonly string _orderServiceUrlWorkflow = "http://localhost:8601/api/orderworkflow";
        private readonly string _kitchenServiceUrl = "http://localhost:8701/api/kitchenwork";

        [Fact(Skip = "Refactoring for public endpoints needed")]
        public async Task CompleteOrderProcessStateTest()
        {
            await CompleteOrderProcessTest(_orderServiceUrlState, _kitchenServiceUrl);
        }
        
        [Fact(Skip = "Refactoring for public endpoints needed")]
        public async Task CompleteOrderProcessActorTest()
        {
            await CompleteOrderProcessTest(_orderServiceUrlActor, _kitchenServiceUrl);
        }
        
        [Fact(Skip = "Refactoring for public endpoints needed")]
        public async Task CompleteOrderProcessWorkflowTest()
        {
            await CompleteOrderProcessTest(_orderServiceUrlWorkflow, _kitchenServiceUrl);
        }
        
        private async Task CompleteOrderProcessTest(string orderServiceUrl, string kitchenServiceUrl)
        {
            // Create a new order
            var orderId = await CreateOrder(orderServiceUrl);

            // Add French fries
            var itemFrenchFries = await AddItemToOrder(orderServiceUrl,orderId, Guid.NewGuid(), "french fries", 1, (decimal) 2.5);

            // Add hamburger
            var itemBurger = await AddItemToOrder(orderServiceUrl,orderId, Guid.NewGuid(), "hamburger", 1, (decimal) 7.5);

            // Confirm the order
            await ConfirmOrder(orderServiceUrl,orderId);

            // Confirm payment
            await ConfirmPayment(orderServiceUrl,orderId);
            
            // Check order status (e.g., processing)
            // disabled as we do not have some waiting time between payment and the start of processing
            // await WaitUntilOrderState(order.Id, OrderDtoState.Paid);
            // await CheckOrderStatus(order.Id, OrderDtoState.Paid);

            // let the kitchen staff prepare the food (very fast ;-))
            await WaitUntilOrderState(orderServiceUrl, orderId, OrderDtoState.Processing);
            
            // Mock kitchen finish for French fries
            await FinishItemInKitchen(kitchenServiceUrl, itemFrenchFries.Id);

            // Mock kitchen finish for hamburger
            await FinishItemInKitchen(kitchenServiceUrl, itemBurger.Id);
            
            await WaitUntilOrderState(orderServiceUrl, orderId, OrderDtoState.Prepared);

            await CheckOrderStatus(orderServiceUrl, orderId, OrderDtoState.Prepared);

            // Mark the order as served
            await ServeOrder(orderServiceUrl,orderId);
        }

        private async Task WaitUntilOrderState(string orderServiceUrl, Guid orderId, OrderDtoState targetState, int maxRetries = 50, int delay = 500)
        {
            var retry = 0;
            OrderDtoState? state; 
            do
            {
                await Task.Delay(delay, Xunit.TestContext.Current.CancellationToken);
                state = await GetOrderState(orderServiceUrl, orderId);
                retry++;
            } while (retry <= maxRetries && (state == null ||  state != targetState));
        }
        
        private async Task<OrderDto?> GetAndWaitUntilOrderHasItem(string orderServiceUrl, Guid orderId, Guid itemId, int maxRetries = 50, int delay = 500)
        {
            var retry = 0;
            OrderDto? order;
            do
            {
                await Task.Delay(delay, Xunit.TestContext.Current.CancellationToken);
                order = await GetOrder(orderServiceUrl, orderId);
                retry++;
            } while (retry <= maxRetries && (order == null || order.Items == null  || !order.Items.Any(i => i.Id == itemId)));

            return order;
        }      

        private async Task<Guid> CreateOrder(string orderServiceUrl)
        {
            var response = await _client.PostAsync($"{orderServiceUrl}/createOrder", new StringContent("{\"type\": \"Inhouse\"}", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
    
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CreateOrderResult>(content);
    
            Assert.NotNull(result);
            return result.OrderId;
        }

        private class CreateOrderResult
        {
            public string? Message { get; set; }
            public Guid OrderId { get; set; }
        }

        private async Task<OrderItemDto> AddItemToOrder(string orderServiceUrl, Guid orderId, Guid productId, string productDescription, int quantity, decimal price)
        {
            var itemData = new OrderItemDto()
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ProductDescription = productDescription,
                Quantity = quantity,
                ItemPrice = price
            };

            var response = await _client.PostAsync($"{orderServiceUrl}/addItem/{orderId}", new StringContent(JsonConvert.SerializeObject(itemData), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            
            var order = await GetAndWaitUntilOrderHasItem(orderServiceUrl, orderId, itemData.Id);
            
            var item = order?.Items?.FirstOrDefault(x => x.Id == itemData.Id);
            Assert.NotNull(item);

            // Check the properties of the returned item
            Assert.Equal(productId, item.ProductId);
            Assert.Equal(productDescription, item.ProductDescription);
            Assert.Equal(quantity, item.Quantity);
            Assert.Equal(price, item.ItemPrice);

            return item;
        }

        private async Task ConfirmOrder(string orderServiceUrl, Guid orderId)
        {
            var response = await _client.PostAsync($"{orderServiceUrl}/confirmOrder/{orderId}", new StringContent("", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        private async Task ConfirmPayment(string orderServiceUrl, Guid orderId)
        {
            var response = await _client.PostAsync($"{orderServiceUrl}/confirmPayment/{orderId}", new StringContent("", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        private async Task CheckOrderStatus(string orderServiceUrl, Guid orderId, OrderDtoState expectedStatus)
        {
            var orderState = await GetOrderState(orderServiceUrl, orderId);
            Assert.Equal(expectedStatus, orderState);
        }

        private async Task<OrderDtoState?> GetOrderState(string orderServiceUrl, Guid orderId)
        {
            var order = await GetOrder(orderServiceUrl, orderId);
            return order?.State;
        }
        
        private async Task<OrderDto?> GetOrder(string orderServiceUrl, Guid orderId)
        {
            var response = await _client.GetAsync($"{orderServiceUrl}/{orderId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var order = JsonConvert.DeserializeObject<OrderDto>(content);
                return order;
            }

            return null;
        }

        private async Task FinishItemInKitchen(string kitchenServiceUrl, Guid itemId)
        {
            var response = await _client.PostAsync($"{kitchenServiceUrl}/itemfinished/{itemId}", new StringContent("", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        private async Task ServeOrder(string orderServiceUrl, Guid orderId)
        {
            var response = await _client.PostAsync($"{orderServiceUrl}/setOrderServed/{orderId}", new StringContent("", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }
    }
}