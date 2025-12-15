import { Clipboard } from '@angular/cdk/clipboard';
import { Component, computed, ElementRef, inject, input, viewChild } from '@angular/core';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { ActivatedRoute, Router } from '@angular/router';
import { MatTooltip } from "@angular/material/tooltip";
import { DocumentElementModel } from '../document-element/document-element';
import { DatePipe } from '@angular/common';
import { WindowService } from '../../../../services/window-service';

@Component({
  selector: 'app-element-info-box',
  imports: [MatIconButton, MatIcon, MatTooltip, DatePipe],
  templateUrl: './element-info-box.html',
  styleUrl: './element-info-box.css'
})
export class ElementInfoBox {
  elementModel = input.required<DocumentElementModel>();

  clipboard = inject(Clipboard);
  //router = inject(Router);
  window = inject(WindowService);

  elementLink = computed(()=>{
    let href = this.window.nativeWindow.location.href;
    if(href.includes("#")){
      let fragmentIndex = href.indexOf("#");
      href = href.substring(0,fragmentIndex);
    }
    return `${href}#${this.elementModel().guid}`;
  });

  infoDiv = viewChild.required<ElementRef<HTMLDivElement>>("infoDiv");

  copyLink(){
    this.clipboard.copy(this.elementLink());
  }

  copyElementGuid(){
    this.clipboard.copy(this.elementModel().guid);
  }

  toggleOpenInfo(){
    this.infoDiv().nativeElement.classList.toggle("open");
  }
}
