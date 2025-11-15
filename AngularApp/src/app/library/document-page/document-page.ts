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
import { DocumentPageService } from './document-page-service';
import { MatBadge } from "@angular/material/badge";
import { ConfirmChange } from '../../dialogs/confirm-change/confirm-change';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { EditInput } from '../../dialogs/edit-input/edit-input';
import { EditTextarea } from '../../dialogs/edit-textarea/edit-textarea';
import { EditIntroduction } from '../../dialogs/edit-introduction/edit-introduction';
import { ParentEditor } from '../../dialogs/parent-editor/parent-editor';

@Component({
  selector: 'app-document-page',
  imports: [MatSidenavModule, MatExpansionModule, MatTooltip, MatButton, MatIcon,
    MatMenu, MatMenuItem, MatMenuTrigger, DocumentElement, MatChipSet, MatChip, RouterLink,
    NgOptimizedImage, MatBadge,MatProgressSpinner],
  templateUrl: './document-page.html',
  styleUrl: './document-page.css',
  providers: [DocumentPageService]
})
export class DocumentPage implements AfterViewInit {
  //clipboard = inject(Clipboard);
  windowService = inject(WindowService);
  readonly dialog = inject(MatDialog);
  //singletonModes = inject(SingletonModes);
  activatedRoute = inject(ActivatedRoute);
  libraryService = inject(LibraryService);
  identityService = inject(IdentityService);
  renderer = inject(Renderer2);
  router = inject(Router);
  documentPageService = inject(DocumentPageService);

  documentGuid = signal<string|null>(null);
  sortedElements = computed(()=>
    this.documentPageService.documentPageModel()?.elements.sort((a,b)=>{
      if(a.order > b.order)return 1;else return -1;
    })
  );

  introductionImageVersion = signal(0);
  introductionImage = computed(()=>`/api/Library/DocumentImage?documentGuid=${this.documentPageService.documentPageModel()?.guid}&v=${this.introductionImageVersion()}`);

  ownerModel = signal<UserProfileModel|null>(null);
  ownerImgSrc = computed(() => this.ownerModel()?.imageAddress);
  isMyDocument = computed(()=>this.ownerModel()?.guid === this.identityService.userModel()?.guid);

  displaySubmitSpinner = signal(false);

  headingElements = signal<HTMLHeadingElement[]>([]);
  introductionHeading = viewChild.required<ElementRef<HTMLHeadingElement>>("introductionHeading");

