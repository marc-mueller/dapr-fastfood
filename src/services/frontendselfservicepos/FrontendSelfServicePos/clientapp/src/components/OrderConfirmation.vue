<template>
  <div class="flex justify-center items-center w-full" data-testid="order-confirmation-page">
  <div class="w-[30rem]">
    <h3 class="text-2xl font-bold mb-4 text-gray-900 dark:text-gray-100" data-testid="order-confirmation-title">
      {{ featureFlags.isEnabled('NewCheckoutExperience') ? 'Review Your Order ✨' : `Order Confirmation (${order?.orderReference})` }}
    </h3>
    <div v-if="!order || order.items.length === 0" class="text-gray-600 dark:text-gray-300" data-testid="empty-order-message">Your order is empty.</div>
    <div v-else data-testid="order-details">
      
      <!-- Loyalty Program Input (Feature Flag) -->
      <div 
        v-if="featureFlags.isEnabled('LoyaltyProgram') && !isPaid" 
        class="mb-4 p-4 bg-blue-50 dark:bg-blue-900 rounded-md"
        data-testid="loyalty-program-section">
        <label class="block text-sm font-medium mb-2 text-gray-900 dark:text-gray-100" data-testid="loyalty-label">
          💳 Loyalty Program Member?
        </label>
        <input 
          v-model="loyaltyNumber"
          type="text" 
          placeholder="Enter your loyalty number (e.g., LOYAL123)"
          class="w-full p-2 border rounded-md dark:bg-gray-800 dark:border-gray-600 dark:text-gray-100 dark:placeholder-gray-400"
          data-testid="loyalty-input"
          maxlength="20"
        />
        <p v-if="loyaltyNumber" class="text-sm text-green-600 dark:text-green-400 mt-1" data-testid="loyalty-discount-message">
          🎉 10% loyalty discount will be applied!
        </p>
      </div>

      <!-- Order Items -->
      <div 
        v-for="item in order.items" 
        :key="item.id" 
        class="flex gap-4 items-center mb-2 text-gray-900 dark:text-gray-100"
        :class="{ 'bg-gradient-to-r from-purple-50 to-pink-50 dark:from-purple-900 dark:to-pink-900 p-2 rounded': featureFlags.isEnabled('NewCheckoutExperience') }"
        :data-testid="`order-item-${item.productDescription.replace(/\s+/g, '-').toLowerCase()}`">
        <span :class="{ 'font-bold text-purple-600 dark:text-purple-300': featureFlags.isEnabled('NewCheckoutExperience') }">
          <span data-testid="item-quantity">{{ item.quantity }}</span>×
        </span>
        <span data-testid="item-name">{{ item.productDescription }}</span>
        <span class="ml-auto" data-testid="item-total">{{ formatCurrency(item.quantity * item.itemPrice) }}</span>
      </div>

      <!-- Pricing Breakdown -->
      <div class="mt-4 pt-3 border-t dark:border-gray-600 text-gray-900 dark:text-gray-100">
        <!-- Subtotal -->
        <div class="flex justify-between text-sm mb-1" data-testid="order-subtotal">
          <span>Subtotal</span>
          <span>{{ formatCurrency(subtotal) }}</span>
        </div>

        <!-- Service Fee (DynamicPricing feature) -->
        <div 
          v-if="displayOrder.serviceFee" 
          class="flex justify-between text-sm mb-1 text-orange-600 dark:text-orange-400" 
          data-testid="service-fee">
          <span>⏰ Peak Hour Service Fee (+20%)</span>
          <span>{{ formatCurrency(displayOrder.serviceFee) }}</span>
        </div>

        <!-- Loyalty Discount (LoyaltyProgram feature) -->
        <div 
          v-if="displayOrder.discount" 
          class="flex justify-between text-sm mb-1 text-green-600 dark:text-green-400" 
          data-testid="loyalty-discount">
          <span>💳 Loyalty Discount (-10%)</span>
          <span>-{{ formatCurrency(displayOrder.discount) }}</span>
        </div>

        <!-- Total -->
        <div class="mt-2 pt-2 border-t dark:border-gray-600 font-bold text-lg flex justify-between" data-testid="order-total">
          <span>Total</span>
          <span>{{ formatCurrency(totalOrderPrice) }}</span>
        </div>
      </div>

      <!-- Pay Button -->
      <button 
        v-if="!isPaid" 
        class="w-full mt-5 p-3 rounded-md text-white font-bold"
        :class="featureFlags.isEnabled('NewCheckoutExperience') 
          ? 'bg-gradient-to-r from-purple-500 to-pink-500 hover:from-purple-600 hover:to-pink-600 transform hover:scale-105 transition-all' 
          : 'bg-green-500 hover:bg-green-600'"
        data-testid="pay-button"
        @click="pay">
        {{ featureFlags.isEnabled('NewCheckoutExperience') ? '🚀 Complete Order' : 'Pay' }}
      </button>

      <!-- Payment Confirmation -->
      <div v-else class="mt-5 font-bold text-green-500" data-testid="payment-confirmation">
        <p data-testid="payment-confirmed-message">Payment confirmed! Thank you for your purchase. You will be redirected shortly.</p>
        <button 
          class="w-full mt-3 p-3 bg-blue-500 text-white rounded-md" 
          data-testid="ok-button"
          @click="navigateToStart">OK</button>
      </div>
    </div>
  </div>
  </div>
