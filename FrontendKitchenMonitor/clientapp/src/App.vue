<template>
  <div class="container mx-auto p-4">
    <WorkMonitor/>
  </div>
</template>

<script>
import WorkMonitor from './components/WorkMonitor.vue';

export default {
  name: 'App',
  components: {
    WorkMonitor
  }
}
</script>

<style>
/* No custom styles needed as Tailwind CSS will handle the styling */
</style>