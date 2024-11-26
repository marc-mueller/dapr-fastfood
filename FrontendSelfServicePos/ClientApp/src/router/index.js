import { createRouter, createWebHistory } from 'vue-router';
import StartScreen from '../components/StartScreen.vue';
import ProductList from '../components/ProductList.vue';
import OrderConfirmation from '../components/OrderConfirmation.vue';

const routes = [
    { path: '/', component: StartScreen },
    { path: '/products', component: ProductList },
    { path: '/order-confirmation', component: OrderConfirmation }
];

const router = createRouter({
    history: createWebHistory(),
    routes
});

export default router;