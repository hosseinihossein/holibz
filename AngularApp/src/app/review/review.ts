import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from "@angular/material/button";
import { MatIcon } from '@angular/material/icon';
import { IconService } from '../services/icon-service';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatBadge } from "@angular/material/badge";
import { MatTooltipModule } from '@angular/material/tooltip';
import { CommentModel, ReviewComment } from './comment/comment';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { BriefUsersList } from '../dialogs/brief-users-list/brief-users-list';
import { EditTextarea } from '../dialogs/edit-textarea/edit-textarea';

@Component({
  selector: 'app-review',
  imports: [MatButtonModule, MatIcon, MatButtonToggleModule, ReactiveFormsModule,MatTooltipModule,
    ReviewComment,MatProgressSpinner,MatPaginatorModule,
  ],
  templateUrl: './review.html',
  styleUrl: './review.css'
})
export class Review {
  iconService = inject(IconService);
  dialog = inject(MatDialog);

  reviewModel = signal<ReviewModel|null>(new ReviewModel(null));
  displaySubmitSpinner = signal(false);

  sortCommentsBy_FormControl = new FormControl<"Newest"|"Oldest"|"Most Agreed">("Most Agreed");

  sortComments(){}
  
  toggleLike(){
    this.reviewModel.update(rm=>{
      rm!.isLiked = !rm?.isLiked;
      if(rm?.isLiked){
        rm!.numberOfLikes += 1;
      }
      else{
        rm!.numberOfLikes -= 1;
      }
      return rm;
    });
  }

  openListOfLikes(){
    this.dialog.open(BriefUsersList, {data:{label:"Likes",type:"Like",totalNumberOfItems:105}, autoFocus:false,});
  }

  handlePageEvent(e: PageEvent) {
    let length = e.length;
    let pageSize = e.pageSize;
    let pageIndex = e.pageIndex;

    this.displaySubmitSpinner.set(true);
    setTimeout(() => {
      this.displaySubmitSpinner.set(false);
    }, 1000);
  }

  onReplyEvent(comment:CommentModel){
    this.reviewModel.update(r=>{
      r?.comments.push(comment);
      return r;
    });
  }

  onNewComment(){
    this.dialog.open(EditTextarea,{data:{label:`New Comment`}}).afterClosed().subscribe(result=>{
          if(result){
            this.displaySubmitSpinner.set(true);
    
            let newComment = new CommentModel(null);
            newComment.text = result;
            this.reviewModel.update(r=>{
              r?.comments.unshift(newComment);
              return r;
            });
    
            setTimeout(() => {
              this.displaySubmitSpinner.set(false);
            }, 1000);
          }
        });
  }

}

export class ReviewModel{
  constructor(reviewModel:ReviewModel|null){
    this.isLiked = reviewModel?.isLiked ?? false;
    this.numberOfLikes = reviewModel?.numberOfLikes ?? 105;
    this.totalNumberOfComments = reviewModel?.totalNumberOfComments ?? 12;
    this.comments = reviewModel?.comments ?? [new CommentModel(null),new CommentModel(null),new CommentModel(null),];
  }
  isLiked:boolean = false;
  numberOfLikes:number = 0;
  totalNumberOfComments:number = 0;
  comments:CommentModel[] = [];
}