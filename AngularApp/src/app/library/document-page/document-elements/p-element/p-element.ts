import { Component, input } from '@angular/core';
import { DocumentElement, DocumentElementModel } from "../document-element/document-element";
import { SectionModel } from '../../../../models/section-model';
import { ScrollLocator } from "../scroll-locator/scroll-locator";

@Component({
  selector: 'app-p-element',
  imports: [],
  templateUrl: './p-element.html',
  styleUrl: './p-element.css'
})
export class PElement {
  elementModel = input.required<DocumentElementModel>();
}
