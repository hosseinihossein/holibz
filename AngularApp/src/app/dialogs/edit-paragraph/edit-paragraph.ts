import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogContent, MatDialogClose, MatDialogActions } from '@angular/material/dialog';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { SingletonModes } from '../../services/singleton-modes';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-edit-paragraph',
  imports: [MatDialogContent, MatLabel, MatFormField, MatDialogActions, MatDialogClose, MatButton, 
    MatInput, ReactiveFormsModule, MatError,
  ],
  templateUrl: './edit-paragraph.html',
  styleUrl: './edit-paragraph.css'
})
export class EditParagraph {
  readonly dialogRef = inject(MatDialogRef<EditParagraph>);
  readonly data = inject<{value:string, enableEdit?:boolean}>(MAT_DIALOG_DATA);
  readonly singletonModes = inject(SingletonModes);

  valueControl = new FormControl(this.data.value,{nonNullable:true,validators:[Validators.required,
    Validators.maxLength(this.singletonModes.elementValueMaxLength())
  ]})
}
