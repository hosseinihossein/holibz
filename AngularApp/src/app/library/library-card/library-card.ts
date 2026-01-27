import { Component, computed, effect, inject, input, OnInit, signal } from '@angular/core';
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { NgOptimizedImage } from "@angular/common";
import { Router } from '@angular/router';
import { LibraryService, OwnerModel } from '../../services/library-service';
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
export class LibraryCard {
  //libraryModel = input.required<LibraryCardModel>();
  libraryGuid = input.required<string>();

  router = inject(Router);
  libraryService = inject(LibraryService);
  identityService = inject(IdentityService);

  libraryModel = signal<LibraryCardModel|null>(null);
  ownerModel = signal<OwnerModel|null>(null);
  libraryImageAddress = computed(()=>this.libraryService.getLibraryImageAddress(this.libraryModel()));

  constructor(){
    effect(()=>{
      if(this.libraryGuid()){
        this.libraryService.requestLibraryModel(this.libraryGuid()).subscribe({
          next: res => {
            if(res){
              this.libraryModel.set(res);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.libraryModel()){
        this.libraryService.requestOwnerModel(this.libraryModel()!.ownerGuid).subscribe({
          next: res => {
            if(res){
              this.ownerModel.set(res);
            }
          },
        });
      }
    });
  }

  openLibrary(){
    this.router.navigate(["/library", this.libraryGuid()]);
  }
}

export class LibraryCardModel {
  constructor(libraryCardModel:LibraryCardModel){
    this.guid = libraryCardModel.guid;
    this.title = libraryCardModel.title;
    this.description = libraryCardModel.description;
    this.shelvesTitles = libraryCardModel.shelvesTitles.map(s=>s);
    this.hasImage = libraryCardModel.hasImage;
    this.integrityVersion = libraryCardModel.integrityVersion;
    this.ownerGuid = libraryCardModel.ownerGuid;
    this.createdAt = libraryCardModel.createdAt;
    this.isDefault = libraryCardModel.isDefault;
    //this.isMyFavorite = libraryCardModel.isMyFavorite;
    this.totalNumberOfUsersInFavor = libraryCardModel.totalNumberOfUsersInFavor;
  }

  guid: string = null!;
  title: string = null!;
  description?: string; 
  shelvesTitles: string[] = [];
  hasImage:boolean = false;
  integrityVersion:number = 0;
  ownerGuid: string = null!;
  createdAt:Date = null!;
  isDefault:boolean = false;
  //isMyFavorite:boolean = false;
  totalNumberOfUsersInFavor:number = 0;
}
