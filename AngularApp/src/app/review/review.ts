import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from "@angular/material/button";
import { MatIcon } from '@angular/material/icon';
import { IconService } from '../services/icon-service';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatBadge } from "@angular/material/badge";
import { MatTooltipModule } from '@angular/material/tooltip';
import { ReviewComment } from './comment/comment';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-review',
  imports: [MatButtonModule, MatIcon, MatButtonToggleModule, ReactiveFormsModule,MatTooltipModule,
    ReviewComment,MatProgressSpinner
  ],
  templateUrl: './review.html',
  styleUrl: './review.css'
})
export class Review {
  iconService = inject(IconService);

  reviewModel = signal<ReviewModel|null>(new ReviewModel(null));
  displaySubmitSpinner = signal(false);

  sortCommentsBy_FormControl = new FormControl<"Newest"|"Oldest"|"Most Agreed">("Newest");

  sortComments(){}
  toggleLike(){
    this.reviewModel.update(rm=>{
      let newReviewModel = new ReviewModel(rm!);
      newReviewModel.isLiked = !newReviewModel.isLiked;
      return newReviewModel;
    });
  }
  openListOfLikes(){}

  showMoreComments(){
    this.displaySubmitSpinner.set(true);
  }

}

export class ReviewModel{
  constructor(reviewModel:ReviewModel|null){
    this.isLiked = reviewModel?.isLiked ?? false;
    this.numberOfLikes = reviewModel?.numberOfLikes ?? 105;
  }
  isLiked:boolean = false;
  numberOfLikes:number = 0;

}