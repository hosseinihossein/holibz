import { Component, ElementRef, input, output, viewChild } from '@angular/core';
import { DocumentElement, DocumentElementModel } from "../document-element/document-element";
import { SectionModel } from '../../../../models/section-model';
import { ScrollLocator } from "../scroll-locator/scroll-locator";

@Component({
  selector: 'app-h2-element',
  imports: [],
  templateUrl: './h2-element.html',
  styleUrl: './h2-element.css'
})
export class H2Element {
  elementModel = input.required<DocumentElementModel>();
  headingInitialized = output<HTMLHeadingElement>();
  headingElement = viewChild.required<ElementRef<HTMLHeadingElement>>("headingElement");

  ngAfterViewInit(): void {
    this.headingInitialized.emit(this.headingElement().nativeElement);
  }
}
