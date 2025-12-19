import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { BackupAdminService, BackupStatus } from './backup-admin-service';
import { WindowService } from '../../services/window-service';

@Component({
  selector: 'app-backup-admin',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions, 
    MatButton],
  templateUrl: './backup-admin.html',
  styleUrl: './backup-admin.css'
})
export class BackupAdmin {
  backupStatus = signal<BackupStatus|null>(null);

  backupService = inject(BackupAdminService);
  windowService = inject(WindowService);

  matCardActions = viewChild.required(MatCardActions, {read:ElementRef});

  constructor(){}

  download() {
    if(this.backupStatus){
      this.backupService.requestDownloadingBackupFile().subscribe({
        next: (blob: Blob) => {
          // Create a temporary link to trigger browser download
          const url = window.URL.createObjectURL(blob);
          const a = this.windowService.nativeWindow.document.createElement('a');
          a.href = url;
          a.download = this.backupStatus()!.fileName; // Suggested filename
          this.matCardActions().nativeElement.appendChild(a);
          a.click();
          this.matCardActions().nativeElement.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: (err) => {
          console.error('Download failed:', err);
          throw(err);
        }
      });
    }
  }
}


