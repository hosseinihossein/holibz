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
import { LibrariesList } from "../../library/libraries-list/libraries-list";
import { MatBadgeModule } from '@angular/material/badge';
import { LibraryService, OwnerModel } from '../../services/library-service';
import { ReviewService } from '../../review/review-service';
import { BriefUsersList } from '../../dialogs/brief-users-list/brief-users-list';
import { GenericList } from '../../library/generic-list/generic-list';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-profile',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent,
    MatIcon, NgOptimizedImage, MatCheckboxModule, MatButtonModule, RouterLink, 
    MatIconButton, MatBadgeModule, MatTooltipModule, GenericList, MatCardActions],
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
      //console.log(JSON.stringify(this.identity_UserModel()));
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
      switch (this.genericListItemType()) {
        case "Libraries":
          this.libraryService.requestLibrariesGuids(this.ownerGuid()!).subscribe({
            next: res => {
              if(res){
                this.genericListItemGuids.set(res);
                this.genericListType.set("Library");
              }
            }
          });
          break;
        case "Shelves":
          this.libraryService.requestUserShelvesGuids(this.ownerGuid()!).subscribe({
            next: res => {
              if(res){
                this.genericListItemGuids.set(res);
                this.genericListType.set("Shelf");
              }
            }
          });
          break;
        case "Documents":
          this.libraryService.requestUserDocumentsGuids(this.ownerGuid()!).subscribe({
            next: res => {
              if(res){
                this.genericListItemGuids.set(res);
                this.genericListType.set("Document");
              }
            }
          });
          break;
        case "FavoriteLibraries":
          this.libraryService.requestFavoriteLibrariesGuids(this.ownerGuid()!).subscribe({
            next: res => {
              if(res){
                this.genericListItemGuids.set(res);
                this.genericListType.set("Library");
              }
            }
          });
          break;
        case "FavoriteShelves":
          this.libraryService.requestFavoriteShelvesGuids(this.ownerGuid()!).subscribe({
            next: res => {
              if(res){
                this.genericListItemGuids.set(res);
                this.genericListType.set("Shelf");
              }
            }
          });
          break;
        case "FavoriteDocuments":
          this.libraryService.requestFavoriteDocumentsGuids(this.ownerGuid()!).subscribe({
            next: res => {
              if(res){
                this.genericListItemGuids.set(res);
                this.genericListType.set("Document");
              }
            }
          });
          break;
      
        default:
          this.libraryService.requestLibrariesGuids(this.ownerGuid()!).subscribe({
            next: res => {
              if(res){
                this.genericListItemGuids.set(res);
                this.genericListType.set("Library");
              }
            }
          });
          break;
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
          }
        },
      });
    }
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
