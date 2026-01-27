import { AfterViewInit, Component, computed, inject, signal } from '@angular/core';
import { ShelfCard, ShelfCardModel } from "../library/shelf-card/shelf-card";
import { LibraryService } from '../services/library-service';
import { SingletonModes } from '../services/singleton-modes';
import { HomeSearch } from './home-search/home-search';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Result } from '../dialogs/result/result';

@Component({
  selector: 'app-home',
  imports: [ShelfCard,HomeSearch],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements AfterViewInit {
  actiatedRoute = inject(ActivatedRoute);
  libraryService = inject(LibraryService);
  singleton = inject(SingletonModes);
  dialog = inject(MatDialog);

  constructor(){}
  ngAfterViewInit(): void {
    this.actiatedRoute.data.subscribe(data=>{
      if(data){
        if(data["accessDenied"]){
          this.dialog.open(Result,{data:{
            status:"warning",
            title:"Access Denied!",
            description: [
              "You're Not Allowed to access the specified route!"
            ],
          }});
        }
      }
    });
  }
}
