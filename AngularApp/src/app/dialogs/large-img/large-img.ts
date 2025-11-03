import { NgOptimizedImage } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatIconButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogContent, MatDialogRef, MatDialogClose } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';

@Component({
  selector: 'app-large-img',
  imports: [MatDialogContent, NgOptimizedImage, MatIcon, MatIconButton, MatTooltip, MatDialogClose],
  templateUrl: './large-img.html',
  styleUrl: './large-img.css'
})
export class LargeImg {
  readonly dialogRef = inject(MatDialogRef<LargeImg>);
  readonly data = inject<{imgSrc: string, imgTitle?: string}>(MAT_DIALOG_DATA);
  //imgSrc = signal(this.data.imgSrc);
  //imgTitle
}
