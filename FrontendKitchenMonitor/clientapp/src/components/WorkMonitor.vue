<script setup>
import { onMounted, computed } from 'vue';
import { useStore } from 'vuex';

const store = useStore();

const finishItem = async (itemId) => {
  try {
    await store.dispatch('finishOrderItem', itemId);
  } catch (err) {
    console.error('Failed to finish item:', err);
  }
};

const pendingOrders = computed(() => store.getters.pendingOrders);

onMounted(() => {
  store.dispatch('fetchPendingOrders');
  store.dispatch('initializeSignalRHub');
});
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold mb-4">Kitchen Monitor</h1>
    <div v-for="order in pendingOrders" :key="order.id" class="bg-white shadow-md rounded-lg p-4 mb-4">
      <h2 class="text-xl font-semibold mb-2">Order {{ order.orderReference }} ({{ order.id }})</h2>
      <ul>
        <li v-for="item in order.items" :key="item.id" class="p-2 border-b last:border-none">
          <div class="flex justify-between items-center">
            <div>
              <span class="font-bold">{{ item.productDescription }}</span>
              <div class="text-sm text-gray-500">Quantity: {{ item.quantity }}</div>
              <div class="text-sm text-gray-500" v-if="item.customerComments">
                Customer Comments: {{ item.customerComments }}
              </div>
              <div class="text-sm text-gray-500">Status: {{ item.state }}</div>
            </div>
            <button @click="finishItem(item.id)" :disabled="item.state !== 'AwaitingPreparation'"
                    class="bg-blue-500 text-white px-4 py-2 rounded disabled:bg-gray-300">
              Finish
            </button>
          </div>
        </li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
/* No scoped styles needed as Tailwind CSS will handle the styling */
</style>