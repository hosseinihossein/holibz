import { Component, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'app-edit-input',
  imports: [MatDialogContent, MatDialogActions, MatFormField, MatInput, MatLabel,
    MatButton, MatDialogClose, ReactiveFormsModule],
  templateUrl: './edit-input.html',
  styleUrl: './edit-input.css'
})
export class EditInput {
  readonly dialogRef = inject(MatDialogRef<EditInput>);
  readonly data = inject<{label:string, value:string}>(MAT_DIALOG_DATA);

  username = signal(new FormControl(this.data.value,{
    nonNullable:true,
    validators:[Validators.required, Validators.minLength(8), Validators.maxLength(60)],
  }));
}
