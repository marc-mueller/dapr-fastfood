import apiClient from './apiClient';
import * as signalR from "@microsoft/signalr";
import { createStore } from 'vuex';

const state = {
  currentOrder: null,
  products: [],
  signalRHubConnection: null
};

const getters = {
  totalOrderPrice(state) {
    if (!state.currentOrder || !state.currentOrder.items) return 0;
    return state.currentOrder.items.reduce((total, item) => {
      return total + item.itemPrice * item.quantity; // Ensure these property names are correct
    }, 0);
  }
};

const mutations = {
  setOrder(state, order) {
    console.log('Committing setOrder with:', order);
    state.currentOrder = order;
  },
  setProducts(state, products) {
    state.products = products;
  },
  // addToCart(state, orderItem) {
  //   if (!state.currentOrder.items) {
  //     state.currentOrder.items = [];
  //   }
  //   const item = state.currentOrder.items.find(i => i.productId === orderItem.productId);
  //   if (item) {
  //     item.quantity += orderItem.quantity;
  //   } else {
  //     state.currentOrder.items.push(orderItem);
  //   }
  // },
  // removeFromCart(state, orderItem) {
  //   if (!state.currentOrder.items) {
  //     state.currentOrder.items = [];
  //   }
  //   const index = state.currentOrder.items.findIndex(i => i.productId === orderItem.productId);
  //   if (index !== -1) {
  //     state.currentOrder.items.splice(index, 1);
  //   }
  // },
  initializeHubConnection(state, hubConnection) {
    state.signalRHubConnection = hubConnection;
  },
  updateOrder(state, order) {
    console.log('Committing updateOrder with:', order);
    state.currentOrder = order;
  }
};

const actions = {
  async createOrder({ dispatch }) {
    try {
      const response = await apiClient.post('/order/createOrder', {});
      const orderAcknowledgement = response.data;
      console.log('Order created with orderId:', orderAcknowledgement.orderId);

      await dispatch('fetchOrder', orderAcknowledgement.orderId);
      await dispatch('initializeHub', orderAcknowledgement.orderId);
    } catch (error) {
      console.error("Error creating order:", error);
    }
  },
  async fetchOrder({ commit }, orderId) {
    try {
      const response = await apiClient.get(`/order/${orderId}`);
      const order = response.data;
      console.log('Fetched order:', order);

      commit('setOrder', order);
    } catch (error) {
      console.error("Error fetching order:", error);
    }
  },
  async fetchProducts({ commit }) {
    try {
      const response = await apiClient.get('/products');
      commit('setProducts', response.data);
    } catch (error) {
      console.error("Error fetching products:", error);
    }
  },
  async addItemToOrder({ state }, orderItem) {
    if (!state.currentOrder || !state.currentOrder.id) {
      console.error("Order ID is not available.");
      return;
    }
    try {
      await apiClient.post(`/order/addItem/${state.currentOrder.id}`, orderItem);
    } catch (error) {
      console.error("Error adding item to order:", error);
    }
  },
  async removeItemFromOrder({ state}, item) {
    if (!state.currentOrder || !state.currentOrder.id) {
      console.error("Order ID is not available.");
      return;
    }
    try {
      await apiClient.post(`/order/removeItem/${state.currentOrder.id}`, item.id);
    } catch (error) {
      console.error("Error removing item from order:", error);
    }
  },
  async confirmOrder({ state }) {
    if (!state.currentOrder || !state.currentOrder.id) {
      console.error("Order ID is not available.");
      return;
    }
    try {
      await apiClient.post(`/order/confirmOrder/${state.currentOrder.id}`);
    } catch (error) {
      console.error("Error confirming order:", error);
    }
  },
  async confirmPayment({ state }) { 
    if (!state.currentOrder || !state.currentOrder.id) {
      console.error("Order ID is not available.");
      return;
    }
    try {
      await apiClient.post(`/order/confirmPayment/${state.currentOrder.id}`);
    } catch (error) {
      console.error("Error confirming payment:", error);
    }
  },
  async initializeHub({ commit/*, state*/ }, orderId) {
    try {
      const hubConnection = new signalR.HubConnectionBuilder()
          .withUrl('/orderUpdateHub')
          .build();

      hubConnection.on('ReceiveOrderUpdate', (order) => {
        // console.log('Order update received:', order);
        // commit('updateOrder', order);
        actions.fetchOrder({ commit }, order.id);
      });

      await hubConnection.start();
      console.log('SignalR connection established.');

      // Initialize the connection for the specific order ID
      hubConnection.invoke('SubscribeToOrder', orderId);

      commit('initializeHubConnection', hubConnection);
    } catch (error) {
      console.error("Error initializing SignalR hub connection:", error);
    }
  }
};

export default createStore({
  state,
  getters,
  mutations,
  actions
});