import { inject, Injectable, signal } from '@angular/core';
import { WindowService } from './window-service';

@Injectable({
  providedIn: 'root'
})
export class SingletonModes {
  windowService = inject(WindowService);

  editMode = signal(false);
  darkMode = signal(false);

  toggleEditMode(){
    this.editMode.update(mode=>!mode);
  }

  toggleDarkMode(){
    this.darkMode.update(mode=>!mode);
    if(this.darkMode()){
      this.windowService.nativeWindow.document.body.classList.add('dark-mode');
    }
    else{
      this.windowService.nativeWindow.document.body.classList.remove('dark-mode');
    }
  }
}
