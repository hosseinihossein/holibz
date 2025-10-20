import { AfterViewInit, Component, ElementRef, inject, OnDestroy, OnInit, signal, viewChild } from '@angular/core';
import { MatButton, MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { AdminService, UsersListFilterModel } from '../../../services/admin-service';
import { MatSidenavModule } from '@angular/material/sidenav';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { catchError, fromEvent, merge, of, startWith, Subscription, switchMap } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { Result } from '../../../dialogs/result/result';

@Component({
  selector: 'app-users-list',
  imports: [MatTableModule, MatButtonModule, MatButtonToggleModule, MatFormFieldModule, 
  MatPaginatorModule, MatProgressSpinner, MatIcon, MatSidenavModule, ReactiveFormsModule, 
  MatInputModule, MatDatepickerModule, MatRadioModule, MatSelectModule, MatSortModule],
  templateUrl: './users-list.html',
  styleUrl: './users-list.css'
})
export class UsersList implements AfterViewInit, OnDestroy {
  private subscription = signal(new Subscription());

  dataSource = signal<UsersListModel[]>([]);
  displayedColumns = signal<string[]>(["UserImage","UserName","UserGuid","Email","EmailConfirmed",
  "DisplayEmailPublicly","CreatedAt","Roles","Actions"]);
  displayLoadingSpinner = signal<boolean>(true);
  resultsLength = signal<number>(0);

  usernameFilter = signal(new FormControl<string|null>(null,{validators:[Validators.maxLength(60)]}));
  emailFilter = signal(new FormControl<string|null>(null,{validators:[Validators.maxLength(60)]}));
  createdFromFilter = signal(new FormControl<Date|null>(null,{validators:[Validators.maxLength(60)]}));
  createdToFilter = signal(new FormControl<Date|null>(null,{validators:[Validators.maxLength(60)]}));
  emailConfirmedFilter = signal(new FormControl<boolean|null>(null));
  displayEmailPubliclyFilter = signal(new FormControl<boolean|null>(null));
  //rolesFilter = signal(new FormControl<string|null>(null,{validators:Validators.maxLength(256)}));
  //hasProfileImageFilter = signal(new FormControl<boolean|null>(null));// because database doesn't have any record for user images

  rolesList = signal<string[]>([]);

  readonly adminService = inject(AdminService);
  readonly dialog = inject(MatDialog);

  paginator = viewChild.required(MatPaginator);
  sort = viewChild.required(MatSort);
  submitFilterButton = viewChild.required<ElementRef<HTMLButtonElement>>("submitFilterBtn");

  constructor(){
    this.adminService.requestRolesList().subscribe({
      next: res => {
        this.rolesList.set(res);
      },
    });
  }
  ngAfterViewInit(): void {
    // If the user changes the sort order, reset back to the first page.
    this.subscription().add(
      this.sort().sortChange.subscribe(() => (this.paginator().pageIndex = 0))
    );
    
    this.subscription().add(
      merge(
        this.sort().sortChange, this.paginator().page, 
        fromEvent(this.submitFilterButton().nativeElement, "click")
      ).pipe(
        startWith({}),
        switchMap(() => {
          this.displayLoadingSpinner.set(true);

          let filterModel = new UsersListFilterModel();
          if(this.usernameFilter().value?.trim()) {filterModel.username = this.usernameFilter().value!;}
          if(this.emailFilter().value?.trim()) {filterModel.email = this.emailFilter().value!;}
          if(this.createdFromFilter().value) {filterModel.createdFrom = this.createdFromFilter().value!;}
          if(this.createdToFilter().value) {filterModel.createdTo = this.createdToFilter().value!;}
          if(this.emailConfirmedFilter().value === true || this.emailConfirmedFilter().value === false) {
            filterModel.emailConfirmed = this.emailConfirmedFilter().value!;
          }
          if(this.displayEmailPubliclyFilter().value === true || this.displayEmailPubliclyFilter().value === false) {
            filterModel.displayEmailPublicly = this.displayEmailPubliclyFilter().value!;
          }
          /*if(this.rolesFilter().value?.trim()) {
            filterModel.roles = this.rolesFilter().value!.replaceAll(" ","").split(",");
          }*/

          return this.adminService.requestUsersListForAdmin(filterModel).pipe(
            catchError(()=> of(null))
          );
        })
      ).subscribe({
        next: res => {
          this.displayLoadingSpinner.set(false);
          if(res === null){
            this.dataSource.set([]);
          }
          else{
            this.dataSource.set(res.usersList);
            this.resultsLength.set(res.totalResultsLength);
          }
        },
      })
    );
  }
  ngOnDestroy(): void {
    this.subscription().unsubscribe();
  }

  deleteUser(userGuid:string){
    this.displayLoadingSpinner.set(true);

    this.adminService.requestDeleteUser(userGuid).subscribe({
      next: res => {
        this.displayLoadingSpinner.set(false);
        if(res.success){
          const dialogRef = this.dialog.open(Result,{
            //panelClass: "success-ResultStatus", 
            data:{
              status: "success",
              title: `Delete User '${res.username}'`,
              description: [`User identity with username '${res.username}' deleted successfully.`,
                "All of their data including their libraries and documents are also deleted.",
                "We also informed them through their email."
              ],
            }
          });
          dialogRef.afterClosed().subscribe(result=>{
            this.submitFilterButton().nativeElement.click();
          });
        }
      },
    });
  }
}
export class UsersListModel {
  ImageAddress:string|null = null;
  UserName:string = "";
  Email:string = "";
  EmailConfirmed:boolean  = false;
  UserGuid:string = "";
  //Description:string|null = null;
  DisplayEmailPublicly:boolean = false;
  CreatedAt: Date|null = null;
  Roles:string[] = [];
}