import { Component, computed, effect, inject, input, model, OnInit, output, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { LibraryService, OwnerModel } from '../../services/library-service';
import { SingletonModes } from '../../services/singleton-modes';
import { RouterLink } from '@angular/router';
import { DatePipe, NgOptimizedImage, ViewportScroller } from '@angular/common';
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
import { MatSnackBar } from '@angular/material/snack-bar';
import { WindowService } from '../../services/window-service';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';

@Component({
  selector: 'app-review-comment',
  imports: [MatCardModule, RouterLink, NgOptimizedImage, MatIcon, MatButtonModule, MatTooltip,
    WaitSpinner,MatMenuModule, DatePipe
  ],
  templateUrl: './comment.html',
  styleUrl: './comment.css'
})
export class ReviewComment implements OnInit {
  commentGuid = input.required<string>();
  deletedCommentsGuids = input<string[]>();

  submitReply = output<string>();
  displayReplies = output();
  deleteComment = output();
  parentComment = output<string>();

  dialog = inject(MatDialog);
  singletonModes= inject(SingletonModes);
  viewportScroller = inject(ViewportScroller);
  iconService = inject(IconService);
  libraryService = inject(LibraryService);
  reviewService = inject(ReviewService);
  identityService  =inject(IdentityService);
  private snackBar = inject(MatSnackBar);
  windowService = inject(WindowService);
  identitySerice = inject(IdentityService);

  commentModel = signal<CommentModel|null>(null);
  writerModel = signal<OwnerModel|null>(null);
  userAvatarSrc = computed(()=>this.singletonModes.getUserImageAddress(this.writerModel()));
  displayWaitSpinner = signal(false);
  isMyComment = computed(()=>this.commentModel()?.writerGuid === this.identityService.userModel()?.guid);
  iThumbsUp = signal(false);
  iThumbsDown = signal(false);

  constructor(){
    effect(()=>{
      if(this.commentGuid()){
        this.reviewService.requestCommentModel(this.commentGuid()).subscribe({
          next: res => {
            if(res){
              this.commentModel.set(res);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.commentModel()){
        this.libraryService.requestOwnerModel(this.commentModel()!.writerGuid).subscribe({
          next: res => {
            if(res){
              this.writerModel.set(res);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.identityService.isAuthenticated() && this.commentGuid()){

        this.reviewService.amIThumbsUp(this.commentGuid()).subscribe({
          next: res => {
            if(res){
              this.iThumbsUp.set(res.iThumbsUp);
            }
          },
        });

        this.reviewService.amIThumbsDown(this.commentGuid()).subscribe({
          next: res => {
            if(res){
              this.iThumbsDown.set(res.iThumbsDown);
            }
          },
        });

      }
    });

    effect(()=>{
      if(this.deletedCommentsGuids()?.includes(this.commentGuid())){
        this.deleteComment.emit();
      }
    });
  }
  ngOnInit(): void {
    this.viewportScroller.setOffset([0,64]);//[xOffset, yOffset]
  }

  goToElement(guid:string){
    if(this.windowService.nativeWindow.document.getElementById(guid)){
      this.viewportScroller.scrollToAnchor(guid, {behavior:'smooth'});
    }
    else{
      this.parentComment.emit(guid);
    }
  }

  openListOfThumbsUps(){
    if(this.commentModel()){
      this.dialog.open(BriefUsersList, {
        data:{label:"Thumb Ups",
          type:"ThumbsUp",
          totalNumberOfItems:this.commentModel()!.numberOfThumbsUps, 
          subjectGuid: this.commentGuid()
        },
        autoFocus:false,
      });
    }
  }
  openListOfThumbsDowns(){
    if(this.commentModel()){
      this.dialog.open(BriefUsersList, {
        data:{label:"Thumb Downs",
          type:"ThumbsDown",
          totalNumberOfItems:this.commentModel()!.numberOfThumbsDowns, 
          subjectGuid: this.commentGuid()
        },
        autoFocus:false,
      });
    }
  }

  showReplies(){
    this.displayReplies.emit();
  }

  onReply(){
    if(this.identityService.isAuthenticated() && this.commentGuid()){
      this.dialog.open(EditTextarea,{data:{label:`Reply to ${this.writerModel()?.username}`}}).afterClosed().subscribe(result=>{
        if(result){
          this.displayWaitSpinner.set(true);

          let replyFormModel = new NewReplyFormModel();
          replyFormModel.parentCommentGuid = this.commentGuid();
          replyFormModel.text = result;
          
          this.reviewService.postNewReply(replyFormModel).subscribe({
            next: res => {
              if(res){
                this.submitReply.emit(res.replyGuid);
                this.commentModel.update(cm=>{
                  cm!.numberOfReplies += 1;
                  return new CommentModel(cm!);
                });
              }
              this.displayWaitSpinner.set(false);
            },
            error: err => {
              this.displayWaitSpinner.set(false);
              throw(err);
            },
          });
        }
      });
    }
    else if(!this.identityService.isAuthenticated()){
      this.snackBar.open("Please Login", "Ok", { duration: 5000 });
    }
  }

  toggleThumbsUp(){
    if(this.identitySerice.isAuthenticated() && this.commentModel()){
      this.reviewService.requestToggleThumbsUp(this.commentGuid()).subscribe({
        next: res => {
          if(res){
            this.commentModel.update(cm=>{
              cm!.numberOfThumbsUps = res.numberOfThumbsUps;
              cm!.numberOfThumbsDowns = res.numberOfThumbsDowns;
              return new CommentModel(cm!);
            });
          }
        },
      });
      this.iThumbsUp.update(up=>!up);
      if(this.iThumbsDown() && this.iThumbsUp()){
        this.iThumbsDown.set(false);
      }
    }
  }
  toggleThumbsDown(){
    if(this.identitySerice.isAuthenticated() && this.commentModel()){
      this.reviewService.requestToggleThumbsDown(this.commentGuid()).subscribe({
        next: res => {
          if(res){
            this.commentModel.update(cm=>{
              cm!.numberOfThumbsDowns = res.numberOfThumbsDowns;
              cm!.numberOfThumbsUps = res.numberOfThumbsUps;
              return new CommentModel(cm!);
            });
          }
        },
      });
      this.iThumbsDown.update(dn=>!dn);
      if(this.iThumbsDown() && this.iThumbsUp()){
        this.iThumbsUp.set(false);
      }
    }
  }

  onDelete(){
    if(!this.identityService.isAuthenticated()){
      this.snackBar.open("Please Login", "Ok", { duration: 5000 });
      return;
    }

    if(this.commentModel()?.writerGuid === this.identityService.userModel()?.guid){
      this.displayWaitSpinner.set(true);
      this.reviewService.requestDeleteComment(this.commentGuid()).subscribe({
        next: res => {
          if(res && res.success){
            this.displayWaitSpinner.set(false);
            this.deleteComment.emit();
          }
        }
      });
    }
    else{
      this.snackBar.open("Only the writer delete their comments!", "Ok", { duration: 5000 });
    }
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
    //this.amIThumbsUp = commentModel.amIThumbsUp;
    //this.amIThumbsDown = commentModel.amIThumbsDown;
    this.numberOfThumbsUps = commentModel.numberOfThumbsUps;
    this.numberOfThumbsDowns = commentModel.numberOfThumbsDowns;
    this.numberOfReplies = commentModel.numberOfReplies;
    this.createdAt = commentModel.createdAt;
  }
  guid:string = null!;
  writerGuid:string = null!;
  isReply:boolean = false;
  replyToGuid?:string|null = null;
  replyToBrief?:string|null = null;
  replyToUsername?:string|null = null;
  text:string = null!;
  //amIThumbsUp:boolean = false;
  //amIThumbsDown:boolean = false;
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
