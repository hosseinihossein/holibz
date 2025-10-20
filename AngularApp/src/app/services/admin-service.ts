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
      "/api/Admin/UsersList", {params:httpParams}
    );
  }

  requestRolesList(){
    return this.httpClient.get<string[]>("/api/Admin/RolesList");
  }

  requestDeleteUser(userGuid:string){
    let httpParams = new HttpParams().set("userGuid", userGuid);
    return this.httpClient.delete<{success:boolean, username:string}>(
      "/api/Admin/DeleteUser", {params: httpParams}
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
  //roles?:string[]
}
export class UsersListResponseModel{
  usersList: UsersListModel[] = [];
  totalResultsLength: number = 0;
}