  shelfGuidToParentLibrariesTitlesMap = computed<Map<string,string>>(()=>{
    let map = new Map<string,string>();
    this.documentPageService.documentPageModel()?.shelves.forEach(shelf=>
      map.set(shelf.guid, shelf.libraries.map(l=>l.title).slice(0,3).join(','))
    );
    return map;
  });
  parentLibraryGuids = computed<string[]>(()=>{
    let allParentLibraries = this.documentPageService.documentPageModel()?.shelves.flatMap(shelf=>shelf.libraries);
    let uniqueParentLibraries:{guid:string,title:string}[] = [];
    allParentLibraries?.forEach(pl=>{
      if(!uniqueParentLibraries.map(upl=>upl.guid).includes(pl.guid)){
        uniqueParentLibraries.push(pl);
      }
    });
    return uniqueParentLibraries.map(upl=>upl.guid);
  });
  
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
              this.documentPageService.documentPageModel.set(res);
              this.documentPageService.unchangedDocumentPageModel.set(new DocumentPageModel(res));
              /*res.elements.forEach(el=>{
                console.log(el.value + " : " + el.order);
              });*/
            }
          },
        });
      }
    });

    effect(() => {
      if(this.documentPageService.documentPageModel()){
        this.identityService.requestUserModel(this.documentPageService.documentPageModel()!.owner.userGuid).subscribe({
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
    if(this.introductionHeading()){
      this.headingElements.update(elements=>[...elements, this.introductionHeading().nativeElement]);
    }
  }

  onHeadingInit(headingElement: HTMLHeadingElement){
    this.headingElements.update(elements=>[...elements, headingElement]);
  }

  confirmDelete(){
    if(this.isMyDocument()){
      const dialogRef = this.dialog.open(ConfirmDelete,{
        data:{
          title: this.documentPageService.documentPageModel()?.title,
          type: "Documents",
        }
      });
      dialogRef.afterClosed().subscribe(result=>{
        if(result === true){
          this.displaySubmitSpinner.set(true);
          this.libraryService.requestDeleteDocument(this.documentPageService.documentPageModel()?.guid!).subscribe({
            next: res => {
              if(res && res.success){
                this.displaySubmitSpinner.set(false);
                this.router.navigate(['/profile']);
              }
            },
            error: err => {
              this.dialog.open(Result,{
                //panelClass: "success-ResultStatus", 
                data:{
                  status: "warning",
                  title: "Error in document deletion",
                  description: ["Something went wrong during deleting the document",
                    JSON.stringify(err)
                  ],
                }
              }).afterClosed().subscribe(()=>this.displaySubmitSpinner.set(false));
              throw(err);
            },
          });
        }
      });
    }
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
    if(this.documentPageService.documentPageModel()){
      let newElementFormModel: NewElementFormModel|null = null;

      if(type === "h1" || type === "h2"){
        const dialogRef = this.dialog.open(EditHeader, {data:{value:type === "h1"?"New Main Heading":"New SubHeading"}});
        dialogRef.afterClosed().subscribe(result=>{
          if(result){
            newElementFormModel = {
              DocumentGuid: this.documentPageService.documentPageModel()?.guid,
              Order: this.documentPageService.documentPageModel()?.elements.length.toString(),
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
              DocumentGuid: this.documentPageService.documentPageModel()?.guid,
              Order: this.documentPageService.documentPageModel()?.elements.length.toString(),
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
              DocumentGuid: this.documentPageService.documentPageModel()?.guid,
              Order: this.documentPageService.documentPageModel()?.elements.length.toString(),
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
              DocumentGuid: this.documentPageService.documentPageModel()?.guid,
              Order: this.documentPageService.documentPageModel()?.elements.length.toString(),
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
              DocumentGuid: this.documentPageService.documentPageModel()?.guid,
              Order: this.documentPageService.documentPageModel()?.elements.length.toString(),
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
              DocumentGuid: this.documentPageService.documentPageModel()?.guid,
              Order: this.documentPageService.documentPageModel()?.elements.length.toString(),
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
          this.documentPageService.documentPageModel()?.elements.push(res);
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
    if(this.documentPageService.documentPageModel()?.hasImage){
      this.dialog.open(LargeImg, {
        data:{
          imgSrc:this.introductionImage(), 
          imgTitle: this.documentPageService.documentPageModel()?.title
        }
      });
    }
  }

  saveEditsOnServer(){
    if(this.documentPageService.documentPageModel() && 
    this.documentPageService.editedElementFormModels().size > 0 &&
    this.isMyDocument()){
      this.displaySubmitSpinner.set(true);
      this.libraryService.submitEditedElements(this.documentPageService.getEditElementFormModelArray()).subscribe({
        next: res => {
          if(res && res.success){
            this.documentPageService.documentPageModel.update(dpm=>{
              for(let editedElement of res.elements){
                let elIndex = dpm!.elements.findIndex(el=>el.guid === editedElement.guid);
                dpm!.elements.splice(elIndex,1,editedElement);
              }
              return dpm;
            });

            this.documentPageService.unchangedDocumentPageModel.set(
              new DocumentPageModel(this.documentPageService.documentPageModel()!)
            );
            this.documentPageService.editedElementFormModels().clear();

            this.displaySubmitSpinner.set(false);
          }
        },
        error: err => {
          let errorMessage = "";
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error?.Owner){
              errorMessage += err.error?.Owner;
            }
            if(err.error?.ParentDocument){
              errorMessage += err.error?.ParentDocument;
            }
            if(err.error?.Guid || err.error?.errors?.Guid){
              errorMessage += err.error?.Guid || err.error?.errors?.Guid;
            }
          }
          else{
            errorMessage = "Something went wrong during saving the changes!";
          }

          this.dialog.open(Result,{data:{
            status: "warning",
            title: "Error in saving changes",
            description: [errorMessage],
          }});

          throw(err);
        },
      });
    }
  }

  confirmExitEditModeWithoutSaving(){
    if(this.documentPageService.editedElementFormModels().size > 0){
      this.dialog.open(ConfirmChange,{
        data:{change:"exit edit mode without saving the changes"}
      }).afterClosed().subscribe(result=>{
        if(result === "yes"){
          this.documentPageService.editedElementFormModels().clear();
          this.documentPageService.documentPageModel.set(
            new DocumentPageModel(this.documentPageService.unchangedDocumentPageModel()!)
          );
          //console.log(JSON.stringify(this.documentPageService.documentPageModel()?.elements));
          //console.log(JSON.stringify(this.documentPageService.unchangedDocumentPageModel()?.elements));
          
          this.documentPageService.toggleEditMode();
        }
      });
    }
    else{
      this.documentPageService.toggleEditMode();
    }
  }

  enterEditMode(){
    if(this.isMyDocument()){
      this.identityService.getCsrf().subscribe({
        next: () => {
          console.log("csrf token recieved successfully.");
        },
        error: err => {
          console.log("couldn't get csrf token");
          throw(err);
        }
      });

      this.documentPageService.toggleEditMode();
    }
  }

  /*editTitle(){
    if(this.isMyDocument() && this.documentPageService.documentPageModel()){
      this.dialog.open(EditInput,{
        data:{
          label: "Title",
          value: this.documentPageService.documentPageModel()?.title,
        }
      }).afterClosed().subscribe(result=>{
        if(result){
          this.libraryService.requestEditDocumentTitle(
            this.documentPageService.documentPageModel()!.guid,
            result
          ).subscribe({
            next: res => {
              if(res && res.success){
                this.documentPageService.documentPageModel.update(dpm=>{
                  dpm!.title = result;
                  return dpm;
                });
                this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
                  dpm!.title = result;
                  return dpm;
                });
              }
            },
          });
        }
      });
    }
  }
  editImage(){

  }
  editDescription(){
    if(this.isMyDocument() && this.documentPageService.documentPageModel()){
      this.dialog.open(EditTextarea,{
        data:{
          label: "Brief Introduction",
          value: this.documentPageService.documentPageModel()?.description,
        }
      }).afterClosed().subscribe(result=>{
        if(result){
          this.libraryService.requestEditDocumentDescription(
            this.documentPageService.documentPageModel()!.guid,
            result
          ).subscribe({
            next: res => {
              if(res && res.success){
                this.documentPageService.documentPageModel.update(dpm=>{
                  dpm!.description = result;
                  return dpm;
                });
                this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
                  dpm!.description = result;
                  return dpm;
                });
              }
            },
          });
        }
      });
    }
  }*/

  editIntroduction(){
    if(this.isMyDocument()){
      this.dialog.open(EditIntroduction,{data:{
        introductionOf:"document", 
        title: this.documentPageService.documentPageModel()!.title,
        description: this.documentPageService.documentPageModel()!.description,
        imageSrc: this.documentPageService.documentPageModel()?.hasImage ? this.introductionImage() : undefined,
        guid: this.documentPageService.documentPageModel()!.guid
      }}).afterClosed().subscribe(result=>{
        if(result){
          if(result === "ImageDelete"){
            this.documentPageService.documentPageModel.update(dpm=>{
              dpm!.hasImage = false;
              return dpm;
            });
            this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
              dpm!.hasImage = false;
              return dpm;
            });
          }
          else{
            this.documentPageService.documentPageModel.update(dpm=>{
              dpm!.title = result.title;
              dpm!.description = result.description;
              dpm!.hasImage = result.imageChanged ?? dpm!.hasImage;
              return dpm;
            });
            this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
              dpm!.title = result.title;
              dpm!.description = result.description;
              dpm!.hasImage = result.imageChanged ?? dpm!.hasImage;
              return dpm;
            });
            
            if(result.imageChanged){
              this.introductionImageVersion.update(v=>{return ++v;});
            }
          }
        }
      });
    }
  }

  editParentShelves(){
    if(this.isMyDocument()){
      this.dialog.open(ParentEditor,{data:{
        parentOf:"document",
        parentLibraryGuids: this.parentLibraryGuids(),
        parentShelfGuids: this.documentPageService.documentPageModel()?.shelves.map(shelf=>shelf.guid),
        childGuid: this.documentPageService.documentPageModel()?.guid,
      }}).afterClosed().subscribe(result=>{
        if(result){
          this.documentPageService.documentPageModel.update(dpm=>{
            dpm!.shelves = result;
            return dpm;
          });
          this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
            dpm!.shelves = result;
            return dpm;
          });
        }
      });
    }
  }
  
}

export class DocumentPageModel {
  constructor(documentPageModel:DocumentPageModel){
    this.guid = documentPageModel.guid;
    this.owner = documentPageModel.owner;
    this.title = documentPageModel.title;
    this.hasImage = documentPageModel.hasImage;
    this.description = documentPageModel.description;
    this.version = documentPageModel.version;
    this.relatedVersions = [...(documentPageModel.relatedVersions.map(a=>Object.create(a)))];
    this.shelves = documentPageModel.shelves;//[...(documentPageModel.shelves.map(a=>Object.create(a)))];
    this.elements = [...(documentPageModel.elements.map(a=>new DocumentElementModel(a)))];
    this.tags = [...documentPageModel.tags];
    this.createdAt = documentPageModel.createdAt;
  }

  guid:string = null!;
  owner:{userGuid:string, userName:string} = null!;
  title:string = null!;
  hasImage:boolean = false;
  description:string = null!;
  version:string = null!;
  relatedVersions:{versionName:string, documentGuid:string}[] = [];
  shelves:{guid:string, title:string, libraries:{guid:string, title:string}[], 
    //owner:{userGuid:string, userName:string},
    documents:{guid:string, title:string}[]}[] = [];
  elements:DocumentElementModel[] = [];
  tags:string[] = [];
  createdAt:Date = null!;
} 