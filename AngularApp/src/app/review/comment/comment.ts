import { Component, computed, inject, input, output, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { OwnerModel } from '../../services/library-service';
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

@Component({
  selector: 'app-review-comment',
  imports: [MatCardModule, RouterLink, NgOptimizedImage, MatIcon, MatButtonModule, MatTooltip,
    MatProgressSpinner
  ],
  templateUrl: './comment.html',
  styleUrl: './comment.css'
})
export class ReviewComment {
  commentModel = input.required<CommentModel>();
  replyEvent = output<CommentModel>();

  dialog = inject(MatDialog);
  singletonModes= inject(SingletonModes);
  viewportScroller = inject(ViewportScroller);
  iconService = inject(IconService);

  //commentModel = signal<CommentModel>(new CommentModel(null));
  writerModel = signal<OwnerModel|null>(null);
  userAvatarSrc = computed(()=>this.singletonModes.getUserImageAddress(this.writerModel()));
  displaySubmitSpinner = signal(false);

  goToElement(guid:string){
    this.viewportScroller.scrollToAnchor(guid, {behavior:'smooth'});
  }

  openListOfThumbsUps(){}
  openListOfThumbsDowns(){}
  openListOfReplies(){}
  onReply(){
    this.dialog.open(EditTextarea,{data:{label:`Reply to ${'hossein'}`}}).afterClosed().subscribe(result=>{
      if(result){
        this.displaySubmitSpinner.set(true);

        let newComment = new CommentModel(null);
        newComment.text = result;
        this.replyEvent.emit(newComment);

        setTimeout(() => {
          this.displaySubmitSpinner.set(false);
        }, 1000);
      }
    });
  }
}

export class CommentModel{
  constructor(commentModel:CommentModel|null){
    this.guid = commentModel?.guid ?? "guid";
    this.isReply = commentModel?.isReply ?? true;
    this.replyToGuid = commentModel?.replyToGuid ?? "repled-to-guid";
    this.replyToBrief = commentModel?.replyToBrief ?? "hey it's great. wooow your'e rock. Lorem ipsum dolor sit amet consectetur adipisicing elit. Deleniti consequuntur impedit ullam praesentium maxime iste maiores! Asperiores nesciunt ut quasi!";
    this.replyToUsername = commentModel?.replyToUsername ?? "hossein"
    this.text = commentModel?.text ?? "Thank you buddy. Lorem ipsum dolor sit amet consectetur adipisicing elit. Deleniti consequuntur impedit ullam praesentium maxime iste maiores! Asperiores nesciunt ut quasi!";
    this.amIThumbsUp = commentModel?.amIThumbsUp ?? true;
    this.amIThumbsDown = commentModel?.amIThumbsDown ?? false;
    this.numberOfThumbsUps = commentModel?.numberOfThumbsUps ?? 18;
    this.numberOfThumbsDowns = commentModel?.numberOfThumbsDowns ?? 7;
    this.numberOfReplies = commentModel?.numberOfReplies ?? 3;
  }
  guid:string = null!;
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
}
