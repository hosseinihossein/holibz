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

  elementValueMaxLength = signal(1000);// 1000 chars
  elementTitleMaxLength = signal(60);// 60 chars
  elementTitleMinLength = signal(3);//  3 chars
  elementFileMaxSize = signal(500);// 500 KB
  introductionTitleMaxLength = signal(60);// 60 chars
  introductionTitleMinLength = signal(3);// 3 chars
  introductionDescriptionMaxLength = signal(500);// 500 chars
  documentIntroductionImageMaxSize = signal(500);// 500 KB
  libraryShelfIntroductionImageMaxSize = signal(120);// 120 KB

  


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

  getUserImageAddress(userModel:{userGuid?:string, integrityVersion?:number, hasImage?:boolean}|null):string|null{
    if(userModel?.hasImage && userModel.userGuid){
      return `/api/Identity/UserImage?userGuid=${userModel.userGuid}&v=${userModel.integrityVersion}`;
    }
    return null;
  }

  

}





