import { Component, input } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { DocumentElementModel } from '../document-element/document-element';

@Component({
  selector: 'app-link-element',
  imports: [MatIcon, MatButton],
  templateUrl: './link-element.html',
  styleUrl: './link-element.css'
})
export class LinkElement {
  elementModel = input.required<DocumentElementModel>();
}
