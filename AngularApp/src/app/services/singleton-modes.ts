import { inject, Injectable, signal } from '@angular/core';
import { WindowService } from './window-service';
import { LibraryCardModel } from '../library/library-card/library-card';
import { ShelfCardModel } from '../library/shelf-card/shelf-card';
import { DocumentCardModel } from '../library/document-card/document-card';
import { DocumentPageModel } from '../library/document-page/document-page';
import { OwnerModel } from './library-service';

@Injectable({
  providedIn: 'root'
})
export class SingletonModes {
  constructor(){
    let theme = localStorage.getItem("theme");
    if(theme && theme == "dark"){
      this.darkMode.set(true);
      this.windowService.nativeWindow.document.body.classList.add('dark-mode');
    }
    else{
      this.darkMode.set(false);
      this.windowService.nativeWindow.document.body.classList.remove('dark-mode');
    }
  }

  windowService = inject(WindowService);

  readonly turnstileSiteKey = "0x4AAAAAAAkeZ2wTzJxqgC_K";

  editMode = signal(false);
  darkMode = signal(false);

  elementStringValue_MaxLength = signal(4000);// 1000 chars
  elementTitle_MaxLength = signal(60);// 60 chars
  elementTitle_MinLength = signal(3);//  3 chars
  elementType_MaxLength = signal(20);
  elementFile_MaxSize = signal(500);// 500 KB
  maxNumberOfElementsInDocument = signal(255);
  introductionTitle_MaxLength = signal(60);// 60 chars
  introductionTitle_MinLength = signal(3);// 3 chars
  introductionDescription_MaxLength = signal(500);// 500 chars
  documentIntroductionImage_MaxSize = signal(500);// 500 KB
  libraryShelfIntroductionImage_MaxSize = signal(120);// 120 KB
  username_MaxLength = signal(60);
  version_MaxLength = signal(30);
  tagName_MaxLength = signal(30);
  userProfileDescription_MaxLength = signal(500);
  comment_MaxLength = signal(500);

  EmptyGuid = "00000000000000000000000000000000";
  RecentlyAddedDocuments_ShelfGuid = "RecentlyAddedDocuments";

  


  toggleEditMode(){
    this.editMode.update(mode=>!mode);
  }
  toggleDarkMode(){
    this.darkMode.update(mode=>!mode);
    if(this.darkMode()){
      this.windowService.nativeWindow.document.body.classList.add('dark-mode');
      localStorage.setItem("theme", "dark");
    }
    else{
      this.windowService.nativeWindow.document.body.classList.remove('dark-mode');
      localStorage.removeItem("theme");
    }
  }

  getUserImageAddress(userModel:{guid?:string, integrityVersion?:number, hasImage?:boolean}|null):string|null{
    if(userModel?.hasImage && userModel.guid){
      return `/api/Identity/UserImage?userGuid=${userModel.guid}&v=${userModel.integrityVersion}`;
    }
    return null;
  }

  

}





