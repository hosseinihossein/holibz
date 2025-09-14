import { Component } from '@angular/core';
import { ShelvesList } from "../shelves-list/shelves-list";
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";

@Component({
  selector: 'app-library-page',
  imports: [ShelvesList, MatCard, MatCardHeader, MatCardContent, MatCardTitle, MatCardAvatar, 
    MatCardSubtitle],
  templateUrl: './library-page.html',
  styleUrl: './library-page.css'
})
export class LibraryPage {

}
