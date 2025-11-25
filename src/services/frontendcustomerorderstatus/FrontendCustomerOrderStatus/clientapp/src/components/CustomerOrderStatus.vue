<script setup>
import { computed, onMounted } from 'vue';
import { useOrderStatusStore } from '@/stores/orderStatusStore';

const os = useOrderStatusStore();

onMounted(async () => {
  await os.initializeSignalRHub();
});

const ordersInPreparation = computed(() => os.ordersInPreparation);
const ordersFinished = computed(() => os.ordersFinished);
</script>

<template>
  <div class="flex h-screen w-full" data-testid="customer-order-status">
    <!-- Orders in Preparation Column -->
    <div class="w-1/2 p-4 border-r border-gray-200" data-testid="preparation-section">
      <h1 class="text-2xl font-bold mb-4" data-testid="preparation-title">Orders in Preparation</h1>
      <ul data-testid="preparation-orders-list">
        <li 
          v-for="order in ordersInPreparation" 
          :key="order.id" 
          class="mb-2 p-2 bg-gray-100 rounded"
          :data-testid="`preparation-order-${order.orderReference.toLowerCase()}`">
          <span :data-testid="`preparation-order-text-${order.orderReference.toLowerCase()}`">Order {{ order.orderReference }} ({{ order.id }})</span>
        </li>
      </ul>
    </div>

    <!-- Finished Orders Column -->
    <div class="w-1/2 p-4" data-testid="finished-section">
      <h1 class="text-2xl font-bold mb-4" data-testid="finished-title">Finished Orders</h1>
      <ul data-testid="finished-orders-list">
        <li 
          v-for="order in ordersFinished" 
          :key="order.id" 
          class="mb-2 p-2 bg-green-100 rounded"
          :data-testid="`finished-order-${order.orderReference.toLowerCase()}`">
          <span :data-testid="`finished-order-text-${order.orderReference.toLowerCase()}`">Order {{ order.orderReference }} ({{ order.id }})</span>
        </li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
h1 {
  text-align: center;
}
ul {
  list-style-type: none;
  padding-left: 0;
}
</style>