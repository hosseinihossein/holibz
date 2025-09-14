import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogContent, MatDialogClose, MatDialogActions } from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { CdkTextareaAutosize } from "@angular/cdk/text-field";

@Component({
  selector: 'app-edit-code',
  imports: [MatDialogContent, MatFormField, MatLabel, MatButton, MatDialogClose,
    MatDialogActions, MatInput],
  templateUrl: './edit-code.html',
  styleUrl: './edit-code.css'
})
export class EditCode {
  readonly dialogRef = inject(MatDialogRef<EditCode>);
  readonly data = inject<{value:string}>(MAT_DIALOG_DATA);
}
