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
        private readonly string _orderServiceUrl = "http://localhost:8601/api/order";
        private readonly string _kitchenServiceUrl = "http://localhost:8701/api/kitchenwork";

        [Fact]
        public async Task CompleteOrderProcessTest()
        {
            // Create a new order
            var order = await CreateOrder();

            // Add French fries
            var itemFrenchFries = await AddItemToOrder(order.Id, Guid.NewGuid(), "french fries", 1, (decimal) 2.5);

            // Add hamburger
            var itemBurger = await AddItemToOrder(order.Id, Guid.NewGuid(), "hamburger", 1, (decimal) 7.5);

            // Confirm the order
            await ConfirmOrder(order.Id);

            // Confirm payment
            await ConfirmPayment(order.Id);
            
            // Check order status (e.g., processing)
            // disabled as we do not have some waiting time between payment and the start of processing
            // await WaitUntilOrderState(order.Id, OrderDtoState.Paid);
            // await CheckOrderStatus(order.Id, OrderDtoState.Paid);

            // let the kitchen staff prepare the food (very fast ;-))
            await WaitUntilOrderState(order.Id, OrderDtoState.Processing);
            
            // Mock kitchen finish for French fries
            await FinishItemInKitchen(itemFrenchFries.Id);

            // Mock kitchen finish for hamburger
            await FinishItemInKitchen(itemBurger.Id);
            
            await WaitUntilOrderState(order.Id, OrderDtoState.Prepared);

            await CheckOrderStatus(order.Id, OrderDtoState.Prepared);

            // Mark the order as served
            await ServeOrder(order.Id);
        }

        private async Task WaitUntilOrderState(Guid orderId, OrderDtoState targetState, int maxRetries = 10, int delay = 500)
        {
            var retry = 0;
            do
            {
                await Task.Delay(delay);
                retry++;
            } while (retry <= maxRetries && (await GetOrderState(orderId)) != targetState);
        }       

        private async Task<OrderDto> CreateOrder()
        {
            var response = await _client.PostAsync($"{_orderServiceUrl}/createOrder", new StringContent("{\"type\": \"Inhouse\"}", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var order = JsonConvert.DeserializeObject<OrderDto>(content);
            Assert.NotNull(order);

            return order;
        }

        private async Task<OrderItemDto> AddItemToOrder(Guid orderId, Guid productId, string productDescription, int quantity, decimal price)
        {
            var itemData = new OrderItemDto()
            {
                ProductId = productId,
                ProductDescription = productDescription,
                Quantity = quantity,
                ItemPrice = price
            };

            var response = await _client.PostAsync($"{_orderServiceUrl}/addItem/{orderId}", new StringContent(JsonConvert.SerializeObject(itemData), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var order = JsonConvert.DeserializeObject<OrderDto>(content);
            var item = order?.Items?.FirstOrDefault(x => x.ProductId == itemData.ProductId);
            Assert.NotNull(item);

            // Check the properties of the returned item
            Assert.Equal(productId, item.ProductId);
            Assert.Equal(productDescription, item.ProductDescription);
            Assert.Equal(quantity, item.Quantity);
            Assert.Equal(price, item.ItemPrice);

            return item;
        }

        private async Task ConfirmOrder(Guid orderId)
        {
            var response = await _client.PostAsync($"{_orderServiceUrl}/confirmOrder/{orderId}", new StringContent("", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        private async Task ConfirmPayment(Guid orderId)
        {
            var response = await _client.PostAsync($"{_orderServiceUrl}/confirmPayment/{orderId}", new StringContent("", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        private async Task CheckOrderStatus(Guid orderId, OrderDtoState expectedStatus)
        {
            var orderState = await GetOrderState(orderId);
            Assert.Equal(expectedStatus, orderState);
        }

        private async Task<OrderDtoState> GetOrderState(Guid orderId)
        {
            var response = await _client.GetAsync($"{_orderServiceUrl}/{orderId}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var order = JsonConvert.DeserializeObject<OrderDto>(content);
            if (order is null)
            {
                throw new ArgumentException($"Order not found for id {orderId}");
            }
            var orderState = order.State;
            return orderState;
        }

        private async Task FinishItemInKitchen(Guid itemId)
        {
            var response = await _client.PostAsync($"{_kitchenServiceUrl}/itemfinished/{itemId}", new StringContent("", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        private async Task ServeOrder(Guid orderId)
        {
            var response = await _client.PostAsync($"{_orderServiceUrl}/setOrderServed/{orderId}", new StringContent("", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }
    }
}