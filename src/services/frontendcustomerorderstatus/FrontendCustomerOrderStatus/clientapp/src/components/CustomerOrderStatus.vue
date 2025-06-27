<script setup>
import { computed, onMounted } from 'vue';
import { useStore } from 'vuex';

const store = useStore();

// Fetch the initial order statuses on mount
onMounted(() => {
  store.dispatch('initializeSignalRHub');
});

const ordersInPreparation = computed(() => store.getters.ordersInPreparation);
const ordersFinished = computed(() => store.getters.ordersFinished);
</script>

<template>
  <div class="flex h-screen w-full">
    <!-- Orders in Preparation Column -->
    <div class="w-1/2 p-4 border-r border-gray-200">
      <h1 class="text-2xl font-bold mb-4">Orders in Preparation</h1>
      <ul>
        <li v-for="order in ordersInPreparation" :key="order.id" class="mb-2 p-2 bg-gray-100 rounded">
          Order {{ order.orderReference }} ({{ order.id }})
        </li>
      </ul>
    </div>

    <!-- Finished Orders Column -->
    <div class="w-1/2 p-4">
      <h1 class="text-2xl font-bold mb-4">Finished Orders</h1>
      <ul>
        <li v-for="order in ordersFinished" :key="order.id" class="mb-2 p-2 bg-green-100 rounded">
          Order {{ order.orderReference }} ({{ order.id }})
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