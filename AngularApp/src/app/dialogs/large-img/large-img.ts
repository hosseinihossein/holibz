import { NgOptimizedImage } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogContent, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-large-img',
  imports: [MatDialogContent,NgOptimizedImage],
  templateUrl: './large-img.html',
  styleUrl: './large-img.css'
})
export class LargeImg {
  readonly dialogRef = inject(MatDialogRef<LargeImg>);
  readonly data = inject<{imgSrc: string, imgTitle?: string}>(MAT_DIALOG_DATA);
  //imgSrc = signal(this.data.imgSrc);
  //imgTitle
}
