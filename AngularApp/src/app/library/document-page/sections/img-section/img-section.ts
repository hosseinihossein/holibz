import { Component, inject, input } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { LargeImg } from '../../../../dialogs/large-img/large-img';
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-img-section',
  imports: [],
  templateUrl: './img-section.html',
  styleUrl: './img-section.css'
})
export class ImgSection {
  sectionModel = input.required<SectionModel>();

  readonly dialog = inject(MatDialog);

  openLargeImage(){
    this.dialog.open(LargeImg, {data:{imgSrc:this.sectionModel().value, imgTitle: this.sectionModel().title}});
  }
}
