import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { IconService } from './services/icon-service';
import { NavBar } from './nav-bar/nav-bar';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavBar],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  iconService = inject(IconService)
}
