import { Component, computed, effect, inject, signal } from '@angular/core';
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
import { MatTooltip } from "@angular/material/tooltip";
import { MatButton } from '@angular/material/button';
import { ParentShelfModel } from '../../library/new-document-form/new-document-form';

@Component({
  selector: 'app-parent-editor',
  imports: [MatDialogContent, MatDialogActions, MatDialogClose, ReactiveFormsModule, MatIcon,
    MatButtonToggleModule, MatFormField, MatError, MatLabel, MatSelect, /*MatOptgroup,*/ MatOption,
    MatProgressSpinner, MatButton],
  templateUrl: './parent-editor.html',
  styleUrl: './parent-editor.css'
})
export class ParentEditor {
  readonly dialogRef = inject(MatDialogRef<ParentEditor>);
  readonly data = inject<{
    parentOf:"document"|"shelf", 
    childGuid:string, 
    parentLibraryGuids?:string[], 
    parentShelfGuids?:string[],
  }>(MAT_DIALOG_DATA);

  identityService = inject(IdentityService);
  libraryService = inject(LibraryService);

  //parentForm = new FormGroup({
  libraryGuids= new FormControl<string[]>(this.data.parentLibraryGuids ?? [], {nonNullable:true, validators:[Validators.required]});
  shelfGuids= new FormControl<string[]>(this.data.parentShelfGuids ?? [], {nonNullable:true, validators: [Validators.required]});
  //});
  //libraryGuids = this.parentForm.get("libraryGuids");
  //shelfGuids = this.parentForm.get("shelfGuids");
  submitErrors = signal<string|null>(null);
  
  displaySubmitSpinner = signal(false);
  allLibraryList = signal<{guid:string,title:string}[]>([]);
  displayedLibraries = signal<string[]>([]);
  allShelfList = signal<ParentShelfModel[]>([]);
  //shelfGuidToParentLibrariesTitlesMap = signal<Map<string,string>>(new Map<string,string>());
  shelfGuidToParentLibrariesTitlesMap = computed<Map<string,string>>(()=>{
    let map = new Map<string,string>();
    this.allShelfList().forEach(shelf=>
      map.set(shelf.guid, shelf.libraries.map(l=>l.title).slice(0,3).join(', '))
    );
    return map;
  });

  constructor(){
    effect(() => {
      if(this.identityService.userModel()?.guid){
        this.libraryService.requestLibraryBriefList(this.identityService.userModel()!.guid!).subscribe({
          next: res => {
            if(res){
              this.allLibraryList.set(res);
              this.displayedLibraries.set(
                this.allLibraryList().filter(lib=>(this.data.parentLibraryGuids ?? []).includes(lib.guid)).map(lib=>lib.title)
              );
            }
          },
        });

        if(this.data.parentOf === "document"){
          this.libraryService.requestUserShelfList(this.identityService.userModel()!.guid!).subscribe({
            next: res => {
              if(res){
                this.allShelfList.set(res);
              }
            },
          });
        }

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
  }

  changeDisplayedLibraries(e:MatButtonToggleChange){
    this.displayedLibraries.set(
      this.allLibraryList().filter(lib=>(e.value as string[]).includes(lib.guid)).map(lib=>lib.title)
    );
  }

  onSubmit(){
    //if(this.parentForm.valid){
      this.displaySubmitSpinner.set(true);

      const calbacks = {
        next: (res: {success:boolean, parentShelves?:ParentShelfModel[]}) => {
          if(res && res.success){
            this.displaySubmitSpinner.set(false);
            if(this.data.parentOf === "document"){
              let parentShelves: ParentShelfModel[];
              if(res.parentShelves){
                parentShelves = res.parentShelves;
              }
              else{
                parentShelves = 
                this.allShelfList().filter(shelf=>this.shelfGuids.value.includes(shelf.guid));
              }
              this.dialogRef.close(parentShelves);
            }
            else if(this.data.parentOf === "shelf"){
              let filteredLibs:{guid:string,title:string}[] = 
              this.allLibraryList().filter(lib=>this.libraryGuids.value.includes(lib.guid));
              this.dialogRef.close(filteredLibs);
            }
          }
        },
        error: (err:any) => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error?.ShelfGuids || err.error?.errors?.ShelfGuids){
              this.submitErrors.set("parent shelves error: " + err.error?.ShelfGuids || err.error?.errors?.ShelfGuids);
            }
            else if(err.error?.LibraryGuids || err.error?.errors?.LibraryGuids){
              this.submitErrors.set("parent libraries error: " + err.error?.LibraryGuids || err.error?.errors?.LibraryGuids);
            }
            else if(err.error?.SubmitError || err.error?.errors?.SubmitError){
              this.submitErrors.set("submit error: " + err.error?.SubmitError || err.error?.errors?.SubmitError);
            }
            else{
              this.submitErrors.set(JSON.stringify(err.error));
            }
          }
          else{
            throw(err);
          }
          this.displaySubmitSpinner.set(false);
        },
      };
      
      if(this.data.parentOf === "document" && this.shelfGuids.valid){
        this.libraryService.editDocumentParentShelves(
          this.data.childGuid, this.shelfGuids!.value
        ).subscribe(calbacks);
      }
      else if(this.data.parentOf === "shelf" && this.libraryGuids.valid){
        this.libraryService.editShelfParentLibraries(
          this.data.childGuid, this.libraryGuids!.value
        ).subscribe(calbacks);
      }
    //}
  }

  /**
 * Compare two string arrays for equality (order doesn't matter)
 * @param {string[]} arr1 
 * @param {string[]} arr2 
 * @returns {boolean}
 */
  arraysEqualUnordered(arr1?:string[], arr2?:string[]): boolean {
    if (!Array.isArray(arr1) || !Array.isArray(arr2)) return false;
    if (arr1.length !== arr2.length) return false;

    // Sort copies to avoid mutating original arrays
    const sorted1 = [...arr1].sort();
    const sorted2 = [...arr2].sort();

    for (let i = 0; i < sorted1.length; i++) {
        if (sorted1[i] !== sorted2[i]) return false;
    }
    return true;
  }

  shelfParentsIncludeAnyOfLibraries(shelf:ParentShelfModel, librariesTitles:string[]){
    for(let lib of shelf.libraries){
      if(librariesTitles.includes(lib.title)){
        return true;
      }
    }
    return false;
  }

}
