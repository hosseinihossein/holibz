import { Component, computed, effect, ElementRef, inject, input, output, signal, viewChild } from '@angular/core';
import { MatSidenavModule } from '@angular/material/sidenav';
import { LibraryCard, LibraryCardModel } from '../library-card/library-card';
import { DocumentCard, DocumentCardModel } from '../document-card/document-card';
import { ShelfCard, ShelfCardModel } from '../shelf-card/shelf-card';
import { MatAccordion } from '@angular/material/expansion';
import { LibraryService } from '../../services/library-service';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatChipGrid, MatChipRow, MatChipInput, MatChipRemove } from '@angular/material/chips';
import { MatIcon } from '@angular/material/icon';
import { MatAutocomplete, MatOption, MatAutocompleteTrigger, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatButton } from '@angular/material/button';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { SingletonModes } from '../../services/singleton-modes';
import { MatButtonToggle, MatButtonToggleChange, MatButtonToggleGroup } from '@angular/material/button-toggle';
import { MatInput } from '@angular/material/input';

@Component({
  selector: 'app-generic-list',
  imports: [MatSidenavModule, LibraryCard, DocumentCard, MatFormField, MatLabel, MatChipGrid, MatChipRow,
    MatIcon, ShelfCard, MatAccordion, MatAutocomplete, MatOption, MatAutocompleteTrigger, MatChipRemove,
    MatButton, ReactiveFormsModule, MatError, MatButtonToggleGroup, MatButtonToggle, MatInput, MatChipInput],
  templateUrl: './generic-list.html',
  styleUrl: './generic-list.css'
})
export class GenericList {
  listType = input.required<"Library"|"Shelf"|"Document">();
  itemGuids = input.required<string[]>();
  tags = input<string[]>([]);
  filterInfo = input<GenericListFilter>(new GenericListFilter());
  openSideNav = input(true);

  removeDocumentFromParentShelf = output<string>();
  submitFilter = output<GenericListFilter>();

  isMyList = signal<boolean>(false);
  libraryModels = signal<LibraryCardModel[]>([]);
  shelfModels = signal<ShelfCardModel[]>([]);
  documentCardModels = signal<DocumentCardModel[]>([]);
/*
  libraryModels_Sorted = computed(()=>this.libraryModels().sort((a,b)=>{
    if(a.createdAt >= b.createdAt){
      return 1;
    }
    else{
      return -1;
    }
  }));
  shelfModels_Sorted = computed(()=>this.shelfModels().sort((a,b)=>{
    if(a.createdAt >= b.createdAt){
      return 1;
    }
    else{
      return -1;
    }
  }));
  documentCardModels_Sorted = computed(()=>this.documentCardModels().sort((a,b)=>{
    if(a.createdAt >= b.createdAt){
      return 1;
    }
    else{
      return -1;
    }
  }));
*/
  libraryService = inject(LibraryService);
  singleton = inject(SingletonModes);

  titleFormControl = new FormControl(this.filterInfo().title,{
    validators: [
      Validators.minLength(this.singleton.introductionTitle_MinLength()),
      Validators.maxLength(this.singleton.introductionTitle_MaxLength()),
    ],
  });

  selectedTags = signal<string[]>([]);
  sortBy = signal<"newest"|"popular">(this.filterInfo().sortBy);
  tagInput = viewChild.required<ElementRef<HTMLInputElement>>("tagInput");

  //requestedLibraryGuids = signal<string[]>([]);

  constructor(){
    this.selectedTags.set(this.filterInfo().tags);
    
    /*effect(()=>{
      if(this.listType() && this.itemGuids()){
          if(this.listType() == "Library"){
            this.getLibraryModels(this.itemGuids()!);
          }
          else if(this.listType() == "Shelf"){
            this.getShelfModels(this.itemGuids()!);
          }
          else if(this.listType() == "Document"){
            this.getDocumentModels(this.itemGuids()!);
          }
        }
    });*/
  }
/*
  getLibraryModels(libGuids:string[]){
    console.log("generic-list.ts -> getLibraryModels()");
    console.log("libGuids = ",JSON.stringify(libGuids));
    //this.libraryModels().length = 0;
    //this.shelfModels().length = 0;
    //this.documentCardModels().length = 0;
    
    this.libraryModels.set([]);
    this.shelfModels.set([]);
    this.documentCardModels.set([]);
    
    
    if(libGuids.length > 0){
      libGuids.forEach(libGuid=>{
        if(!this.requestedLibraryGuids().includes(libGuid)){
          this.requestedLibraryGuids().push(libGuid);
          this.libraryService.requestLibraryModel(libGuid).subscribe({
            next: libModel => {
              if(libModel){
                this.libraryModels().push(libModel);
                //this.libraryModels.update(models=>{
                  //models.push(libModel);
                  //return models.map(model=>new LibraryCardModel(model));
                //});
              }
            },
          });
        }
      });
    }
  }
  getShelfModels(shelfGuids:string[]){
    this.libraryModels().length = 0;
    this.shelfModels().length = 0;
    this.documentCardModels().length = 0;

    if(shelfGuids.length > 0){
      //console.log(shelfGuids);
      shelfGuids.forEach(shelfGuid=>{
        this.libraryService.requestShelfModel(shelfGuid).subscribe({
          next: shelfModel => {
            if(shelfModel){
              this.shelfModels().push(shelfModel);
              //this.shelfModels.update(models=>{
                //models.push(shelfModel);
                //return models.map(model=>new ShelfCardModel(model));
              //});
            }
          },
        });
      });
    }
  }
  getDocumentModels(docGuids:string[]){
    this.libraryModels().length = 0;
    this.shelfModels().length = 0;
    this.documentCardModels().length = 0;

    if(docGuids.length > 0){
      docGuids.forEach(docGuid=>{
        this.libraryService.requestDocumentCardModel(docGuid).subscribe({
          next: docModel => {
            if(docModel){
              this.documentCardModels().push(docModel);
              //this.documentCardModels.update(models=>{
                //models.push(docModel);
                //return models.map(model=>new DocumentCardModel(model));
              //});
            }
          },
        });
      });
    }
  }
*/
  removeTag(tag:string){
    let index = this.selectedTags().indexOf(tag);
    this.selectedTags().splice(index,1);
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
  }
  selectTag(event: MatAutocompleteSelectedEvent):void{
    this.selectedTags().push(event.option.viewValue);
    event.option.deselect();
  }
  onSortBy(e:MatButtonToggleChange){
    this.sortBy.set(e.value);
  }

  onSubmitFiltter(){
    this.submitFilter.emit({
      tags: this.selectedTags(),
      title: this.titleFormControl.value,
      sortBy: this.sortBy(),
    });
  }

}

export class GenericListFilter{
  tags:string[] = [];
  title:string|null = null;
  sortBy:"newest"|"popular" = "newest";
}
