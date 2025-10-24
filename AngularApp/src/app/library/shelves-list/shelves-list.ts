import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from '@angular/material/sidenav';
import { ShelfCard, ShelfModel } from "../shelf-card/shelf-card";
import { MatAccordion } from '@angular/material/expansion';
import { LibraryService } from '../../services/library-service';
import { ActivatedRoute } from '@angular/router';
import { IdentityService, UserProfileModel } from '../../services/identity-service';

@Component({
  selector: 'app-shelves-list',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, ShelfCard, MatAccordion],
  templateUrl: './shelves-list.html',
  styleUrl: './shelves-list.css'
})
export class ShelvesList {
  libraryGuid = input.required<string>();
  userGuid = signal<string|null>(null);

  libraryService = inject(LibraryService);
  activatedRoute = inject(ActivatedRoute);
  identityService = inject(IdentityService);

  shelfModels = signal<ShelfModel[]>([]);
  totalNumberOfUserDocuments = signal(0);
  userModel = signal<UserProfileModel|null>(null);
  userImgSrc = computed(()=>this.userModel()?.imageAddress);
  isMyShelfList = computed(() => this.identityService.isAuthenticated() && this.identityService.userModel()?.guid === this.userGuid());//signal(false);

  constructor(){
    effect(() => {
      this.libraryService.requestShelfList(this.libraryGuid())?.subscribe({
        next: res => {
          if(res){
            this.shelfModels.set(res);
            this.totalNumberOfUserDocuments.set(res.flatMap(shelf=>shelf.documentsGuids).length);
            if(res[0].ownerGuid){
              this.userGuid.set(res[0].ownerGuid);
            }
          }
        },
      });
    });
    
    effect(() => {
      if(this.userGuid()){
        this.identityService.requestUserModel(this.userGuid()!).subscribe({
          next: res => {
            this.userModel.set(res);
          },
        });
      }
    });

    effect(() => {
      if(this.isMyShelfList()){
        this.identityService.getCsrf().subscribe({
          next: () => {
            console.log("Csrf received successfully.");
          },
          error: err => {
            console.error("Couldn't get Csrf!");
            throw(err);
          },
        });
      }
    });
  }
}
