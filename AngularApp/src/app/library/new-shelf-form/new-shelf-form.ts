import { Component, computed, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatOptgroup, MatOption, MatSelect } from '@angular/material/select';
import { JsonPipe } from '@angular/common';
import { LibraryService } from '../../services/library-service';
import { ActivatedRoute, Router } from '@angular/router';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { LibraryCardModel } from '../library-card/library-card';
import { IdentityService } from '../../services/identity-service';

@Component({
  selector: 'app-new-shelf-form',
  imports: [MatFormField,MatSelect,MatOption,MatButton,MatLabel,MatInput,MatIcon,
    ReactiveFormsModule,JsonPipe,MatError,MatProgressSpinner
  ],
  templateUrl: './new-shelf-form.html',
  styleUrl: './new-shelf-form.css'
})
export class NewShelfForm {
  identityService = inject(IdentityService);
  libraryService = inject(LibraryService);
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);

  currentLibraryGuid = signal("DefaultLibrary");
  newShelfForm = signal(new FormGroup({
    libraryGuid: new FormControl(this.currentLibraryGuid(), {nonNullable:true, validators: [Validators.required, Validators.maxLength(32)]}),
    title: new FormControl("", {nonNullable:true, validators: [Validators.required, Validators.maxLength(30),Validators.minLength(3)]}),
    description: new FormControl("", {validators: Validators.maxLength(200)}),
    image: new FormControl<File|null>(null),
  }));
  libraryGuid = computed(()=>this.newShelfForm().get("libraryGuid"));
  title = computed(()=>this.newShelfForm().get("title"));
  description = computed(()=>this.newShelfForm().get("description"));
  image = computed(()=>this.newShelfForm().get("image"));

  previewImgSrc = signal<string|null>(null);
  displaySubmitSpinner = signal(false);
  libraryList = signal<LibraryCardModel[]>([]);

  previewImg = viewChild<ElementRef<HTMLImageElement>>("previewImg");
  imgInput = viewChild.required<ElementRef<HTMLInputElement>>("fileInput");

  constructor(){
    let currentLibraryNameRouteParam = this.activatedRoute.snapshot.paramMap.get("library");
    if(currentLibraryNameRouteParam){
      this.currentLibraryGuid.set(currentLibraryNameRouteParam);
    }

    effect(() => {
      this.libraryService.requestLibraryList(this.identityService.userModel()?.guid)?.subscribe({
        next: res => {
          if(res){
            this.libraryList.set(res);
          }
        },
      });
    });
  }

  onSelectImage(event:Event){
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      if(input.files[0].size > (128 * 1024)){
        //create a form validator
      }
      else{
        this.newShelfForm().get("image")?.setValue(input.files[0]);

        const reader = new FileReader(); // Create a FileReader instance
        // Load the image as a Data URL
        reader.onload = (e)=> {
          this.previewImgSrc.set(e.target!.result as string);
        };
        reader.readAsDataURL(input.files[0]); // Read the file as a Data URL
      }
    }
    else{
      this.previewImgSrc.set(null);
      this.newShelfForm().get("image")?.setValue(null);
    }
  }

  clearImgInput(){
    this.imgInput().nativeElement.value = '';
    this.previewImgSrc.set(null);
    this.newShelfForm().get("image")?.setValue(null);
  }

  onSubmit(){
    if(this.newShelfForm().valid){
      this.displaySubmitSpinner.set(true);
      this.libraryService.createNewShelf(this.newShelfForm().value).subscribe({
        next: res => {
          if(res && res.success){
            this.displaySubmitSpinner.set(false);
            this.router.navigate(['/shelf',res.shelfGuid]);
          }
        },
        error: err => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error?.Title || err.error?.errors?.Title){
              this.title()?.setErrors({submitError: err.error?.Title || err.error?.errors?.Title});
            }
            else if(err.error?.Description || err.error?.errors?.Description){
              this.description()?.setErrors({submitError: err.error?.Description || err.error?.errors?.Description});
            }
            else if(err.error?.Libraryguid || err.error?.errors?.Libraryguid){
              this.libraryGuid()?.setErrors({submitError: err.error?.Libraryguid || err.error?.errors?.Libraryguid});
            }
            else if(err.error?.Image || err.error?.errors?.Image){
              this.image()?.setErrors({submitError: err.error?.Image || err.error?.errors?.Image});
            }
            else{
              this.newShelfForm().setErrors({submitError: err.error});
            }
          }
          else{
            throw(err);
          }
          this.displaySubmitSpinner.set(false);
        },
      });
    }
  }
}
