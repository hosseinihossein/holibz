import { Component, computed, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { OwnerModel } from '../../services/library-service';
import { SingletonModes } from '../../services/singleton-modes';
import { RouterLink } from '@angular/router';
import { NgOptimizedImage, ViewportScroller } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { MatButtonModule } from "@angular/material/button";
import { IconService } from '../../services/icon-service';

@Component({
  selector: 'app-review-comment',
  imports: [MatCardModule, RouterLink, NgOptimizedImage, MatIcon, MatButtonModule],
  templateUrl: './comment.html',
  styleUrl: './comment.css'
})
export class ReviewComment {

  singletonModes= inject(SingletonModes);
  viewportScroller = inject(ViewportScroller);
  iconService = inject(IconService);

  commentModel = signal<CommentModel>(new CommentModel(null));
  writerModel = signal<OwnerModel|null>(null);
  userAvatarSrc = computed(()=>this.singletonModes.getUserImageAddress(this.writerModel()));

  goToElement(guid:string){
    this.viewportScroller.scrollToAnchor(guid, {behavior:'smooth'});
  }
}

export class CommentModel{
  constructor(commentModel:CommentModel|null){
    this.isReply = commentModel?.isReply ?? true;
    this.replyToGuid = commentModel?.replyToGuid ?? "repled-to-guid";
    this.replyToBrief = commentModel?.replyToBrief ?? "hossein: hey it's great. wooow your'e rock.";
    this.text = commentModel?.text ?? "Thank you buddy";
    this.amIThumbsUp = commentModel?.amIThumbsUp ?? true;
    this.amIThumbsDown = commentModel?.amIThumbsDown ?? false;
  }
  isReply:boolean = false;
  replyToGuid:string = null!;
  replyToBrief:string = null!;
  text:string = null!;
  amIThumbsUp:boolean = false;
  amIThumbsDown:boolean = false;
}
