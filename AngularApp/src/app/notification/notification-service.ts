import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { NotificationModel } from './notification';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly httpClient = inject(HttpClient);

  requestNumberOfNotifications(){
    return this.httpClient.get<{numberOfNotifications:number}>(
      "/api/Notification/GetNumberOfNotifications"
    );
  }
  requestNotifications(pageIndex?:number, pageSize?:number){
    let httpParams = new HttpParams();
    if(pageIndex){
      httpParams = httpParams.set("pageIndex",pageIndex);
    }
    if(pageSize){
      httpParams = httpParams.set("pageSize",pageSize);
    }
    return this.httpClient.get<NotificationModel[]>(
      "/api/Notification/GetNotifications", {params: httpParams}
    );
  }
  requestDeleteNotification(notifGuid:string){
    let httpParams = new HttpParams().set("notifGuid", notifGuid);
    return this.httpClient.delete<{success:boolean}>(
      "/api/Notification/DeleteNotification", {params:httpParams}
    );
  }
  requestDeleteAllNotifications(){
    return this.httpClient.delete<{success:boolean}>(
      "/api/Notification/DeleteAllNotifications"
    );
  }
}



