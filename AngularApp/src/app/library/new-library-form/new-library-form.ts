import { Component, computed, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { IdentityService } from '../../services/identity-service';
import { LibraryService } from '../../services/library-service';
import { Router } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { MatIcon } from '@angular/material/icon';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { JsonPipe } from '@angular/common';
import { MatInput } from '@angular/material/input';
import { MatButton, MatIconButton } from '@angular/material/button';
import { Result } from '../../dialogs/result/result';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-new-library-form',
  imports: [ReactiveFormsModule,MatIcon,MatFormField,MatLabel,MatError,MatProgressSpinner,JsonPipe,
    MatInput,MatButton,MatIconButton
  ],
  templateUrl: './new-library-form.html',
  styleUrl: './new-library-form.css'
})
export class NewLibraryForm {
  identityService = inject(IdentityService);
  libraryService = inject(LibraryService);
  router = inject(Router);
  readonly dialog = inject(MatDialog);

  newLibraryForm = signal(new FormGroup({
    title: new FormControl("", {nonNullable:true, validators: [Validators.required, Validators.maxLength(30),Validators.minLength(3)]}),
    description: new FormControl("", {validators: Validators.maxLength(200)}),
    image: new FormControl<File|null>(null),
  }));
  title = computed(()=>this.newLibraryForm().get("title"));
  description = computed(()=>this.newLibraryForm().get("description"));
  image = computed(()=>this.newLibraryForm().get("image"));

  previewImgSrc = signal<string|null>(null);
  displaySubmitSpinner = signal(false);

  previewImg = viewChild<ElementRef<HTMLImageElement>>("previewImg");
  imgInput = viewChild.required<ElementRef<HTMLInputElement>>("fileInput");

  constructor(){
    effect(() => {
      if(this.identityService.userModel()){
        this.identityService.getCsrf().subscribe({
          next: () => {
            console.log("Csrf received successfully.");
          },
          error: err => {
            console.error("Couldn't get Csrf!");
            //throwError(()=>err);//doesn't pass error to the app-error-handler
            throw(err);
          },
        });
      }
    });
  }

  onSelectImage(event:Event){
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      if(input.files[0].size > (128 * 1024)){
        const dialogRef = this.dialog.open(Result,{
          data:{
            status: "warning",
            title: "Image Size Limit",
            description: ["The size of the selected image cannot be larger than 120 KB!"]
          }
        });
        dialogRef.afterClosed().subscribe(()=>{
          this.clearImgInput();
        });
      }
      else{
        this.newLibraryForm().get("image")?.setValue(input.files[0]);

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
      this.newLibraryForm().get("image")?.setValue(null);
    }
  }

  clearImgInput(){
    this.imgInput().nativeElement.value = '';
    this.previewImgSrc.set(null);
    this.newLibraryForm().get("image")?.setValue(null);
  }

  onSubmit(){
    if(this.newLibraryForm().valid){
      this.displaySubmitSpinner.set(true);
      this.libraryService.createNewLibrary(this.newLibraryForm().value).subscribe({
        next: res => {
          if(res && res.success){
            this.displaySubmitSpinner.set(false);
            this.router.navigate(['/library',res.libraryGuid]);
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
            else if(err.error?.Image || err.error?.errors?.Image){
              this.image()?.setErrors({submitError: err.error?.Image || err.error?.errors?.Image});
            }
            else{
              this.newLibraryForm().setErrors({submitError: err.error});
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
