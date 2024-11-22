import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../services/order.service';

@Component({
  selector: 'app-start-screen',
  templateUrl: './start-screen.component.html',
  styleUrls: ['./start-screen.component.css']
})
export class StartScreenComponent {

  constructor(private router: Router, private orderService: OrderService) { }

  startOrdering() {
    this.orderService.createOrder().subscribe(order => {
      this.router.navigate(['/shopping-cart'], { queryParams: { orderId: order.id } });
    });
  }
}
