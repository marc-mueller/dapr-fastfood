<template>
  <div class="flex justify-center items-center w-full h-full bg-gray-100">
    <button class="p-5 text-lg bg-green-500 text-white rounded-md shadow-md" @click="startOrder">
      Start Ordering
    </button>
  </div>
</template>

<script>
export default {
  methods: {
    startOrder() {
      this.$store.dispatch('createOrder');
      this.$router.push('/products');
    }
  }
}
</script>