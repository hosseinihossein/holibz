import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  readonly httpClient = inject(HttpClient);

  requestNumberOfNotifications(){
    return this.httpClient.get<{numberOfNotifications:number}>("/api/Notification/GetNumberOfNotifications");
  }
  requestNotifications(pageIndex?:number, pageSize?:number){
    let httpParams = new HttpParams().set("pageIndex",)
    if()
    return this.httpClient.get<NotificationModel[]>("/api/Notification/GetNotifications");
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

export class NotificationModel {
  Guid:string = null!;
  Title:string = null!;
  Description:string[] = [];
  Link?:string|null = null;
  CreatedAt:Date = null!;
}

