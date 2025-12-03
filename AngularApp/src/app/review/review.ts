import { AfterViewInit, Component, effect, inject, input, signal, viewChild } from '@angular/core';
import { MatButtonModule } from "@angular/material/button";
import { MatIcon } from '@angular/material/icon';
import { IconService } from '../services/icon-service';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatBadge } from "@angular/material/badge";
import { MatTooltipModule } from '@angular/material/tooltip';
import { CommentModel, NewCommentFormModel, NewReplyFormModel, ReviewComment } from './comment/comment';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { BriefUsersList } from '../dialogs/brief-users-list/brief-users-list';
import { EditTextarea } from '../dialogs/edit-textarea/edit-textarea';
import { ReviewService } from './review-service';

@Component({
  selector: 'app-review',
  imports: [MatButtonModule, MatIcon, MatButtonToggleModule, ReactiveFormsModule,MatTooltipModule,
    ReviewComment,MatProgressSpinner,MatPaginatorModule,
  ],
  templateUrl: './review.html',
  styleUrl: './review.css'
})
export class Review {
  subjectGuid = input.required<string>();

  reviewService = inject(ReviewService);
  iconService = inject(IconService);
  dialog = inject(MatDialog);

  paginator = viewChild(MatPaginator);

  reviewModel = signal<ReviewModel>(new ReviewModel(null));
  displaySubmitSpinner = signal(true);
  bunchIndexMap = signal<Map<string,number>>(new Map<string,number>());

  orderCommentsBy_FormControl = new FormControl<"Newest"|"Oldest"|"Most Agreed"|null>(null);

  constructor(){
    effect(()=>{
      if(this.subjectGuid()){
        this.reviewService.requestReviewModel(this.subjectGuid()).subscribe({
          next: res => {
            if(res){
              this.reviewModel.set(res);
              this.bunchIndexMap.set(new Map<string,number>());
              res.comments.forEach(c=>{
                this.bunchIndexMap().set(c.guid, 0);
              });
              this.displaySubmitSpinner.set(false);
            }
          },
        });
      }
    });
  }

  orderComments(){
    this.displaySubmitSpinner.set(true);
    if(this.paginator()){
      this.paginator()!.pageIndex = 0;
    }
    this.requestComments();
  }

  requestComments(){
    this.reviewService.requestComments(
      this.subjectGuid(),
      this.orderCommentsBy_FormControl.value,
      this.paginator()?.pageIndex,
      this.paginator()?.pageSize
    ).subscribe({
      next: res => {
        if(res){
          this.reviewModel.update(rm=>{
            rm.comments = res;
            return new ReviewModel(rm);
          });
          this.bunchIndexMap.set(new Map<string,number>());
          res.forEach(c=>{
            this.bunchIndexMap().set(c.guid, 0);
          });
        }
        this.displaySubmitSpinner.set(false);
      },
    });
  }
  
  toggleLike(){
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

    this.reviewModel.update(rm=>{
      rm.amILiked = !rm.amILiked;
      return new ReviewModel(rm);
    });
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

    this.displaySubmitSpinner.set(true);
    this.requestComments();
  }

  onSubmitReply(replyFormModel:NewReplyFormModel){
    this.displaySubmitSpinner.set(true);
    this.reviewService.postNewReply(replyFormModel).subscribe({
      next: res => {
        if(res){
          this.reviewModel.update(rm=>{
            let index = rm.comments.findIndex(c=>c.guid === replyFormModel.parentCommentGuid);
            rm.comments.splice(index, 0, res);
            return new ReviewModel(rm);
          });
          this.bunchIndexMap().set(res.guid, 0);
        }
        this.displaySubmitSpinner.set(false);
      }
    });
  }
  onDisplayReplies(commentGuid:string){
    this.displaySubmitSpinner.set(true);
    this.reviewService.requestReplies(commentGuid, this.bunchIndexMap().get(commentGuid)).subscribe({
      next: res => {
        if(res){
          this.reviewModel.update(rm=>{
            let index = rm.comments.findIndex(c=>c.guid === commentGuid);
            rm.comments.splice(index, 0, ...res);
            return new ReviewModel(rm);
          });
          this.bunchIndexMap().set(commentGuid, (this.bunchIndexMap().get(commentGuid) ?? 0) + 1);
          res.forEach(c=>{
            this.bunchIndexMap().set(c.guid, 0);
          });
        }
        this.displaySubmitSpinner.set(false);
      },
    });
  }
  onNewComment(){
    this.dialog.open(EditTextarea,{data:{label:`New Comment`}}).afterClosed().subscribe((result)=>{
      if(result){
        this.displaySubmitSpinner.set(true);

        let newComment = new NewCommentFormModel();
        newComment.parentSubjectGuid = this.subjectGuid();
        newComment.text = result;

        this.reviewService.postNewComment(newComment).subscribe({
          next: res => {
            if(res){
              this.reviewModel.update(rm=>{
                rm.comments.unshift(res);
                return new ReviewModel(rm);
              });
              this.bunchIndexMap().set(res.guid, 0);
            }
            this.displaySubmitSpinner.set(false);
          }
        });
      }
    });
  }
  onDeleteComment(commentGuid:string){
    this.displaySubmitSpinner.set(true);
    this.reviewService.requestDeleteComment(commentGuid).subscribe({
      next: res => {
        if(res && res.success){
          this.deleteCommentAndRepliesRecursively(commentGuid, this.reviewModel().comments)
        }
      }
    });
  }
  deleteCommentAndRepliesRecursively(commentGuid:string, comments:CommentModel[]){
    let index = comments.findIndex(c=>c.guid === commentGuid);
    comments.splice(index,1);
    comments.forEach(c=>{
      if(c.replyToGuid === commentGuid){
        this.deleteCommentAndRepliesRecursively(c.guid, comments);
      }
    });
  }

}

export class ReviewModel{
  constructor(reviewModel:ReviewModel|null){
    this.amILiked = reviewModel?.amILiked ?? false;
    this.numberOfLikes = reviewModel?.numberOfLikes ?? 0;
    this.totalNumberOfComments = reviewModel?.totalNumberOfComments ?? 0;
    this.comments = reviewModel?.comments.map(c=>new CommentModel(c)) ?? [];
  }
  amILiked:boolean = false;
  numberOfLikes:number = 0;
  totalNumberOfComments:number = 0;
  comments:CommentModel[] = [];
}