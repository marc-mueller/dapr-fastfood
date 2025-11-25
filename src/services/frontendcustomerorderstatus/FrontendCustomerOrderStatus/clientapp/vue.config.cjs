const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  lintOnSave: true,
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
  },
  chainWebpack: config => {
    // Locate the ESLintWebpackPlugin instance by its plugin key ('eslint')
    config.plugin('eslint').tap(([options]) => {
      // Remove the now-invalid 'extensions' option so ESLint v9 won't choke
      delete options.extensions;
      return [options];
    });
  }
})
