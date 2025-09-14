import { AfterViewInit, Component, inject, input } from '@angular/core';
import { EditBox } from "../edit-box/edit-box";
import { SingletonModes } from '../../../../services/singleton-modes';
import { SectionModel } from '../../../../models/section-model';
import { H1Section } from '../h1-section/h1-section';
import { H2Section } from '../h2-section/h2-section';
import { PSection } from '../p-section/p-section';
import { ImgSection } from '../img-section/img-section';
import { CodeSection } from '../code-section/code-section';
import { FileSection } from '../file-section/file-section';
import { LinkSection } from '../link-section/link-section';

@Component({
  selector: 'app-section',
  imports: [EditBox, H1Section, H2Section, PSection, ImgSection, CodeSection, FileSection, LinkSection],
  templateUrl: './section.html',
  styleUrl: './section.css'
})
export class Section implements AfterViewInit {
  singletonModes = inject(SingletonModes);
  sectionModel = input.required<SectionModel>();

  constructor(){
    console.log("app-section constructor!");
  }
  ngAfterViewInit(): void {
    console.log("app-section after view inir!");
  }
}
