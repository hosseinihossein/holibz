import { DatePipe } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { NotificationModel, NotificationService } from './notification-service';

@Component({
  selector: 'app-notification',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions, 
    MatButton, DatePipe],
  templateUrl: './notification.html',
  styleUrl: './notification.css'
})
export class Notification {
  notifs = signal<NotificationModel[]>([]);

  notifService = inject(NotificationService);

  constructor(){
    effect(()=>{
      this.notifService.requestNotifications().subscribe({
        next: res => {
          if(res){
            this.notifs.set(res);
          }
        },
      });
    });
  }
}
