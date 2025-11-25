<template>
  <div class="flex overflow-hidden w-full" data-testid="products-page">
    <div class="p-6 overflow-y-auto" data-testid="products-list">
      <!-- Added padding to the right to ensure spacing -->
      <div v-for="(products, category) in groupedProducts" :key="category" class="mb-8" :data-testid="`category-${category}`">
        <h2 class="text-2xl font-bold mb-4 dark:text-gray-100">{{ category }}</h2>
        <div class="flex flex-wrap gap-5">
          <!-- Added class and inline style to product to avoid overflow -->
          <div 
            v-for="product in products" 
            :key="product.id" 
            :data-testid="`product-card-${product.id}`"
            :data-product-name="product.title"
            class="flex-grow-0 flex-shrink-0 bg-white dark:bg-gray-800 p-4 border dark:border-gray-700 rounded-lg shadow dark:shadow-gray-900/50 min-w-[250px] md:min-w-[300px]" 
            style="max-width: calc(33.333% - 20px);">
            <img :src="product.imageUrl || defaultImage" :alt="product.title" class="product-image w-full h-auto mb-3 rounded-lg" />
            <h3 class="text-lg font-bold mb-1 text-gray-900 dark:text-gray-100" :data-testid="`product-name-${product.id}`">{{ product.title }}</h3>
            <p class="mb-2 text-gray-600 dark:text-gray-300">{{ product.description }}</p>
            <p class="font-bold mb-2 text-gray-900 dark:text-gray-100">
              <strong>Price:</strong> {{ formatCurrency(product.price) }}
            </p>
            <div class="flex items-center justify-between mb-3">
              <button 
                class="p-2 bg-gray-200 dark:bg-gray-700 dark:text-gray-200 hover:bg-gray-300 dark:hover:bg-gray-600 rounded-md" 
                :data-testid="`decrease-quantity-${product.id}`"
                @click="decreaseQuantity(product)">-</button>
              <span class="dark:text-gray-200" :data-testid="`quantity-${product.id}`">{{ product.quantity || 1 }}</span>
              <button 
                class="p-2 bg-gray-200 dark:bg-gray-700 dark:text-gray-200 hover:bg-gray-300 dark:hover:bg-gray-600 rounded-md" 
                :data-testid="`increase-quantity-${product.id}`"
                @click="increaseQuantity(product)">+</button>
            </div>
            <button 
              class="w-full p-3 bg-blue-500 hover:bg-blue-600 text-white rounded-md" 
              :data-testid="`add-to-cart-${product.id}`"
              @click="addToCart(product)">Add</button>
          </div>
        </div>
      </div>
    </div>
    <div class="min-w-[25rem] w-[20%] flex-shrink-0 h-screen p-5 bg-gray-100 dark:bg-gray-800 overflow-y-auto">
      <ShoppingCart />
    </div>
  </div>
</template>

<script>
import ShoppingCart from './ShoppingCart.vue';
import { useOrderStore } from '@/stores/orderStore';

export default {
  components: { ShoppingCart },
  setup() {
    const orderStore = useOrderStore();
    return { orderStore };
  },
  data() {
    return {
      defaultImage: '/path/to/default-image.png',
    };
  },
  computed: {
    products() { return this.orderStore.products; },
    groupedProducts() {
      return this.products.reduce((acc, product) => {
        (acc[product.category] = acc[product.category] || []).push(product);
        return acc;
      }, {});
    },
  },
  methods: {
    increaseQuantity(product) {
      product.quantity = (product.quantity || 1) + 1;
    },
    decreaseQuantity(product) {
      if (product.quantity > 1) product.quantity--;
    },
    async addToCart(product) {
      if (!product.quantity) product.quantity = 1;
      const genId = () => (typeof crypto !== 'undefined' && crypto.randomUUID) ? crypto.randomUUID() : Math.random().toString(36).slice(2);
      const orderItem = {
        id: genId(),
        productId: product.id,
        quantity: product.quantity,
        itemPrice: product.price ?? 0,
        productDescription: product.title,
      };
      await this.orderStore.addItemToOrder(orderItem);
    },
    formatCurrency(value) {
      if (value === undefined || value === null) return '$0.00';
      return `$${value.toFixed(2)}`;
    },
  },
  created() {
    this.orderStore.fetchProducts();
  },
};
</script>

<style scoped>
.product-image { max-width: 100%; max-height: 200px; object-fit: cover; }
.fixed-cart { position: fixed; top: 0; right: 0; height: 100vh; overflow-y: auto; }
</style>