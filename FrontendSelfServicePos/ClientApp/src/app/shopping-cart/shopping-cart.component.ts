import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../services/order.service';
import { ProductService } from '../services.product.service';
import { SignalRService } from '../services/signalr.service';
import { Product } from '../models/product.model';
import { CartItem } from '../models/cart-item.model';
import { Order } from '../models/order.model';

@Component({
  selector: 'app-shopping-cart',
  templateUrl: './shopping-cart.component.html',
  styleUrls: ['./shopping-cart.component.css']
})
export class ShoppingCartComponent implements OnInit {
  products: Product[] = [];
  cart: CartItem[] = [];
  order: Order | null = null;

  constructor(
    private productService: ProductService,
    private orderService: OrderService,
    private signalRService: SignalRService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.signalRService.startConnection();
    this.signalRService.addOrderUpdateListener(this.onOrderUpdate.bind(this));
  }

  loadProducts(): void {
    this.productService.getProducts().subscribe((products) => {
      this.products = products;
    });
  }

  addToCart(product: Product): void {
    const existingItem = this.cart.find((item) => item.product.id === product.id);
    if (existingItem) {
      existingItem.quantity++;
    } else {
      this.cart.push({ product, quantity: 1 });
    }
  }

  removeFromCart(product: Product): void {
    const index = this.cart.findIndex((item) => item.product.id === product.id);
    if (index !== -1) {
      this.cart.splice(index, 1);
    }
  }

  confirmOrder(): void {
    if (this.order) {
      this.orderService.confirmOrder(this.order.id).subscribe(() => {
        this.router.navigate(['/order-confirmation']);
      });
    }
  }

  onOrderUpdate(order: Order): void {
    if (this.order && this.order.id === order.id) {
      this.order = order;
    }
  }
}
