import { Component } from '@angular/core';

@Component({
  selector: 'app-scroll-locator',
  imports: [],
  templateUrl: './scroll-locator.html',
  styleUrl: './scroll-locator.css',
  styles:[`
    :host{
      position: absolute;
      top: -64px;
      width: 100%;
    }
  `],
})
export class ScrollLocator {

}
