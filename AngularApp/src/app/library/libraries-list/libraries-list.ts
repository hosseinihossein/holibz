import { Component, effect, inject, input, signal } from '@angular/core';
import { LibraryCard, LibraryCardModel } from "../library-card/library-card";
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { MatTooltip } from '@angular/material/tooltip';
import { SingletonModes } from '../../services/singleton-modes';
import { LibraryService } from '../../services/library-service';

@Component({
  selector: 'app-libraries-list',
  imports: [LibraryCard, MatButton, MatIcon, MatBadge,MatTooltip],
  templateUrl: './libraries-list.html',
  styleUrl: './libraries-list.css'
})
export class LibrariesList {
  userGuid = input<string|null>(null);

  singletonModes = inject(SingletonModes);
  libraryService = inject(LibraryService);

  libraryCards = signal<LibraryCardModel[]>([]);

  constructor(){
    effect(()=>{
      this.libraryService.requestLibraries(this.userGuid())?.subscribe({
        next: res => {
          this.libraryCards.set(res);
        },
      });
    });
  }

  

}
