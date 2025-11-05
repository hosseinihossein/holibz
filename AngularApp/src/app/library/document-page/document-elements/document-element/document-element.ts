import { AfterViewInit, Component, inject, input, model, output } from '@angular/core';
import { EditBox } from "../edit-box/edit-box";
import { SingletonModes } from '../../../../services/singleton-modes';
import { SectionModel } from '../../../../models/section-model';
import { H1Element } from '../h1-element/h1-element';
import { H2Element } from '../h2-element/h2-element';
import { PElement } from '../p-element/p-element';
import { ImgElement } from '../img-element/img-element';
import { CodeElement } from '../code-element/code-element';
import { FileElement } from '../file-element/file-element';
import { LinkElement } from '../link-element/link-element';

@Component({
  selector: 'app-document-element',
  imports: [EditBox, H1Element, H2Element, PElement, ImgElement, CodeElement, FileElement, LinkElement],
  templateUrl: './document-element.html',
  styleUrl: './document-element.css'
})
export class DocumentElement /*implements AfterViewInit*/ {
  elementModel = input.required<DocumentElementModel>();

  singletonModes = inject(SingletonModes);

  headingInitialized = output<HTMLHeadingElement>();
  editElement = output<DocumentElementModel>();
  deleteElement = output<string>();

  /*passChildHeadingInitEventToParent(headingElement:HTMLHeadingElement){
    this.headingInitialized.emit(headingElement);
  }
  passChildDeleteEventToParent(elementGuid:string){
    this.deleteElement.emit(elementGuid);
  }
  passChildEditEventToParent(editedElement:DocumentElementModel){
    this.editElement.emit(editedElement);
  }*/
}

export class DocumentElementModel {
  guid:string = null!;
  ownerGuid:string = null!;
  type: "h1"|"h2"|"p"|"img"|"code"|"file"|"link" = null!;
  value:string = null!;
  order:number = null!;
  title?: string;
  updatedAt:Date = null!;
}
