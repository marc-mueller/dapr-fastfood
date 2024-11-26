const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  devServer: {
    proxy: {
      '/api': {
        target: 'http://localhost:8801', // The backend server URL
        changeOrigin: true
      },
      '/orderUpdateHub': {
        target: 'http://localhost:8801', // Backend server URL for SignalR
        changeOrigin: true,
        ws: true // Proxy websockets
      }
    }
  }
})
