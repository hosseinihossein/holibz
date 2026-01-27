import { Component, effect, inject, input, model, OnInit, output, signal, viewChild } from '@angular/core';
import { MatButtonModule } from "@angular/material/button";
import { MatIcon } from '@angular/material/icon';
import { IconService } from '../services/icon-service';
import { MatButtonToggleChange, MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NewCommentFormModel, ReviewComment } from './comment/comment';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { BriefUsersList } from '../dialogs/brief-users-list/brief-users-list';
import { EditTextarea } from '../dialogs/edit-textarea/edit-textarea';
import { ReviewService } from './review-service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { IdentityService } from '../services/identity-service';
import { ViewportScroller } from '@angular/common';
import { WaitSpinner } from '../shared/wait-spinner/wait-spinner';

@Component({
  selector: 'app-review',
  imports: [MatButtonModule, MatIcon, MatButtonToggleModule,MatTooltipModule,
    ReviewComment,WaitSpinner,MatPaginatorModule,
  ],
  templateUrl: './review.html',
  styleUrl: './review.css'
})
export class Review implements OnInit {
  subjectGuid = input.required<string>();
  requestedCommentGuid = model<string|null>(null);

  reviewService = inject(ReviewService);
  iconService = inject(IconService);
  dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  identityService = inject(IdentityService);
  viewportScroller = inject(ViewportScroller);

  paginator = viewChild(MatPaginator);

  reviewModel = signal<ReviewModel>(new ReviewModel(null));
  displayWaitSpinner = signal(true);
  bunchIndexMap = signal<Map<string,number>>(new Map<string,number>());

  orderCommentsBy = signal<"Newest"|"Oldest"|"Most Agreed"|null>(null);

  iLiked = signal(false);
  deletedCommentsGuids = signal<string[]>([]);

