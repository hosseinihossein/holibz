import { Component, input } from '@angular/core';
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { NgOptimizedImage } from "@angular/common";

@Component({
  selector: 'app-library-card',
  imports: [MatCard, MatCardHeader, MatCardAvatar, MatCardTitle, MatCardSubtitle, MatCardContent,
    MatIcon, NgOptimizedImage],
  templateUrl: './library-card.html',
  styleUrl: './library-card.css'
})
export class LibraryCard {
  model = input<LibraryCardModel>();
}

export class LibraryCardModel {
  Guid: string = "";
  Title: string = "";
  Description: string|null = null; 
  ShelvesTitles: string[] = [];
  HasImage:boolean = false;
  OwnerUsername: string = "";
}
