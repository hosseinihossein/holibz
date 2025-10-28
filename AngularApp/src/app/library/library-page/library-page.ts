import { Component, computed, effect, inject, signal } from '@angular/core';
import { ShelvesList } from "../shelves-list/shelves-list";
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle, MatCardActions } from "@angular/material/card";
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LibraryService } from '../../services/library-service';
import { LibraryCardModel } from '../library-card/library-card';
import { NgOptimizedImage } from '@angular/common';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { MatIcon } from '@angular/material/icon';
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'app-library-page',
  imports: [ShelvesList, MatCard, MatCardHeader, MatCardContent, MatCardTitle, MatCardAvatar,
    MatCardSubtitle, NgOptimizedImage, MatIcon, MatCardActions, RouterLink, MatButton],
  templateUrl: './library-page.html',
  styleUrl: './library-page.css'
})
export class LibraryPage {
  libraryGuid = signal<string|null>(null);

  activatedRoute = inject(ActivatedRoute);
  librarySerice = inject(LibraryService);
  identityService = inject(IdentityService);
  libraryModel = signal<LibraryCardModel|null>(null);
  isMyLibrary = computed(() => this.identityService.isAuthenticated() && 
  this.libraryModel()?.ownerGuid === this.identityService.userModel()?.guid);
  userModel = signal<UserProfileModel|null>(null);

  constructor(){
    let libraryGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("libraryGuid");
    if(libraryGuidRouteParam){
      this.libraryGuid.set(libraryGuidRouteParam);
    }
    if(!this.libraryGuid()){
      if(this.librarySerice.currentLibraryModel()){
        this.libraryModel.set(this.librarySerice.currentLibraryModel());
        //this.librarySerice.currentLibraryModel.set(null);

        this.userModel.set(this.librarySerice.currentOwnerUserModel());
        //this.librarySerice.currentOwnerUserModel.set(null);
      }
    }
    else{
      effect(() => {
        if(this.libraryGuid()){
          this.librarySerice.requestLibraryModel(this.libraryGuid()!).subscribe({
            next: res => {
              if(res){
                this.libraryModel.set(res);
                if(res.guid){
                  this.libraryGuid.set(res.guid);//not necessary
                }
              }
            },
          });
        }
      });
    }

    effect(() => {
      if(!this.userModel() && this.libraryModel()){
        this.identityService.requestUserModel(this.libraryModel()?.ownerGuid!).subscribe({
          next: res => {
            if(res){
              this.userModel.set(res);
            }
          },
        });
      }
    });
  }
}
