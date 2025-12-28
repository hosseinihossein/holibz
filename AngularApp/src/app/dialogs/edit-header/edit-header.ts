import { Component, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose, MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { SingletonModes } from '../../services/singleton-modes';

@Component({
  selector: 'app-edit-header',
  imports: [MatDialogContent, MatDialogActions, MatFormField, MatInput, MatLabel,
    MatButton, MatDialogClose, ReactiveFormsModule, MatError,
  ],
  templateUrl: './edit-header.html',
  styleUrl: './edit-header.css'
})
export class EditHeader {
  readonly dialogRef = inject(MatDialogRef<EditHeader>);
  readonly data = inject<{value:string, enableEdit?:boolean}>(MAT_DIALOG_DATA);
  readonly singletonModes = inject(SingletonModes);

  valueControl = new FormControl(this.data.value,{nonNullable:true,validators:[Validators.required,
    Validators.maxLength(this.singletonModes.elementTitle_MaxLength()), 
    Validators.minLength(this.singletonModes.elementTitle_MinLength())
  ]})
}
