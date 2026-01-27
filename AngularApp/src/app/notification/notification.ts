import { DatePipe } from '@angular/common';
import { Component, effect, inject, signal, viewChild } from '@angular/core';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { NotificationService } from './notification-service';
import { Router, RouterLink } from '@angular/router';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatIcon } from '@angular/material/icon';
import { WaitSpinner } from '../shared/wait-spinner/wait-spinner';

@Component({
  selector: 'app-notification',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions, 
    MatButton, DatePipe, MatPaginatorModule, WaitSpinner, MatIcon],
  templateUrl: './notification.html',
  styleUrl: './notification.css'
})
export class Notification {
  notifications = signal<NotificationModel[]>([]);
  totalNumberOfNotifs = signal<number>(0);

  notifService = inject(NotificationService);
  router = inject(Router);

  paginator = viewChild.required(MatPaginator);

  displayWaitSpinner = signal(true);

  constructor(){
    this.notifService.requestNotifications().subscribe({
      next: res => {
        if(res){
          this.notifications.set(res);
        }
        this.displayWaitSpinner.set(false);
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
    this.displayWaitSpinner.set(true);
    this.notifService.requestDeleteNotification(notifGuid).subscribe({
      next: res => {
        if(res && res.success){
          this.notifications.update(notifs=>{
            let notifIndex = notifs.findIndex(n=>n.guid === notifGuid);
            notifs.splice(notifIndex, 1);
            return notifs;
          });
        }
        this.displayWaitSpinner.set(false);
      },
    });
  }
  deleteAllNotifs(){
    this.displayWaitSpinner.set(true);
    this.notifService.requestDeleteAllNotifications().subscribe({
      next: res => {
        if(res && res.success){
          this.notifications.set([]);
        }
        this.displayWaitSpinner.set(false);
      },
    });
  }

  handlePageEvent(e: PageEvent) {
    //let length = e.length;
    let pageSize = e.pageSize;
    let pageIndex = e.pageIndex;

    this.displayWaitSpinner.set(true);
    this.notifService.requestNotifications(pageIndex, pageSize).subscribe({
      next: res => {
        if(res){
          this.notifications.set(res);
        }
        this.displayWaitSpinner.set(false);
      },
    });
  }

  openNotif(link:string){
    this.router.navigateByUrl(link);
  }

}

export class NotificationModel {
  guid:string = null!;
  title:string = null!;
  description:string[] = [];
  link?:string|null = null;
  createdAt:Date = null!;
}
