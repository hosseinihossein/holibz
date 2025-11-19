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

  libraryCard_Storage = signal<RuCache<LibraryCardModel>>(new RuCache<LibraryCardModel>());
  shelfCard_Storage = signal<RuCache<ShelfCardModel>>(new RuCache<ShelfCardModel>());
  documentCard_Storage = signal<RuCache<DocumentCardModel>>(new RuCache<DocumentCardModel>());
  documentPage_Storage = signal<RuCache<DocumentPageModel>>(new RuCache<DocumentPageModel>());
  owner_Storage = signal<RuCache<OwnerModel>>(new RuCache<OwnerModel>());


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

export class RuCache<T extends {guid:string}>{
  private capacity:number = 50;
  private cache:T[] = [];

  getWithGuid(guid:string):T|null{
    let index = this.cache.findIndex(value=>value.guid === guid);
    if(index >= 0){
      let element = this.cache[index];
      this.cache.splice(index,1);
      this.cache.unshift(element);
      return element;
    }
    else{
      return null;
    }
  }

  add(...newValues:T[]){
    newValues.forEach(newValue=>{
      let index = this.cache.findIndex(value=>value.guid === newValue.guid);
      if(index >= 0){
        this.cache.splice(index,1);
      }
    });

    if((this.cache.length + newValues.length) > this.capacity){
      let numberOfExceededElements = this.cache.length + newValues.length - this.capacity;
      let exceededElementsStartIndex = this.cache.length - numberOfExceededElements;
      this.cache.splice(exceededElementsStartIndex);
    }
    
    this.cache.unshift(...newValues);
  }


}
