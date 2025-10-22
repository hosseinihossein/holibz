import { Component, inject, signal } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from "@angular/material/sidenav";
import { LibrariesList } from "../../library/libraries-list/libraries-list";
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from "@angular/router";
import { IdentityService } from '../../services/identity-service';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-dashboard',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, LibrariesList, MatButtonModule, 
    RouterLink, MatIcon],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  activeSection = signal<null|"MyLibraries">(null);

  identityService = inject(IdentityService);
}
