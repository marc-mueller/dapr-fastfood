<template>
  <div class="flex justify-center items-center w-full h-full bg-gray-100 dark:bg-gray-900" data-testid="welcome-screen">
    <button 
      class="p-5 text-2xl bg-green-500 hover:bg-green-600 text-white rounded-md shadow-md dark:shadow-green-900/50" 
      data-testid="start-ordering-button"
      @click="startOrder">
      Start Ordering
    </button>
  </div>
</template>

<script>
import { useOrderStore } from '@/stores/orderStore';
import { useFeatureFlagsStore } from '@/stores/featureFlags';
import { useRouter } from 'vue-router';

export default {
  setup() {
    const orderStore = useOrderStore();
    const featureFlags = useFeatureFlagsStore();
    const router = useRouter();
    
    async function startOrder() {
      // Generate new userId for this order session (kiosk mode)
      // This ensures each order gets fresh targeting evaluation (e.g., 50% rollout)
      featureFlags.startNewOrderSession();
      
      await orderStore.createOrder();
      router.push('/products');
    }
    return { startOrder };
  }
}
</script>