import { Component, inject, input, OnInit, signal } from '@angular/core';
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { NgOptimizedImage } from "@angular/common";
import { Router } from '@angular/router';
import { LibraryService } from '../../services/library-service';

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
  libraryModel = input.required<LibraryModel>();

  router = inject(Router);
  libraryService = inject(LibraryService);

  openLibrary(){
    this.libraryService.currentLibraryModel.set(this.libraryModel());
    this.router.navigate(["/library"]);
  }
}

export class LibraryModel {
  guid?: string;
  title?: string;
  description?: string; 
  shelvesTitles?: string[];
  hasImage?:boolean;
  ownerUsername?: string;
  ownerGuid?: string;
  createdAt?:Date;
}
