import { Component, effect, inject, input, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { RouterLink } from '@angular/router';
import { LibraryCard, LibraryCardModel } from '../library-card/library-card';
import { DocumentCard, DocumentCardModel } from '../document-card/document-card';
import { ShelfCard, ShelfCardModel } from '../shelf-card/shelf-card';
import { MatAccordion } from '@angular/material/expansion';
import { LibraryService } from '../../services/library-service';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';

@Component({
  selector: 'app-generic-list',
  imports: [/*MatButton, MatIcon, RouterLink,*/ MatSidenavModule, LibraryCard, DocumentCard, 
    ShelfCard, MatAccordion, WaitSpinner],
  templateUrl: './generic-list.html',
  styleUrl: './generic-list.css'
})
export class GenericList {
  listType = input.required<"Library"|"Shelf"|"Document">();
  itemGuids = input.required<string[]>();

  isMyList = signal<boolean>(false);
  libraryModels = signal<LibraryCardModel[]>([]);
  shelfModels = signal<ShelfCardModel[]>([]);
  documentCardModels = signal<DocumentCardModel[]>([]);

  libraryService = inject(LibraryService);

  displayWaitSpinner = signal<boolean>(false);

  constructor(){
    effect(()=>{
      if(this.listType() && this.itemGuids()){
          if(this.listType() == "Library"){
            this.getLibraryModels(this.itemGuids()!);
          }
          else if(this.listType() == "Shelf"){
            this.getShelfModels(this.itemGuids()!);
          }
          else if(this.listType() == "Document"){
            this.getDocumentModels(this.itemGuids()!);
          }
        }
    });
  }
  
  getLibraryModels(libGuids:string[]){
    this.libraryModels().length = 0;
    this.shelfModels().length = 0;
    this.documentCardModels().length = 0;
    
    if(libGuids.length > 0){
      libGuids.forEach(libGuid=>{
        this.libraryService.requestLibraryModel(libGuid).subscribe({
          next: libModel => {
            if(libModel){
              this.libraryModels().push(libModel);
            }
          },
        });
      });
    }
  }
  getShelfModels(shelfGuids:string[]){
    this.libraryModels().length = 0;
    this.shelfModels().length = 0;
    this.documentCardModels().length = 0;

    if(shelfGuids.length > 0){
      //console.log(shelfGuids);
      shelfGuids.forEach(shelfGuid=>{
        this.libraryService.requestShelfModel(shelfGuid).subscribe({
          next: shelfModel => {
            if(shelfModel){
              this.shelfModels().push(shelfModel);
            }
          },
        });
      });
    }
  }
  getDocumentModels(docGuids:string[]){
    this.libraryModels().length = 0;
    this.shelfModels().length = 0;
    this.documentCardModels().length = 0;

    if(docGuids.length > 0){
      docGuids.forEach(docGuid=>{
        this.libraryService.requestDocumentCardModel(docGuid).subscribe({
          next: docModel => {
            if(docModel){
              this.documentCardModels().push(docModel);
            }
          },
        });
      });
    }
  }

}
