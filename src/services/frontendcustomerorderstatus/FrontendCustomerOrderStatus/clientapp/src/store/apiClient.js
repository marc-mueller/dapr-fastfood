import axios from 'axios';

const apiClient = axios.create({
    baseURL: '/api', // Update with actual backend URL if different
    headers: {
        'Content-Type': 'application/json',
    },
});

export default {
    async getOrder(orderId) {
        return apiClient.get(`/order/${orderId}`);
    },
};