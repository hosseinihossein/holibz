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

@Component({
  selector: 'app-brief-users-list',
  imports: [MatDialogModule, NgOptimizedImage, MatIcon, RouterLink,MatButton,MatFormField,MatInput,
    MatLabel,MatProgressSpinner
  ],
  templateUrl: './brief-users-list.html',
  styleUrl: './brief-users-list.css',
  host:{
    'style':"padding:16px;position:relative;"
  }
})
export class BriefUsersList {
  //readonly dialogRef = inject(MatDialogRef<BriefUsersList>);
  readonly data = inject<{label:string, documentGuid?:string, commentGuid?:string,
    totalNumberOfItems:number,type:"Like"|"ThumbsUp"|"ThumbsDown"
  }>(MAT_DIALOG_DATA);
  readonly singleton = inject(SingletonModes);

  okBtn = viewChild<MatButton>("okBtn");

  users = signal<OwnerModel[]>([]);
  displaySubmitSpinner = signal(false);

  constructor(){
    if(!this.data.label){
      this.data.label = "Brief Users List";
    }
    if(!this.data.documentGuid && !this.data.commentGuid){
      this.users.set([
        {guid:"",hasImage:false,integrityVersion:0,username:"hossein"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hassan"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hossein"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hassan"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hossein"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hassan"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hossein"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hassan"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hossein"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hassan"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hossein"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hassan"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hossein"},
        {guid:"",hasImage:false,integrityVersion:0,username:"hassan"},
      ]);
    }
  }

  onMore(){
    this.displaySubmitSpinner.set(true);
    setTimeout(() => {
      this.displaySubmitSpinner.set(false);
    }, 1000);
  }

  onUsernameFilter(){
    this.displaySubmitSpinner.set(true);
    setTimeout(() => {
      this.displaySubmitSpinner.set(false);
    }, 1000);
  }
}
