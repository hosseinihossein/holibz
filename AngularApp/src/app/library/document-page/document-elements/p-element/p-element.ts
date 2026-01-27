import { Component, input } from '@angular/core';
import { DocumentElement, DocumentElementModel } from "../document-element/document-element";

@Component({
  selector: 'app-p-element',
  imports: [],
  templateUrl: './p-element.html',
  styleUrl: './p-element.css'
})
export class PElement {
  elementModel = input.required<DocumentElementModel>();
}
