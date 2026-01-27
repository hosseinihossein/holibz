import { Component, input } from '@angular/core';
import { MatFabButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { DocumentElement, DocumentElementModel } from "../document-element/document-element";

@Component({
  selector: 'app-file-element',
  imports: [MatIcon, MatFabButton],
  templateUrl: './file-element.html',
  styleUrl: './file-element.css'
})
export class FileElement {
  elementModel = input.required<DocumentElementModel>();
}
