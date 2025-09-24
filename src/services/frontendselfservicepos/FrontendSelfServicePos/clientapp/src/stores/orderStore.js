import { defineStore } from 'pinia'
import apiClient from '@/store/apiClient'
import * as signalR from '@microsoft/signalr'

export const useOrderStore = defineStore('order', {
  state: () => ({
    currentOrder: null,
    products: [],
    signalRHubConnection: null,
  }),
  getters: {
    totalOrderPrice: (state) => {
      if (!state.currentOrder || !state.currentOrder.items) return 0
      return state.currentOrder.items.reduce((total, item) => total + (item.itemPrice * item.quantity), 0)
    }
  },
  actions: {
    async createOrder() {
      const response = await apiClient.post('/order/createOrder', {})
      const { orderId } = response.data
      await this.fetchOrder(orderId)
      await this.initializeHub(orderId)
    },
    async fetchOrder(orderId) {
      const response = await apiClient.get(`/order/${orderId}`)
      this.currentOrder = response.data
    },
    async fetchProducts() {
      const response = await apiClient.get('/products')
      this.products = response.data
    },
    async addItemToOrder(orderItem) {
      if (!this.currentOrder?.id) return
      await apiClient.post(`/order/addItem/${this.currentOrder.id}`, orderItem)
    },
    async removeItemFromOrder(item) {
      if (!this.currentOrder?.id) return
      await apiClient.post(`/order/removeItem/${this.currentOrder.id}`, item.id)
    },
    async confirmOrder() {
      if (!this.currentOrder?.id) return
      await apiClient.post(`/order/confirmOrder/${this.currentOrder.id}`)
    },
    async confirmPayment() {
      if (!this.currentOrder?.id) return
      await apiClient.post(`/order/confirmPayment/${this.currentOrder.id}`)
    },
    async initializeHub(orderId) {
      const hubConnection = new signalR.HubConnectionBuilder().withUrl('/orderUpdateHub').build()
      hubConnection.on('ReceiveOrderUpdate', (order) => {
        this.fetchOrder(order.id)
      })
      await hubConnection.start()
      await hubConnection.invoke('SubscribeToOrder', orderId)
      this.signalRHubConnection = hubConnection
    }
  }
})
