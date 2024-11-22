import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/orderupdatehub`)
      .build();
  }

  startConnection(): void {
    this.hubConnection
      .start()
      .then(() => console.log('SignalR connection started'))
      .catch(err => console.log('Error while starting SignalR connection: ' + err));
  }

  addOrderUpdateListener(orderId: string, callback: (order: any) => void): void {
    this.hubConnection.on(`ReceiveOrderUpdate_${orderId}`, callback);
  }

  removeOrderUpdateListener(orderId: string): void {
    this.hubConnection.off(`ReceiveOrderUpdate_${orderId}`);
  }

  subscribeToOrder(orderId: string): void {
    this.hubConnection.invoke('SubscribeToOrder', orderId)
      .catch(err => console.error('Error while subscribing to order: ' + err));
  }

  unsubscribeFromOrder(orderId: string): void {
    this.hubConnection.invoke('UnsubscribeFromOrder', orderId)
      .catch(err => console.error('Error while unsubscribing from order: ' + err));
  }
}
