import { Component, input } from '@angular/core';
import { MatFabButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { DocumentElement } from "../document-element/document-element";
import { SectionModel } from '../../../../models/section-model';
import { DocumentService } from '../../../../services/document-service';

@Component({
  selector: 'app-file-element',
  imports: [MatIcon, MatFabButton],
  templateUrl: './file-element.html',
  styleUrl: './file-element.css'
})
export class FileElement {
  sectionModel = input.required<SectionModel>();
}
