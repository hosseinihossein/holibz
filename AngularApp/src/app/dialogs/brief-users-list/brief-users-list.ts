import { AfterViewInit, Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogContent, MatDialogModule } from "@angular/material/dialog";
import { OwnerModel } from '../../services/library-service';
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

@Component({
  selector: 'app-brief-users-list',
  imports: [MatDialogModule, NgOptimizedImage, MatIcon, RouterLink,MatButton,MatFormField,MatInput,
    MatLabel,MatProgressSpinner,ReactiveFormsModule
  ],
  templateUrl: './brief-users-list.html',
  styleUrl: './brief-users-list.css',
  host:{
    'style':"padding:16px;position:relative;"
  }
})
export class BriefUsersList {
  readonly data = inject<{
    label?:string, 
    subjectGuid:string, 
    totalNumberOfItems:number,
    type:"Like"|"ThumbsUp"|"ThumbsDown",
  }>(MAT_DIALOG_DATA);

  readonly singleton = inject(SingletonModes);
  reviewService = inject(ReviewService);

  users = signal<OwnerModel[]>([]);
  displaySubmitSpinner = signal(true);
  bunch = signal(1);
  filterControl = new FormControl("",{validators:[Validators.maxLength(32)]});

  constructor(){
    if(!this.data.label){
      this.data.label = "Users List";
    }
    this.requestUsers();
  }

  onMore(){
    this.displaySubmitSpinner.set(true);
    this.requestUsers();
  }

  onUsernameFilter(){
    this.displaySubmitSpinner.set(true);
    this.bunch.set(1);
    this.requestUsers();
  }

  requestUsers(){
    const callBacks = {
      next: (res:OwnerModel[]) => {
        if(res){
          this.users.set(res);
          this.bunch.update(b=>++b);
          this.displaySubmitSpinner.set(false);
        }
      },
    };

    let filter = null;
    if(this.filterControl.valid && this.filterControl.value?.trim()){
      filter = this.filterControl.value.trim();
    }

    if(this.data.subjectGuid){
      if(this.data.type === "Like"){
        this.reviewService.requestLikesUserList(this.data.subjectGuid, this.bunch(), filter).subscribe(callBacks);
      }
      else if(this.data.type === "ThumbsUp"){
        this.reviewService.requestThumbsUpUserList(this.data.subjectGuid, this.bunch(), filter).subscribe(callBacks);
      }
      else if(this.data.type === "ThumbsDown"){
        this.reviewService.requestThumbsDownUserList(this.data.subjectGuid, this.bunch(), filter).subscribe(callBacks);
      }
    }
  }

}