</template>

<script>
import { computed, watch, onBeforeUnmount, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useOrderStore } from '@/stores/orderStore';
import { useFeatureFlagsStore } from '@/stores/featureFlags';

export default {
  setup() {
    const router = useRouter();
    const orderStore = useOrderStore();
    const featureFlags = useFeatureFlagsStore();
    const order = computed(() => orderStore.currentOrder);
    const loyaltyNumber = ref('');
    
    // Calculate subtotal from items
    const subtotal = computed(() => {
      if (!order.value?.items) return 0;
      return order.value.items.reduce((sum, item) => sum + (item.quantity * item.itemPrice), 0);
    });

    // Client-side discount calculation (will be validated by backend on payment)
    const clientSideDiscount = computed(() => {
      if (!loyaltyNumber.value || !featureFlags.isEnabled('LoyaltyProgram')) return 0;
      return Math.round(subtotal.value * 0.10 * 100) / 100; // 10% discount
    });

    // Override order.discount with client-side calculation for display
    const displayOrder = computed(() => {
      if (!order.value) return null;
      return {
        ...order.value,
        discount: clientSideDiscount.value || order.value.discount
      };
    });

    // Calculate total including service fee and discount
    const totalOrderPrice = computed(() => {
      const base = subtotal.value;
      const serviceFee = order.value?.serviceFee || 0;
      const discount = clientSideDiscount.value || order.value?.discount || 0;
      return base + serviceFee - discount;
    });

    const isPaid = computed(() => order.value && (order.value.state === 'Paid' || order.value.state === 'Processing'));
    let redirectTimer = null;

    watch(order, (newVal) => {
      if (newVal && (newVal.state === 'Paid' || newVal.state === 'Processing')) {
        if (redirectTimer) {
          clearTimeout(redirectTimer);
          redirectTimer = null;
        }
        redirectTimer = setTimeout(() => {
          navigateToStart();
        }, 3000);
      }
    }, { deep: true });

  function pay() { 
    orderStore.confirmPayment(); 
  }
  
  function navigateToStart() {
    if (redirectTimer) {
      clearTimeout(redirectTimer);
      redirectTimer = null;
    }
    loyaltyNumber.value = ''; // Clear loyalty number
    
    // Generate new userId for next order session (kiosk mode)
    // This ensures the next customer gets fresh targeting evaluation
    featureFlags.startNewOrderSession();
    
    router.push('/');
  }

    onBeforeUnmount(() => {
      if (redirectTimer) {
        clearTimeout(redirectTimer);
        redirectTimer = null;
      }
    });

    function formatCurrency(value) {
      if (value === undefined || value === null) return '$0.00';
      return `$${value.toFixed(2)}`;
    }

    return { 
      order, 
      displayOrder,
      totalOrderPrice,
      subtotal, 
      isPaid, 
      pay, 
      navigateToStart, 
      formatCurrency,
      featureFlags,
      loyaltyNumber
    };
  }
}
</script>