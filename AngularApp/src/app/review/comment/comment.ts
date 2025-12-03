import { Component, computed, effect, inject, input, model, output, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { LibraryService, OwnerModel } from '../../services/library-service';
import { SingletonModes } from '../../services/singleton-modes';
import { RouterLink } from '@angular/router';
import { NgOptimizedImage, ViewportScroller } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { MatButtonModule } from "@angular/material/button";
import { IconService } from '../../services/icon-service';
import { MatTooltip } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { EditTextarea } from '../../dialogs/edit-textarea/edit-textarea';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { BriefUsersList } from '../../dialogs/brief-users-list/brief-users-list';
import { ReviewService } from '../review-service';
import { IdentityService } from '../../services/identity-service';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-review-comment',
  imports: [MatCardModule, RouterLink, NgOptimizedImage, MatIcon, MatButtonModule, MatTooltip,
    MatProgressSpinner,MatMenuModule,
  ],
  templateUrl: './comment.html',
  styleUrl: './comment.css'
})
export class ReviewComment {
  commentModel = input.required<CommentModel>();
  submitReply = output<NewReplyFormModel>();
  displayReplies = output();
  deleteComment = output();
  thumbsUp = output();
  thumbsDown = output();

  dialog = inject(MatDialog);
  singletonModes= inject(SingletonModes);
  viewportScroller = inject(ViewportScroller);
  iconService = inject(IconService);
  libraryService = inject(LibraryService);
  reviewService = inject(ReviewService);
  identityService  =inject(IdentityService);

  //commentModel = signal<CommentModel>(new CommentModel(null));
  writerModel = signal<OwnerModel|null>(null);
  userAvatarSrc = computed(()=>this.singletonModes.getUserImageAddress(this.writerModel()));
  displaySubmitSpinner = signal(false);
  isMyComment = computed(()=>this.commentModel().writerGuid === this.identityService.userModel()?.guid);

  constructor(){
    effect(()=>{
      if(this.commentModel()){
        this.libraryService.requestOwnerModel(this.commentModel().writerGuid).subscribe({
          next: res => {
            if(res){
              this.writerModel.set(res);
            }
          },
        });
      }
    });
  }

  goToElement(guid:string){
    this.viewportScroller.scrollToAnchor(guid, {behavior:'smooth'});
  }

  openListOfThumbsUps(){
    this.dialog.open(BriefUsersList, {
      data:{label:"Thumb Ups",
        type:"ThumbsUp",
        totalNumberOfItems:this.commentModel()?.numberOfThumbsUps, 
        subjectGuid: this.commentModel().guid
      },
      autoFocus:false,
    });
  }
  openListOfThumbsDowns(){
    this.dialog.open(BriefUsersList, {
      data:{label:"Thumb Downs",
        type:"ThumbsDown",
        totalNumberOfItems:this.commentModel()?.numberOfThumbsDowns, 
        subjectGuid: this.commentModel().guid
      },
      autoFocus:false,
    });
  }

  showReplies(){
    this.displayReplies.emit();
  }

  onReply(){
    this.dialog.open(EditTextarea,{data:{label:`Reply to ${this.writerModel()?.username}`}}).afterClosed().subscribe(result=>{
      if(result){
        let replyFormModel = new NewReplyFormModel();
        replyFormModel.parentCommentGuid = this.commentModel().guid;
        replyFormModel.text = result;
        this.submitReply.emit(replyFormModel);
      }
    });
  }
}

export class CommentModel{
  constructor(commentModel:CommentModel){
    this.guid = commentModel.guid;
    this.writerGuid = commentModel.writerGuid;
    this.isReply = commentModel.isReply;
    this.replyToGuid = commentModel.replyToGuid;
    this.replyToBrief = commentModel.replyToBrief;
    this.replyToUsername = commentModel.replyToUsername;
    this.text = commentModel.text;
    this.amIThumbsUp = commentModel.amIThumbsUp;
    this.amIThumbsDown = commentModel.amIThumbsDown;
    this.numberOfThumbsUps = commentModel.numberOfThumbsUps;
    this.numberOfThumbsDowns = commentModel.numberOfThumbsDowns;
    this.numberOfReplies = commentModel.numberOfReplies;
    this.createdAt = commentModel.createdAt;
  }
  guid:string = null!;
  writerGuid:string = null!;
  isReply:boolean = false;
  replyToGuid:string = null!;
  replyToBrief:string = null!;
  replyToUsername:string = null!;
  text:string = null!;
  amIThumbsUp:boolean = false;
  amIThumbsDown:boolean = false;
  numberOfThumbsUps:number = 0;
  numberOfThumbsDowns:number = 0;
  numberOfReplies:number = 0;
  createdAt:Date = null!;
}

export class NewCommentFormModel{
  parentSubjectGuid:string = "";
  text:string = "";
}
export class NewReplyFormModel{
  parentCommentGuid:string = "";
  text:string = "";
}
