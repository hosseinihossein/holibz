import { AfterViewInit, Component, computed, effect, ElementRef, inject, signal, viewChild, viewChildren } from '@angular/core';
import { FormBuilder,FormControl,FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatOptgroup, MatOption, MatSelect } from '@angular/material/select';
import { MatTooltip } from '@angular/material/tooltip';
import { JsonPipe } from '@angular/common';
import { IdentityService } from '../../services/identity-service';
import { LibraryService, NewDocumentFormModel } from '../../services/library-service';
import { ActivatedRoute, Router } from '@angular/router';
import { LibraryCardModel } from '../library-card/library-card';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { ShelfCardModel } from '../shelf-card/shelf-card';
import { MatButtonToggleChange, MatButtonToggleModule } from '@angular/material/button-toggle';

@Component({
  selector: 'app-new-document-form',
  imports: [MatFormField, MatLabel, MatInput, MatButton, MatIconButton, MatIcon, MatTooltip, MatSelect,
    MatOption, MatOptgroup,ReactiveFormsModule,JsonPipe,MatError,MatProgressSpinner,
    MatButtonToggleModule],
  templateUrl: './new-document-form.html',
  styleUrl: './new-document-form.css'
})
export class NewDocumentForm {
  identityService = inject(IdentityService);
  libraryService = inject(LibraryService);
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);

  //currentLibraryGuid = signal("DefaultLibrary");
  //currentShelfGuid = signal("DefaultShelf");

  newDocumentForm = signal(new FormGroup({
    //library: new FormControl(""),
    shelfGuids: new FormControl("DefaultShelf", {nonNullable:true, validators: [Validators.required]}),
    title: new FormControl("", {nonNullable:true, validators: [Validators.required, Validators.maxLength(30),Validators.minLength(3)]}),
    description: new FormControl("", {validators: Validators.maxLength(200)}),
    image: new FormControl<File|null>(null),
  }));
  //library = computed(()=>this.newDocumentForm().get("library"));
  shelfGuids = computed(()=>this.newDocumentForm().get("shelfGuids"));
  title = computed(()=>this.newDocumentForm().get("title"));
  description = computed(()=>this.newDocumentForm().get("description"));
  image = computed(()=>this.newDocumentForm().get("image"));
  
  previewImgSrc = signal<string|null>(null);
  displaySubmitSpinner = signal(false);
  allLibraryList = signal<LibraryCardModel[]>([]);
  displayedLibraries = signal<string[]>([]);
  allShelfList = signal<ShelfCardModel[]>([]);

  previewImg = viewChild<ElementRef<HTMLImageElement>>("previewImg");
  imgInput = viewChild.required<ElementRef<HTMLInputElement>>("fileInput");

  constructor(){
    let currentShelfGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("shelfGuid");
    if(currentShelfGuidRouteParam){
      this.newDocumentForm().controls["shelfGuids"].setValue(currentShelfGuidRouteParam);
    }

    effect(() => {
      this.libraryService.requestLibraryList(this.identityService.userModel()?.guid)?.subscribe({
        next: res => {
          if(res){
            this.allLibraryList.set(res);
            this.displayedLibraries.set(res.map(l=>l.title));
          }
        },
      });

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

    effect(() => {
      for(let libraryModel of this.allLibraryList()){
        this.libraryService.requestShelfList(libraryModel.guid).subscribe({
          next: res => {
            if(res){
              this.allShelfList.update(shelfList=>[...shelfList, ...res]);
            }
          },
        });
      }
    });
  }

  changeDisplayedLibraries(e:MatButtonToggleChange){
    this.displayedLibraries.set(e.value);
  }

  onSelectImage(event:Event){
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      if(input.files[0].size > (250 * 1024)){
        //create a form validator
      }
      else{
        this.newDocumentForm().get("image")?.setValue(input.files[0]);

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
      this.newDocumentForm().get("image")?.setValue(null);
    }
  }

  clearImgInput(){
    this.imgInput().nativeElement.value = '';
    this.previewImgSrc.set(null);
    this.newDocumentForm().get("image")?.setValue(null);
  }

  onSubmit(){
    if(this.newDocumentForm().valid){
      this.displaySubmitSpinner.set(true);
      let newDocumentFormModel: NewDocumentFormModel = {
        description: this.description()?.value,
        image: this.image()?.value,
        shelfGuids: this.shelfGuids()?.value.split(',').map<string>(s=>s.trim()),
        title: this.title()?.value,
      }
      this.libraryService.createNewDocument(newDocumentFormModel).subscribe({
        next: res => {
          if(res && res.success){
            this.displaySubmitSpinner.set(false);
            this.router.navigate(['/shelf',res.documentGuid]);
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
            else if(err.error?.ShelfGuids || err.error?.errors?.ShelfGuids){
              this.shelfGuids()?.setErrors({submitError: err.error?.ShelfGuids || err.error?.errors?.ShelfGuids});
            }
            else if(err.error?.Image || err.error?.errors?.Image){
              this.image()?.setErrors({submitError: err.error?.Image || err.error?.errors?.Image});
            }
            else{
              this.newDocumentForm().setErrors({submitError: err.error});
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
