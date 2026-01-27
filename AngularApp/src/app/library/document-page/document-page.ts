import { AfterViewInit, Component, computed, effect, ElementRef, inject, input, Renderer2, signal, viewChild, viewChildren } from '@angular/core';
import { MatButton, MatIconButton } from '@angular/material/button';
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
import { DocumentElement, DocumentElementModel } from './document-elements/document-element/document-element';
import { MatChip, MatChipSet } from "@angular/material/chips";
import { EditTags } from '../../dialogs/edit-tags/edit-tags';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {  LibraryService, NewElementFormModel, OwnerModel } from '../../services/library-service';
import { IdentityService } from '../../services/identity-service';
import { DatePipe, NgOptimizedImage, ViewportScroller } from '@angular/common';
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
import { EditInput } from '../../dialogs/edit-input/edit-input';
import { EditIntroduction } from '../../dialogs/edit-introduction/edit-introduction';
import { ParentEditor } from '../../dialogs/parent-editor/parent-editor';
import { ParentShelfModel } from '../new-document-form/new-document-form';
import { Review } from '../../review/review';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';
import { IconService } from '../../services/icon-service';
import { BriefUsersList } from '../../dialogs/brief-users-list/brief-users-list';

@Component({
  selector: 'app-document-page',
  imports: [MatSidenavModule, MatExpansionModule, MatTooltip, MatButton, MatIcon,
    MatMenu, MatMenuItem, MatMenuTrigger, DocumentElement, MatChipSet, MatChip, RouterLink,
    NgOptimizedImage, MatBadge, WaitSpinner, Review, DatePipe,MatIconButton],
  templateUrl: './document-page.html',
  styleUrl: './document-page.css',
  providers: [DocumentPageService]
})
export class DocumentPage implements AfterViewInit/*, AfterViewChecked*/ {
  //clipboard = inject(Clipboard);
  windowService = inject(WindowService);
  readonly dialog = inject(MatDialog);
  singleton = inject(SingletonModes);
  activatedRoute = inject(ActivatedRoute);
  libraryService = inject(LibraryService);
  identityService = inject(IdentityService);
  renderer = inject(Renderer2);
  router = inject(Router);
  documentPageService = inject(DocumentPageService);
  viewportScroller = inject(ViewportScroller);
  clipboard = inject(Clipboard);
  iconService = inject(IconService);

  documentGuid = signal<string|null>(null);
  requestedCommentGuid = signal<string|null>(null);
  sortedElements = computed(()=>
    this.documentPageService.documentPageModel()?.elements.sort((a,b)=>{
      if(a.order > b.order)return 1;else return -1;
    })
  );
  sortedVersions = computed(()=> 
    this.documentPageService.documentPageModel()?.relatedVersions.sort((a,b)=>{
      if(a.versionName > b.versionName)return 1;else return -1;
    })
  );

  introductionImage = computed(()=>this.libraryService.getDocumentImageAddress(this.documentPageService.documentPageModel()));

  ownerModel = signal<OwnerModel|null>(null);
  ownerImgSrc = computed(() => this.singleton.getUserImageAddress(this.ownerModel()));
  isMyDocument = computed(()=>this.ownerModel()?.guid === this.identityService.userModel()?.guid);
  isMyFavorite = signal(false);

  displayWaitSpinner = signal(false);

  headingElements = signal<HTMLHeadingElement[]>([]);
  introductionHeading = viewChild.required<ElementRef<HTMLHeadingElement>>("introductionHeading");
  reviewComponent = viewChild.required(Review,{read:ElementRef});

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

  documentLink = computed(()=>{
    let href = this.windowService.nativeWindow.location.href;
    if(href.includes("?")){
      let queryParamsIndex = href.indexOf("?");
      href = href.substring(0,queryParamsIndex);
    }
    if(href.includes("#")){
      let fragmentIndex = href.indexOf("#");
      href = href.substring(0,fragmentIndex);
    }
    return href;
  });
  
