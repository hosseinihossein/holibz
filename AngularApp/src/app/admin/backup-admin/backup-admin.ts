import { Component, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardModule, MatCardSubtitle, MatCardTitle } from '@angular/material/card';

@Component({
  selector: 'app-backup-admin',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions, 
    MatButton],
  templateUrl: './backup-admin.html',
  styleUrl: './backup-admin.css'
})
export class BackupAdmin {
  backupStatus = signal<BackupStatus|null>(null);

  constructor(){
    
  }
}

export class BackupStatus {
  overall_Status:string = null!;
  identity_SeedStatus:string = null!;
  library_SeedStatus:string = null!;
  review_SeedStatus:string = null!;
  notification_SeedStatus:string = null!;
  description:string[] = [];
  createdAt:Date = null!;
}
