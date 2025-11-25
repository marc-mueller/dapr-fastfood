<template>
  <div class="p-5 bg-gray-100 dark:bg-gray-800 rounded-lg shadow-md dark:shadow-gray-900/50" data-testid="shopping-cart">
    <h3 class="text-2xl font-bold mb-4 text-gray-900 dark:text-gray-100" data-testid="shopping-cart-title">Shopping Cart</h3>
    <div v-if="cart.length === 0" class="text-gray-600 dark:text-gray-300" data-testid="empty-cart-message">Your cart is empty.</div>
    <div v-else data-testid="cart-items-container">
      <div 
        v-for="item in cart" 
        :key="item.id" 
        class="flex justify-between items-center mb-3 p-3 bg-white dark:bg-gray-700 rounded-lg shadow dark:shadow-gray-900/50"
        :data-testid="`cart-item-${item.id}`"
        :data-product-id="item.productId"
        :data-product-name="item.productDescription">
        <span class="text-gray-900 dark:text-gray-100" :data-testid="`cart-item-text-${item.id}`">
          {{ item.productDescription }} - {{ item.quantity }} x {{ formatCurrency(item.itemPrice) }} = {{ formatCurrency(item.quantity * item.itemPrice) }}
        </span>
        <button 
          class="p-2 bg-red-500 hover:bg-red-600 text-white rounded-md" 
          :data-testid="`remove-item-${item.id}`"
          @click="removeFromCart(item)">Remove</button>
      </div>
      <div class="mt-2 font-bold text-gray-900 dark:text-gray-100" data-testid="cart-total">
        <strong>Total: </strong>{{ formatCurrency(totalOrderPrice) }}
      </div>
      <button 
        class="w-full p-3 bg-blue-500 hover:bg-blue-600 text-white rounded-md mt-4" 
        data-testid="order-button"
        @click="handleConfirmOrder">Order</button>
    </div>
  </div>
</template>

<script>
import { computed } from 'vue';
import { storeToRefs } from 'pinia';
import { useOrderStore } from '@/stores/orderStore';

export default {
  setup() {
    const orderStore = useOrderStore();
    const { totalOrderPrice } = storeToRefs(orderStore); // getter
    const cart = computed(() => orderStore.currentOrder?.items || []);

    return { orderStore, totalOrderPrice, cart };
  },
  methods: {
    async removeFromCart(item) {
      await this.orderStore.removeItemFromOrder(item);
    },
    async handleConfirmOrder() {
      await this.orderStore.confirmOrder();
      this.$router.push('/order-confirmation');
    },
    formatCurrency(value) {
      if (value === undefined || value === null) {
        return '$0.00';
      }
      return `$${value.toFixed(2)}`;
    }
  }
}
</script>