import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { UsersListModel } from '../admin/identity-admin/users-list/users-list';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  readonly httpClient = inject(HttpClient);

  requestUsersListForAdmin(filterModel:UsersListFilterModel){
    let httpParams = new HttpParams();
    if(filterModel.username){
      httpParams.set("UserName", filterModel.username);
    }
    if(filterModel.email){
      httpParams.set("Email", filterModel.email);
    }
    if(filterModel.emailConfirmed){
      httpParams.set("EmailConfirmed", filterModel.emailConfirmed);
    }
    if(filterModel.displayEmailPublicly){
      httpParams.set("DisplayEmailPublicly", filterModel.displayEmailPublicly);
    }
    if(filterModel.createdFrom){
      httpParams.set("CreatedFrom", JSON.stringify(filterModel.createdFrom));
    }
    if(filterModel.createdTo){
      httpParams.set("CreatedTo", JSON.stringify(filterModel.createdTo));
    }

    return this.httpClient.get<UsersListResponseModel>(
      "/api/Identity/UsersList", {params:httpParams}
    );
  }

  requestRolesList(){
    return this.httpClient.get<string[]>("/api/Identity/RolesList");
  }

  requestDeleteUser(userGuid:string){
    let httpParams = new HttpParams().set("userGuid", userGuid);
    return this.httpClient.delete<{success:boolean, username:string}>(
      "/api/Identity/DeleteUser", {params: httpParams}
    );
  }

}

export class UsersListFilterModel{
  username?:string;
  email?:string;
  createdFrom?:Date;
  createdTo?:Date;
  emailConfirmed?:boolean;
  displayEmailPublicly?:boolean;
  page?: number;
  pageSize?: number;
  sortProperty?: string;
  sortDirection?: string;
}
export class UsersListResponseModel{
  usersList: UsersListModel[] = [];
  totalResultsLength: number = 0;
}
