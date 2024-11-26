<template>
  <div class="p-5">
    <h3 class="text-2xl font-bold mb-4">Order Confirmation ({{ order.orderReference }})</h3>
    <div v-if="!order || order.items.length === 0">Your order is empty.</div>
    <div v-else>
      <div v-for="item in order.items" :key="item.id" class="flex justify-between items-center mb-2">
        {{ item.productDescription }} - {{ item.quantity }} x {{ formatCurrency(item.itemPrice) }} = {{ formatCurrency(item.quantity * item.itemPrice) }}
      </div>
      <div class="mt-2 font-bold">
        <strong>Total: </strong>{{ formatCurrency(totalOrderPrice) }}
      </div>
      <button v-if="!isPaid" class="mt-5 p-3 bg-green-500 text-white rounded-md" @click="pay">Pay</button>
      <div v-if="isPaid" class="mt-5 font-bold text-green-500">
        <p>Payment confirmed! Thank you for your purchase. You will be redirected shortly.</p>
        <button class="mt-3 p-3 bg-blue-500 text-white rounded-md" @click="navigateToStart">OK</button>
      </div>
    </div>
  </div>
</template>

<script>
import { mapGetters } from 'vuex';

export default {
  computed: {
    order() {
      return this.$store.state.currentOrder;
    },
    ...mapGetters(['totalOrderPrice']),
    isPaid() {
      console.log("Order state:", this.order?.state);
      return (this.order?.state === "Paid" || this.order?.state === "Processing");
    }
  },
  watch: {
    order: {
      deep: true,
      handler(newVal) {
        if (newVal && (newVal.state === "Paid" || newVal.state === "Processing") ) {
          console.log("Order state is paid, navigating to start page in 3 seconds...");
          setTimeout(() => {
            this.navigateToStart();
          }, 3000);
        }
      }
    }
  },
  methods: {
    pay() {
      this.$store.dispatch('confirmPayment');
    },
    navigateToStart() {
      this.$router.push('/');
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