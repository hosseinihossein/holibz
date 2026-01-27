import { Component, computed, effect, inject, signal } from '@angular/core';
//import { ShelvesList } from "../shelves-list/shelves-list";
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
import { GenericList, GenericListFilter } from '../generic-list/generic-list';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { IconService } from '../../services/icon-service';
import { BriefUsersList } from '../../dialogs/brief-users-list/brief-users-list';

@Component({
  selector: 'app-library-page',
  imports: [MatCard, MatCardHeader, MatCardContent, MatCardTitle, MatCardAvatar,
    MatCardSubtitle, NgOptimizedImage, MatIcon, MatCardActions, RouterLink, MatButton, MatIconButton,
    MatMenuModule, GenericList, WaitSpinner, MatPaginatorModule, MatTooltip],
  templateUrl: './library-page.html',
  styleUrl: './library-page.css'
})
export class LibraryPage {
  libraryGuid = signal<string|null>(null);

  activatedRoute = inject(ActivatedRoute);
  libraryService = inject(LibraryService);
  identityService = inject(IdentityService);
  dialog = inject(MatDialog);
  router = inject(Router);
  iconService = inject(IconService);

  libraryModel = signal<LibraryCardModel|null>(null);
  isMyLibrary = computed(() => this.identityService.isAuthenticated() && 
  this.libraryModel()?.ownerGuid === this.identityService.userModel()?.guid);
  ownerModel = signal<OwnerModel|null>(null);
  
  displayWaitSpinner = signal(false);

  introductionImage = computed(()=>this.libraryService.getLibraryImageAddress(this.libraryModel()));

  genericListItemGuids = signal<string[]>([]);
  pageIndex = signal<number>(0);
  pageSize = signal<number>(10);
  totalNumberOfShelves = signal<number>(0);
  genericListTags = signal<string[]>([]);
  filterInfo = signal<GenericListFilter>(new GenericListFilter());
  isMyFavorite = signal(false);

  constructor(){
    this.activatedRoute.paramMap.subscribe(params=>{
      if(params.has("libraryGuid")){
        this.libraryGuid.set(params.get("libraryGuid"));
      }
    });

    effect(() => {
      if(this.libraryGuid()){
        this.libraryService.requestLibraryModel(this.libraryGuid()!).subscribe({
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
        this.libraryService.requestOwnerModel(this.libraryModel()!.ownerGuid).subscribe({
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
        this.libraryService.requestTotalNumberOfLibraryShelves(this.libraryGuid()!,this.filterInfo()).subscribe({
          next: res => {
            if(res){
              this.totalNumberOfShelves.set(res.totalNumberOfItems);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.libraryGuid()){
        this.displayWaitSpinner.set(true);
        this.libraryService.requestLibraryShelvesGuids(this.libraryGuid()!, this.pageIndex(), 
        this.pageSize(), this.filterInfo()).subscribe({
          next: res => {
            if(res){
              this.genericListItemGuids.set(res);
              this.displayWaitSpinner.set(false);
            }
          }
        });
      }
    });

    effect(()=>{
      if(this.libraryGuid()){
        this.libraryService.requestLibraryTags(this.libraryGuid()!).subscribe({
          next: res => {
            if(res && res.length > 0){
              this.genericListTags.set(res);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.identityService.isAuthenticated() && this.libraryGuid()){
        this.libraryService.isMyFavoriteLibrary(this.libraryGuid()!).subscribe({
          next: res => {
            if(res){
              this.isMyFavorite.set(res.isMyFavorite);
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
          this.libraryService.requestDeleteLibrary(this.libraryGuid()!).subscribe({
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

  handlePageEvent(e: PageEvent) {
    //let length = e.length;
    this.pageSize.set(e.pageSize);
    this.pageIndex.set(e.pageIndex);
  }

  onSubmitFilter(filter:GenericListFilter){
    this.filterInfo.set(filter);
  }

  toggleFavorite(){
    if(this.identityService.isAuthenticated() && this.libraryGuid() && this.libraryModel()){
      this.libraryService.requestToToggleFavoriteLibrary(this.libraryGuid()!).subscribe({
        next: res => {
          if(res && res.success){
            this.isMyFavorite.update(f=>!f);
          }
        },
      });
    }
  }

  displayUsersInFavor(){
    if(this.libraryGuid() && this.libraryModel()){
      this.dialog.open(BriefUsersList,{data:{
        label: "Users In Favor",
        subjectGuid: this.libraryGuid(),
        totalNumberOfItems: this.libraryModel()!.totalNumberOfUsersInFavor,
        type: "InFavorOfLibrary"
      }});
    }
  }

}
