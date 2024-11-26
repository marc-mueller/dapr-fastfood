import { createStore } from 'vuex';
import apiClient from './apiClient';
import * as signalR from '@microsoft/signalr';

export default createStore({
    state: {
        pendingOrders: [],
        signalRHubConnection: null,
    },
    getters: {
        pendingOrders: (state) => state.pendingOrders,
    },
    mutations: {
        setPendingOrders(state, orders) {
            state.pendingOrders = orders;
        },
        updateOrder(state, updatedOrder) {
            const index = state.pendingOrders.findIndex((order) => order.id === updatedOrder.id);
            if (index !== -1) {
                state.pendingOrders[index] = updatedOrder;
            } else {
                state.pendingOrders.push(updatedOrder);
            }
        },
        removeOrder(state, orderId) {
            state.pendingOrders = state.pendingOrders.filter(order => order.id !== orderId);
        },
        initializeHubConnection(state, hubConnection) {
            state.signalRHubConnection = hubConnection;
        },
    },
    actions: {
        async fetchPendingOrders({ commit }) {
            try {
                const response = await apiClient.getPendingOrders();
                commit('setPendingOrders', response.data);
            } catch (error) {
                console.error('Error fetching pending orders:', error);
            }
        },
        async fetchOrderAndUpdateStore({ commit }, orderId) {
            try {
                const response = await apiClient.getPendingOrder(orderId);
                commit('updateOrder', response.data);
            } catch (error) {
                if (error.response && error.response.status === 404) {
                    commit('removeOrder', orderId);
                } else {
                    console.error('Error fetching pending order:', error);
                }
            }
        },
        async finishOrderItem(_, itemId) {
            try {
                await apiClient.finishOrderItem(itemId);
            } catch (error) {
                console.error('Error finishing order item:', error);
            }
        },
        initializeSignalRHub({ commit, dispatch }) {
            const connection = new signalR.HubConnectionBuilder()
                .withUrl('/kitchenorderupdatehub')
                .build();

            connection.on('kitchenorderupdated', (orderId) => {
                dispatch('fetchOrderAndUpdateStore', orderId);
            });
            
            connection.start()
                .then(async () => {
                    commit('initializeHubConnection', connection);
                    await connection.invoke('SubscribeToWork');
                })
                .catch((err) => console.error('Error establishing SignalR connection:', err));
        },
    },
});