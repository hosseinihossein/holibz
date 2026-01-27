import { Component, computed, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { SingletonModes } from '../../services/singleton-modes';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';

@Component({
  selector: 'app-edit-image',
  imports: [MatDialogContent, MatButton,MatDialogActions,MatDialogClose,WaitSpinner],
  templateUrl: './edit-user-image.html',
  styleUrl: './edit-user-image.css'
})
export class EditUserImage {
  readonly editImageDialogRef = inject(MatDialogRef<EditUserImage>);
  //readonly data = inject<{currentImgSrc:string|null}>(MAT_DIALOG_DATA);
  readonly identityService = inject(IdentityService);
  readonly singletonModes = inject(SingletonModes);
  
  currentImgSrc = computed(()=>this.singletonModes.getUserImageAddress(this.identityService.userModel()));
  selectedFile = signal<File | null>(null);
  previewImgSrc = signal(this.currentImgSrc());

  previewImg = viewChild<ElementRef<HTMLImageElement>>("previewImg");

  displayWaitSpinner = signal(false);
  errorResponse = signal<{message:string}|null>(null);

  constructor(){}

  onSelectImage(event:Event){
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.errorResponse.set(null);
      if(input.files[0].size > (120 * 1024)){
        this.errorResponse.set({message: "Selected image must be less than 120 KB!"});
      }
      else{
        this.selectedFile.set(input.files[0]);

        const reader = new FileReader(); // Create a FileReader instance

        // Load the image as a Data URL
        reader.onload = (e)=> {
          if(this.previewImg()){
            this.previewImgSrc.set(e.target!.result as string ?? this.currentImgSrc());
          }
        };

        reader.readAsDataURL(this.selectedFile()!); // Read the file as a Data URL
      }
    }
    else{
      this.selectedFile.set(null);
      this.previewImgSrc.set(this.currentImgSrc());
    }
  }

  onSubmit(){
    if(this.selectedFile()){
      this.displayWaitSpinner.set(true);

      this.identityService.submitUserImage(this.selectedFile()!).subscribe({
        next: res => {
          if(res.success){
            let newUserModel = new UserProfileModel(this.identityService.userModel());
            newUserModel.hasImage = res.hasImage;
            newUserModel.integrityVersion = res.integrityVersion;
            this.identityService.updateUserModel(newUserModel);
            this.displayWaitSpinner.set(false);
            this.editImageDialogRef.close();
          }
        },
        error: err => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error.UserImageFile || err.error.errors?.UserImageFile){
              this.errorResponse.set({message: err.error.UserImageFile || err.error.errors?.UserImageFile});
            }
            else{
              console.error("err: "+JSON.stringify(err));
              console.error("err.error: "+JSON.stringify(err.error));
              console.error("err.error.errors: "+JSON.stringify(err.error.errors));
              throw(err);
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

  onDelete(){
    this.displayWaitSpinner.set(true);

    this.identityService.deleteUserImage().subscribe({
      next: res => {
        if(res.success){
          let newUserModel = new UserProfileModel(this.identityService.userModel());
          newUserModel.hasImage = false;
          this.identityService.updateUserModel(newUserModel);
          this.displayWaitSpinner.set(false);
          this.editImageDialogRef.close();
        }
      },
      error: err => {
        this.errorResponse.set({message: "Error happened during delete request!"});
        console.error("err: "+JSON.stringify(err));
        console.error("err.error: "+JSON.stringify(err.error));
        console.error("err.error.errors: "+JSON.stringify(err.error.errors));
        this.displayWaitSpinner.set(false);
        throw(err);
      },
    });
  }

}
