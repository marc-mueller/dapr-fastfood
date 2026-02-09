---
applyTo: "**/*.vue,**/*.ts"
---

# Vue.js / TypeScript Guidelines

## Component Structure

Use Vue 3 Composition API with `<script setup>` or Options API with `setup()`:

```vue
<template>
  <div class="p-5 bg-gray-100 dark:bg-gray-800" data-testid="component-name">
    <!-- Always include data-testid for testing -->
  </div>
</template>

<script>
import { computed } from 'vue';
import { storeToRefs } from 'pinia';
import { useOrderStore } from '@/stores/orderStore';

export default {
  setup() {
    const orderStore = useOrderStore();
    const { totalOrderPrice } = storeToRefs(orderStore);
    const cart = computed(() => orderStore.currentOrder?.items || []);

    return { orderStore, totalOrderPrice, cart };
  },
  methods: {
    async handleAction() {
      await this.orderStore.someAction();
      this.$router.push('/next-route');
    }
  }
}
</script>
```

## State Management (Pinia)

Stores live in `src/stores/`:

```typescript
import { defineStore } from 'pinia';

export const useOrderStore = defineStore('order', {
  state: () => ({
    currentOrder: null as Order | null,
    items: [] as OrderItem[]
  }),
  getters: {
    totalOrderPrice: (state) => state.items.reduce((sum, item) => sum + item.price * item.quantity, 0)
  },
  actions: {
    async confirmOrder() {
      // API calls via fetch or axios
    }
  }
});
```

## Styling

Use Tailwind CSS 4.1+ utility classes directly in templates:

```vue
<div class="p-5 bg-gray-100 dark:bg-gray-800 rounded-lg shadow-md">
  <h3 class="text-2xl font-bold mb-4 text-gray-900 dark:text-gray-100">Title</h3>
  <button class="w-full p-3 bg-blue-500 hover:bg-blue-600 text-white rounded-md">
    Action
  </button>
</div>
```

**Dark mode**: Always include `dark:` variants for backgrounds, text, shadows.

## Testing Attributes

Always add `data-testid` attributes for Vitest/Playwright tests:

```vue
<div data-testid="shopping-cart">
  <div v-for="item in cart" :key="item.id" :data-testid="`cart-item-${item.id}`">
    <button :data-testid="`remove-item-${item.id}`" @click="removeItem(item)">Remove</button>
  </div>
  <button data-testid="order-button" @click="confirmOrder">Order</button>
</div>
```

## API Integration

Frontend services communicate via ASP.NET Core backend (same host) which proxies to Dapr:

```typescript
// Typical API call pattern
async function fetchOrder(orderId: string): Promise<Order> {
  const response = await fetch(`/api/order/${orderId}`);
  return response.json();
}
```

## Real-time Updates (SignalR)

```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('/orderhub')
  .withAutomaticReconnect()
  .build();

connection.on('OrderUpdated', (order: Order) => {
  // Handle update
});

await connection.start();
```

## Project Structure

```
clientapp/
├── src/
│   ├── components/     # Vue components
│   ├── stores/         # Pinia stores
│   ├── views/          # Route views (if using Vue Router)
│   ├── App.vue         # Root component
│   └── main.ts         # Entry point
├── package.json
├── vite.config.ts
└── tailwind.config.js
```

## Build Commands

```bash
npm install     # Install dependencies
npm run dev     # Development server with HMR
npm run build   # Production build (output to dist/)
npm test        # Run Vitest tests
npm run test:watch  # Watch mode
```
