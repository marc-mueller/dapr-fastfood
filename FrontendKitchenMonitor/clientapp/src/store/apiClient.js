import axios from 'axios';

const apiClient = axios.create({
    baseURL: '/api', // Update with actual backend URL if different
    headers: {
        'Content-Type': 'application/json',
    },
});

export default {
    async getPendingOrders() {
        return apiClient.get('/kitchenwork/pendingorders');
    },
    async getPendingOrder(orderId) {
        return apiClient.get(`/kitchenwork/pendingorder/${orderId}`);
    },
    async finishOrderItem(itemId) {
        return apiClient.post(`/kitchenwork/itemfinished/${itemId}`);
    },
};