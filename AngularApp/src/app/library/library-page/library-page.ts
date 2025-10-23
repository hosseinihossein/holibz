import { Component, inject, signal } from '@angular/core';
import { ShelvesList } from "../shelves-list/shelves-list";
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { ActivatedRoute } from '@angular/router';
import { LibraryService } from '../../services/library-service';
import { LibraryModel } from '../library-card/library-card';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-library-page',
  imports: [ShelvesList, MatCard, MatCardHeader, MatCardContent, MatCardTitle, MatCardAvatar, 
    MatCardSubtitle, NgOptimizedImage],
  templateUrl: './library-page.html',
  styleUrl: './library-page.css'
})
export class LibraryPage {
  guid = signal<string|null>(null);

  activatedRoute = inject(ActivatedRoute);
  librarySerice = inject(LibraryService);
  libraryModel = signal<LibraryModel|null>(null);

  constructor(){
    let libraryGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("guid");
    if(libraryGuidRouteParam){
      this.guid.set(libraryGuidRouteParam);
    }
    if(!this.guid()){
      if(this.librarySerice.currentLibraryModel()){
        this.libraryModel.set(this.librarySerice.currentLibraryModel());
        this.librarySerice.currentLibraryModel.set(null);
      }
    }
    else{
      this.librarySerice.requestLibraryModel(this.guid()!).subscribe({
        next: res => {
          if(res){
            this.libraryModel.set(res);
          }
        },
      });
    }
  }
}
