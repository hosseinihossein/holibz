import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose, MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'app-edit-header',
  imports: [MatDialogContent, MatDialogActions, MatFormField, MatInput, MatLabel,
    MatButton, MatDialogClose,
  ],
  templateUrl: './edit-header.html',
  styleUrl: './edit-header.css'
})
export class EditHeader {
  readonly dialogRef = inject(MatDialogRef<EditHeader>);
  readonly data = inject<{value:string, enableDelete?:boolean}>(MAT_DIALOG_DATA);
}
