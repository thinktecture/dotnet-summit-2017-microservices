import {Injectable} from '@angular/core';
import {Http} from '@angular/http';
import 'rxjs/add/operator/map';
import {AuthenticatedHttpService} from './authenticatedHttpService';

@Injectable()
export class OrdersService {
  private _apiUrl = 'http://localhost:7777/api/orders';

  constructor(private _http: AuthenticatedHttpService) {
  }

  public getOrders() {
    return this._http.get(this._apiUrl)
      .map(result => result.json());
  }
}
