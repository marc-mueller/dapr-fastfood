import axios from 'axios';

const apiClient = axios.create({
    baseURL: '/api', // Use relative URL; proxy will forward to the backend
    headers: {
        "Content-type": "application/json"
    }
});

export default apiClient;