import { createStore } from 'vuex';
import apiClient from './apiClient';
import * as signalR from '@microsoft/signalr';

export default createStore({
    state: {
        ordersInPreparation: [],
        ordersFinished: [],
        signalRHubConnection: null,
    },
    getters: {
        ordersInPreparation: (state) => state.ordersInPreparation,
        ordersFinished: (state) => state.ordersFinished,
    },
    mutations: {
        updateOrder(state, updatedOrder) {
            // Remove from both collections first
            state.ordersInPreparation = state.ordersInPreparation.filter(order => order.id !== updatedOrder.id);
            state.ordersFinished = state.ordersFinished.filter(order => order.id !== updatedOrder.id);

            if (updatedOrder.type !== 'Inhouse') {
                return;
            }

            // Add to the appropriate collection based on status
            switch(updatedOrder.state) {
                case 'Paid':
                case 'Processing':
                    state.ordersInPreparation.push(updatedOrder);
                    break;
                case 'Closed':
                case 'Prepared':
                    state.ordersFinished.push(updatedOrder);
                    // Keep only the latest 10 finished orders
                    if (state.ordersFinished.length > 10) {
                        state.ordersFinished.shift();
                    }
                    break;
                default:
                    break;
            }
        },
        removeOrder(state, orderId) {
            state.ordersInPreparation = state.ordersInPreparation.filter(order => order.id !== orderId);
            state.ordersFinished = state.ordersFinished.filter(order => order.id !== orderId);
        },
        initializeHubConnection(state, connection) {
            state.signalRHubConnection = connection;
        }
    },
    actions: {
        async fetchOrderAndUpdateStore({ commit }, orderId) {
            try {
                const response = await apiClient.getOrder(orderId);
                commit('updateOrder', response.data);
            } catch (error) {
                if (error.response && error.response.status === 404) {
                    commit('removeOrder', orderId);
                } else {
                    console.error('Error fetching pending order:', error);
                }
            }
        },
        initializeSignalRHub({ commit, dispatch }) {
            const connection = new signalR.HubConnectionBuilder()
                .withUrl('/orderupdatehub')
                .build();

            connection.on('ReceiveOrderUpdate', (order) => {
                dispatch('fetchOrderAndUpdateStore', order.id);
            });

            connection.start()
                .then(async () => {
                    commit('initializeHubConnection', connection);
                    await connection.invoke('SubscribeToOrderUpdates');
                })
                .catch((err) => console.error('Error establishing SignalR connection:', err));
        },
    },
});