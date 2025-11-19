import { AfterViewInit, Component, computed, effect, ElementRef, inject, input, OnInit, Renderer2, signal, viewChild } from '@angular/core';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardContent, MatCardFooter, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { SingletonModes } from '../../services/singleton-modes';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';
import { LibraryService, OwnerModel } from '../../services/library-service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-document-card',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions,
    MatButton, MatIconButton, MatIcon, MatTooltip,NgOptimizedImage,RouterLink],
  templateUrl: './document-card.html',
  styleUrl: './document-card.css'
})
export class DocumentCard {
  documentCardModel = input.required<DocumentCardModel>();
  mini = input(false);
  alone = input(false);
  
  singletonModes = inject(SingletonModes);
  identityService = inject(IdentityService);
  libraryService = inject(LibraryService);
  //router = inject(Router);
  renderer = inject(Renderer2);
  
  documentCard = viewChild.required(MatCard,{read: ElementRef});
  
  appearance = signal<"outlined"|"raised"|"filled">("outlined");
  ownerModel = signal<OwnerModel|null>(null);
  userAvatarSrc = computed(()=>this.singletonModes.getUserImageAddress(this.ownerModel()));
  documentImageAddress = computed(()=>this.libraryService.getDocumentImageAddress(this.documentCardModel()));

  constructor(){
    this.ownerModel.set(this.libraryService.currentOwnerUserModel());

    effect(()=>{
      if(this.documentCardModel() && this.ownerModel()?.userGuid !== this.documentCardModel().ownerGuid){
        this.libraryService.requestOwnerModel(this.documentCardModel().ownerGuid).subscribe({
          next: res => {
            if(res){
              this.ownerModel.set(res);
            }
          },
        });
      }
    });
  }

  raiseCard(){
    this.appearance.set("raised");
    this.renderer.addClass(this.documentCard().nativeElement, "raised");
  }
  outlineCard(){
    this.appearance.set("outlined");
    this.renderer.removeClass(this.documentCard().nativeElement, "raised");
  }
}

export class DocumentCardModel{
  guid:string = null!;
  title:string = null!;
  description:string = null!;
  headers:string[] = [];
  hasImage:boolean = false;
  integrityVersion:number = 0;
  ownerGuid:string = null!;
}
