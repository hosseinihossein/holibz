import { Component } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from '@angular/material/sidenav';
import { ShelfCard } from "../shelf-card/shelf-card";
import { MatAccordion } from '@angular/material/expansion';

@Component({
  selector: 'app-shelves-list',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, ShelfCard, MatAccordion],
  templateUrl: './shelves-list.html',
  styleUrl: './shelves-list.css'
})
export class ShelvesList {

}
