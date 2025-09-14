import { AfterViewInit, Component, computed, ElementRef, inject, signal, viewChild, viewChildren } from '@angular/core';
import { FormBuilder,FormControl,FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatOptgroup, MatOption, MatSelect } from '@angular/material/select';
import { MatTooltip } from '@angular/material/tooltip';
import { JsonPipe } from '@angular/common';

@Component({
  selector: 'app-new-document-form',
  imports: [MatFormField, MatLabel, MatInput, MatButton, MatIconButton, MatIcon, MatTooltip, MatSelect,
    MatOption, MatOptgroup,ReactiveFormsModule,JsonPipe,MatError],
  templateUrl: './new-document-form.html',
  styleUrl: './new-document-form.css'
})
export class NewDocumentForm {
  newDocumentForm = signal(new FormGroup({
    library: new FormControl(""),
    shelf: new FormControl("default"),
    title: new FormControl(""),
    description: new FormControl(""),
    image: new FormControl<File|null>(null),
  }));
  library = computed(()=>this.newDocumentForm().get("library"));
  shelf = computed(()=>this.newDocumentForm().get("shelf"));
  title = computed(()=>this.newDocumentForm().get("title"));
  description = computed(()=>this.newDocumentForm().get("description"));
  image = computed(()=>this.newDocumentForm().get("image"));
  
  previewImgSrc = signal<string|null>(null);

  previewImg = viewChild<ElementRef<HTMLImageElement>>("previewImg");
  imgInput = viewChild.required<ElementRef<HTMLInputElement>>("fileInput");

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
    console.log(this.newDocumentForm().value)
  }

}
