import { computed, Injectable, signal } from '@angular/core';
import { SectionModel } from '../models/section-model';
import { DocumentInfo } from '../models/document-info';
//import { ShelfModel } from '../models/shelf-model';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  allSectionModels = signal<SectionModel[]>([
    { guid : "s000", type:  "h1", value : "Title of the Document", order : 0 },
    { guid : "s001", type:  "p", value : "Angular Material components depend on system variables defined as CSS variables through the material.theme Sass mixin. This page provides guidance and documentation for using these variables to customize components.", 
      order : 1 },
    { guid : "s002", type:  "img", value : "computer001.webp", order : 2, title: "computer image" },
    { guid : "s003", type:  "h2", value : "Colors", order : 3 },
    { guid : "s004", type:  "p", value : "Material Design uses color to create accessible, personal color schemes that communicate your product's hierarchy, state, and brand. See Material Design's Color System page to learn more about its use and purpose.", 
      order : 4 },
    { guid : "s005", type:  "p", value : "The following colors are the most often used in Angular Material components. Use these colors and follow their uses to add theme colors to your application's custom components.", 
      order : 5 },
    { guid : "s006", type:  "code", value : `<button matFab extended>Lorem ipsum dolor, sit amet consectetur adipisicing elit. Dignissimos quidem hic culpa, ex nesciunt vel impedit ipsa atque quasi fuga.
    <mat-icon>home</mat-icon>
    Home
</button>;
<button matFab extended>
    <mat-icon>home</mat-icon>
    Home
</button>`, order : 6 },
    { guid : "s007", type:  "p", value : "Lorem ipsum dolor sit amet consectetur, adipisicing elit. Corporis architecto beatae debitis neque quia mollitia!", 
      order : 7 },
    { guid : "s008", type:  "link", value : "https://angular.dev", order : 8, title: "Angular" },
    { guid : "s009", type:  "link", value : "https://material.angular.dev", order : 9, title: "Materials" },
    { guid : "s010", type:  "link", value : "https://fonts.google.com/icons", order :10, title: "Google icons" },
    { guid : "s011", type:  "h1", value : "Attached Files:", order :11 },
    { guid : "s012", type:  "file", value : "computer001.webp", order :12, title: "computer image" },
    { guid : "s013", type:  "file", value : "defaultProfile.jpg", order :13, title: "default profile image" },
    { guid : "s014", type:  "file", value : "HLlogo-LightMode.png", order :14, title: "Holibz logo for lightmode" },
    /*{ guid : "s015", type:  "h2", value : "Colors", order :15 },*/
  ]);
  //getSectionsOfDocument(documentGuid: string){update allSectionModels}
  //uploadImgeSection()
  //uploadFileSection()
  deleteSection(sectionGuid:string){
    this.allSectionModels.update(sections=>{
      let index = sections.findIndex(section=>section.guid === sectionGuid);
      sections.splice(index,1);
      return sections;
    });
  }
  mockAddSection(type:"h1"|"h2"|"p"|"img"|"code"|"file"|"link"){
    this.allSectionModels.update(sections=>{
      let newSection:SectionModel={
        guid: "s0"+sections.length,
        type: type,
        value: type==="h1"||type==="h2"||type==="code"||type==="p" ? "New Section" : "",
        order: sections.length,
      }
      return [...sections, newSection];
    });
  }
  mockEditSection(sectionInfo:{guid:string, value?:string, file?:File, title?:string}){
    this.allSectionModels.update(sections=>{
      let index = sections.findIndex(section=>section.guid === sectionInfo.guid);

      if(sectionInfo.value) sections[index].value = sectionInfo.value;
      if(sectionInfo.file) sections[index].value = sectionInfo.file.name;
      if(sectionInfo.title) sections[index].title = sectionInfo.title;
      
      return sections;
    });
  }
  increaseSectionOrder(sectionGuid:string){
    let changedSectionIndex = this.allSectionModels().findIndex(section=> section.guid === sectionGuid);
    let oldOrder = this.allSectionModels()[changedSectionIndex].order;

    if(oldOrder >= (this.allSectionModels().length - 1)) return;
    
    this.allSectionModels.update(sections=>{
      let newOrder = oldOrder + 1;
      let changedBySectionIndex = sections.findIndex(section=>section.order===newOrder);
      sections[changedSectionIndex].order++;
      sections[changedBySectionIndex].order--;
      
      return [...sections];
    });
  }
  decreaseSectionOrder(sectionGuid:string){
    let changedSectionIndex = this.allSectionModels().findIndex(section=> section.guid === sectionGuid);
    let oldOrder = this.allSectionModels()[changedSectionIndex].order;
    
    if(oldOrder <= 0) return;
    
    this.allSectionModels.update(sections=>{
      let newOrder = oldOrder - 1;
      let changedBySectionIndex = sections.findIndex(section=>section.order===newOrder);
      sections[changedSectionIndex].order--;
      sections[changedBySectionIndex].order++;
      
      return [...sections];
    });
  }

  documentInfo = signal<DocumentInfo>({
    guid: "d001", 
    category: "frontend", 
    version: "20.1.6", 
    tags: ["form", "login-form"],
    shelvesGuids: ["shelf001", "shelf002", "shelf003"]
  });
  //getDocumentInfo(documentGuid:string, documentVersion?:string){update documentInfo}
  getDocumentVersions(documentGuid:string):string[]{
    return ["20.1.6","19","18","17"];
  }
  updateDocumentTags(tags:string[]){
    this.documentInfo.update(info=>({...info,tags:tags}));
  }

  shelves= signal<any[]>([
    {guid : "shelf001", title:"shelf one", description:"this is shelf one", documentsGuids:["doc0","doc1doc1doc1doc1doc1doc1doc1doc1doc1", "doc2","doc3"]},
    {guid : "shelf002", title:"shelf two shelf two", description:"this is shelf two", documentsGuids:["doc4", "doc5","doc6"]},
    {guid : "shelf003", title:"shelf three", description:"this is shelf three", documentsGuids:["doc7", "doc8","doc9"]},
  ]);
  //getDocumentShelves(documentGuid:string){update shelves}
  
  allTags = signal(["form","login-form","user-account","card","image-card","navbar"]);
  updateAllTags(category:string){
    //update allTags  
  }
}