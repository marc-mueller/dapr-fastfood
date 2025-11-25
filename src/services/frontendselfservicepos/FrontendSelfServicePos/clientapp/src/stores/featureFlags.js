import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';

export const useFeatureFlagsStore = defineStore('featureFlags', () => {
    const flags = ref({
        LoyaltyProgram: false,
        NewCheckoutExperience: false,
        DarkMode: true
    });

    const loading = ref(false);
    const error = ref(null);
    const currentUserId = ref(null);
    let pollingInterval = null;
    const POLLING_INTERVAL_MS = 10000; // 10 seconds - matches Azure App Config cache refresh

    // Generate a new user ID for each order session (kiosk mode)
    // This ensures percentage rollouts work per-order, not per-browser
    function generateNewUserId() {
        currentUserId.value = crypto.randomUUID();
        return currentUserId.value;
    }

    async function fetchFlags() {
        loading.value = true;
        error.value = null;
        try {
            // Use current userId for targeting filter evaluation
            // If no userId exists yet, generate one
            const userId = currentUserId.value || generateNewUserId();
            
            const response = await axios.get('/api/FeatureFlags', { 
                params: { userId } 
            });
            flags.value = response.data;
        } catch (err) {
            console.error('Failed to load feature flags:', err);
            error.value = 'Failed to load feature flags';
        } finally {
            loading.value = false;
        }
    }

    function startPolling() {
        // Clear any existing interval
        stopPolling();
        
        // Initial fetch
        fetchFlags();
        
        // Set up polling - fetch flags every 30 seconds to detect runtime changes
        pollingInterval = setInterval(() => {
            fetchFlags();
        }, POLLING_INTERVAL_MS);
    }

    function stopPolling() {
        if (pollingInterval) {
            clearInterval(pollingInterval);
            pollingInterval = null;
        }
    }

    function isEnabled(flagName) {
        return flags.value[flagName] === true;
    }

    // Start a new order session - generates new userId for fresh targeting evaluation
    // Call this when user clicks "Start Ordering" to simulate new customer
    function startNewOrderSession() {
        generateNewUserId();
        fetchFlags(); // Immediately fetch with new userId
    }

    return { 
        flags, 
        loading, 
        error, 
        fetchFlags, 
        startPolling, 
        stopPolling, 
        isEnabled,
        startNewOrderSession,
        currentUserId
    };
});
