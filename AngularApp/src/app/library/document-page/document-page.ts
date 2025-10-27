import { AfterViewInit, Component, computed, effect, ElementRef, inject, input, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIcon } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatTooltip } from '@angular/material/tooltip';
import { WindowService } from '../../services/window-service';
import { MatDialog } from '@angular/material/dialog';
import { MatMenu, MatMenuItem, MatMenuTrigger } from "@angular/material/menu";
import { ConfirmDelete } from '../../dialogs/confirm-delete/confirm-delete';
import { Clipboard } from '@angular/cdk/clipboard';
import { SingletonModes } from '../../services/singleton-modes';
import { DocumentService } from '../../services/document-service';
import { DocumentElement, DocumentElementModel } from './document-elements/document-element/document-element';
import { MatChip, MatChipSet } from "@angular/material/chips";
import { EditTags } from '../../dialogs/edit-tags/edit-tags';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LibraryService } from '../../services/library-service';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { DocumentCardModel } from '../document-card/document-card';
import { ShelfCardModel } from '../shelf-card/shelf-card';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-document-page',
  imports: [MatSidenavModule, MatExpansionModule, MatTooltip, MatButton, MatIcon,
    MatMenu, MatMenuItem, MatMenuTrigger, DocumentElement, MatChipSet, MatChip, RouterLink,
  NgOptimizedImage],
  templateUrl: './document-page.html',
  styleUrl: './document-page.css'
})
export class DocumentPage implements AfterViewInit {
  //documentGuid = input.required<string>();
  documentGuid = signal<string|null>(null);
  //containerShelves = signal<ShelfCardModel[]|null>(null);
  //documentElements = signal<DocumentElementModel[]|null>(null);
  documentPageModel = signal<DocumentPageModel|null>(null);

  //ownerGuid = signal<string|null>(null);
  ownerModel = signal<UserProfileModel|null>(null);
  ownerImgSrc = computed(() => this.ownerModel()?.imageAddress);

  //clipboard = inject(Clipboard);
  windowService = inject(WindowService);
  readonly dialog = inject(MatDialog);
  singletonModes = inject(SingletonModes);
  elementRef = inject(ElementRef);
  //documentService = inject(DocumentService);
  activatedRoute = inject(ActivatedRoute);
  libraryService = inject(LibraryService);
  identityService = inject(IdentityService);

  sortedElements = computed(()=>this.documentPageModel()?.elements.sort((a,b)=>{if(a.order > b.order)return 1;else return -1;}));
  
  constructor(){
    console.log("app-document constructor!");
    
    let documentGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("documentGuid");
    if(documentGuidRouteParam){
      this.documentGuid.set(documentGuidRouteParam);
    }

    effect(() => {
      if(this.documentGuid()){
        this.libraryService.requestDocumentPageModel(this.documentGuid()!).subscribe({
          next: res => {
            if(res){
              this.documentPageModel.set(res);
            }
          },
        });
      }
    });

    effect(() => {
      if(this.documentPageModel()){
        this.identityService.requestUserModel(this.documentPageModel()!.ownerGuid).subscribe({
          next: res => {
            if(res){
              this.ownerModel.set(res);
            }
          },
        });
      }
    });
  }

  ngAfterViewInit(): void {
    console.log("app-document after view init!");
    //a better prefered approach renderer2
    const viewPortObserver = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          this.windowService.nativeWindow.document.getElementById("overview-"+entry.target.id)?.classList.add("active");
        } else {
          this.windowService.nativeWindow.document.getElementById("overview-"+entry.target.id)?.classList.remove("active");
        }
      });
    });

    const headers = (this.elementRef.nativeElement as HTMLElement).getElementsByClassName("headerSection");
    for(let header of headers){
      viewPortObserver.observe(header);
    }
  }

  confirmDelete(){
    const dialogRef = this.dialog.open(ConfirmDelete);
    dialogRef.afterClosed().subscribe(result=>{
      if(result === "true"){
        console.log("this document has been deleted!");
      }
    });
  }

  editTags(){
    const dialogRef = this.dialog.open(EditTags);
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        //this.documentService.updateDocumentTags(result);
      }
    });
  }

  addNewSection(type:"h1" | "h2" | "p" | "img" | "code" | "file" | "link"){
    //this.documentService.mockAddSection(type);
  }

}

export class DocumentPageModel {
  guid:string = null!;
  ownerGuid:string = null!;
  title:string = null!;
  hasImage:boolean = false;
  description:string = null!;
  version:string = null!;
  relatedVersions?:{versionName:string, documentGuid:string}[]
  shelves:{guid:string, title:string, description?:string, 
    documents:{guid:string, title:string, description?:string}[]}[] = [];
  elements:DocumentElementModel[] = [];
  tags:string[] = [];
  createdAt:Date = null!;
} 