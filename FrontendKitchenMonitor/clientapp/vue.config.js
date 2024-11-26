const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  devServer: {
    proxy: {
      '/api': {
        target: 'http://localhost:8802', // The backend server URL
        changeOrigin: true
      },
      '/kitchenorderupdatehub': {
        target: 'http://localhost:8802', // Backend server URL for SignalR
        changeOrigin: true,
        ws: true // Proxy websockets
      }
    }
  }
})
