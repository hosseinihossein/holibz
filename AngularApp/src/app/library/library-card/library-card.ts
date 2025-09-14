import { Component } from '@angular/core';
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";

@Component({
  selector: 'app-library-card',
  imports: [MatCard, MatCardHeader, MatCardAvatar, MatCardTitle, MatCardSubtitle, MatCardContent],
  templateUrl: './library-card.html',
  styleUrl: './library-card.css'
})
export class LibraryCard {
  
}
