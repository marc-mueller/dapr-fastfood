const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  devServer: {
    proxy: {
      '/api': {
        target: 'http://localhost:8803', // The backend server URL
        changeOrigin: true
      },
      '/orderupdatehub': {
        target: 'http://localhost:8803', // Backend server URL for SignalR
        changeOrigin: true,
        ws: true // Proxy websockets
      }
    }
  }
})
