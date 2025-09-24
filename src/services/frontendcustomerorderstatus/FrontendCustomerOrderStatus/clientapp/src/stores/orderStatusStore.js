import { defineStore } from 'pinia'
import apiClient from '@/store/apiClient'
import * as signalR from '@microsoft/signalr'

export const useOrderStatusStore = defineStore('orderStatus', {
  state: () => ({ ordersInPreparation: [], ordersFinished: [], signalRHubConnection: null }),
  getters: {
    ordersInPreparationList: (s) => s.ordersInPreparation,
    ordersFinishedList: (s) => s.ordersFinished,
  },
  actions: {
    updateOrder(updatedOrder) {
      this.ordersInPreparation = this.ordersInPreparation.filter(o => o.id !== updatedOrder.id)
      this.ordersFinished = this.ordersFinished.filter(o => o.id !== updatedOrder.id)
      if (updatedOrder.type !== 'Inhouse') return
      switch (updatedOrder.state) {
        case 'Paid':
        case 'Processing':
          this.ordersInPreparation.push(updatedOrder)
          break
        case 'Closed':
        case 'Prepared':
          this.ordersFinished.push(updatedOrder)
          if (this.ordersFinished.length > 10) this.ordersFinished.shift()
          break
      }
    },
    removeOrder(orderId) {
      this.ordersInPreparation = this.ordersInPreparation.filter(o => o.id !== orderId)
      this.ordersFinished = this.ordersFinished.filter(o => o.id !== orderId)
    },
    async fetchOrderAndUpdateStore(orderId) {
      try {
        const response = await apiClient.getOrder(orderId)
        this.updateOrder(response.data)
      } catch (error) {
        if (error.response && error.response.status === 404) this.removeOrder(orderId)
        else console.error('Error fetching pending order:', error)
      }
    },
    async initializeSignalRHub() {
      const connection = new signalR.HubConnectionBuilder().withUrl('/orderupdatehub').build()
      connection.on('ReceiveOrderUpdate', (order) => this.fetchOrderAndUpdateStore(order.id))
      await connection.start()
      await connection.invoke('SubscribeToOrderUpdates')
      this.signalRHubConnection = connection
    },
  }
})
