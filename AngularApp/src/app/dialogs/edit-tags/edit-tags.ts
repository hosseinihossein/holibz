import { Component, inject, signal } from '@angular/core';
import { DocumentService } from '../../services/document-service';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatChipGrid, MatChipInput, MatChipRemove, MatChipRow } from "@angular/material/chips";
import { MatAutocomplete, MatAutocompleteSelectedEvent, MatAutocompleteTrigger, MatOption } from '@angular/material/autocomplete';
import { MatIcon } from '@angular/material/icon';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from "@angular/material/dialog";
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'app-edit-tags',
  imports: [MatFormField, MatLabel, MatChipGrid, MatChipRow, MatChipRemove, MatChipInput, MatAutocomplete,
    MatIcon, MatAutocompleteTrigger, MatOption, MatDialogContent,MatDialogActions,MatDialogClose,
    MatButton,],
  templateUrl: './edit-tags.html',
  styleUrl: './edit-tags.css'
})
export class EditTags {
  readonly dialogRef = inject(MatDialogRef<EditTags>);
  //readonly data = inject<{tags:string[]}>(MAT_DIALOG_DATA);
  //tags = signal(this.data.tags);
  documentService = inject(DocumentService);
  tags = signal(this.documentService.documentInfo().tags);

  removeTag(removedTag:string){
    this.tags.update(tags=>{
      let newTags:string[]=[];
      tags.forEach(tag=>{if(tag !== removedTag){newTags.push(tag)}});
      return newTags;
    });
  }

  selectTag(event: MatAutocompleteSelectedEvent):void{
    this.tags.update(oldTags=>[...oldTags, event.option.viewValue]);
    event.option.deselect();
  }
}
