import { Component, input } from '@angular/core';
import { DocumentElement, DocumentElementModel } from "../document-element/document-element";
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-h1-element',
  imports: [],
  templateUrl: './h1-element.html',
  styleUrl: './h1-element.css'
})
export class H1Element {
  elementModel = input.required<DocumentElementModel>();
}
