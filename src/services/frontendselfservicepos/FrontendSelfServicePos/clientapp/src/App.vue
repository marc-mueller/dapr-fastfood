<template>
  <div id="app" class="flex h-screen" :class="{ 'dark': isDarkMode }">
    <router-view />
  </div>
</template>

<script setup>
import { onMounted, onUnmounted, computed } from 'vue';
import { useFeatureFlagsStore } from '@/stores/featureFlags';

const featureFlags = useFeatureFlagsStore();

const isDarkMode = computed(() => featureFlags.isEnabled('DarkMode'));

onMounted(() => {
  // Start polling for feature flag changes (fetches every 30 seconds)
  // This ensures runtime changes in Azure App Configuration are reflected
  featureFlags.startPolling();
});

onUnmounted(() => {
  // Clean up polling when app is destroyed
  featureFlags.stopPolling();
});
</script>

<style>
html, body {
  height: 100%;
}

#app {
  font-family: ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
  color: #1f2937;
  height: 100%;
}

#app.dark {
  background-color: #111827;
  color: #f3f4f6;
}
</style>