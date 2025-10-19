import { Component, input, OnInit, viewChild } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatMenuTrigger, MatMenu } from '@angular/material/menu';

@Component({
  selector: 'app-dropdown-button',
  imports: [MatButton, MatIcon, MatMenuTrigger],
  templateUrl: './dropdown-button.html',
  styleUrl: './dropdown-button.css'
})
export class DropdownButton implements OnInit {
  menu = input.required<MatMenu>();
  btn = viewChild.required(MatButton);
  icon = viewChild.required(MatIcon);

  ngOnInit(){
    this.icon()._elementRef.nativeElement.style.transition = "all 100ms";
  }

  open(){
    this.btn()._elementRef.nativeElement.classList.add("open");
  }

  close(){
    this.btn()._elementRef.nativeElement.classList.remove("open");
  }

}
