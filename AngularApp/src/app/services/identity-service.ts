import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { map, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class IdentityService {
  private token_StorageKey = "jwt_token";
  //private myGuid_StorageKey = "my_guid";
  private user_StorageKey = "user_model";
  private tokenExpiration_StorageKey = "token_expire";
  private httpClient = inject(HttpClient);

  signup(formValue:Partial<{username: string; email: string; password: string; CfTurnstileResponse: string;}>){
    return this.httpClient.post<{success:boolean}>(
      "/api/Identity/signup", 
      formValue
    );
  }

  login(formValue:Partial<{UsernameOrEmail: string; password: string; CfTurnstileResponse: string;}>){
    return this.httpClient.post<{token:string, expiresInHours:string, user: UserModel}>(
      "/api/Identity/login", 
      formValue
    ).pipe(
      tap({
        next: res => {
          localStorage.setItem(this.token_StorageKey, res.token);

          let expireDate = new Date(Date.now());
          expireDate.setHours(expireDate.getHours() + Number(res.expiresInHours));
          localStorage.setItem(this.tokenExpiration_StorageKey, expireDate.toString());

          this.userModel.set(res.user);
          localStorage.setItem(this.user_StorageKey, JSON.stringify(this.userModel()));

          this.isAuthenticated.set(true);
        },
      }),
    );
  }

  
  logout(){
    localStorage.removeItem(this.token_StorageKey);
    localStorage.removeItem(this.user_StorageKey);
    localStorage.removeItem(this.tokenExpiration_StorageKey);
    this.isAuthenticated.set(false);
    console.log("user logout!");
  }
  
  isAuthenticated = signal(this.hasRecord());
  userModel = signal<UserModel | null>(this.getUserModel());
  token = signal<string | null>(this.getToken());
  
  private hasRecord():boolean{
    if(localStorage.getItem(this.token_StorageKey) && 
    localStorage.getItem(this.tokenExpiration_StorageKey) &&
    localStorage.getItem(this.user_StorageKey)){
      let expireDate = Date.parse(localStorage.getItem(this.tokenExpiration_StorageKey)!);
      if(Date.now() < expireDate){
        return true;
      }
      else{
        this.logout();
      }
    }
    return false;
  }
  
  private getToken(): string | null{
    if(this.isAuthenticated()){
      return localStorage.getItem(this.token_StorageKey);
    }
    return null;
  }
  private getUserModel(): UserModel | null{
    if(this.isAuthenticated()){
      return JSON.parse(localStorage.getItem(this.user_StorageKey)!);
    }
    return null;
  }

  resendEmailValidation(email: string, CfTurnstileResponse: string){
    return this.httpClient.post<{success:boolean}>(
      "/api/Identity/ResendEmailValidation", {email, CfTurnstileResponse}
    );
  }
  cahngeEmail(email: string, CfTurnstileResponse: string){
    return this.httpClient.post<{success:boolean}>(
      "/api/Identity/ChangeEmail", {email, CfTurnstileResponse}
    );
  }
  forgetPassword(email: string, CfTurnstileResponse: string){
    return this.httpClient.post<{success:boolean}>(
      "/api/Identity/ForgetPassword", {email, CfTurnstileResponse}
    );
  }

  getUserImageAddress(userGuid:string){
    return this.httpClient.get<string>(`/api/Identity/GetUserImageAddress?userGuid=${userGuid}`);
  }
  getUserName(userGuid:string){
    return this.httpClient.get<string>(`/api/Identity/GetUserName?userGuid=${userGuid}`);
  }
  getUserDescription(userGuid:string){
    return this.httpClient.get<string>(`/api/Identity/GetUserDescription?userGuid=${userGuid}`);
  }
  getUserEmail(userGuid:string){
    return this.httpClient.get<string>(`/api/Identity/GetUserEmail?userGuid=${userGuid}`);
  }

  submitUserImage(){}

  updateUserModel(){
    this.httpClient.get<UserModel>("/api/Identity/GetUserModel").subscribe({
      next: res=>{
        this.userModel.set(res);
        localStorage.setItem(this.user_StorageKey, JSON.stringify(this.userModel()));
      }
    })
  }

}

export interface UserModel
{
  guid: string;
  username: string;
  description: string;
  imageAddress: string;
  email: string;
}