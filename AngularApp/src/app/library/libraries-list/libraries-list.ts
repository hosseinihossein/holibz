import { Component, inject } from '@angular/core';
import { LibraryCard } from "../library-card/library-card";
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { MatTooltip } from '@angular/material/tooltip';
import { SingletonModes } from '../../services/singleton-modes';

@Component({
  selector: 'app-libraries-list',
  imports: [LibraryCard, MatButton, MatIcon, MatBadge,MatTooltip],
  templateUrl: './libraries-list.html',
  styleUrl: './libraries-list.css'
})
export class LibrariesList {
  singletonModes = inject(SingletonModes);
}
