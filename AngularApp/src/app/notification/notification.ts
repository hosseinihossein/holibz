import { DatePipe } from '@angular/common';
import { Component, effect, inject, signal, viewChild } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { NotificationService } from './notification-service';
import { RouterLink } from '@angular/router';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-notification',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions, 
    MatButton, DatePipe, RouterLink, MatPaginatorModule, MatProgressSpinner],
  templateUrl: './notification.html',
  styleUrl: './notification.css'
})
export class Notification {
  notifications = signal<NotificationModel[]>([]);
  totalNumberOfNotifs = signal<number>(0);

  notifService = inject(NotificationService);

  paginator = viewChild.required(MatPaginator);

  displaySubmitSpinner = signal(true);

  constructor(){
    this.notifService.requestNotifications().subscribe({
      next: res => {
        if(res){
          this.notifications.set(res);
        }
        this.displaySubmitSpinner.set(false);
      },
    });
    this.notifService.requestNumberOfNotifications().subscribe({
      next: res => {
        if(res){
          this.totalNumberOfNotifs.set(res.numberOfNotifications);
        }
      },
    });
  }

  deleteNotif(notifGuid:string){
    this.displaySubmitSpinner.set(true);
    this.notifService.requestDeleteNotification(notifGuid).subscribe({
      next: res => {
        if(res && res.success){
          this.notifications.update(notifs=>{
            let notifIndex = notifs.findIndex(n=>n.guid === notifGuid);
            notifs.splice(notifIndex, 1);
            return notifs;
          });
        }
        this.displaySubmitSpinner.set(false);
      },
    });
  }
  deleteAllNotifs(){
    this.displaySubmitSpinner.set(true);
    this.notifService.requestDeleteAllNotifications().subscribe({
      next: res => {
        if(res && res.success){
          this.notifications.set([]);
        }
        this.displaySubmitSpinner.set(false);
      },
    });
  }

  handlePageEvent(e: PageEvent) {
    //let length = e.length;
    let pageSize = e.pageSize;
    let pageIndex = e.pageIndex;

    this.displaySubmitSpinner.set(true);
    this.notifService.requestNotifications(pageIndex, pageSize).subscribe({
      next: res => {
        if(res){
          this.notifications.set(res);
        }
        this.displaySubmitSpinner.set(false);
      },
    });
  }

}

export class NotificationModel {
  guid:string = null!;
  title:string = null!;
  description:string[] = [];
  link?:string|null = null;
  createdAt:Date = null!;
}
