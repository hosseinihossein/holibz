import { Component, inject, input, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { RouterLink } from '@angular/router';
import { LibraryCard, LibraryCardModel } from '../library-card/library-card';
import { DocumentCard, DocumentCardModel } from '../document-card/document-card';
import { ShelfCard, ShelfCardModel } from '../shelf-card/shelf-card';
import { MatAccordion } from '@angular/material/expansion';
import { LibraryService } from '../../services/library-service';

@Component({
  selector: 'app-generic-list',
  imports: [MatButton, MatIcon, RouterLink, MatSidenavModule, LibraryCard, DocumentCard, 
    ShelfCard, MatAccordion],
  templateUrl: './generic-list.html',
  styleUrl: './generic-list.css'
})
export class GenericList {
  listType = input.required<string>();
  parentGuid = input<string>();
  isFavorite = input<boolean>(false);

  isMyList = signal<boolean>(false);
  libraryModels = signal<LibraryCardModel[]>([]);
  shelfModels = signal<ShelfCardModel[]>([]);
  documentCardModels = signal<DocumentCardModel[]>([]);

  libraryService = inject(LibraryService);

  constructor(){
    effect(()=>{
      if(this.listType() && this.parentGuid()){
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

          }
          else{
            
          }
        }
        else if(this.listType() === "Document"){
          
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

}
