import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'app-edit-textarea',
  imports: [MatDialogContent, MatLabel, MatFormField, MatDialogActions, MatDialogClose, MatButton, 
    MatInput],
  templateUrl: './edit-textarea.html',
  styleUrl: './edit-textarea.css'
})
export class EditTextarea {
  readonly dialogRef = inject(MatDialogRef<EditTextarea>);
  readonly data = inject<{label:string, value:string}>(MAT_DIALOG_DATA);
}
