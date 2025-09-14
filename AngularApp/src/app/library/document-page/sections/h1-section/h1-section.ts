import { Component, input } from '@angular/core';
import { Section } from "../section/section";
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-h1-section',
  imports: [],
  templateUrl: './h1-section.html',
  styleUrl: './h1-section.css'
})
export class H1Section {
  sectionModel = input.required<SectionModel>();
}
