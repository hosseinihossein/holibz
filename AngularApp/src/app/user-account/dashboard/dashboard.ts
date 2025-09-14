import { Component } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from "@angular/material/sidenav";
import { LibrariesList } from "../../library/libraries-list/libraries-list";
import { Profile } from "../profile/profile";

@Component({
  selector: 'app-dashboard',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, Profile],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard {

}
