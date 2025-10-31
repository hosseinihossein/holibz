import { AfterViewInit, Component, computed, effect, ElementRef, inject, input, OnInit, signal, viewChild } from '@angular/core';
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
export class DocumentCard implements AfterViewInit {
  documentCardModel = input.required<DocumentCardModel>();
  //docGuid = input.required<string>();
  //userGuid = input.required<string>();
  mini = input(false);
  alone = input(false);

  singletonModes = inject(SingletonModes);
  identityService = inject(IdentityService);
  libraryService = inject(LibraryService);
  //router = inject(Router);

  documentCard = viewChild(MatCard,{read:ElementRef});
  mainImg = viewChild<ElementRef<HTMLImageElement>>("mainImg");
  
  appearance = signal<"outlined"|"raised"|"filled">("outlined");
  userModel = signal<UserProfileModel|null>(null);
  userAvatarSrc = computed(()=>this.userModel()?.imageAddress);
  //documentCardModel = signal<DocumentCardModel|null>(null);

  constructor(){
    this.userModel.set(this.libraryService.currentOwnerUserModel());

    effect(()=>{
      if(this.documentCardModel() && this.userModel()?.guid !== this.documentCardModel().ownerGuid){
        this.identityService.requestUserModel(this.documentCardModel().ownerGuid).subscribe({
          next: res => {
            if(res){
              this.userModel.set(res);
            }
          },
        });
      }
    });
  }
  ngAfterViewInit(): void {
    if(this.mainImg() && this.mini()){
      this.mainImg()?.nativeElement.addEventListener("mouseenter", ()=>{
        this.mainImg()!.nativeElement.height = 150;
      });
      this.mainImg()?.nativeElement.addEventListener("mouseleave", ()=>{
        this.mainImg()!.nativeElement.height = 100;
      });
    }
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
  guid:string = null!;
  title:string = null!;
  description:string = null!;
  headers:string[] = [];
  hasImage:boolean = false;
  ownerGuid:string = null!;
}
