import { Component, inject, input } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { LargeImg } from '../../../../dialogs/large-img/large-img';
import { DocumentElementModel } from '../document-element/document-element';
import { NgOptimizedImage } from "@angular/common";

@Component({
  selector: 'app-img-element',
  imports: [NgOptimizedImage],
  templateUrl: './img-element.html',
  styleUrl: './img-element.css'
})
export class ImgElement {
  elementModel = input.required<DocumentElementModel>();

  readonly dialog = inject(MatDialog);

  openLargeImage(){
    this.dialog.open(LargeImg, {data:{imgSrc:this.elementModel().value, imgTitle: this.elementModel().title}});
  }
}
