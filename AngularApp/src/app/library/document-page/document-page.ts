import { AfterViewInit, Component, computed, effect, ElementRef, inject, input, Renderer2, signal, viewChild, viewChildren } from '@angular/core';
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
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EditElementFormModel, LibraryService, NewElementFormModel } from '../../services/library-service';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { DocumentCardModel } from '../document-card/document-card';
import { ShelfCardModel } from '../shelf-card/shelf-card';
import { NgOptimizedImage } from '@angular/common';
import { Result } from '../../dialogs/result/result';
import { EditHeader } from '../../dialogs/edit-header/edit-header';
import { EditCode } from '../../dialogs/edit-code/edit-code';
import { EditParagraph } from '../../dialogs/edit-paragraph/edit-paragraph';
import { EditLink } from '../../dialogs/edit-link/edit-link';
import { EditImageTitle } from '../../dialogs/edit-image-title/edit-image-title';
import { EditFile } from '../../dialogs/edit-file/edit-file';
import { LargeImg } from '../../dialogs/large-img/large-img';

@Component({
  selector: 'app-document-page',
  imports: [MatSidenavModule, MatExpansionModule, MatTooltip, MatButton, MatIcon,
    MatMenu, MatMenuItem, MatMenuTrigger, DocumentElement, MatChipSet, MatChip, RouterLink,
  NgOptimizedImage],
  templateUrl: './document-page.html',
  styleUrl: './document-page.css'
})
export class DocumentPage implements AfterViewInit {
  documentGuid = signal<string|null>(null);
  documentPageModel = signal<DocumentPageModel|null>(null);
  sortedElements = computed(()=>
    this.documentPageModel()?.elements.sort((a,b)=>{if(a.order > b.order)return 1;else return -1;})
  );

  ownerModel = signal<UserProfileModel|null>(null);
  ownerImgSrc = computed(() => this.ownerModel()?.imageAddress);

  displaySubmitSpinner = signal(false);

  //clipboard = inject(Clipboard);
  windowService = inject(WindowService);
  readonly dialog = inject(MatDialog);
  singletonModes = inject(SingletonModes);
  activatedRoute = inject(ActivatedRoute);
  libraryService = inject(LibraryService);
  identityService = inject(IdentityService);
  renderer = inject(Renderer2);
  router = inject(Router);

  headingElements = signal<HTMLHeadingElement[]>([]);
  introductionHeadint = viewChild.required<ElementRef<HTMLHeadingElement>>("introductionHeading");
  
