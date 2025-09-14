import { Clipboard } from '@angular/cdk/clipboard';
import { Component, inject, input } from '@angular/core';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-code-section',
  imports: [MatIcon, MatIconButton, MatTooltip],
  templateUrl: './code-section.html',
  styleUrl: './code-section.css'
})
export class CodeSection {
  clipboard = inject(Clipboard);
  sectionModel = input.required<SectionModel>();
  
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
