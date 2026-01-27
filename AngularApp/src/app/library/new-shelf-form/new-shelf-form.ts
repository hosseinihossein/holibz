import { Component, computed, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { JsonPipe } from '@angular/common';
import { LibraryService } from '../../services/library-service';
import { ActivatedRoute, Router } from '@angular/router';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { LibraryCardModel } from '../library-card/library-card';
import { IdentityService } from '../../services/identity-service';
import { Result } from '../../dialogs/result/result';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';

@Component({
  selector: 'app-new-shelf-form',
  imports: [MatFormField,/*MatSelect,MatOption,*/MatButton,MatLabel,MatInput,MatIcon,
    ReactiveFormsModule,MatError,WaitSpinner,MatIconButton,MatButtonToggleModule
  ],
  templateUrl: './new-shelf-form.html',
  styleUrl: './new-shelf-form.css'
})
export class NewShelfForm {
  identityService = inject(IdentityService);
  libraryService = inject(LibraryService);
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  readonly dialog = inject(MatDialog);

  newShelfForm = new FormGroup({
    libraryGuids: new FormControl<string[]>([], {nonNullable:true, validators: [Validators.required, Validators.maxLength(32)]}),
    title: new FormControl("", {nonNullable:true, validators: [Validators.required, Validators.maxLength(30),Validators.minLength(3)]}),
    description: new FormControl("", {validators: Validators.maxLength(200)}),
    image: new FormControl<File|null>(null),
  });
  libraryGuid = this.newShelfForm.get("libraryGuid");
  title = this.newShelfForm.get("title");
  description = this.newShelfForm.get("description");
  image = this.newShelfForm.get("image");

  previewImgSrc = signal<string|null>(null);
  displayWaitSpinner = signal(false);
  libraryList = signal<LibraryCardModel[]>([]);

  previewImg = viewChild<ElementRef<HTMLImageElement>>("previewImg");
  imgInput = viewChild.required<ElementRef<HTMLInputElement>>("fileInput");

  constructor(){
    let currentLibraryGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("libraryGuid");
    if(currentLibraryGuidRouteParam){
      this.newShelfForm.controls["libraryGuids"].setValue([currentLibraryGuidRouteParam]);
    }

    effect(() => {
      if(this.identityService.userModel()){
        this.libraryService.requestLibraryList(this.identityService.userModel()!.guid!).subscribe({
          next: res => {
            if(res){
              this.libraryList.set(res);
            }
          },
        });
        
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
        this.newShelfForm.get("image")?.setValue(input.files[0]);

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
      this.newShelfForm.get("image")?.setValue(null);
    }
  }

  clearImgInput(){
    this.imgInput().nativeElement.value = '';
    this.previewImgSrc.set(null);
    this.newShelfForm.get("image")?.setValue(null);
  }

  onSubmit(){
    if(this.newShelfForm.valid){
      console.log(JSON.stringify(this.newShelfForm.value));
      this.displayWaitSpinner.set(true);
      this.libraryService.createNewShelf(this.newShelfForm.value).subscribe({
        next: res => {
          if(res && res.success){
            this.displayWaitSpinner.set(false);
            this.router.navigate(['/shelf',res.shelfGuid]);
          }
        },
        error: err => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error?.Title || err.error?.errors?.Title){
              this.title?.setErrors({submitError: err.error?.Title || err.error?.errors?.Title});
            }
            else if(err.error?.Description || err.error?.errors?.Description){
              this.description?.setErrors({submitError: err.error?.Description || err.error?.errors?.Description});
            }
            else if(err.error?.Libraryguid || err.error?.errors?.Libraryguid){
              this.libraryGuid?.setErrors({submitError: err.error?.Libraryguid || err.error?.errors?.Libraryguid});
            }
            else if(err.error?.Image || err.error?.errors?.Image){
              this.image?.setErrors({submitError: err.error?.Image || err.error?.errors?.Image});
            }
            else{
              this.newShelfForm.setErrors({submitError: JSON.stringify(err.error)});
            }
          }
          else{
            throw(err);
          }
          this.displayWaitSpinner.set(false);
        },
      });
    }
  }
}
