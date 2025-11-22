import { Component, computed, effect, inject, input, OnInit, signal } from '@angular/core';
import { MatAccordion, MatExpansionPanel, MatExpansionPanelActionRow, MatExpansionPanelDescription, MatExpansionPanelHeader, MatExpansionPanelTitle } from "@angular/material/expansion";
import { MatTooltip } from '@angular/material/tooltip';
import { DocumentCard, DocumentCardModel } from "../document-card/document-card";
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { LibraryService, OwnerModel } from '../../services/library-service';
import { Router } from '@angular/router';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';
import { SingletonModes } from '../../services/singleton-modes';

@Component({
  selector: 'app-shelf-card',
  imports: [MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle,
    MatExpansionPanelDescription, MatTooltip, DocumentCard, MatExpansionPanelActionRow, MatButton, 
    MatIcon, MatBadge, NgOptimizedImage],
  templateUrl: './shelf-card.html',
  styleUrl: './shelf-card.css'
})
export class ShelfCard {
  shelfModel = input.required<ShelfCardModel>();

  router = inject(Router);
  libraryService = inject(LibraryService);
  identityService = inject(IdentityService);
  singleton = inject(SingletonModes);

  ownerModel = signal<OwnerModel|null>(null);
  ownerImgSrc = computed(()=>this.singleton.getUserImageAddress(this.ownerModel()));
  shelfImageAddress = computed(()=>this.libraryService.getShelfImageAddress(this.shelfModel()));

  constructor(){
    effect(()=>{
      if(this.shelfModel()){
        this.libraryService.requestOwnerModel(this.shelfModel().ownerGuid!).subscribe({
          next: res => {
            if(res){
              this.ownerModel.set(res);
            }
          },
        });
      }
    });
  }

  openShelf(){
    this.router.navigate(["/shelf", this.shelfModel().guid]);
  }
}

export class ShelfCardModel{
  constructor(shelfCardModel:ShelfCardModel){
    this.guid = shelfCardModel.guid;
    this.ownerGuid = shelfCardModel.ownerGuid;
    this.title = shelfCardModel.title;
    this.description = shelfCardModel.description;
    this.libraries = shelfCardModel.libraries.map(a=>Object.create(a));
    this.documentCardModels = shelfCardModel.documentCardModels.map(a=>new DocumentCardModel(a));
    this.totalNumberOfShelfDocuments = shelfCardModel.totalNumberOfShelfDocuments;
    this.createdAt = shelfCardModel.createdAt;
    this.hasImage = shelfCardModel.hasImage;
    this.integrityVersion = shelfCardModel.integrityVersion;
    this.isDefault = shelfCardModel.isDefault;
  }

  guid:string = null!;
  ownerGuid:string = null!;
  title:string = null!;
  description?:string;
  libraries:{guid:string, title:string}[] = [];
  documentCardModels:DocumentCardModel[] = [];
  totalNumberOfShelfDocuments:number = 0;
  createdAt:Date = null!;
  hasImage:boolean = false;
  integrityVersion:number = 0;
  isDefault:boolean = false;
}
