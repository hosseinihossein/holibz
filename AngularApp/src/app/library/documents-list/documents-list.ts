import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from "@angular/material/sidenav";
import { DocumentCard, DocumentCardModel } from '../document-card/document-card';
import { LibraryService, OwnerModel } from '../../services/library-service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { MatIcon } from '@angular/material/icon';
import { SingletonModes } from '../../services/singleton-modes';

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
  singleton = inject(SingletonModes);

  documentCardModels = signal<DocumentCardModel[]>([]);

  constructor(){
    
    effect(() => {
      if(this.shelfGuid()){
        this.libraryService.requestDocumentCardList(this.shelfGuid())?.subscribe({
          next: res => {
            if(res){
              this.documentCardModels.set(res);
            }
          },
        });
      }
    });

  }
}
