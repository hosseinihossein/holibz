import { Component, inject, signal } from '@angular/core';
import { ShelfCard, ShelfCardModel } from "../library/shelf-card/shelf-card";
import { LibraryService } from '../services/library-service';
import { SingletonModes } from '../services/singleton-modes';

@Component({
  selector: 'app-home',
  imports: [ShelfCard],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home {
  recentlyAddedDocs_ShelfCardModel = signal<ShelfCardModel|null>(null);

  libraryService = inject(LibraryService);
  singleton = inject(SingletonModes);

  constructor(){
    this.libraryService.requestShelfModel(this.singleton.RecentlyAddedDocuments_ShelfGuid).subscribe({
      next: res => {
        if(res){
          if(res.guid == this.singleton.EmptyGuid){
            res.guid = this.singleton.RecentlyAddedDocuments_ShelfGuid;
          }
          this.recentlyAddedDocs_ShelfCardModel.set(res);
        }
      },
    });
  }
}