  constructor(){
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
        this.identityService.requestUserModel(this.documentPageModel()!.owner.userGuid).subscribe({
          next: res => {
            if(res){
              this.ownerModel.set(res);
            }
          },
        });
      }
    });

    effect(()=>{
      if(this.headingElements()){
        const viewPortObserver = new IntersectionObserver((entries) => {
          entries.forEach(entry => {
            let overviewHeader = this.windowService.nativeWindow.document.getElementById("overview-"+entry.target.id);
            if(overviewHeader){
              if (entry.isIntersecting) {
                this.renderer.addClass(overviewHeader!, "active");
              } else {
                this.renderer.removeClass(overviewHeader!, "active");
              }
            }
          });
        });
  
        for(let heading of this.headingElements()){
          viewPortObserver.observe(heading);
        }
      }
    });
    
  }
  ngAfterViewInit(): void {
    if(this.introductionHeadint()){
      this.headingElements.update(elements=>[...elements, this.introductionHeadint().nativeElement]);
    }
  }

  onHeadingInit(headingElement: HTMLHeadingElement){
    this.headingElements.update(elements=>[...elements, headingElement]);
  }

  confirmDelete(){
    const dialogRef = this.dialog.open(ConfirmDelete);
    dialogRef.afterClosed().subscribe(result=>{
      if(result === true){
        this.displaySubmitSpinner.set(true);
        this.libraryService.requestDeleteDocument(this.documentPageModel()?.guid!).subscribe({
          next: res => {
            if(res && res.success){
              this.router.navigate(['/profile']);
            }
          },
          error: err => {
            this.dialog.open(Result,{
              //panelClass: "success-ResultStatus", 
              data:{
                status: "warning",
                title: "Error in document deletion",
                description: ["Something went wrong in document deletion",
                  JSON.stringify(err)
                ],
              }
            });
          },
        });
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

  addNewElement(type:"h1" | "h2" | "p" | "img" | "code" | "file" | "link"){
    if(this.documentPageModel()){
      let newElementFormModel: NewElementFormModel|null = null;

      if(type === "h1" || type === "h2"){
        const dialogRef = this.dialog.open(EditHeader, {data:{value:type === "h1"?"New Main Heading":"New SubHeading"}});
        dialogRef.afterClosed().subscribe(result=>{
          if(result){
            newElementFormModel = {
              DocumentGuid: this.documentPageModel()?.guid,
              Order: this.documentPageModel()?.elements.length.toString(),
              Type: type,
              Value: result,
            };
            this.requestForNewElement(newElementFormModel);
          }
        });
      }
      else if(type === "code"){
        const dialogRef = this.dialog.open(EditCode,{data:{value:"New Code"}});
        dialogRef.afterClosed().subscribe(result=>{
          if(result){
            newElementFormModel = {
              DocumentGuid: this.documentPageModel()?.guid,
              Order: this.documentPageModel()?.elements.length.toString(),
              Type: type,
              Value: result,
            };
            this.requestForNewElement(newElementFormModel);
          }
        });
      }
      else if(type === "p"){
        const dialogRef = this.dialog.open(EditParagraph, {data:{value:"New Paragraph"}});
        dialogRef.afterClosed().subscribe(result => {
          if(result){
            newElementFormModel = {
              DocumentGuid: this.documentPageModel()?.guid,
              Order: this.documentPageModel()?.elements.length.toString(),
              Type: type,
              Value: result,
            };
            this.requestForNewElement(newElementFormModel);
          }
        });
      }
      else if(type === "link"){
        const dialogRef = this.dialog.open(EditLink, {data:{title:"New Link", value:""}});
        dialogRef.afterClosed().subscribe(result=>{
          if(result){
            newElementFormModel = {
              DocumentGuid: this.documentPageModel()?.guid,
              Order: this.documentPageModel()?.elements.length.toString(),
              Type: type,
              Value: result.value,
              Title: result.title,
            };
            this.requestForNewElement(newElementFormModel);
          }
        });
      }
      else if(type === "img"){
        const dialogRef = this.dialog.open(EditImageTitle, {data:{title:"Image Title", value:""}});
        dialogRef.afterClosed().subscribe(result=>{
          if(result){
            newElementFormModel = {
              DocumentGuid: this.documentPageModel()?.guid,
              Order: this.documentPageModel()?.elements.length.toString(),
              Type: type,
              Title: result.title,
              File: result.file,
            };
            this.requestForNewElement(newElementFormModel);
          }
        });
      }
      else if(type === "file"){
        const dialogRef = this.dialog.open(EditFile, {data:{title:"File Title", value:""}});
        dialogRef.afterClosed().subscribe(result=>{
          if(result){
            newElementFormModel = {
              DocumentGuid: this.documentPageModel()?.guid,
              Order: this.documentPageModel()?.elements.length.toString(),
              Type: type,
              Title: result.title,
              File: result.file,
            };
            this.requestForNewElement(newElementFormModel);
          }
        });
      }
    }
  }
  private requestForNewElement(newElementFormModel: NewElementFormModel){
    this.libraryService.createNewElement(newElementFormModel).subscribe({
      next: res => {
        if(res){
          this.documentPageModel()?.elements.push(res);
        }
      },
      error: err => {
        this.dialog.open(Result,{
          data:{
            status: "warning",
            title: "Error in adding element",
            description: ["Something went wrong when adding the new elemenet!",
              JSON.stringify(err)
            ],
          }
        });
      },
    });
  }

  openLargeImage(){
    if(this.documentPageModel()?.hasImage){
      this.dialog.open(LargeImg, {
        data:{
          imgSrc:`/api/Library/DocumentImage?documentGuid=${this.documentPageModel()?.guid}`, 
          imgTitle: this.documentPageModel()?.title
        }
      });
    }
  }

  onDeleteElement(elementGuid:string){
    if(this.documentPageModel()){
      let elementIndex = this.documentPageModel()!.elements.findIndex(el=>el.guid === elementGuid);
      if(elementIndex >= 0){
        this.documentPageModel.update(dpm=>{
          dpm!.elements.splice(elementIndex,1);
          return dpm;
        });
      }
    }
  }
  onEditElement(editedElement:DocumentElementModel){
    if(this.documentPageModel()){
      let elementIndex = this.documentPageModel()!.elements.findIndex(el=>el.guid === editedElement.guid);
      if(elementIndex >= 0){
        this.documentPageModel.update(dpm=>{
          dpm!.elements.splice(elementIndex,1,editedElement);
          return dpm;
        });
      }
    }
  }
  saveEditsOnServer(){
    if(this.documentPageModel()){
      this.libraryService.submitEditedElements().subscribe({
        next: res => {
          if(res && res.success){
            this.documentPageModel.update(dpm=>{
              dpm!.elements = res.elements;
              return dpm;
            });
            this.libraryService.editedElements().clear();
          }
        },
      });
    }
  }

}

export class DocumentPageModel {
  guid:string = null!;
  owner:{userGuid:string, userName:string} = null!;
  title:string = null!;
  hasImage:boolean = false;
  description:string = null!;
  version:string = null!;
  relatedVersions:{versionName:string, documentGuid:string}[] = [];
  shelves:{guid:string, title:string, libraryTitle:string, 
    owner:{userGuid:string, userName:string},
    documents:{guid:string, title:string}[]}[] = [];
  elements:DocumentElementModel[] = [];
  tags:string[] = [];
  createdAt:Date = null!;
} 