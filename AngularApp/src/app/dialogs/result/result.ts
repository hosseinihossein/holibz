import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';
import { Router } from '@angular/router';

@Component({
  selector: 'app-result',
  imports: [MatDialogContent, MatDialogActions, MatDialogClose, MatButton, MatDialogTitle],
  templateUrl: './result.html',
  styleUrl: './result.css'
})
export class Result {
  //readonly dialogRef = inject(MatDialogRef<Result>);
  readonly data = inject<Partial<ResultDialogInputData>>(MAT_DIALOG_DATA);
  readonly router = inject(Router);

  goToLinkAddress(){
    if(this.data.link){
      this.router.navigate([this.data.link.address]);
    }
  }
}
export class ResultDialogInputData {
  status: "success" | "info" | "warning" = "info";
  title:string | null = null; 
  description:string[] | null = null; 
  link:{name:string, address:string} | null = null;
}
