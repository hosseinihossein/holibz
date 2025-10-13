import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-confirm-change',
  imports: [MatDialogTitle, MatDialogContent, MatDialogActions, MatButton, MatDialogClose, MatIcon],
  templateUrl: './confirm-change.html',
  styleUrl: './confirm-change.css'
})
export class ConfirmChange {
  readonly dialogRef = inject(MatDialogRef<ConfirmChange>);
  readonly data = inject<{change:string}>(MAT_DIALOG_DATA);
}
