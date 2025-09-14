import { Component, input } from '@angular/core';
import { Section } from "../section/section";
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-p-section',
  imports: [],
  templateUrl: './p-section.html',
  styleUrl: './p-section.css'
})
export class PSection {
  sectionModel = input.required<SectionModel>();
}
