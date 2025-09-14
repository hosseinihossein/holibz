import { Component, input } from '@angular/core';
import { MatFabButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { Section } from "../section/section";
import { SectionModel } from '../../../../models/section-model';
import { DocumentService } from '../../../../services/document-service';

@Component({
  selector: 'app-file-section',
  imports: [MatIcon, MatFabButton],
  templateUrl: './file-section.html',
  styleUrl: './file-section.css'
})
export class FileSection {
  sectionModel = input.required<SectionModel>();
}
