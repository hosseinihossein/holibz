import { Component, inject } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from "@angular/material/sidenav";
import { DocumentCard } from '../document-card/document-card';

@Component({
  selector: 'app-documents-list',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, DocumentCard, ],
  templateUrl: './documents-list.html',
  styleUrl: './documents-list.css'
})
export class DocumentsList {
  
}
