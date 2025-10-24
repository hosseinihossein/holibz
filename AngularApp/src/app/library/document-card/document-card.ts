import { Component, computed, effect, ElementRef, inject, input, OnInit, signal, viewChild } from '@angular/core';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardContent, MatCardFooter, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { SingletonModes } from '../../services/singleton-modes';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';
import { LibraryService } from '../../services/library-service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-document-card',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions,
    MatButton, MatIconButton, MatIcon, MatTooltip,NgOptimizedImage,RouterLink],
  templateUrl: './document-card.html',
  styleUrl: './document-card.css'
})
export class DocumentCard implements OnInit {
  docGuid = input.required<string>();
  userGuid = input.required<string>();
  mini = input(false);
  alone = input(false);

  singletonModes = inject(SingletonModes);
  identityService = inject(IdentityService);
  libraryService = inject(LibraryService);
  router = inject(Router);

  documentCard = viewChild(MatCard,{read:ElementRef});
  
  appearance = signal<"outlined"|"raised"|"filled">("outlined");
  userModel = signal<UserProfileModel|null>(null);
  userAvatarSrc = computed(()=>this.userModel()?.imageAddress);
  documentCardModel = signal<DocumentCardModel|null>(null);

  constructor(){}
  ngOnInit(): void {
    effect(() => {
      this.identityService.requestUserModel(this.userGuid()).subscribe({
        next: res => {
          if(res){
            this.userModel.set(res);
          }
        },
      });
    });

    effect(() => {
      this.libraryService.requestDocumentModel(this.docGuid()).subscribe({
        next: res => {
          if(res){
            this.documentCardModel.set(res);
          }
        },
      });
    });
  }

  raiseCard(){
    this.appearance.set("raised");
    this.documentCard()?.nativeElement.classList.add("raised");
  }
  outlineCard(){
    this.appearance.set("outlined");
    this.documentCard()?.nativeElement.classList.remove("raised");
  }
}

export class DocumentCardModel{
  guid?:string;
  title?:string;
  description?:string;
  headers?:string[];
  hasImage?:boolean;
}
