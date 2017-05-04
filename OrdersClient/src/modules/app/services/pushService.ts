import {Injectable} from '@angular/core';
import {BehaviorSubject} from 'rxjs/BehaviorSubject';
import {SecurityService} from './securityService';

@Injectable()
export class PushService {
  private _hubConnection;
  private _connection;
  private _shippingsProxy;

  private baseUrl = 'http://localhost:7777/';

  public orderShipping: BehaviorSubject<string> = new BehaviorSubject(null);
  public orderCreated: BehaviorSubject<string> = new BehaviorSubject(null);

  constructor(private _securityService: SecurityService) {
    this._hubConnection = $.hubConnection;
  }

  public start(): void {
    if (this._connection || !this._securityService.accessToken) {
      return;
    }

    this._connection = this._hubConnection(this.baseUrl);
    this._connection.qs = { 'authorization': this._securityService.accessToken };
    this._shippingsProxy = this._connection.createHubProxy('ordersHub');

    this._shippingsProxy.on('orderCreated', () => {
      this.orderCreated.next(null);
    });

    this._shippingsProxy.on('shippingCreated', (orderId) => {
      this.orderShipping.next(orderId);
    });

    this._connection.start()
      .done(() => console.log('SignalR connection established.'))
      .fail(() => console.error('SignalR connection not established.'));
  }

  public stop(): void {
    if (this._connection) {
      this._connection.stop();
    }

    this._connection = undefined;
    this._shippingsProxy = undefined;
  }
}
