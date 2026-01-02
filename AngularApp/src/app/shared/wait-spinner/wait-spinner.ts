import { Component } from '@angular/core';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-wait-spinner',
  imports: [MatProgressSpinner],
  templateUrl: './wait-spinner.html',
  styleUrl: './wait-spinner.css'
})
export class WaitSpinner {

}
