import { Component } from '@angular/core';
import { MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose } from '@angular/material/dialog';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-confirm-delete',
  imports: [MatDialogTitle, MatDialogContent, MatDialogActions, MatButton, MatDialogClose, MatIcon],
  templateUrl: './confirm-delete.html',
  styleUrl: './confirm-delete.css'
})
export class ConfirmDelete {

}
