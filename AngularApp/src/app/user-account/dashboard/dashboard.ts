import { Component, signal } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from "@angular/material/sidenav";
import { LibrariesList } from "../../library/libraries-list/libraries-list";
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'app-dashboard',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, LibrariesList, MatButton],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  activeSection = signal<null|"MyLibraries">(null);
}
