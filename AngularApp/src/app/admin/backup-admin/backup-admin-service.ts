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
    return this.httpClient.get("/api/Backup/GenerateBackupFile", {
      responseType: "blob",//browsers handle Blob streaming efficiently
      observe: "body",//by default return response body, so it's optional
    });
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
  fileSize:number = 0;
  fileName:string = null!;
}
