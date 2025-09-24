<template>
  <div class="p-5 bg-gray-100 rounded-lg shadow-md">
    <h3 class="text-2xl font-bold mb-4">Shopping Cart</h3>
    <div v-if="cart.length === 0">Your cart is empty.</div>
    <div v-else>
      <div v-for="item in cart" :key="item.id" class="flex justify-between items-center mb-3 p-3 bg-white rounded-lg shadow">
        {{ item.productDescription }} - {{ item.quantity }} x {{ formatCurrency(item.itemPrice) }} = {{ formatCurrency(item.quantity * item.itemPrice) }}
        <button class="p-2 bg-red-500 text-white rounded-md" @click="removeFromCart(item)">Remove</button>
      </div>
      <div class="mt-2 font-bold">
        <strong>Total: </strong>{{ formatCurrency(totalOrderPrice) }}
      </div>
      <button class="w-full p-3 bg-blue-500 text-white rounded-md mt-4" @click="handleConfirmOrder">Order</button>
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