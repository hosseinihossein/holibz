import { Component, input } from '@angular/core';
import { DocumentElement } from "../document-element/document-element";
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-h2-element',
  imports: [],
  templateUrl: './h2-element.html',
  styleUrl: './h2-element.css'
})
export class H2Element {
  sectionModel = input.required<SectionModel>();
}
