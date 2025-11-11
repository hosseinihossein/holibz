import { Component, effect, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonToggleChange, MatButtonToggleModule } from '@angular/material/button-toggle';
import { MAT_DIALOG_DATA, MatDialog, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatOptgroup, MatOption, MatSelect } from '@angular/material/select';
import { IdentityService } from '../../services/identity-service';
import { LibraryService } from '../../services/library-service';
import { LibraryCardModel } from '../../library/library-card/library-card';
import { ShelfCardModel } from '../../library/shelf-card/shelf-card';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';

@Component({
  selector: 'app-parent-editor',
  imports: [MatDialogContent,MatDialogActions,MatDialogClose, ReactiveFormsModule, MatIcon, 
    MatButtonToggleModule, MatFormField, MatError, MatLabel, MatSelect, MatOptgroup,MatOption,
    MatProgressSpinner],
  templateUrl: './parent-editor.html',
  styleUrl: './parent-editor.css'
})
export class ParentEditor {
  readonly dialogRef = inject(MatDialogRef<ParentEditor>);
  readonly data = 
  inject<{parentOf:string, parentLibraryGuids:string[], parentShelfGuids:string[], childGuid:string}>(
    MAT_DIALOG_DATA
  );

  identityService = inject(IdentityService);
  libraryService = inject(LibraryService);

  parentForm = new FormGroup({
    libraryGuids: new FormControl<string[]>([...this.data.parentLibraryGuids], {nonNullable:true, validators:[Validators.required]}),
    shelfGuids: new FormControl<string[]>([...this.data.parentShelfGuids], {nonNullable:true, validators: [Validators.required]}),
  });
  libraryGuids = this.parentForm.get("libraryGuids");
  shelfGuids = this.parentForm.get("shelfGuids");
  
  displaySubmitSpinner = signal(false);
  allLibraryList = signal<LibraryCardModel[]>([]);
  displayedLibraries = signal<string[]>([]);
  allShelfList = signal<ShelfCardModel[]>([]);

  constructor(){
    effect(() => {
      if(this.identityService.userModel()?.guid){
        this.libraryService.requestLibraryList(this.identityService.userModel()!.guid!)?.subscribe({
          next: res => {
            if(res){
              this.allLibraryList.set(res);
              this.displayedLibraries.set(res.map(l=>l.title));
            }
          },
        });

        this.libraryService.requestUserShelfList(this.identityService.userModel()!.guid!).subscribe({
          next: res => {
            if(res){
              this.allShelfList.update(shelfList=>[...shelfList, ...res]);
            }
          },
        });

        this.identityService.getCsrf().subscribe({
          next: () => {
            console.log("Csrf received successfully.");
          },
          error: err => {
            console.error("Couldn't get Csrf!");
            throw(err);
          },
        });
      }
    });

    /*effect(() => {
      for(let libraryModel of this.allLibraryList()){
        this.libraryService.requestShelfList(libraryModel.guid).subscribe({
          next: res => {
            if(res){
              this.allShelfList.update(shelfList=>[...shelfList, ...res]);
            }
          },
        });
      }
    });*/
  }

  changeDisplayedLibraries(e:MatButtonToggleChange){
    this.displayedLibraries.set(
      this.allLibraryList().filter(lib=>(e.value as string[]).includes(lib.guid)).map(lib=>lib.title)
    );
  }

  onSubmit(){
    if(this.parentForm.valid){
      this.displaySubmitSpinner.set(true);

      const calbacks = {
        next: (res:{success:boolean}) => {
          if(res && res.success){
            this.displaySubmitSpinner.set(false);
            if(this.data.parentOf === "document"){
              this.dialogRef.close(this.shelfGuids?.value);
            }
            else if(this.data.parentOf === "shelf"){
              this.dialogRef.close(this.libraryGuids?.value);
            }
          }
        },
        error: (err:any) => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error?.ShelfGuids || err.error?.errors?.ShelfGuids){
              this.parentForm?.setErrors({submitError: "parent shelves error: " + err.error?.ShelfGuids || err.error?.errors?.ShelfGuids});
            }
            else if(err.error?.LibraryGuids || err.error?.errors?.LibraryGuids){
              this.parentForm?.setErrors({submitError: "parent libraries error: " + err.error?.LibraryGuids || err.error?.errors?.LibraryGuids});
            }
            else if(err.error?.SubmitError || err.error?.errors?.SubmitError){
              this.parentForm?.setErrors({submitError: "submit error: " + err.error?.SubmitError || err.error?.errors?.SubmitError});
            }
            else{
              this.parentForm.setErrors({submitError: err.error});
            }
          }
          else{
            throw(err);
          }
          this.displaySubmitSpinner.set(false);
        },
      };
      
      if(this.data.parentOf === "document"){
        this.libraryService.editDocumentParentShelves(
          this.data.childGuid, this.shelfGuids!.value
        ).subscribe(calbacks);
      }
      else if(this.data.parentOf === "shelf"){
        this.libraryService.editShelfParentLibraries(
          this.data.childGuid, this.libraryGuids!.value
        ).subscribe(calbacks);
      }
    }
  }
}
