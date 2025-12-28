import { Component, inject, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogContent, MatDialogClose, MatDialogActions } from '@angular/material/dialog';
import { MatFormField, MatLabel, MatError } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { CdkTextareaAutosize } from "@angular/cdk/text-field";
import { LibraryService } from '../../services/library-service';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { SingletonModes } from '../../services/singleton-modes';

@Component({
  selector: 'app-edit-code',
  imports: [MatDialogContent, MatFormField, MatLabel, MatButton, MatDialogClose,
    MatDialogActions, MatInput, ReactiveFormsModule, MatError],
  templateUrl: './edit-code.html',
  styleUrl: './edit-code.css'
})
export class EditCode {
  readonly dialogRef = inject(MatDialogRef<EditCode>);
  readonly data = inject<{value:string, enableEdit?:boolean}>(MAT_DIALOG_DATA);
  readonly singletonModes = inject(SingletonModes);

  inputControl = new FormControl(this.data.value,{nonNullable:true, validators:[Validators.required,
    Validators.maxLength(this.singletonModes.elementStringValue_MaxLength())
  ]})
}
