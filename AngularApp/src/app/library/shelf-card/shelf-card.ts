import { Component } from '@angular/core';
import { MatAccordion, MatExpansionPanel, MatExpansionPanelActionRow, MatExpansionPanelDescription, MatExpansionPanelHeader, MatExpansionPanelTitle } from "@angular/material/expansion";
import { MatTooltip } from '@angular/material/tooltip';
import { DocumentCard } from "../document-card/document-card";
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';

@Component({
  selector: 'app-shelf-card',
  imports: [MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle,
    MatExpansionPanelDescription, MatTooltip, DocumentCard, MatExpansionPanelActionRow, MatButton, 
    MatIcon, MatBadge],
  templateUrl: './shelf-card.html',
  styleUrl: './shelf-card.css'
})
export class ShelfCard {

}
