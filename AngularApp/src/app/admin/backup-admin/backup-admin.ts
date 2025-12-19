import { Component, effect, ElementRef, inject, OnDestroy, signal, viewChild } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { BackupAdminService, BackupStatus } from './backup-admin-service';
import { WindowService } from '../../services/window-service';
import { DatePipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { Result } from '../../dialogs/result/result';

@Component({
  selector: 'app-backup-admin',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatCardActions, 
    MatButton, DatePipe],
  templateUrl: './backup-admin.html',
  styleUrl: './backup-admin.css'
})
export class BackupAdmin implements OnDestroy {
  private refreshInterval = 0;
  backupStatus = signal<BackupStatus|null>(null);

  backupService = inject(BackupAdminService);
  windowService = inject(WindowService);
  dialog = inject(MatDialog);

  matCardActions = viewChild.required(MatCardActions, {read:ElementRef});

  constructor(){
    this.backupService.requestBackupStatus().subscribe({
      next: res => {
        if(res){
          this.backupStatus.set(res);
        }
      },
      error: err => {
        console.log(JSON.stringify(err));
        this.backupStatus.set(null);
      },
    });
  }
  ngOnDestroy(): void {
    clearInterval(this.refreshInterval);
  }

  downloadFile() {
    if(this.backupStatus()){
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
  deleteFile(){
    if(this.backupStatus()){
      this.backupService.requestDeleteBackup().subscribe({
        next: ()=>{
          this.backupStatus.set(null);
        },
      });
    }
  }
  generateFile(){
    this.backupService.requestGeneratingBackupFile().subscribe({
      next: () => {
        //define interval
        this.refreshInterval = setInterval(() => {
          this.backupService.requestBackupStatus().subscribe({
            next: res => {
              if(res){
                this.backupStatus.set(res);
              }
            },
            error: err => {
              console.log(JSON.stringify(err));
              this.backupStatus.set(null);
            },
          });
        }, 10000);//every 10 seconds

        //display result
        this.dialog.open(Result, {data:{
          status: "success",
          title: "Generating Backup File",
          description: [
            "Your request for generating the backup file sent successfully.",
            "Please wait untill the backup file gets ready.",
            "You can check whether it's ready to download or not from the status information that gets refreshed every 10 seconds."
          ],
        }});
      },
    });
  }

}


