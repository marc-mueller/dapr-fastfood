import { Component, OnInit } from '@angular/core';
import { SignalRService } from './services/signalr.service';
import { OrderService } from './services/order.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'FrontendSelfServicePos';

  constructor(private signalRService: SignalRService, private orderService: OrderService) {}

  ngOnInit() {
    this.signalRService.startConnection();
    this.signalRService.addOrderUpdateListener();
  }
}