  constructor(){

    effect(()=>{
      if(this.subjectGuid()){
        this.displayWaitSpinner.set(true);
        this.orderCommentsBy.set(null);
        this.bunchIndexMap.set(new Map<string,number>());
        this.deletedCommentsGuids.set([]);
        this.iLiked.set(false);
        
        this.reviewService.requestReviewModel(this.subjectGuid(), this.requestedCommentGuid()).subscribe({
          next: res => {
            if(res){
              this.reviewModel.set(res);
              this.bunchIndexMap.set(new Map<string,number>());
              res.commentsGuids.forEach(cGuid=>{
                this.bunchIndexMap().set(cGuid, 0);
              });

              if(this.requestedCommentGuid()){
                setTimeout(() => {
                  this.goToComment(this.requestedCommentGuid()!);
                }, 1000);
              }

              this.displayWaitSpinner.set(false);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.identityService.isAuthenticated() && this.subjectGuid()){
        this.reviewService.amILiked(this.subjectGuid()).subscribe({
          next: res => {
            if(res){
              this.iLiked.set(res.iLiked);
            }
          },
        });
      }
      else{
        this.iLiked.set(false);
      }
    });

  }
  ngOnInit(): void {
    this.viewportScroller.setOffset([0,64]);//[xOffset, yOffset]
  }

  goToComment(guid:string){
    this.viewportScroller.scrollToAnchor(guid, {behavior:'smooth'});
  }

  orderComments(e:MatButtonToggleChange){
    this.displayWaitSpinner.set(true);
    this.orderCommentsBy.set(e.value);
    if(this.paginator()){
      this.paginator()!.pageIndex = 0;
    }
    this.requestComments();
  }
  requestComments(){
    this.reviewService.requestComments(
      this.subjectGuid(),
      this.orderCommentsBy(),
      this.paginator()?.pageIndex,
      this.paginator()?.pageSize
    ).subscribe({
      next: res => {
        if(res){
          this.reviewModel.update(rm=>{
            rm.commentsGuids = res;
            return new ReviewModel(rm);
          });
          this.bunchIndexMap.set(new Map<string,number>());
          res.forEach(cGuid=>{
            this.bunchIndexMap().set(cGuid, 0);
          });
        }
        this.displayWaitSpinner.set(false);
      },
    });
  }
  
  toggleLike(){
    if(this.identityService.isAuthenticated()){
      this.reviewService.requestToggleLike(this.subjectGuid()).subscribe({
        next: res => {
          if(res){
            this.reviewModel.update(rm=>{
              rm.numberOfLikes = res.numberOfLikes;
              return new ReviewModel(rm);
            });
          }
        },
      });
  
      this.iLiked.update(l=>!l);
    }
    else{
      this.snackBar.open("Please Login", "Ok", { duration: 5000 });
    }
  }
  openListOfLikes(){
    this.dialog.open(BriefUsersList, {
      data:{label:"Likes",
        type:"Like",
        totalNumberOfItems:this.reviewModel()?.numberOfLikes, 
        subjectGuid: this.subjectGuid()
      },
      autoFocus:false,
    });
  }

  handlePageEvent(/*e: PageEvent*/) {
    //let length = e.length;
    //let pageSize = e.pageSize;
    //let pageIndex = e.pageIndex;

    this.displayWaitSpinner.set(true);
    this.requestComments();
  }

  onSubmitReply(parentCommentGuid:string,replyGuid:string){
    this.displayWaitSpinner.set(true);
    this.reviewModel.update(rm=>{
      let index = rm.commentsGuids.indexOf(parentCommentGuid) + 1;
      rm.commentsGuids.splice(index, 0, replyGuid);
      return new ReviewModel(rm);
    });
    this.bunchIndexMap().set(replyGuid, 0);
  }
  onDisplayReplies(commentGuid:string){
    this.displayWaitSpinner.set(true);
    this.reviewService.requestReplies(commentGuid, this.bunchIndexMap().get(commentGuid)).subscribe({
      next: res => {
        if(res){
          this.reviewModel.update(rm=>{
            let index = rm.commentsGuids.indexOf(commentGuid) + 1;
            rm.commentsGuids.splice(index, 0, ...res);
            return new ReviewModel(rm);
          });
          this.bunchIndexMap().set(commentGuid, (this.bunchIndexMap().get(commentGuid) ?? 0) + 1);
          res.forEach(cGuid=>{
            this.bunchIndexMap().set(cGuid, 0);
          });
        }
        this.displayWaitSpinner.set(false);
      },
    });
  }
  onNewComment(){
    if(this.identityService.isAuthenticated()){
      this.dialog.open(EditTextarea,{data:{label:`New Comment`}}).afterClosed().subscribe((result)=>{
        if(result){
          this.displayWaitSpinner.set(true);
  
          let newComment = new NewCommentFormModel();
          newComment.parentSubjectGuid = this.subjectGuid();
          newComment.text = result;
  
          this.reviewService.postNewComment(newComment).subscribe({
            next: res => {
              if(res){
                this.reviewModel.update(rm=>{
                  rm.commentsGuids.unshift(res.commentGuid);
                  return new ReviewModel(rm);
                });
                this.bunchIndexMap().set(res.commentGuid, 0);
              }
              this.displayWaitSpinner.set(false);
            },
            error: err => {
              this.displayWaitSpinner.set(false);
              throw(err);
            }
          });
        }
      });
    }
    else{
      this.snackBar.open("Please Login", "Ok", { duration: 5000 });
    }
  }
  onDeleteComment(commentGuid:string){
    let index = this.reviewModel().commentsGuids.indexOf(commentGuid);
    this.reviewModel().commentsGuids.splice(index, 1);
    this.bunchIndexMap().delete(commentGuid);
    this.deletedCommentsGuids.update(guids=>{
      guids.push(commentGuid);
      return guids.map(g=>g);
    });
  }
  /*deleteCommentAndRepliesRecursively(commentGuid:string, comments:CommentModel[]){
    let index = comments.findIndex(c=>c.guid === commentGuid);
    comments.splice(index,1);
    comments.forEach(c=>{
      if(c.replyToGuid === commentGuid){
        this.deleteCommentAndRepliesRecursively(c.guid, comments);
      }
    });
  }*/

}

export class ReviewModel{
  constructor(reviewModel:ReviewModel|null){
    //this.amILiked = reviewModel?.amILiked ?? false;
    this.numberOfLikes = reviewModel?.numberOfLikes ?? 0;
    this.totalNumberOfComments = reviewModel?.totalNumberOfComments ?? 0;
    this.commentsGuids = reviewModel?.commentsGuids.map(c=>c) ?? [];
  }
  //amILiked:boolean = false;
  numberOfLikes:number = 0;
  totalNumberOfComments:number = 0;
  commentsGuids:string[] = [];
}