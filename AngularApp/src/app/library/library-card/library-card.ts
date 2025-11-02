import { Component, effect, inject, input, OnInit, signal } from '@angular/core';
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { NgOptimizedImage } from "@angular/common";
import { Router } from '@angular/router';
import { LibraryService } from '../../services/library-service';
import { IdentityService, UserProfileModel } from '../../services/identity-service';

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
export class LibraryCard implements OnInit {
  libraryModel = input.required<LibraryCardModel>();

  router = inject(Router);
  libraryService = inject(LibraryService);
  identityService = inject(IdentityService);

  userModel = signal<UserProfileModel|null>(null);

  constructor(){
    effect(()=>{
      if(this.libraryModel() && !this.userModel()){
        this.identityService.requestUserModel(this.libraryModel().ownerGuid).subscribe({
          next: res => {
            if(res){
              this.userModel.set(res);
            }
          },
        });
      }
    });
  }
  ngOnInit(): void {
    if(this.libraryModel().ownerGuid == this.libraryService.currentOwnerUserModel()?.guid){
      this.userModel.set(this.libraryService.currentOwnerUserModel());
    }
  }

  openLibrary(){
    this.libraryService.currentLibraryModel.set(this.libraryModel());
    this.router.navigate(["/library", this.libraryModel().guid]);
  }
}

export class LibraryCardModel {
  guid: string = null!;
  title: string = null!;
  description?: string; 
  shelvesTitles: string[] = [];
  hasImage:boolean = false;
  //ownerUsername?: string;
  ownerGuid: string = null!;
  createdAt:Date = null!;
}
