import { Component, computed, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MatAutocomplete, MatOption, MatAutocompleteTrigger, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatChipGrid, MatChipRow, MatChipRemove, MatChipInput } from '@angular/material/chips';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { LibraryService } from '../../services/library-service';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { SingletonModes } from '../../services/singleton-modes';
import { MatCard } from '@angular/material/card';
import { GenericList } from '../generic-list/generic-list';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonToggle, MatButtonToggleChange, MatButtonToggleGroup } from '@angular/material/button-toggle';
import { ActivatedRoute } from '@angular/router';
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'app-search',
  imports: [MatFormField, MatLabel, MatAutocomplete, MatChipGrid, MatChipRow, MatIcon, MatOption,
    MatAutocompleteTrigger, MatChipRemove, MatInput, ReactiveFormsModule, MatError, MatCard, GenericList,
    MatPaginatorModule, MatButtonToggleGroup, MatButtonToggle, MatChipInput,MatButton],
  templateUrl: './search.html',
  styleUrl: './search.css'
})
export class Search {
  libraryService = inject(LibraryService);
  singleton = inject(SingletonModes);
  activatedRoute = inject(ActivatedRoute);

  suggestedTags = signal<string[]>([]);
  selectedTags = signal<string[]>([]);

  titleFormControl = new FormControl("",{
    validators: [
      Validators.minLength(this.singleton.introductionTitle_MinLength()),
      Validators.maxLength(this.singleton.introductionTitle_MaxLength()),
    ],
  });
  
  tagInput = viewChild.required<ElementRef<HTMLInputElement>>("tagInput");
  
  disableSearchButton = computed(()=>{
    if(this.selectedTags().length == 0 && (!this.titleFormControl.value ||
    this.titleFormControl.value.trim().length < 3)){
      return true;
    }
    else{
      return false;
    }
  });
  
  displayWaitSpinner = signal(true);

  genericListItemGuids = signal<string[]>([]);
  pageIndex = signal<number>(0);
  pageSize = signal<number>(10);
  totalNumberOfDocuments = signal<number>(0);
  sortBy = signal<"newest"|"popular">("newest");

  constructor(){
    this.activatedRoute.queryParamMap.subscribe(queryParams=>{
      if(queryParams.has("title")){
        this.titleFormControl.setValue(queryParams.get("title"));
      }
      if(queryParams.has("tags")){
        this.selectedTags.set(queryParams.get("tags")?.split(",") ?? []);
      }
    });

    this.libraryService.searchDocuments(this.selectedTags(),this.titleFormControl.value,
    this.sortBy(),this.pageIndex(),this.pageSize()).subscribe({
      next: res => {
        if(res){
          this.genericListItemGuids.set(res);
          this.displayWaitSpinner.set(false);
        }
      },
    });

    this.libraryService.requestTotalNumberOfSearchDocuments(this.selectedTags(),
    this.titleFormControl.value).subscribe({
      next: res => {
        if(res){
          this.totalNumberOfDocuments.set(res.totalNumberOfItems);
        }
      },
    });
  }

  removeTag(removedTag:string){
    let removedTagIndex = this.selectedTags().indexOf(removedTag);
    this.selectedTags().splice(removedTagIndex,1);
  }
  selectTag(event: MatAutocompleteSelectedEvent):void{
    this.selectedTags().push(event.option.viewValue);
    event.option.deselect();
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

  search(){
    this.displayWaitSpinner.set(true);

    this.libraryService.searchDocuments(this.selectedTags(),this.titleFormControl.value,
    this.sortBy(),this.pageIndex(),this.pageSize()).subscribe({
      next: res => {
        if(res){
          this.genericListItemGuids.set(res);
          this.displayWaitSpinner.set(false);
        }
      },
    });

    this.libraryService.requestTotalNumberOfSearchDocuments(this.selectedTags(),
    this.titleFormControl.value).subscribe({
      next: res => {
        if(res){
          this.totalNumberOfDocuments.set(res.totalNumberOfItems);
        }
      },
    });
  }

  handlePageEvent(e: PageEvent) {
    //let length = e.length;
    this.pageSize.set(e.pageSize);
    this.pageIndex.set(e.pageIndex);
    this.search();
  }

  onSortBy(e:MatButtonToggleChange){
    this.sortBy.set(e.value);
  }

}
