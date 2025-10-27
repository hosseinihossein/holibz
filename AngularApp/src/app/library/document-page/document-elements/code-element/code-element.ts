import { Clipboard } from '@angular/cdk/clipboard';
import { Component, inject, input } from '@angular/core';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { SectionModel } from '../../../../models/section-model';
import { DocumentElementModel } from '../document-element/document-element';

@Component({
  selector: 'app-code-element',
  imports: [MatIcon, MatIconButton, MatTooltip],
  templateUrl: './code-element.html',
  styleUrl: './code-element.css'
})
export class CodeElement {
  clipboard = inject(Clipboard);
  elementModel = input.required<DocumentElementModel>();
  
  copyCode(event:Event){
    if(event.currentTarget instanceof HTMLButtonElement){
      let btn = event.currentTarget as HTMLButtonElement;
      let code = btn.nextElementSibling?.innerHTML ??  '';
      code = code.replaceAll("&lt;","<");
      code = code.replaceAll("&gt;",">");
      this.clipboard.copy(code);
    }
  }
}
