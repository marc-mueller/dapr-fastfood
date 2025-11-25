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
  const items = (x.items?.map(y => ({ id: y.id, name: y.productDescription, quantity: y.quantity, state: y.state })) ?? [])
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
  <div class="container mx-auto p-4" data-testid="kitchen-monitor">
    <h1 class="text-2xl font-bold mb-4" data-testid="kitchen-title">Kitchen Work Monitor</h1>
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4" data-testid="orders-container">
      <div 
        v-for="order in pendingOrders" 
        :key="order.id" 
        class="p-4 bg-white rounded shadow"
        :data-testid="`order-card-${order.name.toLowerCase()}`">
        <h2 class="text-xl font-semibold mb-2" :data-testid="`order-title-${order.name.toLowerCase()}`">{{ order.name }} ({{ order.id }})</h2>
        <ul class="space-y-2" :data-testid="`order-items-${order.name.toLowerCase()}`">
          <li 
            v-for="item in order.orderItems" 
            :key="item.id" 
            class="flex items-center justify-between"
            :data-testid="`order-item-${item.id}`"
            :data-order-ref="order.name.toLowerCase()"
            :data-product-name="item.name">
            <span>
              <span :data-testid="`item-quantity-${item.id}`">{{ item.quantity }}</span>× 
              <span :data-testid="`item-name-${item.id}`">{{ item.name }}</span>
            </span>
            <div class="space-x-2">
              <span 
                v-if="item.state === 'Finished'" 
                class="text-green-600 font-semibold"
                :data-testid="`item-finished-${item.id}`">Finished</span>
              <button 
                v-else 
                class="px-3 py-1 bg-blue-500 text-white rounded" 
                :data-testid="`finish-button-${item.id}`"
                @click="finishOrderItem(item.id)">Finish</button>
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