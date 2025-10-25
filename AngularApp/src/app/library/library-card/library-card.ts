import { Component, inject, input, OnInit, signal } from '@angular/core';
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { NgOptimizedImage } from "@angular/common";
import { Router } from '@angular/router';
import { LibraryService } from '../../services/library-service';
import { UserProfileModel } from '../../services/identity-service';

@Component({
  selector: 'app-library-card',
  imports: [MatCard, MatCardHeader, MatCardAvatar, MatCardTitle, MatCardSubtitle, MatCardContent,
    MatIcon, NgOptimizedImage],
  templateUrl: './library-card.html',
  styleUrl: './library-card.css',
  host: {
    "(click)": "openLibrary()",
  }
})
export class LibraryCard {
  libraryModel = input.required<LibraryCardModel>();

  router = inject(Router);
  libraryService = inject(LibraryService);

  userModel = signal<UserProfileModel|null>(null);

  constructor(){
    this.userModel.set(this.libraryService.currentOwnerUserModel());
  }

  openLibrary(){
    this.libraryService.currentLibraryModel.set(this.libraryModel());
    this.router.navigate(["/library"]);
  }
}

export class LibraryCardModel {
  guid?: string;
  title?: string;
  description?: string; 
  shelvesTitles?: string[];
  hasImage?:boolean;
  //ownerUsername?: string;
  ownerGuid?: string;
  createdAt?:Date;
}
