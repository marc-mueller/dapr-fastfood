import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { AppComponent } from './app.component';
import { StartScreenComponent } from './start-screen/start-screen.component';
import { ShoppingCartComponent } from './shopping-cart/shopping-cart.component';
import { OrderConfirmationComponent } from './order-confirmation/order-confirmation.component';

import { OrderService } from './services/order.service';
import { ProductService } from './services/product.service';
import { SignalRService } from './services/signalr.service';

const appRoutes: Routes = [
  { path: '', component: StartScreenComponent },
  { path: 'shopping-cart', component: ShoppingCartComponent },
  { path: 'order-confirmation', component: OrderConfirmationComponent }
];

@NgModule({
  declarations: [
    AppComponent,
    StartScreenComponent,
    ShoppingCartComponent,
    OrderConfirmationComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    ReactiveFormsModule,
    RouterModule.forRoot(appRoutes)
  ],
  providers: [
    OrderService,
    ProductService,
    SignalRService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
