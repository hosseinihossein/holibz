import { Component, computed, effect, ElementRef, inject, input, signal, viewChild } from '@angular/core';
import { MatButtonModule, MatIconButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { SingletonModes } from '../../services/singleton-modes';
import { MatTooltip, MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { EditUserImage } from '../../dialogs/edit-user-image/edit-user-image';
import { EditInput } from '../../dialogs/edit-input/edit-input';
import { EditTextarea } from '../../dialogs/edit-textarea/edit-textarea';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { SendLinkToEmail } from '../../dialogs/send-link-to-email/send-link-to-email';
import { MatCheckbox, MatCheckboxModule } from '@angular/material/checkbox';
import { ConfirmChange } from '../../dialogs/confirm-change/confirm-change';
import { ChangePassword } from '../../dialogs/change-password/change-password';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
//import { LibrariesList } from "../../library/libraries-list/libraries-list";
import { MatBadgeModule } from '@angular/material/badge';
import { LibraryService, OwnerModel } from '../../services/library-service';
import { ReviewService } from '../../review/review-service';
import { BriefUsersList } from '../../dialogs/brief-users-list/brief-users-list';
import { GenericList, GenericListFilter } from '../../library/generic-list/generic-list';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';

@Component({
  selector: 'app-profile',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent,
    MatIcon, NgOptimizedImage, MatCheckboxModule, MatButtonModule, RouterLink, 
    MatIconButton, MatBadgeModule, MatTooltipModule, GenericList, MatCardActions,
    MatPaginatorModule,WaitSpinner],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile {
  ownerGuid = signal<string|null>(null);
  isMyProfile = computed(() => this.ownerGuid() === this.identityService.userModel()?.guid);

  singleton = inject(SingletonModes);
  identityService = inject(IdentityService);
  activatedRoute = inject(ActivatedRoute);
  router = inject(Router);
  libraryService = inject(LibraryService);
  reviewService = inject(ReviewService);
  dialog = inject(MatDialog);
  snackBar = inject(MatSnackBar);

  identity_OwnerModel = signal<UserProfileModel|null>(null);
  userImgSrc = computed(()=>this.singleton.getUserImageAddress(this.identity_OwnerModel()));
  username = computed(()=>this.identity_OwnerModel()?.username);
  description = computed(()=>this.identity_OwnerModel()?.description);
  email = computed(()=>this.identity_OwnerModel()?.email);
  displayEmailPublicly = computed(()=>this.identity_OwnerModel()?.displayEmailPublicly);

  library_OwnerModel = signal<UserProfileInfo|null>(null);
  userTotalLikes = signal<number>(0);

  genericListType = signal<"Library"|"Shelf"|"Document">("Library");
  genericListItemGuids = signal<string[]>([]);
  genericListItemType = signal<"Libraries"|"Shelves"|"Documents"|"FavoriteLibraries"|"FavoriteShelves"|
  "FavoriteDocuments">("Libraries");
  pageIndex = signal<number>(0);
  pageSize = signal<number>(10);
  totalNumberOfItems = signal<number>(0);
  genericListTags = signal<string[]>([]);
  filterInfo = signal<GenericListFilter>(new GenericListFilter());

  displayWaitSpinner = signal(false);

  constructor(){
    let userGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("userGuid");
    if(userGuidRouteParam){
      this.ownerGuid.set(userGuidRouteParam);
    }
    else if(this.identityService.isAuthenticated() && this.identityService.userModel()){
      this.ownerGuid.set(this.identityService.userModel()!.guid);
    }
    else{
      this.router.navigate(['/login'],{queryParams:{returnUrl:'/profile'}});
    }
    
    effect(()=>{
      if(this.isMyProfile()){
        this.identity_OwnerModel.set(this.identityService.userModel());
      }
      else{
        this.identityService.requestUserModel(this.ownerGuid()!).subscribe({
          next: res=>this.identity_OwnerModel.set(res),
        });
      }
    });

    effect(()=>{
      this.libraryService.requestUserProfileInfo(this.ownerGuid()!).subscribe({
        next: res => {
          if(res){
            this.library_OwnerModel.set(res);
          }
        },
      });
    });

    effect(()=>{
      this.reviewService.requestUserTotalLikes(this.ownerGuid()!).subscribe({
        next: res => {
          if(res){
            this.userTotalLikes.set(res.totalNumberOfLikes);
          }
        },
      });
    });

    effect(()=>{
      if(this.ownerGuid()){
        this.displayWaitSpinner.set(true);

        switch (this.genericListItemType()) {
          case "Libraries":
            this.libraryService.requestUserLibrariesGuids(this.ownerGuid()!,this.pageIndex(),
            this.pageSize(), this.filterInfo()).subscribe({
              next: res => {
                if(res){
                  this.genericListItemGuids.set(res);
                  this.genericListType.set("Library");
                }
              }
            });
            break;
          case "Shelves":
            this.libraryService.requestUserShelvesGuids(this.ownerGuid()!,this.pageIndex(),
            this.pageSize(), this.filterInfo()).subscribe({
              next: res => {
                if(res){
                  this.genericListItemGuids.set(res);
                  this.genericListType.set("Shelf");
                }
              }
            });
            break;
          case "Documents":
            this.libraryService.requestUserDocumentsGuids(this.ownerGuid()!,this.pageIndex(),
            this.pageSize(), this.filterInfo()).subscribe({
              next: res => {
                if(res){
                  this.genericListItemGuids.set(res);
                  this.genericListType.set("Document");
                }
              }
            });
            break;
          case "FavoriteLibraries":
            this.libraryService.requestFavoriteLibrariesGuids(this.ownerGuid()!,this.pageIndex(),
            this.pageSize(), this.filterInfo()).subscribe({
              next: res => {
                if(res){
                  this.genericListItemGuids.set(res);
                  this.genericListType.set("Library");
                }
              }
            });
            break;
          case "FavoriteShelves":
            this.libraryService.requestFavoriteShelvesGuids(this.ownerGuid()!,this.pageIndex(),
            this.pageSize(), this.filterInfo()).subscribe({
              next: res => {
                if(res){
                  this.genericListItemGuids.set(res);
                  this.genericListType.set("Shelf");
                }
              }
            });
            break;
          case "FavoriteDocuments":
            this.libraryService.requestFavoriteDocumentsGuids(this.ownerGuid()!,this.pageIndex(),
            this.pageSize(), this.filterInfo()).subscribe({
              next: res => {
                if(res){
                  this.genericListItemGuids.set(res);
                  this.genericListType.set("Document");
                }
              }
            });
            break;
        
          default:
            this.libraryService.requestUserLibrariesGuids(this.ownerGuid()!,this.pageIndex(),
            this.pageSize(), this.filterInfo()).subscribe({
              next: res => {
                if(res){
                  this.genericListItemGuids.set(res);
                  this.genericListType.set("Library");
                }
              }
            });
            break;
        }
        
        this.displayWaitSpinner.set(false);
      }
    });

    effect(()=>{
      if(this.ownerGuid()){
        
        const callBack = {
          next: (res:string[]) => {
            if(res && res.length > 0){
              this.genericListTags.set(res);
            }
          }
        };

        switch (this.genericListItemType()) {
          case "Libraries":
          case "Shelves":
          case "Documents":
            this.libraryService.requestUserTags(this.ownerGuid()!).subscribe(callBack);
            break;
          case "FavoriteLibraries":
            this.libraryService.requestUserFavoriteLibrariesTags(this.ownerGuid()!).subscribe(callBack);
            break;
          case "FavoriteShelves":
            this.libraryService.requestUserFavoriteShelvesTags(this.ownerGuid()!).subscribe(callBack);
            break;
          case "FavoriteDocuments":
            this.libraryService.requestUserFavoriteDocumentsTags(this.ownerGuid()!).subscribe(callBack);
            break;
        
          default:
            this.libraryService.requestUserTags(this.ownerGuid()!).subscribe(callBack);
            break;
        }
      }
    });

    effect(()=>{
      if(this.ownerGuid()){
        
        const callBack = {
          next: (res:{totalNumberOfItems:number}) => {
            if(res){
              this.totalNumberOfItems.set(res.totalNumberOfItems);
            }
          }
        };

        switch (this.genericListItemType()) {
          case "Libraries":
            this.libraryService.requestTotalNumberOfUserLibraries(this.ownerGuid()!,this.filterInfo()).subscribe(callBack);
            break;
          case "Shelves":
            this.libraryService.requestTotalNumberOfUserShelves(this.ownerGuid()!,this.filterInfo()).subscribe(callBack);
            break;
          case "Documents":
            this.libraryService.requestTotalNumberOfUserDocuments(this.ownerGuid()!,this.filterInfo()).subscribe(callBack);
            break;
          case "FavoriteLibraries":
            this.libraryService.requestTotalNumberOfUserFavoriteLibraries(this.ownerGuid()!,this.filterInfo()).subscribe(callBack);
            break;
          case "FavoriteShelves":
            this.libraryService.requestTotalNumberOfUserFavoriteShelves(this.ownerGuid()!,this.filterInfo()).subscribe(callBack);
            break;
          case "FavoriteDocuments":
            this.libraryService.requestTotalNumberOfUserFavoriteDocuments(this.ownerGuid()!,this.filterInfo()).subscribe(callBack);
            break;
        
          default:
            this.libraryService.requestTotalNumberOfUserLibraries(this.ownerGuid()!,this.filterInfo()).subscribe(callBack);
            break;
        }
      }
    });

  }

  displayFollowersList(){
    if(this.ownerGuid()){
      this.dialog.open(BriefUsersList,{data:{
        label: "Followers",
        subjectGuid: this.ownerGuid(),
        totalNumberOfItems: this.library_OwnerModel()?.numberOfFollowers,
        type: "Follower",
      }});
    }
  }
  displayFollowingsList(){
    if(this.ownerGuid()){
      this.dialog.open(BriefUsersList,{data:{
        label: "Followings",
        subjectGuid: this.ownerGuid(),
        totalNumberOfItems: this.library_OwnerModel()?.numberOfFollowings,
        type: "Following",
      }});
    }
  }

  displayLibraries(){
    this.genericListItemType.set("Libraries");
  }
  displayShelves(){
    this.genericListItemType.set("Shelves");
  }
  displayDocuments(){
    this.genericListItemType.set("Documents");
  }

  displayStatics(){}

  favoriteLibraries(){
    this.genericListItemType.set("FavoriteLibraries");
  }
  favoriteShelves(){
    this.genericListItemType.set("FavoriteShelves");
  }
  favoriteDocuments(){
    this.genericListItemType.set("FavoriteDocuments");
  }

  follow(){
    if(this.identityService.isAuthenticated() && !this.isMyProfile() && this.ownerGuid() && 
    this.library_OwnerModel() && !this.library_OwnerModel()!.iFollow){
      this.libraryService.requestToFollow(this.ownerGuid()!).subscribe({
        next: res => {
          if(res){
            this.library_OwnerModel()!.iFollow = true;
            this.snackBar.open("Followed Successfully", "Ok", { duration: 5000 });
          }
        },
      });
    }
  }
  unFollow(){
    if(this.identityService.isAuthenticated() && !this.isMyProfile() && this.ownerGuid() && 
    this.library_OwnerModel() && this.library_OwnerModel()!.iFollow){
      this.libraryService.requestToUnFollow(this.ownerGuid()!).subscribe({
        next: res => {
          if(res){
            this.library_OwnerModel()!.iFollow = false;
            this.snackBar.open("UnFollowed Successfully", "Ok", { duration: 5000 });
          }
        },
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

}

export class UserProfileInfo{
  //guid:string = null!;
  numberOfLibraries:number = 0;
  numberOfShelves:number = 0;
  numberOfDocuments:number = 0;
  numberOfFollowers:number = 0;
  numberOfFollowings:number = 0;
  numberOfFavoriteLibraries:number = 0;
  numberOfFavoriteShelves:number = 0;
  numberOfFavoriteDocuments:number = 0;
  iFollow:boolean = false;
}
