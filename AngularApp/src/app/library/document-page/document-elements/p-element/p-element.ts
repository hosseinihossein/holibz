import { Component, input } from '@angular/core';
import { DocumentElement } from "../document-element/document-element";
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-p-element',
  imports: [],
  templateUrl: './p-element.html',
  styleUrl: './p-element.css'
})
export class PElement {
  sectionModel = input.required<SectionModel>();
}
