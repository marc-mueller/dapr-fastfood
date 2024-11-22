import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../services/order.service';
import { SignalRService } from '../services/signalr.service';
import { Order } from '../models/order.model';

@Component({
  selector: 'app-order-confirmation',
  templateUrl: './order-confirmation.component.html',
  styleUrls: ['./order-confirmation.component.css']
})
export class OrderConfirmationComponent implements OnInit {
  order: Order | null = null;

  constructor(
    private orderService: OrderService,
    private signalRService: SignalRService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.order = this.orderService.getCurrentOrder();
    this.signalRService.startConnection();
    this.signalRService.addOrderUpdateListener(this.onOrderUpdate.bind(this));
  }

  onOrderUpdate(order: Order): void {
    this.order = order;
    if (order.status === 'Paid') {
      setTimeout(() => {
        this.router.navigate(['/']);
      }, 3000);
    }
  }

  confirmPayment(): void {
    if (this.order) {
      this.orderService.confirmPayment(this.order.id).subscribe();
    }
  }
}
