import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogContent, MatDialogClose, MatDialogActions } from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'app-edit-link',
  imports: [MatDialogContent,MatButton,MatLabel,MatInput,MatDialogClose,MatDialogActions, MatFormField],
  templateUrl: './edit-link.html',
  styleUrl: './edit-link.css'
})
export class EditLink {
  readonly dialogRef = inject(MatDialogRef<EditLink>);
  readonly data = inject<{value:string,title:string, enableDelete?:boolean}>(MAT_DIALOG_DATA);
}
