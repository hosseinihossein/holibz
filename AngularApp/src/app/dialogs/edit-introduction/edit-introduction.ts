import { JsonPipe } from '@angular/common';
import { Component, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { IdentityService } from '../../services/identity-service';
import { LibraryService } from '../../services/library-service';
import { ActivatedRoute, Router } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialog, MatDialogActions, MatDialogContent, MatDialogRef, MatDialogClose } from '@angular/material/dialog';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { SingletonModes } from '../../services/singleton-modes';

@Component({
  selector: 'app-edit-introduction',
  imports: [ReactiveFormsModule, MatIcon, MatFormField, MatLabel, MatError, MatProgressSpinner,
    MatInput, MatButton, MatIconButton, MatDialogContent, MatDialogActions, MatDialogClose],
  templateUrl: './edit-introduction.html',
  styleUrl: './edit-introduction.css'
})
export class EditIntroduction {
  readonly dialogRef = inject(MatDialogRef<EditIntroduction>);
  readonly data = inject<{introductionOf:string, title:string, description:string, imageSrc?:string, guid:string}>(MAT_DIALOG_DATA);

  identityService = inject(IdentityService);
  libraryService = inject(LibraryService);
  singletonModes = inject(SingletonModes);

  introductionForm = new FormGroup({
    title: new FormControl(this.data.title, {nonNullable:true, validators: [Validators.required, 
      Validators.maxLength(this.singletonModes.introductionTitleMaxLength()),
      Validators.minLength(this.singletonModes.introductionTitleMinLength())]}),
    description: new FormControl(this.data.description, {nonNullable:true,validators: Validators.maxLength(this.singletonModes.introductionDescriptionMaxLength())}),
    image: new FormControl<File|null>(null),
  });
  title = this.introductionForm.get("title");
  description = this.introductionForm.get("description");
  image = this.introductionForm.get("image");

  previewImgSrc = signal<string|undefined>(this.data.imageSrc);
  displaySubmitSpinner = signal(false);
  imageMaxSize = signal(120);//default 120 KB
  
  previewImg = viewChild<ElementRef<HTMLImageElement>>("previewImg");
  imgInput = viewChild.required<ElementRef<HTMLInputElement>>("fileInput");

  constructor(){
    if(this.data.introductionOf === "document"){
      this.imageMaxSize.set(this.singletonModes.documentIntroductionImageMaxSize())
    }
    else if(this.data.introductionOf === "library" ||
      this.data.introductionOf === "shelf"){
        this.imageMaxSize.set(this.singletonModes.libraryShelfIntroductionImageMaxSize())
    }
    effect(() => {
      if(this.identityService.userModel()){
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

  onSelectImage(event:Event){
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      if(input.files[0].size > (this.imageMaxSize() * 1024)){
        this.image?.setErrors({sizeError: `The size of the image cannot be more than ${this.imageMaxSize()} KB!`});
      }
      else{
        this.image?.setValue(input.files[0]);

        const reader = new FileReader(); // Create a FileReader instance
        // Load the image as a Data URL
        reader.onload = (e)=> {
          this.previewImgSrc.set(e.target!.result as string);
        };
        reader.readAsDataURL(input.files[0]); // Read the file as a Data URL
      }
    }
    else{
      this.previewImgSrc.set(this.data.imageSrc);
      this.image?.setValue(null);
    }
  }

  clearImgInput(){
    this.imgInput().nativeElement.value = '';
    this.previewImgSrc.set(this.data.imageSrc);
    this.image?.setValue(null);
  }

  onSubmit(){
    if(this.introductionForm.valid){
      this.displaySubmitSpinner.set(true);
      
      const callbacks = {
        next: (res:{success:boolean, introduction:{title:string,description:string,hasImage:boolean,integrityVersion:number}}) => {
          if(res && res.success){
            this.displaySubmitSpinner.set(false);
            this.dialogRef.close(res.introduction);
          }
        },
        error: (err:any) => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error?.Guid || err.error?.errors?.Guid){
              this.introductionForm.setErrors({submitError: err.error?.Guid || err.error?.errors?.Guid});
            }
            else if(err.error?.Authorization || err.error?.errors?.Authorization){
              this.introductionForm.setErrors({submitError: err.error?.Authorization || err.error?.errors?.Authorization});
            }
            else if(err.error?.Title || err.error?.errors?.Title){
              this.title?.setErrors({submitError: err.error?.Title || err.error?.errors?.Title});
            }
            else if(err.error?.Description || err.error?.errors?.Description){
              this.description?.setErrors({submitError: err.error?.Description || err.error?.errors?.Description});
            }
            else if(err.error?.Image || err.error?.errors?.Image){
              this.image?.setErrors({submitError: err.error?.Image || err.error?.errors?.Image});
            }
            else{
              this.introductionForm.setErrors({submitError: err.error});
            }
          }
          else{
            throw(err);
          }
          this.displaySubmitSpinner.set(false);
        },
      };

      if(this.data.introductionOf === "document"){
        this.libraryService.editDocumentIntroduction(this.data.guid,this.title!.value,
          this.description!.value, this.image?.value ?? undefined).subscribe(callbacks);
      }
      else if(this.data.introductionOf === "library"){
        this.libraryService.editLibraryIntroduction(this.data.guid,this.title!.value,
          this.description!.value, this.image?.value ?? undefined).subscribe(callbacks);
      }
      else if(this.data.introductionOf === "shelf"){
        this.libraryService.editShelfIntroduction(this.data.guid,this.title!.value,
          this.description!.value, this.image?.value ?? undefined).subscribe(callbacks);
      }
    }
  }

  onDeleteImage(){
    this.displaySubmitSpinner.set(true);
      
    const callbacks = {
      next: (res:{success:boolean}) => {
        if(res && res.success){
          this.displaySubmitSpinner.set(false);
          this.dialogRef.close("ImageDelete");
        }
      },
      error: (err:any) => {
        this.displaySubmitSpinner.set(false);
        throw(err);
      },
    };

    if(this.data.introductionOf === "document"){
      this.libraryService.deleteDocumentIntroductionImage(this.data.guid).subscribe(callbacks);
    }
    else if(this.data.introductionOf === "library"){
      this.libraryService.deleteLibraryIntroductionImage(this.data.guid).subscribe(callbacks);
    }
    else if(this.data.introductionOf === "shelf"){
      this.libraryService.deleteShelfIntroductionImage(this.data.guid).subscribe(callbacks);
    }
  }


}
