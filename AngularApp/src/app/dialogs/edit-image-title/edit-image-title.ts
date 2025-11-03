import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from "@angular/material/dialog";
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { LibraryService } from '../../services/library-service';
import { Result } from '../result/result';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';

@Component({
  selector: 'app-edit-image-title',
  imports: [MatDialogContent, MatFormField, MatButton,MatDialogActions,MatDialogClose,MatLabel,MatInput],
  templateUrl: './edit-image-title.html',
  styleUrl: './edit-image-title.css'
})
export class EditImageTitle {
  readonly dialogRef = inject(MatDialogRef<EditImageTitle>);
  readonly data = inject<{value:string, title:string, enableEdit?:boolean}>(MAT_DIALOG_DATA);
  
  selectedFile = signal<File | null>(null);
  previewImgSrc = signal(this.data.value);

  previewImg = viewChild<ElementRef<HTMLImageElement>>("previewImg");

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
    else{
      this.selectedFile.set(null);
      this.previewImgSrc.set(this.data.value);
    }
  }


}
