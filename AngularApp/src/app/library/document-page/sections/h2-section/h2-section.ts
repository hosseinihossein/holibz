import { Component, input } from '@angular/core';
import { Section } from "../section/section";
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-h2-section',
  imports: [],
  templateUrl: './h2-section.html',
  styleUrl: './h2-section.css'
})
export class H2Section {
  sectionModel = input.required<SectionModel>();
}
