import { Component, input } from '@angular/core';
import { MatIconButton } from '@angular/material/button';
import { MatMenu, MatMenuTrigger } from '@angular/material/menu';

@Component({
  selector: 'app-account-dropdown',
  imports: [MatIconButton, MatMenuTrigger],
  templateUrl: './account-dropdown.html',
  styleUrl: './account-dropdown.css'
})
export class AccountDropdown {
  menu = input.required<MatMenu>();
  imgSrc = input("/defaultProfile.jpg");
}
