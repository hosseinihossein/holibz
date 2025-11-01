import { Component, computed, effect, inject, signal } from '@angular/core';
import { DocumentsList } from "../documents-list/documents-list";
import { MatCard, MatCardActions, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { SingletonModes } from '../../services/singleton-modes';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatTooltip } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { EditInput } from '../../dialogs/edit-input/edit-input';
import { EditTextarea } from '../../dialogs/edit-textarea/edit-textarea';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LibraryService } from '../../services/library-service';
import { ShelfCardModel } from '../shelf-card/shelf-card';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-shelf-page',
  imports: [DocumentsList, MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatIcon,
    MatCardAvatar, MatBadge, MatIconButton, MatTooltip, MatButton, MatCardActions, RouterLink,
    NgOptimizedImage],
  templateUrl: './shelf-page.html',
  styleUrl: './shelf-page.css'
})
export class ShelfPage {
  shelfGuid = signal<string|null>(null);

  signletonModes = inject(SingletonModes);
  dialog = inject(MatDialog);
  activatedRoute = inject(ActivatedRoute);
  librarySerice = inject(LibraryService);
  identityService = inject(IdentityService);
  
  shelfModel = signal<ShelfCardModel|null>(null);

  userModel = signal<UserProfileModel|null>(null);
  isMyShelf = computed(() => this.identityService.isAuthenticated() && 
  this.shelfModel()?.ownerGuid === this.identityService.userModel()?.guid);

  constructor(){
    let libraryGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("shelfGuid");
    if(libraryGuidRouteParam){
      this.shelfGuid.set(libraryGuidRouteParam);
    }
    if(this.librarySerice.currentShelfModel()?.guid === this.shelfGuid()){
        this.shelfModel.set(this.librarySerice.currentShelfModel());
        this.userModel.set(this.librarySerice.currentOwnerUserModel());
    }
    else{
      effect(() => {
        if(this.shelfGuid()){
          this.librarySerice.requestShelfModel(this.shelfGuid()!).subscribe({
            next: res => {
              if(res){
                this.shelfModel.set(res);
                if(res.guid){
                  this.shelfGuid.set(res.guid);//not necessary
                }
              }
            },
          });
        }
      });
    }

    effect(() => {
      if(!this.userModel() && this.shelfModel()){
        this.identityService.requestUserModel(this.shelfModel()?.ownerGuid!).subscribe({
          next: res => {
            if(res){
              this.userModel.set(res);
            }
          },
        });
      }
    });
  }

  openEditTitleDialog(){
    const dialogRef = this.dialog.open(EditInput,{data:{label: 'Edit Shelf Title', value: this.shelfModel()?.title}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        //this.shelfTitle.set(result);
      }
    });
  }
  
  openEditDescriptionDialog(){
    const dialogRef = this.dialog.open(EditTextarea,{data:{label: 'Edit Shelf Description', value: this.shelfModel()?.description}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        //this.shelfDescription.set(result);
      }
    });
  }
}
