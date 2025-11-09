import { inject, Injectable, signal } from '@angular/core';
import { WindowService } from './window-service';

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
  elementValueMaxLength = signal(1000);//max characters for each element value
  elementTitleMaxLength = signal(60);//max characters for each element title
  elementTitleMinLength = signal(3);//min characters for each element title
  elementFileMaxSize = signal(500);// max file size in KB for each element file
  introductionTitleMaxLength = signal(60);//max characters for introduction title
  introductionTitleMinLength = signal(3);//min characters for introduction title
  introductionDescriptionMaxLength = signal(500);//max characters for introduction description
  documentIntroductionImageMaxSize = signal(500);// max file size in KB for introduction image of documents
  libraryShelfIntroductionImageMaxSize = signal(120);// max file size in KB for introduction image of libraries anshelves

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
}
