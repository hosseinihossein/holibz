import { Component, computed, effect, inject, input, OnInit, signal } from '@angular/core';
import { MatAccordion, MatExpansionPanel, MatExpansionPanelActionRow, MatExpansionPanelDescription, MatExpansionPanelHeader, MatExpansionPanelTitle } from "@angular/material/expansion";
import { MatTooltip } from '@angular/material/tooltip';
import { DocumentCard, DocumentCardModel } from "../document-card/document-card";
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { LibraryService } from '../../services/library-service';
import { Router } from '@angular/router';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-shelf-card',
  imports: [MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle,
    MatExpansionPanelDescription, MatTooltip, DocumentCard, MatExpansionPanelActionRow, MatButton, 
    MatIcon, MatBadge, NgOptimizedImage],
  templateUrl: './shelf-card.html',
  styleUrl: './shelf-card.css'
})
export class ShelfCard implements OnInit {
  shelfModel = input.required<ShelfCardModel>();

  router = inject(Router);
  libraryService = inject(LibraryService);
  identityService = inject(IdentityService);

  userModel = signal<UserProfileModel|null>(null);
  userImgSrc = computed(()=>this.userModel()?.imageAddress);

  constructor(){}

  ngOnInit(): void {
    this.identityService.requestUserModel(this.shelfModel().ownerGuid!).subscribe({
      next: res => {
        if(res){
          this.userModel.set(res);
        }
      },
    });
  }

  openShelf(){
    this.libraryService.currentShelfModel.set(this.shelfModel());
    this.router.navigate(["/shelf", this.shelfModel().guid]);
  }
}

export class ShelfCardModel{
  guid:string = null!;
  ownerGuid:string = null!;
  title:string = null!;
  description?:string;
  libraries:{guid:string, title:string}[] = [];
  documentCardModels:DocumentCardModel[] = [];
  totalNumberOfShelfDocuments:number = 0;
  createdAt:Date = null!;
  hasImage:boolean = false;
}
