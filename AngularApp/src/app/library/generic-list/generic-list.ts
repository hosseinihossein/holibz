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
  isFavorite = input<boolean>(false);
  parentGuid = input<string>();
  itemGuids = input<string[]>();//can be used ffor search component

  isMyList = signal<boolean>(false);
  libraryModels = signal<LibraryCardModel[]>([]);
  shelfModels = signal<ShelfCardModel[]>([]);
  documentCardModels = signal<DocumentCardModel[]>([]);

  libraryService = inject(LibraryService);

  displayWaitSpinner = signal<boolean>(false);

  constructor(){
    effect(()=>{
      if(this.listType()){
        if(this.parentGuid()){
          if(this.listType() === "Library"){
            if(this.isFavorite()){
              this.libraryService.requestFavotiteLibrariesGuids(this.parentGuid()!).subscribe({
                next: libGuids => {
                  if(libGuids){
                    this.getLibraryModels(libGuids);
                  }
                },
              });
            }
            else{
              this.libraryService.requestLibrariesGuids(this.parentGuid()!).subscribe({
                next: libGuids => {
                  if(libGuids){
                    this.getLibraryModels(libGuids);
                  }
                },
              });
            }
          }
          else if(this.listType() === "Shelf"){
            if(this.isFavorite()){
              this.libraryService.requestFavotiteShelvesGuids(this.parentGuid()!).subscribe({
                next: shelfGuids => {
                  this.getShelfModels(shelfGuids);
                }
              });
            }
            else{
              this.libraryService.requestShelvesGuids(this.parentGuid()!).subscribe({
                next: shelfGuids => {
                  if(shelfGuids){
                    this.getShelfModels(shelfGuids);
                  }
                },
              });
            }
          }
          else if(this.listType() === "Document"){
            if(this.isFavorite()){
              this.libraryService.requestFavotiteDocumentsGuids(this.parentGuid()!).subscribe({
                next: docGuids => {
                  this.getDocumentModels(docGuids);
                }
              });
            }
            else{
              this.libraryService.requestDocumentsGuids(this.parentGuid()!).subscribe({
                next: docGuids => {
                  if(docGuids){
                    this.getDocumentModels(docGuids);
                  }
                },
              });
            }
          }
        }
        else if(this.itemGuids()) {
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
      }
    });
  }
  
  getLibraryModels(libGuids:string[]){
    libGuids.forEach(libGuid=>{
      this.libraryService.requestLibraryModel(libGuid).subscribe({
        next: libModel => {
          if(libModel){
            this.libraryModels.set([...this.libraryModels(), libModel]);
          }
        },
      });
    });
  }
  getShelfModels(shelfGuids:string[]){
    shelfGuids.forEach(shelfGuid=>{
      this.libraryService.requestShelfModel(shelfGuid).subscribe({
        next: shelfModel => {
          if(shelfModel){
            this.shelfModels.set([...this.shelfModels(), shelfModel]);
          }
        },
      });
    });
  }
  getDocumentModels(docGuids:string[]){
    docGuids.forEach(docGuid=>{
      this.libraryService.requestDocumentCardModel(docGuid).subscribe({
        next: docModel => {
          if(docModel){
            this.documentCardModels.set([...this.documentCardModels(), docModel]);
          }
        },
      });
    });
  }

}
