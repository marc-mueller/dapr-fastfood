<template>
  <div class="flex p-5 space-x-5">
    <div class="w-3/4 pr-6 overflow-y-auto">
      <!-- Added padding to the right to ensure spacing -->
      <div v-for="(products, category) in groupedProducts" :key="category" class="mb-8">
        <h2 class="text-2xl font-bold mb-4">{{ category }}</h2>
        <div class="flex flex-wrap gap-5">
          <!-- Added class and inline style to product to avoid overflow -->
          <div v-for="product in products" :key="product.id" class="flex-grow-0 flex-shrink-0 bg-white p-4 border rounded-lg shadow min-w-[250px] md:min-w-[300px]" style="max-width: calc(33.333% - 20px);">
            <img :src="product.imageUrl || defaultImage" :alt="product.title" class="product-image w-full h-auto mb-3 rounded-lg" />
            <h3 class="text-lg font-bold mb-1">{{ product.title }}</h3>
            <p class="mb-2">{{ product.description }}</p>
            <p class="font-bold mb-2">
              <strong>Price:</strong> {{ formatCurrency(product.price) }}
            </p>
            <div class="flex items-center justify-between mb-3">
              <button class="p-2 bg-gray-200 rounded-md" @click="decreaseQuantity(product)">-</button>
              <span>{{ product.quantity || 1 }}</span>
              <button class="p-2 bg-gray-200 rounded-md" @click="increaseQuantity(product)">+</button>
            </div>
            <button class="w-full p-3 bg-blue-500 text-white rounded-md" @click="addToCart(product)">Add</button>
          </div>
        </div>
      </div>
    </div>
    <div class="w-1/4 fixed top-0 right-0 h-screen p-5 bg-gray-100 overflow-y-auto">
      <ShoppingCart />
    </div>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';
import ShoppingCart from './ShoppingCart.vue';
import { v4 as uuidv4 } from 'uuid';

export default {
  components: {
    ShoppingCart,
  },
  data() {
    return {
      defaultImage: '/path/to/default-image.png', // Path to a default image
    };
  },
  computed: {
    ...mapState(['products']),
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
      if (product.quantity > 1) {
        product.quantity--;
      }
    },
    ...mapActions(['addItemToOrder']),
    async addToCart(product) {
      if (!product.quantity) {
        product.quantity = 1;
      }

      const orderItem = {
        id: uuidv4(),
        productId: product.id,
        quantity: product.quantity,
        itemPrice: product.price ?? 0, // Ensure itemPrice is set and handle undefined case
        productDescription: product.title, // Product title as description
      };

      await this.addItemToOrder(orderItem);
    },
    formatCurrency(value) {
      if (value === undefined || value === null) {
        return '$0.00';
      }
      return `$${value.toFixed(2)}`;
    },
  },
  created() {
    this.$store.dispatch('fetchProducts');
  },
};
</script>

<style scoped>
/* Added styles to manage image sizes and overflow */
.product-image {
  max-width: 100%;
  max-height: 200px; /* Adjust according to design */
  object-fit: cover; /* Maintain aspect ratio and cover the box */
}
.fixed-cart {
  position: fixed;
  top: 0;
  right: 0;
  height: 100vh;
  overflow-y: auto; /* Allowing scroll if content overflows */
}
</style>