<template>
  <div class="flex justify-center items-center w-full h-full bg-gray-100">
    <button class="p-5 text-2xl bg-green-500 text-white rounded-md shadow-md" @click="startOrder">
      Start Ordering
    </button>
  </div>
</template>

<script>
import { useOrderStore } from '@/stores/orderStore';
import { useRouter } from 'vue-router';

export default {
  setup() {
    const orderStore = useOrderStore();
    const router = useRouter();
    async function startOrder() {
      await orderStore.createOrder();
      router.push('/products');
    }
    return { startOrder };
  }
}
</script>