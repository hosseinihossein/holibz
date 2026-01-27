import { AfterViewInit, Component, computed, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogContent, MatDialogModule } from "@angular/material/dialog";
import { LibraryService, OwnerModel } from '../../services/library-service';
import { NgOptimizedImage } from "@angular/common";
import { SingletonModes } from '../../services/singleton-modes';
import { MatIcon } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatProgressSpinner, MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ReviewService } from '../../review/review-service';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';

@Component({
  selector: 'app-brief-users-list',
  imports: [MatDialogModule, NgOptimizedImage, MatIcon, RouterLink,MatButton,MatFormField,MatInput,
    MatLabel,WaitSpinner,ReactiveFormsModule
  ],
  templateUrl: './brief-users-list.html',
  styleUrl: './brief-users-list.css',
  host:{
    'style':"padding:16px;position:relative;"
  }
})
export class BriefUsersList implements AfterViewInit {
  readonly data = inject<{
    label?:string, 
    subjectGuid:string, 
    totalNumberOfItems:number,
    type:"Like"|"ThumbsUp"|"ThumbsDown"|"Follower"|"Following"|"InFavorOfLibrary"|"InFavorOfShelf"|"InFavorOfDocument",
  }>(MAT_DIALOG_DATA);

  readonly singleton = inject(SingletonModes);
  reviewService = inject(ReviewService);
  libraryService = inject(LibraryService);

  users = signal<OwnerModel[]>([]);
  displayMore = computed(()=>this.users().length < this.data.totalNumberOfItems);
  displayWaitSpinner = signal(true);
  bunchIndex = signal(0);
  filterControl = new FormControl("",{validators:[Validators.maxLength(32)]});

  constructor(){
    if(!this.data.label){
      this.data.label = "Users List";
    }
    this.requestUsers();
  }
  ngAfterViewInit(): void {
    this.filterControl.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe({
      next: () => {
        this.displayWaitSpinner.set(true);
        this.bunchIndex.set(0);
        this.requestUsers();
      },
    });
  }

  onMore(){
    this.displayWaitSpinner.set(true);
    this.requestUsers();
  }

  requestUsers(){
    const callBacks = {
      next: (res:OwnerModel[]) => {
        if(res){
          this.users.set(res);
          this.bunchIndex.update(b=>++b);
          this.displayWaitSpinner.set(false);
        }
      },
    };

    let filter = null;
    if(this.filterControl.valid && this.filterControl.value?.trim()){
      filter = this.filterControl.value.trim();
    }

    if(this.data.subjectGuid){
      if(this.data.type === "Like"){
        this.reviewService.requestLikesUserList(this.data.subjectGuid, this.bunchIndex(), filter).subscribe(callBacks);
      }
      else if(this.data.type === "ThumbsUp"){
        this.reviewService.requestThumbsUpUserList(this.data.subjectGuid, this.bunchIndex(), filter).subscribe(callBacks);
      }
      else if(this.data.type === "ThumbsDown"){
        this.reviewService.requestThumbsDownUserList(this.data.subjectGuid, this.bunchIndex(), filter).subscribe(callBacks);
      }
      else if(this.data.type === "Follower"){
        this.libraryService.requestFollowers(this.data.subjectGuid, this.bunchIndex(), filter).subscribe(callBacks);
      }
      else if(this.data.type === "Following"){
        this.libraryService.requestFollowings(this.data.subjectGuid, this.bunchIndex(), filter).subscribe(callBacks);
      }
      else if(this.data.type === "InFavorOfLibrary"){
        this.libraryService.requestUsersInFavorOfLibrary(this.data.subjectGuid, this.bunchIndex(), filter).subscribe(callBacks);
      }
      else if(this.data.type === "InFavorOfShelf"){
        this.libraryService.requestUsersInFavorOfShelf(this.data.subjectGuid, this.bunchIndex(), filter).subscribe(callBacks);
      }
      else if(this.data.type === "InFavorOfDocument"){
        this.libraryService.requestUsersInFavorOfDocument(this.data.subjectGuid, this.bunchIndex(), filter).subscribe(callBacks);
      }
    }
  }

}