  constructor(){
    this.activatedRoute.paramMap.subscribe(params=>{
      if(params.has("documentGuid")){
        this.documentGuid.set(params.get("documentGuid"));
      }
    });
    this.activatedRoute.queryParamMap.subscribe(queryParams=>{
      if(queryParams.has("commentGuid")){
        this.requestedCommentGuid.set(queryParams.get("commentGuid"));
      }
    });

    effect(() => {
      if(this.documentGuid()){
        this.libraryService.requestDocumentPageModel(this.documentGuid()!).subscribe({
          next: res => {
            if(res){
              this.documentPageService.documentPageModel.set(res);
              this.documentPageService.unchangedDocumentPageModel.set(new DocumentPageModel(res));
            }
          },
        });
      }
    });

    effect(() => {
      if(this.documentPageService.documentPageModel()){
        this.libraryService.requestOwnerModel(this.documentPageService.documentPageModel()!.owner.userGuid).subscribe({
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
    
    effect(()=>{
      if(this.identityService.isAuthenticated()){
        this.identityService.getCsrf().subscribe({
          next: () => {
            console.log("csrf token recieved successfully.");
          },
          error: err => {
            console.log("couldn't get csrf token");
            throw(err);
          }
        });
      }
    });

    effect(()=>{
      if(this.identityService.isAuthenticated() && this.documentGuid()){
        this.libraryService.isMyFavoriteDocument(this.documentGuid()!).subscribe({
          next: res =>{
            if(res){
              this.isMyFavorite.set(res.isMyFavorite);
            }
          },
        });
      }
    });
  }
  
  ngAfterViewInit(): void {
    this.viewportScroller.setOffset([0,64]);//[xOffset, yOffset]

    if(this.introductionHeading()){
      this.headingElements.update(elements=>[this.introductionHeading().nativeElement, ...elements]);
    }
    if(this.reviewComponent()){
      this.headingElements.update(elements=>[...elements, this.reviewComponent().nativeElement]);
    }

    this.activatedRoute.fragment.subscribe(fragment=>{
      if(fragment){
        let elementGuid = fragment;
        setTimeout(()=>{
          this.goToElement(elementGuid);
        }, 1000);
      }
    });
  }

  onHeadingInit(headingElement: HTMLHeadingElement){
    this.headingElements.update(elements=>[...elements, headingElement]);
  }

  confirmDelete(){
    if(this.isMyDocument()){
      const dialogRef = this.dialog.open(ConfirmDelete,{
        data:{
          title: `if you click on 'Yes', document '${this.documentPageService.documentPageModel()?.title}' and its contents will be permanently deleted!`,
          type: "Document",
        }
      });
      dialogRef.afterClosed().subscribe(result=>{
        if(result === true){
          this.displayWaitSpinner.set(true);
          this.libraryService.requestDeleteDocument(this.documentPageService.documentPageModel()?.guid!).subscribe({
            next: res => {
              if(res && res.success){
                this.displayWaitSpinner.set(false);
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
              }).afterClosed().subscribe(()=>this.displayWaitSpinner.set(false));
              throw(err);
            },
          });
        }
      });
    }
  }

  editTags(){
    if(this.documentGuid()){
      const dialogRef = this.dialog.open(EditTags, {data:{
        tags:this.documentPageService.documentPageModel()?.tags
      }});
      dialogRef.afterClosed().subscribe((result?:string[])=>{
        if(result){
          this.libraryService.requestEditDocumentTags(result, this.documentGuid()!).subscribe({
            next: res =>{
              if(res){
                this.documentPageService.documentPageModel.update(dpm=>{
                  dpm!.tags = res;
                  return new DocumentPageModel(dpm!);
                });
                this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
                  dpm!.tags = res;
                  return new DocumentPageModel(dpm!);
                });
              }
            },
            error: err => {
              console.log(JSON.stringify(err));
            },
          });
        }
      });
    }
  }

  goToElement(guid:string){
    this.viewportScroller.scrollToAnchor(guid, {behavior:'smooth'});
  }

  addNewElement(type:"h1" | "h2" | "p" | "img" | "code" | "file" | "link"){
    if(this.documentPageService.documentPageModel()){

      if(this.documentPageService.documentPageModel()!.elements.length >= this.singleton.maxNumberOfElementsInDocument()){
        this.dialog.open(Result,{data:{
          status: "warning",
          title: "Maximum Number Of Elements Reached",
          description:[
            "You can not add a new element to this document!",
            `There can not be more elements than ${this.singleton.maxNumberOfElementsInDocument()} in a document!`,
          ],
        }});
        return;
      }

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
          this.documentPageService.documentPageModel.update(dpm=>{
            dpm?.elements.push(res);
            return new DocumentPageModel(dpm!);
          });
          this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
            dpm?.elements.push(res);
            return new DocumentPageModel(dpm!);
          });
          
          setTimeout(()=>{
            this.goToElement(res.guid);
          }, 1000);
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
      this.displayWaitSpinner.set(true);
      this.libraryService.submitEditedElements(this.documentPageService.documentPageModel()!.guid,
        this.documentPageService.getEditElementFormModelArray()).subscribe({
        next: res => {
          if(res && res.success){
            this.documentPageService.documentPageModel.update(dpm=>{
              dpm!.elements = res.elements;
              return new DocumentPageModel(dpm!);
            });
            this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
              dpm!.elements = res.elements;
              return new DocumentPageModel(dpm!);
            });

            this.documentPageService.editedElementFormModels().clear();

            //this.libraryService.documentPage_Storage().add(this.documentPageService.documentPageModel()!);

            this.displayWaitSpinner.set(false);
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

          this.documentPageService.toggleEditMode();
        }
      });
    }
    else{
      this.documentPageService.toggleEditMode();
    }
  }

  enterEditMode(){
    if(this.identityService.isAuthenticated()){
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
              return new DocumentPageModel(dpm!);
            });
            this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
              dpm!.hasImage = false;
              return new DocumentPageModel(dpm!);
            });
            //this.libraryService.documentPage_Storage().add(this.documentPageService.documentPageModel()!);
          }
          else{
            this.documentPageService.documentPageModel.update(dpm=>{
              dpm!.title = result.title;
              dpm!.description = result.description;
              dpm!.hasImage = result.hasImage;
              dpm!.integrityVersion = result.integrityVersion;
              return new DocumentPageModel(dpm!);
            });
            this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
              dpm!.title = result.title;
              dpm!.description = result.description;
              dpm!.hasImage = result.hasImage;
              dpm!.integrityVersion = result.integrityVersion;
              return new DocumentPageModel(dpm!);
            });
            //this.libraryService.documentPage_Storage().add(this.documentPageService.documentPageModel()!);
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
      }}).afterClosed().subscribe((result:ParentShelfModel[])=>{
        if(result){
          //console.log(JSON.stringify(result));
          let resultShelves = result.map(shelf=>new ParentShelfModel(shelf));
          this.documentPageService.documentPageModel.update(dpm=>{
            dpm!.shelves = resultShelves;
            return new DocumentPageModel(dpm!);
          });
          this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
            dpm!.shelves = resultShelves;
            return new DocumentPageModel(dpm!);
          });
          //this.libraryService.documentPage_Storage().add(this.documentPageService.documentPageModel()!);
        }
      });
    }
  }

  addRelatedVersion(){
    if(this.isMyDocument() && this.documentPageService.documentPageModel()){
      this.dialog.open(EditInput, {data:{
        label:"Document Fingerprint",
        value: "",
        maxLength: 32,
        minLength: 32,
      }}).afterClosed().subscribe((result:string)=>{
        if(result){
          this.displayWaitSpinner.set(true);
          this.libraryService.requestAddVersionRelationship(this.documentGuid()!, result).subscribe({
            next: res => {
              if(res){
                this.documentPageService.documentPageModel.update(dpm=>{
                  dpm!.relatedVersions = res;
                  return new DocumentPageModel(dpm!);
                });
                this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
                  dpm!.relatedVersions = res;
                  return new DocumentPageModel(dpm!);
                });

                this.displayWaitSpinner.set(false);
              }
            },
          });
        }
      });
    }
  }
  editVersionName(){
    if(this.isMyDocument() && this.documentPageService.documentPageModel()){
      this.dialog.open(EditInput,{data:{
        label:"Edit Document's Version Name",
        value: this.documentPageService.documentPageModel()!.version,
        maxLength: 32,
        minLength: 1,
      }}).afterClosed().subscribe((result:string)=>{
        if(result){
          this.displayWaitSpinner.set(true);
          this.libraryService.requestEditVersionName(this.documentGuid()!,result).subscribe({
            next: res => {
              if(res){
                this.documentPageService.documentPageModel.update(dpm=>{
                  dpm!.version = res.version;
                  return new DocumentPageModel(dpm!);
                });
                this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
                  dpm!.version = res.version;
                  return new DocumentPageModel(dpm!);
                });
                
                this.displayWaitSpinner.set(false);
              }
            }
          });
        }
      });
    }
  }
  createNewVersionOfDocument(){
    if(this.isMyDocument() && this.documentGuid()){
      this.dialog.open(EditInput,{data:{
        label:"New Version Name",
        value:"",
        maxLength: 32,
        minLength: 1,
      }}).afterClosed().subscribe((result:string)=>{
        if(result){
          this.displayWaitSpinner.set(true);
          this.libraryService.requestCreateNewDocumentVersion(this.documentGuid()!,result).subscribe({
            next: res => {
              if(res){
                this.displayWaitSpinner.set(false);
                this.router.navigate(["/document",res.newVersionGuid]);
              }
            },
          });
        }
      });
    }
  }
  removeRelatedVersion(){
    if(this.isMyDocument() && this.documentPageService.documentPageModel()){
      this.displayWaitSpinner.set(true);
      this.libraryService.requestDeleteVersionRelationship(this.documentGuid()!).subscribe({
        next: res => {
          if(res && res.success){
            this.documentPageService.documentPageModel.update(dpm=>{
              dpm!.relatedVersions = [];
              return new DocumentPageModel(dpm!);
            });
            this.documentPageService.unchangedDocumentPageModel.update(dpm=>{
              dpm!.relatedVersions = [];
              return new DocumentPageModel(dpm!);
            });
            
            this.displayWaitSpinner.set(false);
          }
        }
      });
    }
  }

  copyDocumentLink(){
    if(this.documentGuid()){
      this.clipboard.copy(this.documentLink());
    }
  }
  copyDocumentGuid(){
    if(this.documentGuid()){
      this.clipboard.copy(this.documentGuid()!);
    }
  }

  toggleFavorite(){
    if(this.identityService.isAuthenticated() && this.documentGuid() && this.documentPageService.documentPageModel()){
      this.libraryService.requestToToggleFavoriteDocument(this.documentGuid()!).subscribe({
        next: res => {
          if(res && res.success){
            this.isMyFavorite.update(f=>!f);
          }
        },
      });
    }
  }

  displayUsersInFavor(){
    if(this.documentGuid() && this.documentPageService.documentPageModel()){
      this.dialog.open(BriefUsersList,{data:{
        label: "Users In Favor",
        subjectGuid: this.documentGuid(),
        totalNumberOfItems: this.documentPageService.documentPageModel()!.totalNumberOfUsersInFavor,
        type: "InFavorOfDocument"
      }});
    }
  }
  
}

