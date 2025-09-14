import { Component, computed, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatOptgroup, MatOption, MatSelect } from '@angular/material/select';
import { JsonPipe } from '@angular/common';

@Component({
  selector: 'app-new-shelf-form',
  imports: [MatFormField,MatSelect,MatOptgroup,MatOption,MatButton,MatLabel,MatInput,MatIcon,
    ReactiveFormsModule,JsonPipe,MatError
  ],
  templateUrl: './new-shelf-form.html',
  styleUrl: './new-shelf-form.css'
})
export class NewShelfForm {
  newShelfForm = signal(new FormGroup({
    library: new FormControl(""),
    title: new FormControl(""),
    description: new FormControl(""),
  }));
  library = computed(()=>this.newShelfForm().get("library"));
  title = computed(()=>this.newShelfForm().get("title"));
  description = computed(()=>this.newShelfForm().get("description"));
}
