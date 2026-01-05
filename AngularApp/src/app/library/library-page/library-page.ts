import { Component, computed, effect, inject, signal } from '@angular/core';
import { ShelvesList } from "../shelves-list/shelves-list";
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle, MatCardActions } from "@angular/material/card";
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LibraryService, OwnerModel } from '../../services/library-service';
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
import { MatMenuModule } from '@angular/material/menu';
import { GenericList } from '../generic-list/generic-list';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';

@Component({
  selector: 'app-library-page',
  imports: [MatCard, MatCardHeader, MatCardContent, MatCardTitle, MatCardAvatar,
    MatCardSubtitle, NgOptimizedImage, MatIcon, MatCardActions, RouterLink, MatButton, MatIconButton,
    MatMenuModule,GenericList,WaitSpinner],
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
  ownerModel = signal<OwnerModel|null>(null);
  
  displayWaitSpinner = signal(false);

  introductionImage = computed(()=>this.librarySerice.getLibraryImageAddress(this.libraryModel()));

  genericListItemGuids = signal<string[]>([]);

  constructor(){
    this.activatedRoute.paramMap.subscribe(params=>{
      if(params.has("libraryGuid")){
        this.libraryGuid.set(params.get("libraryGuid"));
      }
    });

    effect(() => {
      if(this.libraryGuid()){
        this.librarySerice.requestLibraryModel(this.libraryGuid()!).subscribe({
          next: res => {
            if(res){
              this.libraryModel.set(res);
            }
          },
        });
      }
    });
    
    effect(() => {
      if(this.libraryModel()){
        this.librarySerice.requestOwnerModel(this.libraryModel()!.ownerGuid).subscribe({
          next: res => {
            if(res){
              this.ownerModel.set(res);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.libraryGuid()){
        this.librarySerice.requestShelvesGuids(this.libraryGuid()!).subscribe({
          next: res => {
            if(res){
              this.genericListItemGuids.set(res);
            }
          }
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
              return new LibraryCardModel(lm!);
            });
          }
          else{
            this.libraryModel.update(lm=>{
              lm!.title = result.title;
              lm!.description = result.description;
              lm!.hasImage = result.hasImage;
              lm!.integrityVersion = result.integrityVersion;
              return new LibraryCardModel(lm!);
            });
          }
        }
      });
    }
  }

  deleteLibrary(){
    if(this.isMyLibrary()){
      this.dialog.open(ConfirmDelete,
        {data:{
          type:"Library",
          title:`if you click on 'Yes', library '${this.libraryModel()!.title}' will be deleted but its shelves will remain available through other libraries!`}
        }
      ).afterClosed().subscribe(result=>{
        if(result === true){
          this.displayWaitSpinner.set(true);
          this.librarySerice.requestDeleteLibrary(this.libraryGuid()!).subscribe({
            next: res => {
              if(res && res.success){
                this.displayWaitSpinner.set(false);
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
              }).afterClosed().subscribe(()=>this.displayWaitSpinner.set(false));
              throw(err);
            },
          });
        }
      });
    }
  }

}
