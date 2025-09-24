import { defineStore } from 'pinia'
import apiClient from '@/store/apiClient'
import * as signalR from '@microsoft/signalr'

export const useKitchenStore = defineStore('kitchen', {
  state: () => ({ pendingOrders: [], signalRHubConnection: null }),
  getters: {
    pendingOrdersList: (state) => state.pendingOrders,
  },
  actions: {
    async fetchPendingOrders() {
      const response = await apiClient.getPendingOrders()
      this.pendingOrders = response.data
    },
    async fetchOrderAndUpdateStore(orderId) {
      try {
        const response = await apiClient.getPendingOrder(orderId)
        const idx = this.pendingOrders.findIndex(o => o.id === orderId)
        if (idx !== -1) this.pendingOrders[idx] = response.data
        else this.pendingOrders.push(response.data)
      } catch (error) {
        if (error.response && error.response.status === 404) {
          this.pendingOrders = this.pendingOrders.filter(o => o.id !== orderId)
        } else {
          console.error('Error fetching pending order:', error)
        }
      }
    },
    async finishOrderItem(itemId) {
      try { await apiClient.finishOrderItem(itemId) } catch (e) { console.error('Error finishing order item:', e) }
    },
    async initializeSignalRHub() {
      const connection = new signalR.HubConnectionBuilder().withUrl('/kitchenorderupdatehub').build()
      // Refresh full list on updates; avoids 404s when an order is removed server-side
      connection.on('kitchenorderupdated', () => this.fetchPendingOrders())
      await connection.start()
      await connection.invoke('SubscribeToWork')
      this.signalRHubConnection = connection
    },
  }
})