export class DocumentPageModel {
  constructor(documentPageModel:DocumentPageModel){
    this.guid = documentPageModel.guid;
    this.owner = documentPageModel.owner;
    this.title = documentPageModel.title;
    this.hasImage = documentPageModel.hasImage;
    this.integrityVersion = documentPageModel.integrityVersion;
    this.description = documentPageModel.description;
    this.version = documentPageModel.version;
    this.relatedVersions = documentPageModel.relatedVersions.map(a=>Object.create(a));
    this.shelves = documentPageModel.shelves.map(shelf=>new ParentShelfModel(shelf));
    this.elements = documentPageModel.elements.map(a=>new DocumentElementModel(a));
    this.tags = documentPageModel.tags.map(t=>t);
    this.createdAt = documentPageModel.createdAt;
    //this.isMyFavorite = documentPageModel.isMyFavorite;
    this.totalNumberOfUsersInFavor = documentPageModel.totalNumberOfUsersInFavor;
  }

  guid:string = null!;
  owner:{userGuid:string, userName:string} = null!;
  title:string = null!;
  hasImage:boolean = false;
  integrityVersion:number = 0;
  description:string = null!;
  version:string = null!;
  relatedVersions:{versionName:string, documentGuid:string}[] = [];
  shelves:ParentShelfModel[] = [];
  elements:DocumentElementModel[] = [];
  tags:string[] = [];
  createdAt:Date = null!;
  //isMyFavorite:boolean = false;
  totalNumberOfUsersInFavor:number = 0;
} 