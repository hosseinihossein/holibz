import { Component, ElementRef, inject, input, signal, viewChild } from '@angular/core';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardContent, MatCardFooter, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { SingletonModes } from '../../services/singleton-modes';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';

@Component({
  selector: 'app-document-card',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions,
    MatButton, MatIconButton, MatIcon, MatTooltip],
  templateUrl: './document-card.html',
  styleUrl: './document-card.css'
})
export class DocumentCard {
  imgSrc = input("");
  title = input.required<string>();
  description =  input.required<string>();
  avatarSrc =  input("defaultProfile.jpg");
  ownerUsername =  input.required<string>();
  mini = input(false);
  alone = input(false);

  singletonModes = inject(SingletonModes);

  documentCard = viewChild(MatCard,{read:ElementRef});
  
  appearance = signal<"outlined"|"raised"|"filled">("outlined");

  raiseCard(){
    this.appearance.set("raised");
    this.documentCard()?.nativeElement.classList.add("raised");
  }
  outlineCard(){
    this.appearance.set("outlined");
    this.documentCard()?.nativeElement.classList.remove("raised");
  }
}
