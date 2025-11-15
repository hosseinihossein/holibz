import { Component, computed, effect, inject, signal } from '@angular/core';
import { ShelvesList } from "../shelves-list/shelves-list";
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle, MatCardActions } from "@angular/material/card";
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LibraryService } from '../../services/library-service';
import { LibraryCardModel } from '../library-card/library-card';
import { NgOptimizedImage } from '@angular/common';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { MatIcon } from '@angular/material/icon';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { EditIntroduction } from '../../dialogs/edit-introduction/edit-introduction';
import { MatTooltip } from '@angular/material/tooltip';
import { ConfirmDelete } from '../../dialogs/confirm-delete/confirm-delete';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { Result } from '../../dialogs/result/result';

@Component({
  selector: 'app-library-page',
  imports: [ShelvesList, MatCard, MatCardHeader, MatCardContent, MatCardTitle, MatCardAvatar,
    MatCardSubtitle, NgOptimizedImage, MatIcon, MatCardActions, RouterLink, MatButton, MatIconButton,
    MatTooltip,MatProgressSpinner],
  templateUrl: './library-page.html',
  styleUrl: './library-page.css'
})
export class LibraryPage {
  libraryGuid = signal<string|null>(null);

  activatedRoute = inject(ActivatedRoute);
  librarySerice = inject(LibraryService);
  identityService = inject(IdentityService);
  dialog = inject(MatDialog);
  router = inject(Router);

  libraryModel = signal<LibraryCardModel|null>(null);
  isMyLibrary = computed(() => this.identityService.isAuthenticated() && 
  this.libraryModel()?.ownerGuid === this.identityService.userModel()?.guid);
  userModel = signal<UserProfileModel|null>(null);
  
  displaySubmitSpinner = signal(false);

  introductionImageVersion = signal(0);
  introductionImage = computed(()=>`/api/Library/LibraryImage?libraryGuid=${this.libraryModel()!.guid}&v=${this.introductionImageVersion()}`);

  constructor(){
    let libraryGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("libraryGuid");
    if(libraryGuidRouteParam){
      this.libraryGuid.set(libraryGuidRouteParam);
    }
    if(this.librarySerice.currentLibraryModel()?.guid === this.libraryGuid()){
        this.libraryModel.set(this.librarySerice.currentLibraryModel());
        this.userModel.set(this.librarySerice.currentOwnerUserModel());
    }
    else{
      effect(() => {
        if(this.libraryGuid()){
          this.librarySerice.requestLibraryModel(this.libraryGuid()!).subscribe({
            next: res => {
              if(res){
                this.libraryModel.set(res);
                /*if(res.guid){
                  this.libraryGuid.set(res.guid);//not necessary
                }*/
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

  editIntroduction(){
    if(this.isMyLibrary()){
      this.dialog.open(EditIntroduction,{data:{
        introductionOf:"library", 
        title: this.libraryModel()!.title,
        description: this.libraryModel()!.description ?? "",
        imageSrc: this.libraryModel()?.hasImage ? this.introductionImage() : undefined,
        guid: this.libraryModel()!.guid
      }}).afterClosed().subscribe(result=>{
        if(result){
          if(result === "ImageDelete"){
            this.libraryModel.update(lm=>{
              lm!.hasImage = false;
              return lm;
            });
          }
          else{
            this.libraryModel.update(lm=>{
              lm!.title = result.title;
              lm!.description = result.description;
              lm!.hasImage = result.imageChanged ?? lm!.hasImage;
              return lm;
            });
            
            if(result.imageChanged){
              this.introductionImageVersion.update(v=>{return ++v;});
            }
          }
        }
      });
    }
  }

  deleteLibrary(){
    if(this.isMyLibrary()){
      this.dialog.open(ConfirmDelete,
        {data:{type:"Library",title:this.libraryModel()!.title}}
      ).afterClosed().subscribe(result=>{
        if(result === true){
          this.displaySubmitSpinner.set(true);
          this.librarySerice.requestDeleteLibrary(this.libraryGuid()!).subscribe({
            next: res => {
              if(res && res.success){
                this.displaySubmitSpinner.set(false);
                this.router.navigate(['libraries', this.identityService.userModel()!.guid]);
              }
            },
            error: err => {
              this.dialog.open(Result,{
                //panelClass: "success-ResultStatus", 
                data:{
                  status: "warning",
                  title: "Error in library deletion",
                  description: ["Something went wrong during deleting the library!",
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
