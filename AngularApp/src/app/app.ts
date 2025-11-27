import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { IconService } from './services/icon-service';
import { NavBar } from './nav-bar/nav-bar';
import { Review } from './review/review';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavBar,Review],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  iconService = inject(IconService)
}
