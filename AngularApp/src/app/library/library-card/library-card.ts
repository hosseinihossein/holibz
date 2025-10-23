import { Component, input, OnInit } from '@angular/core';
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
  libraryCardModel = input.required<LibraryCardModel>();
}

export class LibraryCardModel {
  guid?: string;
  title?: string;
  description?: string; 
  shelvesTitles?: string[];
  hasImage?:boolean;
  ownerUsername?: string;
}
