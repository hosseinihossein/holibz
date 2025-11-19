import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatMenu, MatMenuTrigger } from '@angular/material/menu';
import { IdentityService } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';
import { SingletonModes } from '../../services/singleton-modes';

@Component({
  selector: 'app-account-dropdown',
  imports: [MatIconButton, MatMenuTrigger, MatIcon, NgOptimizedImage],
  templateUrl: './account-dropdown.html',
  styleUrl: './account-dropdown.css'
})
export class AccountDropdown {
  menu = input.required<MatMenu>();
  identityService = inject(IdentityService);
  singleton = inject(SingletonModes);

  imgSrc = computed(() => this.singleton.getUserImageAddress(this.identityService.userModel()));
    
  readonly imgBtn = "padding: 0px; width: 50px; height: 50px; transform: translateY(3px);"

}
