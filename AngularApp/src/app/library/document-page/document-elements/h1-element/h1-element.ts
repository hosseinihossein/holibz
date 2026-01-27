import { AfterViewInit, Component, ElementRef, input, output, viewChild } from '@angular/core';
import { DocumentElement, DocumentElementModel } from "../document-element/document-element";

@Component({
  selector: 'app-h1-element',
  imports: [],
  templateUrl: './h1-element.html',
  styleUrl: './h1-element.css'
})
export class H1Element implements AfterViewInit{
  elementModel = input.required<DocumentElementModel>();
  headingInitialized = output<HTMLHeadingElement>();
  headingElement = viewChild.required<ElementRef<HTMLHeadingElement>>("headingElement");

  ngAfterViewInit(): void {
    this.headingInitialized.emit(this.headingElement().nativeElement);
  }
}
