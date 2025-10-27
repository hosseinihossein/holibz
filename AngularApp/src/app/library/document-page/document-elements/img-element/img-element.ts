import { Component, inject, input } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { LargeImg } from '../../../../dialogs/large-img/large-img';
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-img-element',
  imports: [],
  templateUrl: './img-element.html',
  styleUrl: './img-element.css'
})
export class ImgElement {
  sectionModel = input.required<SectionModel>();

  readonly dialog = inject(MatDialog);

  openLargeImage(){
    this.dialog.open(LargeImg, {data:{imgSrc:this.sectionModel().value, imgTitle: this.sectionModel().title}});
  }
}
