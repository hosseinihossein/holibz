import { AfterViewInit, Component, computed, ElementRef, inject, input } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIcon } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatTooltip } from '@angular/material/tooltip';
import { WindowService } from '../../services/window-service';
import { MatDialog } from '@angular/material/dialog';
import { MatMenu, MatMenuItem, MatMenuTrigger } from "@angular/material/menu";
import { ConfirmDelete } from '../../dialogs/confirm-delete/confirm-delete';
import { Clipboard } from '@angular/cdk/clipboard';
import { SingletonModes } from '../../services/singleton-modes';
import { DocumentService } from '../../services/document-service';
import { Section } from './sections/section/section';
import { MatChip, MatChipSet } from "@angular/material/chips";
import { EditTags } from '../../dialogs/edit-tags/edit-tags';

@Component({
  selector: 'app-document-page',
  imports: [MatSidenavModule, MatExpansionModule, MatTooltip, MatButton, MatIcon,
    MatMenu, MatMenuItem, MatMenuTrigger, Section, MatChipSet, MatChip],
  templateUrl: './document-page.html',
  styleUrl: './document-page.css'
})
export class DocumentPage implements AfterViewInit {
  documentGuid = input.required<string>();

  clipboard = inject(Clipboard);
  windowService = inject(WindowService);
  readonly dialog = inject(MatDialog);
  singletonModes = inject(SingletonModes);
  elementRef = inject(ElementRef);
  documentService = inject(DocumentService);

  sortedSectoins = computed(()=>this.documentService.allSectionModels().sort((a,b)=>{if(a.order > b.order)return 1;else return -1;}))
  
  constructor(){
    console.log("app-document constructor!");
  }

  ngAfterViewInit(): void {
    console.log("app-document after view inir!");
    //a better prefered approach renderer2
    const viewPortObserver = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          this.windowService.nativeWindow.document.getElementById("overview-"+entry.target.id)?.classList.add("active");
        } else {
          this.windowService.nativeWindow.document.getElementById("overview-"+entry.target.id)?.classList.remove("active");
        }
      });
    });

    const headers = (this.elementRef.nativeElement as HTMLElement).getElementsByClassName("headerSection");
    for(let header of headers){
      viewPortObserver.observe(header);
    }
  }

  confirmDelete(){
    const dialogRef = this.dialog.open(ConfirmDelete);
    dialogRef.afterClosed().subscribe(result=>{
      if(result === "true"){
        console.log("this document has been deleted!");
      }
    });
  }

  editTags(){
    const dialogRef = this.dialog.open(EditTags);
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.documentService.updateDocumentTags(result);
      }
    });
  }

  addNewSection(type:"h1" | "h2" | "p" | "img" | "code" | "file" | "link"){
    this.documentService.mockAddSection(type);
  }

}
