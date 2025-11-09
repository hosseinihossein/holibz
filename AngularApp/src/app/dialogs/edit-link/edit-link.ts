import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogContent, MatDialogClose, MatDialogActions } from '@angular/material/dialog';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { SingletonModes } from '../../services/singleton-modes';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-edit-link',
  imports: [MatDialogContent,MatButton,MatLabel,MatInput,MatDialogClose,MatDialogActions, MatFormField,
    ReactiveFormsModule, MatError,
  ],
  templateUrl: './edit-link.html',
  styleUrl: './edit-link.css'
})
export class EditLink {
  readonly dialogRef = inject(MatDialogRef<EditLink>);
  readonly data = inject<{value:string,title:string, enableEdit?:boolean}>(MAT_DIALOG_DATA);
  readonly singletonModes = inject(SingletonModes);

  valueControl = new FormControl(this.data.value,{nonNullable:true,validators:[Validators.required,
    Validators.maxLength(this.singletonModes.elementValueMaxLength()),
    Validators.minLength(this.singletonModes.elementTitleMinLength())
  ]});
  titleControl = new FormControl(this.data.title,{nonNullable:true,validators:[Validators.required,
    Validators.maxLength(this.singletonModes.elementTitleMaxLength()),
    Validators.minLength(this.singletonModes.elementTitleMinLength())
  ]});
}
