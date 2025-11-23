import { Component, input } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { SectionModel } from '../../../../models/section-model';
import { DocumentElementModel } from '../document-element/document-element';
import { ScrollLocator } from "../scroll-locator/scroll-locator";

@Component({
  selector: 'app-link-element',
  imports: [MatIcon, MatButton],
  templateUrl: './link-element.html',
  styleUrl: './link-element.css'
})
export class LinkElement {
  elementModel = input.required<DocumentElementModel>();
}
