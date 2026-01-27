import { Component, DOCUMENT, effect, ElementRef, inject, OnDestroy, signal, viewChild } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { MatToolbar } from '@angular/material/toolbar';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatActionList } from '@angular/material/list';
import { MatMenu, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { AccountDropdown } from "./account-dropdown/account-dropdown";
import { MatTooltip, MatTooltipModule } from '@angular/material/tooltip';
import { DropdownButton } from '../shared/dropdown-button/dropdown-button';
import { WindowService } from '../services/window-service';
import { SingletonModes } from '../services/singleton-modes';
import { RouterLink } from '@angular/router';
import { IdentityService } from '../services/identity-service';
import { NgOptimizedImage } from '@angular/common';
import { NotificationService } from '../notification/notification-service';
import { MatBadge } from '@angular/material/badge';

@Component({
  selector: 'app-nav-bar',
  imports: [MatIcon, MatToolbar, MatButton, MatIconButton, MatActionList, MatMenu, MatMenuTrigger, MatMenuItem,
    DropdownButton, AccountDropdown, MatTooltip, RouterLink,NgOptimizedImage, MatBadge],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.css'
})
export class NavBar implements OnDestroy {
  windowService = inject(WindowService);
  //document = inject(DOCUMENT);
  singletonModes = inject(SingletonModes);
  identityService = inject(IdentityService);
  notifService = inject(NotificationService);

  numberOfNotifications = signal<number>(0);
  notificationInterval = signal<number>(0);
  
  displayShadow = signal(false);

  constructor(){
    effect(()=>{
      if(this.identityService.isAuthenticated()){
        this.notificationInterval.set(setInterval(() => {
          this.notifService.requestNumberOfNotifications().subscribe({
            next: res => {
              if(res){
                this.numberOfNotifications.set(res.numberOfNotifications);
              }
            }
          });
        }, 10_000));//every 10 seconds
      }
      else{
        clearInterval(this.notificationInterval());
        this.numberOfNotifications.set(0);
      }
    });

    this.windowService.nativeWindow.addEventListener("scroll", ()=>{
      if(this.windowService.nativeWindow.scrollY >= 5){
        this.displayShadow.set(true);
      }
      else{
        this.displayShadow.set(false);
      }
    });
  }
  
  ngOnDestroy(): void {
    clearInterval(this.notificationInterval());
  }

}
