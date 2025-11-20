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
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LibraryService, OwnerModel } from '../../services/library-service';
import { ShelfCardModel } from '../shelf-card/shelf-card';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';
import { EditIntroduction } from '../../dialogs/edit-introduction/edit-introduction';
import { ParentEditor } from '../../dialogs/parent-editor/parent-editor';
import { ConfirmDelete } from '../../dialogs/confirm-delete/confirm-delete';
import { Result } from '../../dialogs/result/result';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-shelf-page',
  imports: [DocumentsList, MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatIcon,
    MatCardAvatar, MatBadge, MatIconButton, MatButton, MatCardActions, RouterLink,
    NgOptimizedImage,MatProgressSpinner,MatMenuModule],
  templateUrl: './shelf-page.html',
  styleUrl: './shelf-page.css'
})
export class ShelfPage {
  shelfGuid = signal<string|null>(null);
  ownerGuid = signal<string|null>(null);

  signletonModes = inject(SingletonModes);
  dialog = inject(MatDialog);
  activatedRoute = inject(ActivatedRoute);
  librarySerice = inject(LibraryService);
  identityService = inject(IdentityService);
  router = inject(Router);
  
  shelfModel = signal<ShelfCardModel|null>(null);

  ownerModel = signal<OwnerModel|null>(null);
  isMyShelf = computed(() => this.identityService.isAuthenticated() && 
  this.shelfModel()?.ownerGuid === this.identityService.userModel()?.guid);

  displaySubmitSpinner = signal(false);

  introductionImageVersion = signal(0);
  introductionImage = computed(()=>this.librarySerice.getShelfImageAddress(this.shelfModel()));

  constructor(){
    this.activatedRoute.paramMap.subscribe(params=>{
      if(params.has("shelfGuid")){
        this.shelfGuid.set(params.get("shelfGuid"));
      }
      if(params.has("ownerGuid")){
        this.ownerGuid.set(params.get("ownerGuid"));
      }
    });
    
    effect(() => {
      if(this.shelfGuid()){
        this.librarySerice.requestShelfModel(this.shelfGuid()!).subscribe({
          next: res => {
            if(res){
              this.shelfModel.set(res);
            }
          },
        });
      }
    });
    

    effect(() => {
      if(this.ownerGuid()){
        this.librarySerice.requestOwnerModel(this.ownerGuid()!).subscribe({
          next: res => {
            if(res){
              this.ownerModel.set(res);
            }
          },
        });
      }
    });
  }

  editIntroduction(){
    if(this.isMyShelf()){
      this.dialog.open(EditIntroduction,{data:{
        introductionOf:"shelf", 
        title: this.shelfModel()!.title,
        description: this.shelfModel()!.description ?? "",
        imageSrc: this.shelfModel()!.hasImage ? this.introductionImage() : undefined,
        guid: this.shelfModel()!.guid
      }}).afterClosed().subscribe(result=>{
        if(result){
          if(result === "ImageDelete"){
            this.shelfModel.update(shm=>{
              shm!.hasImage = false;
              return shm;
            });
          }
          else{
            this.shelfModel.update(shm=>{
              shm!.title = result.title;
              shm!.description = result.description;
              shm!.hasImage = result.imageChanged ?? shm!.hasImage;
              return shm;
            });
            
            if(result.imageChanged){
              this.introductionImageVersion.update(v=>{return ++v;});
            }
          }
        }
      });
    }
  }

  editParentLibraries(){
    if(this.isMyShelf()){
      this.dialog.open(ParentEditor,{data:{
        parentOf:"shelf",
        parentLibraryGuids: this.shelfModel()?.libraries.map(value=>value.guid),
        //parentShelfGuids: this.documentPageService.documentPageModel()?.shelves.map(shelf=>shelf.guid),
        childGuid: this.shelfModel()?.guid,
      }}).afterClosed().subscribe(result=>{
        if(result){
          this.shelfModel.update(shelf=>{
            shelf!.libraries = result;
            return shelf;
          });
        }
      });
    }
  }

  deleteShelf(){
    if(this.isMyShelf()){
      this.dialog.open(ConfirmDelete,{
        data:{
          title: `if you click on 'Yes', shelf '${this.shelfModel()?.title}' will be deleted but its documents will remain available through other shelves!`,
          type: "Shelf",
        }
      }).afterClosed().subscribe(result=>{
        if(result === true){
          this.librarySerice.requestDeleteShelf(this.shelfGuid()!).subscribe({
            next: res => {
              if(res && res.success){
                this.router.navigate(['libraries', this.identityService.userModel()!.guid]);
              }
            },
            error: err => {
              this.dialog.open(Result,{
                //panelClass: "success-ResultStatus", 
                data:{
                  status: "warning",
                  title: "Error in shelf deletion",
                  description: ["Something went wrong during deleting the shelf!",
                    JSON.stringify(err)
                  ],
                }
              }).afterClosed().subscribe(()=>this.displaySubmitSpinner.set(false));
              throw(err);
            },
          });
        }
      });
    }
  }
  
}
