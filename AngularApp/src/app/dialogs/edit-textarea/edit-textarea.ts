import { Component, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { SingletonModes } from '../../services/singleton-modes';

@Component({
  selector: 'app-edit-textarea',
  imports: [MatDialogContent, MatLabel, MatFormField, MatDialogActions, MatDialogClose, MatButton, 
    MatInput, ReactiveFormsModule],
  templateUrl: './edit-textarea.html',
  styleUrl: './edit-textarea.css'
})
export class EditTextarea {
  readonly dialogRef = inject(MatDialogRef<EditTextarea>);
  readonly data = inject<{label:string, value:string, enableDelete?:boolean}>(MAT_DIALOG_DATA);
  singletonModes = inject(SingletonModes);

  myText = new FormControl(this.data.value,{
    nonNullable:true,
    validators:[Validators.required, Validators.maxLength(this.singletonModes.elementStringValue_MaxLength())],
  });
}
