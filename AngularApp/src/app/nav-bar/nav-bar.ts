import { Component, DOCUMENT, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { MatToolbar } from '@angular/material/toolbar';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatActionList } from '@angular/material/list';
import { MatMenu, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { AccountDropdown } from "../account-dropdown/account-dropdown";
import { MatTooltip } from '@angular/material/tooltip';
import { DropdownButton } from '../dropdown-button/dropdown-button';
import { WindowService } from '../services/window-service';
import { SingletonModes } from '../services/singleton-modes';
import { RouterLink } from '@angular/router';
import { IdentityService } from '../services/identity-service';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-nav-bar',
  imports: [MatIcon, MatToolbar, MatButton, MatIconButton, MatActionList, MatMenu, MatMenuTrigger, MatMenuItem,
    DropdownButton, AccountDropdown, MatTooltip, RouterLink,NgOptimizedImage],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.css'
})
export class NavBar {
  windowService = inject(WindowService);
  document = inject(DOCUMENT);
  singletonModes = inject(SingletonModes);
  identityService = inject(IdentityService);
  
  displayShadow = signal(false);

  constructor(){
    this.windowService.nativeWindow.addEventListener("scroll", ()=>{
      if(this.windowService.nativeWindow.scrollY >= 5){
        this.displayShadow.set(true);
      }
      else{
        this.displayShadow.set(false);
      }
    });
  }

}
