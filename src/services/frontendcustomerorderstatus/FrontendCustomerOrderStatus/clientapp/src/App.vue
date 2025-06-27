<template>
  <div class="container mx-auto p-4">
    <CustomerOrderStatus/>
  </div>
</template>

<script>
import CustomerOrderStatus from './components/CustomerOrderStatus.vue';

export default {
  name: 'App',
  components: {
    CustomerOrderStatus
  }
}
</script>

<style>
/* No custom styles needed as Tailwind CSS will handle the styling */
</style>