import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogContent, MatDialogActions, MatDialogClose } from '@angular/material/dialog';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { SingletonModes } from '../../services/singleton-modes';

@Component({
  selector: 'app-edit-file',
  imports: [MatDialogContent, MatFormField, MatButton,MatDialogActions,MatDialogClose,MatLabel,MatInput,
    MatError,ReactiveFormsModule,
  ],
  templateUrl: './edit-file.html',
  styleUrl: './edit-file.css'
})
export class EditFile {
  readonly dialogRef = inject(MatDialogRef<EditFile>);
  readonly data = inject<{value:string, title:string, enableEdit?:boolean}>(MAT_DIALOG_DATA);
  readonly singletoneModes = inject(SingletonModes);

  selectedFile = signal<File | null>(null);
  fileNameSrc = signal(this.data.value);
  fileSizeError = signal<string|null>(null);

  fileName = viewChild<ElementRef<HTMLImageElement>>("fileName");

  titleControl = new FormControl(this.data.title,{nonNullable:true, validators:[Validators.required, 
    Validators.maxLength(this.singletoneModes.elementTitle_MaxLength()), 
    Validators.minLength(this.singletoneModes.elementTitle_MinLength())
  ]})

  onSelectFile(event:Event){
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      if(input.files[0].size > (this.singletoneModes.elementFile_MaxSize() * 1024)){
        this.fileSizeError.set(
          `The size of choosen file cannot be more than ${this.singletoneModes.elementFile_MaxSize()} KB!`
        );
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
