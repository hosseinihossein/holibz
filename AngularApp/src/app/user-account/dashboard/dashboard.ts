import { Component, inject, signal } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from "@angular/material/sidenav";
import { LibrariesList } from "../../library/libraries-list/libraries-list";
import { MatButtonModule } from '@angular/material/button';
import { RouterLink, RouterOutlet } from "@angular/router";
import { IdentityService } from '../../services/identity-service';
import { MatIcon } from '@angular/material/icon';
import { UserAccountManager } from '../user-account-manager/user-account-manager';

@Component({
  selector: 'app-dashboard',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, MatButtonModule,
    RouterLink, MatIcon, RouterOutlet],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {
  activeSection = signal<null|string>(null);

  identityService = inject(IdentityService);
}
