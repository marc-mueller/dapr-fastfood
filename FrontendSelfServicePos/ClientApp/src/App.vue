<template>
  <div id="app" class="flex">
    <router-view />
  </div>
</template>

<script>
export default {
  name: 'App'
}
</script>

<style>
html, body {
  height: 100%;
}

#app {
  @apply font-sans text-gray-800 h-full;
}
</style>