import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MatError, MatFormField, MatFormFieldModule, MatHint, MatLabel } from '@angular/material/form-field';
import { MatChipGrid, MatChipInput, MatChipRemove, MatChipRow } from "@angular/material/chips";
import { MatAutocomplete, MatAutocompleteSelectedEvent, MatAutocompleteTrigger, MatOption } from '@angular/material/autocomplete';
import { MatIcon } from '@angular/material/icon';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from "@angular/material/dialog";
import { MatButton } from '@angular/material/button';
import { LibraryService } from '../../services/library-service';
import { debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-edit-tags',
  imports: [MatFormFieldModule, MatLabel, MatChipGrid, MatChipRow, MatChipRemove, MatChipInput, MatAutocomplete,
    MatIcon, MatAutocompleteTrigger, MatOption, MatDialogContent,MatDialogActions,MatDialogClose,
    MatButton,MatHint,],
  templateUrl: './edit-tags.html',
  styleUrl: './edit-tags.css'
})
export class EditTags {
  readonly dialogRef = inject(MatDialogRef<EditTags>);
  readonly data = inject<{tags:string[]}>(MAT_DIALOG_DATA);
  
  libraryService = inject(LibraryService);
  suggestedTags = signal<string[]>([]);
  selectedTags = signal<string[]>(this.data.tags);

  tagInput = viewChild.required<ElementRef<HTMLInputElement>>("tagInput");

  displayErrorText = signal<boolean>(false);

  removeTag(removedTag:string){
    let removedTagIndex = this.selectedTags().indexOf(removedTag);
    this.selectedTags().splice(removedTagIndex,1);
  }

  selectTag(event: MatAutocompleteSelectedEvent):void{
    this.selectedTags().push(event.option.viewValue);
    event.option.deselect();
  }

  onInput(){
    this.displayErrorText.set(false);
    const allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
    let filteredValue = this.tagInput().nativeElement.value.trim();
    filteredValue = filteredValue.toUpperCase();
    for(let i = 0; i<filteredValue.length; i++){
      let char = filteredValue.charAt(i);
      if(!allowedCharacters.includes(char)){
        filteredValue = filteredValue.replace(char, "");
        i--;
        this.displayErrorText.set(true);
      }
    }

    this.tagInput().nativeElement.value = filteredValue;

    if(filteredValue.length < 3) return;

    this.libraryService.requestTags(filteredValue).pipe(
      debounceTime(1000),
      distinctUntilChanged(),
    ).subscribe({
      next: res => {
        if(res){
          this.suggestedTags.set(res);
        }
      },
      error: err =>{
        console.log(JSON.stringify(err));
      },
    });
  }

  onEnter(){
    if(!this.selectedTags().includes(this.tagInput().nativeElement.value)){
      this.selectedTags.update(oldTags=>[...oldTags, this.tagInput().nativeElement.value]);
    }
    this.tagInput().nativeElement.value = "";
  }

}
