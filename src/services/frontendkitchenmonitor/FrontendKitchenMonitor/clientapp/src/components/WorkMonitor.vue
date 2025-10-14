<script setup>
import { computed, onMounted } from 'vue'
import { useKitchenStore } from '@/stores/kitchenStore'

const ks = useKitchenStore()

onMounted(async () => {
  await ks.fetchPendingOrders()
  await ks.initializeSignalRHub()
})

// Presentational projection with items sorted: unfinished first, finished at the bottom
const pendingOrders = computed(() => ks.pendingOrders.map(x => {
  const items = (x.items?.map(y => ({ id: y.id, name: y.productDescription, state: y.state, quantity: y.quantity })) ?? [])
    .slice()
    .sort((a, b) => {
      const aFinished = a.state === 'Finished'
      const bFinished = b.state === 'Finished'
      if (aFinished === bFinished) return 0
      return aFinished ? 1 : -1 // push finished to the bottom
    })
  return {
    id: x.id,
    name: x.orderReference,
    orderItems: items
  }
}))

async function finishOrderItem(itemId) {
  await ks.finishOrderItem(itemId)
  await ks.fetchPendingOrders()
}
</script>

<template>
  <div class="container mx-auto p-4">
    <h1 class="text-2xl font-bold mb-4">Kitchen Work Monitor</h1>
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div v-for="order in pendingOrders" :key="order.id" class="p-4 bg-white rounded shadow">
        <h2 class="text-xl font-semibold mb-2">{{ order.name }} ({{ order.id }})</h2>
        <ul class="space-y-2">
          <li v-for="item in order.orderItems" :key="item.id" class="flex items-center justify-between">
            <span>{{ item.quantity }}x {{ item.name }}</span>
            <div class="space-x-2">
              <span v-if="item.state === 'Finished'" class="text-green-600 font-semibold">Finished</span>
              <button v-else class="px-3 py-1 bg-blue-500 text-white rounded" @click="finishOrderItem(item.id)">Finish</button>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
  
</template>

<style scoped>
/* Tailwind handles styling */
</style>