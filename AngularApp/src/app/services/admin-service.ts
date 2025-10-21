import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { UsersListModel } from '../admin/identity-admin/users-list/users-list';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  readonly httpClient = inject(HttpClient);

  requestUsersListForAdmin(filterModel:UsersListFilterModel){
    //console.log("filterModel: ",JSON.stringify(filterModel));
    let httpParams = new HttpParams();
    if(filterModel.username){
      httpParams=httpParams.set("UserName", filterModel.username);
    }
    if(filterModel.email){
      httpParams=httpParams.set("Email", filterModel.email);
    }
    if(filterModel.emailConfirmed || filterModel.emailConfirmed === false){
      httpParams=httpParams.set("EmailConfirmed", filterModel.emailConfirmed);
    }
    if(filterModel.displayEmailPublicly || filterModel.displayEmailPublicly === false){
      httpParams=httpParams.set("DisplayEmailPublicly", filterModel.displayEmailPublicly);
    }
    if(filterModel.createdFrom){
      httpParams=httpParams.set("CreatedFrom", JSON.stringify(filterModel.createdFrom));
    }
    if(filterModel.createdTo){
      httpParams=httpParams.set("CreatedTo", JSON.stringify(filterModel.createdTo));
    }
    if(filterModel.page || filterModel.page === 0){
      httpParams=httpParams.set("Page", filterModel.page);
    }
    if(filterModel.pageSize){
      httpParams=httpParams.set("PageSize", filterModel.pageSize);
    }
    if(filterModel.sortDirection){
      httpParams=httpParams.set("SortDirection", filterModel.sortDirection);
    }
    if(filterModel.sortProperty){
      httpParams=httpParams.set("SortProperty", filterModel.sortProperty);
    }

    //console.log("httpParams: ",JSON.stringify(httpParams));

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
