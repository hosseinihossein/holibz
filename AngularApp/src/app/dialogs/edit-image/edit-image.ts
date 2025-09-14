import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-edit-image',
  imports: [MatDialogContent, MatButton,MatDialogActions,MatDialogClose],
  templateUrl: './edit-image.html',
  styleUrl: './edit-image.css'
})
export class EditImage {
  readonly dialogRef = inject(MatDialogRef<EditImage>);
  readonly data = inject<{value:string}>(MAT_DIALOG_DATA);
  
  selectedFile = signal<File | null>(null);
  previewImgSrc = signal(this.data.value);

  previewImg = viewChild<ElementRef<HTMLImageElement>>("previewImg");

  constructor(){}

  onSelectImage(event:Event){
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      if(input.files[0].size > (250 * 1024)){
        //create a form validator
      }
      else{
        this.selectedFile.set(input.files[0]);

        const reader = new FileReader(); // Create a FileReader instance

        // Load the image as a Data URL
        reader.onload = (e)=> {
          if(this.previewImg()){
            //this.previewImg()!.nativeElement.src = e.target!.result as string ?? this.data.value; // Set the image source
            this.previewImgSrc.set(e.target!.result as string ?? this.data.value);
          }
        };

        reader.readAsDataURL(this.selectedFile()!); // Read the file as a Data URL
      }
    }
  }
}
