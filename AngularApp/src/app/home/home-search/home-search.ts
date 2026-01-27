import { Component, computed, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocomplete, MatAutocompleteSelectedEvent, MatAutocompleteTrigger, MatOption } from '@angular/material/autocomplete';
import { MatButton } from '@angular/material/button';
import { MatChipGrid, MatChipRow, MatChipRemove, MatChipInput } from '@angular/material/chips';
import { MatError, MatFormField, MatLabel, MatHint } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { SingletonModes } from '../../services/singleton-modes';
import { LibraryService } from '../../services/library-service';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home-search',
  imports: [MatFormField, MatLabel, MatInput, MatChipGrid, MatChipRow, MatChipRemove, MatIcon,
    MatButton, MatChipInput, MatAutocompleteTrigger, MatAutocomplete, ReactiveFormsModule,MatError,
    MatOption, ],
  templateUrl: './home-search.html',
  styleUrl: './home-search.css'
})
export class HomeSearch {
  router = inject(Router);
  singleton = inject(SingletonModes);
  libraryService = inject(LibraryService);

  suggestedTags = signal<string[]>([]);
  selectedTags = signal<string[]>([]);

  titleFormControl = new FormControl("",{
    validators: [
      //Validators.required,
      Validators.minLength(this.singleton.introductionTitle_MinLength()),
      Validators.maxLength(this.singleton.introductionTitle_MaxLength()),
    ],
  });

  tagInput = viewChild.required<ElementRef<HTMLInputElement>>("tagInput");

  constructor(){}

  removeTag(removedTag:string){
    this.selectedTags.update(tags=>{
      let removedTagIndex = tags.indexOf(removedTag);
      tags.splice(removedTagIndex,1);
      return tags.map(tag=>tag);
    });
  }
  selectTag(event: MatAutocompleteSelectedEvent):void{
    this.selectedTags.update(tags=>{
      tags.push(event.option.viewValue);
      return tags.map(tag=>tag);
    });
    //event.option.deselect();
    this.suggestedTags.set([]);
  }
  onInput(){
    const allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
    let filteredValue = this.tagInput().nativeElement.value.trim();
    filteredValue = filteredValue.toUpperCase();
    for(let i = 0; i<filteredValue.length; i++){
      let char = filteredValue.charAt(i);
      if(!allowedCharacters.includes(char)){
        filteredValue = filteredValue.replace(char, "");
        i--;
      }
    }
    this.tagInput().nativeElement.value = filteredValue;

    if(filteredValue.length < 3){
      this.suggestedTags.set([]);
      return;
    }

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

  search(){
    this.router.navigate(["search"],{queryParams:{
      title:this.titleFormControl.value ?? "",
      tags:this.selectedTags().join(","),
    }});
  }
  
}
