<template>
  <div id="app" class="min-h-screen flex">
    <router-view />
  </div>
</template>

<script>
export default {
  name: 'App'
}
</script>

<style>
#app {
  @apply font-sans text-gray-800;
}
</style>