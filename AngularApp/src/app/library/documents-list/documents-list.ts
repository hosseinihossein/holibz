import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from "@angular/material/sidenav";
import { DocumentCard, DocumentCardModel } from '../document-card/document-card';
import { LibraryService } from '../../services/library-service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-documents-list',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, DocumentCard, ],
  templateUrl: './documents-list.html',
  styleUrl: './documents-list.css'
})
export class DocumentsList {
  shelfGuid = input.required<string>();

  libraryService = inject(LibraryService);
  activatedRoute = inject(ActivatedRoute);
  identityService = inject(IdentityService);

  documentCardModels = signal<DocumentCardModel[]>([]);
  userModel = signal<UserProfileModel|null>(null);
  userImgSrc = computed(()=>this.userModel()?.imageAddress);
  isMyDocumentList = computed(() => this.identityService.isAuthenticated() && 
  this.identityService.userModel()?.guid === this.userModel()?.guid);
  //totalNumberOfShelfDocuments = signal(0);

  constructor(){
    this.userModel.set(this.libraryService.currentOwnerUserModel());
    //this.totalNumberOfShelfDocuments.set(this.libraryService.currentShelfModel()?.totalNumberOfShelfDocuments!);

    effect(() => {
      this.libraryService.requestDocumentCardList(this.shelfGuid())?.subscribe({
        next: res => {
          if(res){
            this.documentCardModels.set(res);
          }
        },
      });
    });
    
    effect(() => {
      if(!this.userModel() && this.documentCardModels() && this.documentCardModels().length > 0){
        this.identityService.requestUserModel(this.documentCardModels()[0].ownerGuid!).subscribe({
          next: res => {
            this.userModel.set(res);
          },
        });
      }
    });

    /*effect(() => {
      if(this.isMyDocumentList()){
        this.identityService.getCsrf().subscribe({
          next: () => {
            console.log("Csrf received successfully.");
          },
          error: err => {
            console.error("Couldn't get Csrf!");
            throw(err);
          },
        });
      }
    });*/
  }
}
