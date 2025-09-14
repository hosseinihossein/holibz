import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogContent, MatDialogClose, MatDialogActions } from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'app-edit-paragraph',
  imports: [MatDialogContent, MatLabel, MatFormField, MatDialogActions, MatDialogClose, MatButton, 
    MatInput
  ],
  templateUrl: './edit-paragraph.html',
  styleUrl: './edit-paragraph.css'
})
export class EditParagraph {
  readonly dialogRef = inject(MatDialogRef<EditParagraph>);
  readonly data = inject<{value:string}>(MAT_DIALOG_DATA);
}
