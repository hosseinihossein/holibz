import { Component, computed, effect, inject, signal } from '@angular/core';
//import { DocumentsList } from "../documents-list/documents-list";
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
import { GenericList, GenericListFilter } from '../generic-list/generic-list';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { IconService } from '../../services/icon-service';
import { BriefUsersList } from '../../dialogs/brief-users-list/brief-users-list';

@Component({
  selector: 'app-shelf-page',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatIcon,
    MatCardAvatar, MatBadge, MatIconButton, MatButton, MatCardActions, RouterLink,MatTooltip,
    NgOptimizedImage,WaitSpinner,MatMenuModule,GenericList,MatPaginatorModule],
  templateUrl: './shelf-page.html',
  styleUrl: './shelf-page.css'
})
export class ShelfPage {
  shelfGuid = signal<string|null>(null);

  signletonModes = inject(SingletonModes);
  dialog = inject(MatDialog);
  activatedRoute = inject(ActivatedRoute);
  libraryService = inject(LibraryService);
  identityService = inject(IdentityService);
  router = inject(Router);
  iconService = inject(IconService);
  
  shelfModel = signal<ShelfCardModel|null>(null);

  ownerModel = signal<OwnerModel|null>(null);
  isMyShelf = computed(() => this.identityService.isAuthenticated() && 
  this.shelfModel()?.ownerGuid === this.identityService.userModel()?.guid);

  displayWaitSpinner = signal(false);

  introductionImage = computed(()=>this.libraryService.getShelfImageAddress(this.shelfModel()));

  genericListItemGuids = signal<string[]>([]);
  pageIndex = signal<number>(0);
  pageSize = signal<number>(10);
  totalNumberOfDocuments = signal<number>(0);
  genericListTags = signal<string[]>([]);
  filterInfo = signal<GenericListFilter>(new GenericListFilter());
  isMyFavorite = signal(false);

  constructor(){
    this.activatedRoute.paramMap.subscribe(params=>{
      if(params.has("shelfGuid")){
        this.shelfGuid.set(params.get("shelfGuid"));
      }
    });
    
    effect(() => {
      if(this.shelfGuid()){
        this.libraryService.requestShelfModel(this.shelfGuid()!).subscribe({
          next: res => {
            if(res){
              this.shelfModel.set(res);
            }
          },
        });
      }
    });

    effect(() => {
      if(this.shelfModel()){
        this.libraryService.requestOwnerModel(this.shelfModel()!.ownerGuid).subscribe({
          next: res => {
            if(res){
              this.ownerModel.set(res);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.shelfGuid()){
        this.libraryService.requestTotalNumberOfShelfDocuments(this.shelfGuid()!,this.filterInfo()).subscribe({
          next: res => {
            if(res){
              this.totalNumberOfDocuments.set(res.totalNumberOfItems);
            }
          }
        });
      }
    });

    effect(()=>{
      if(this.shelfGuid()){
        this.displayWaitSpinner.set(true);
        this.libraryService.requestShelfDocumentsGuids(this.shelfGuid()!,this.pageIndex(), 
        this.pageSize(), this.filterInfo()).subscribe({
          next: res => {
            if(res){
              this.genericListItemGuids.set(res);
              this.displayWaitSpinner.set(false);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.shelfGuid()){
        this.libraryService.requestShelfTags(this.shelfGuid()!).subscribe({
          next: res => {
            if(res && res.length > 0){
              this.genericListTags.set(res);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.identityService.isAuthenticated() && this.shelfGuid()){
        this.libraryService.isMyFavoriteShelf(this.shelfGuid()!).subscribe({
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
              return new ShelfCardModel(shm!);
            });
          }
          else{
            this.shelfModel.update(shm=>{
              shm!.title = result.title;
              shm!.description = result.description;
              shm!.hasImage = result.hasImage;
              shm!.integrityVersion = result.integrityVersion;
              return new ShelfCardModel(shm!);
            });
          }
        }
      });
    }
  }

  editParentLibraries(){
    if(this.isMyShelf()){
      this.dialog.open(ParentEditor,{data:{
        parentOf:"shelf",
        parentLibraryGuids: this.shelfModel()?.librariesGuids,
        //parentShelfGuids: this.documentPageService.documentPageModel()?.shelves.map(shelf=>shelf.guid),
        childGuid: this.shelfModel()?.guid,
      }}).afterClosed().subscribe((result:string[])=>{
        if(result){
          this.shelfModel.update(shelf=>{
            shelf!.librariesGuids = result;
            return new ShelfCardModel(shelf!);
          });
        }
      });
    }
  }
  removeDocumentFromThisShelf(documentGuid:string){
    if(this.shelfGuid()){
      this.displayWaitSpinner.set(true);
      this.libraryService.removeDoumentFromParentShelf(documentGuid, this.shelfGuid()!).subscribe({
        next: res => {
          if(res && res.success){
            this.genericListItemGuids.update(docsGuids=>{
              let index = docsGuids.indexOf(documentGuid);
              docsGuids.splice(index, 1);
              return docsGuids;
            });
          }
          this.displayWaitSpinner.set(false);
        },
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
          this.libraryService.requestDeleteShelf(this.shelfGuid()!).subscribe({
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
    if(this.identityService.isAuthenticated() && this.shelfGuid() && this.shelfModel()){
      this.libraryService.requestToToggleFavoriteShelf(this.shelfGuid()!).subscribe({
        next: res => {
          if(res && res.success){
            this.isMyFavorite.update(f=>!f);
          }
        },
      });
    }
  }

  displayUsersInFavor(){
    if(this.shelfGuid() && this.shelfModel()){
      this.dialog.open(BriefUsersList,{data:{
        label: "Users In Favor",
        subjectGuid: this.shelfGuid(),
        totalNumberOfItems: this.shelfModel()!.totalNumberOfUsersInFavor,
        type: "InFavorOfShelf"
      }});
    }
  }
  
}
