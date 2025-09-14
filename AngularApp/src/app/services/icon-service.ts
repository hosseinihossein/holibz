import { inject, Injectable } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';

@Injectable({
  providedIn: 'root'
})
export class IconService {
  private matIconRegistry = inject(MatIconRegistry);

  constructor()
  {
    this.matIconRegistry.setDefaultFontSetClass("material-symbols-outlined");
  }

  fillIcon(){
    return "font-variation-settings:  'FILL' 1;";
  }
}
