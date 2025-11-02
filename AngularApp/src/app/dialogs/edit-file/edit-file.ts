import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogContent, MatDialogActions, MatDialogClose } from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'app-edit-file',
  imports: [MatDialogContent, MatFormField, MatButton,MatDialogActions,MatDialogClose,MatLabel,MatInput],
  templateUrl: './edit-file.html',
  styleUrl: './edit-file.css'
})
export class EditFile {
  readonly dialogRef = inject(MatDialogRef<EditFile>);
  readonly data = inject<{value:string, title:string}>(MAT_DIALOG_DATA);

  selectedFile = signal<File | null>(null);
  fileNameSrc = signal(this.data.value);

  fileName = viewChild<ElementRef<HTMLImageElement>>("fileName");

  constructor(){}

  onSelectFile(event:Event){
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      if(input.files[0].size > (250 * 1024)){
        //create a form validator
      }
      else{
        this.selectedFile.set(input.files[0]);
        this.fileNameSrc.set(this.selectedFile()?.name ?? "selected file name!");
        /*if(this.fileName()){
        }*/
        /*
        const reader = new FileReader(); // Create a FileReader instance

        // Load the image as a Data URL
        reader.onload = (e)=> {
        };

        reader.readAsDataURL(this.selectedFile()!); // Read the file as a Data URL
        */
      }
    }
    else{
      this.selectedFile.set(null);
      this.fileNameSrc.set(this.data.value);
    }
  }
}
