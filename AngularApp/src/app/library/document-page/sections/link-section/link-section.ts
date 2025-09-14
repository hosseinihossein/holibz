import { Component, input } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-link-section',
  imports: [MatIcon, MatButton],
  templateUrl: './link-section.html',
  styleUrl: './link-section.css'
})
export class LinkSection {
  sectionModel = input.required<SectionModel>();
}
