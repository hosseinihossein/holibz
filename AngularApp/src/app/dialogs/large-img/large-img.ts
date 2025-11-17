import { NgOptimizedImage } from '@angular/common';
import { AfterViewInit, Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MatIconButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogContent, MatDialogRef, MatDialogClose } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { WindowService } from '../../services/window-service';

@Component({
  selector: 'app-large-img',
  imports: [MatDialogContent, NgOptimizedImage, MatIcon, MatIconButton, MatTooltip, MatDialogClose],
  templateUrl: './large-img.html',
  styleUrl: './large-img.css'
})
export class LargeImg implements AfterViewInit {
  readonly dialogRef = inject(MatDialogRef<LargeImg>);
  readonly data = inject<{imgSrc: string, imgTitle?: string}>(MAT_DIALOG_DATA);
  
  theImage = viewChild<ElementRef<HTMLImageElement>>("theImage");
  windowService = inject(WindowService);
  
  ngAfterViewInit(): void {
    if(this.theImage()){
      let windowWidth90 = Math.floor(this.windowService.nativeWindow.innerWidth * 90 / 100);
      if(this.theImage()!.nativeElement.offsetWidth < windowWidth90){
        this.theImage()!.nativeElement.style.cursor = "zoom-in";
        this.theImage()!.nativeElement.addEventListener("click", ()=>{
          this.theImage()!.nativeElement.classList.toggle("zoomedIn");
          if(this.theImage()!.nativeElement.classList.contains("zoomedIn")){
            this.theImage()!.nativeElement.style.cursor = "zoom-out";
          }
          else{
            this.theImage()!.nativeElement.style.cursor = "zoom-in";
          }
        });
      }
    }
  }

}
