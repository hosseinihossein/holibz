import { Component, input } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { SectionModel } from '../../../../models/section-model';

@Component({
  selector: 'app-link-element',
  imports: [MatIcon, MatButton],
  templateUrl: './link-element.html',
  styleUrl: './link-element.css'
})
export class LinkElement {
  sectionModel = input.required<SectionModel>();
}
