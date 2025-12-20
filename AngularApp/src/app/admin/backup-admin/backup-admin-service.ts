import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BackupAdminService {
  readonly httpClient = inject(HttpClient);

  requestBackupStatus(){
    return this.httpClient.get<BackupStatus>("/api/Backup/GetBackupStatus");
  }
  requestGeneratingBackupFile(){
    return this.httpClient.get("/api/Backup/GenerateBackupFile");
  }
  requestDownloadingBackupFile(){
    return this.httpClient.get("/api/Backup/DownloadBackupFile", {
      responseType: "blob",//browsers handle Blob streaming efficiently
      observe: "body",//by default return response body, so it's optional
    });
  }
  requestDeleteBackup(){
    return this.httpClient.delete("/api/Backup/DeleteBackupFile");
  }

}

export class BackupStatus {
  overall_Status:string = null!;
  createdAt:Date = null!;
  fileSize:number = 0;
  fileName:string = null!;
  readyToDownload:boolean = false;
}
