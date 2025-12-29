import { HttpClient, HttpHeaders } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { map, tap } from 'rxjs';
//import { toSignal } from '@angular/core/rxjs-interop';

@Injectable({
  providedIn: 'root'
})
export class IdentityService {
  private token_StorageKey = "jwt_token";
  //private myGuid_StorageKey = "my_guid";
  private user_StorageKey = "user_model";
  private tokenExpiration_StorageKey = "token_expire";
  private httpClient = inject(HttpClient);

  isAuthenticated = signal(false);
  userModel = signal<UserProfileModel | null>(null);
  token = signal<string | null>(null);

  constructor(){
    this.isAuthenticated.set(this.hasRecord());
    this.userModel.set(this.getUserModelFromLocalStorage());
    this.token.set(this.getTokenFromLocalStorage());
  }

  signup(formValue:Partial<{username: string; email: string; password: string; CfTurnstileResponse: string;}>){
    return this.httpClient.post<{success:boolean}>(
      "/api/Identity/Signup", 
      formValue
    );
  }

  login(formValue:Partial<{UsernameOrEmail: string; password: string; CfTurnstileResponse: string;}>){
    return this.httpClient.post<{token:string, expiresInHours:string, user: UserProfileModel}>(
      "/api/Identity/login", 
      formValue
    ).pipe(
      tap({
        next: res => {
          this.token.set(res.token);
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
    this.userModel.set(null);
    this.token.set(null);
    console.log("user logout!");
  }
  
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
  
  private getTokenFromLocalStorage(): string | null{
    if(this.isAuthenticated()){
      return localStorage.getItem(this.token_StorageKey);
    }
    return null;
  }
  private getUserModelFromLocalStorage(): UserProfileModel | null{
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
  changeEmail(email: string, CfTurnstileResponse: string){
    return this.httpClient.post<{success:boolean}>(
      "/api/Identity/ChangeEmail", {email, CfTurnstileResponse}
    );
  }
  forgetPassword(email: string, CfTurnstileResponse: string){
    return this.httpClient.post<{success:boolean}>(
      "/api/Identity/ForgetPassword", {email, CfTurnstileResponse}
    );
  }

  requestUserModel(userGuid:string){
    return this.httpClient.get<UserProfileModel>(`/api/Identity/GetUserModel?userGuid=${userGuid}`);
  }

  getCsrf(){
    return this.httpClient.get("/api/Identity/GetCsrf");
  }

  submitUserImage(file: File){
    const formData = new FormData();
    formData.append('userImageFile', file);
    return this.httpClient.post<{success: boolean, hasImage: boolean, integrityVersion:number}>(
      "/api/Identity/SubmitUserImage", formData
    );
  }
  deleteUserImage(){
    return this.httpClient.delete<{success:boolean}>(
      "/api/Identity/DeleteUserImage"
    );
  }
  submitUserName(username:string){
    return this.httpClient.post<{success:boolean, token:string}>(
      `/api/Identity/SubmitUsername`, {username}
    ).pipe(tap({
      next: res => {
        this.token.set(res.token);
        localStorage.setItem(this.token_StorageKey, res.token);
      },
    }));
  }
  submitDescription(description:string){
    return this.httpClient.post<{success:boolean}>(
      `/api/Identity/SubmitDescription`, {description}
    );
  }
  submitDisplayEmailPublicly(displayPublicly:boolean){
    return this.httpClient.post<{success:boolean}>(
      "/api/Identity/SubmitDisplayEmailPublicly", {displayPublicly}
    );
  }

  updateUserModel(newUserModel:UserProfileModel){
    if(this.isAuthenticated()){
      this.userModel.set(newUserModel);
      localStorage.setItem(this.user_StorageKey, JSON.stringify(newUserModel));
    }
  }

  submitChangePassword(
  changePasswordForm: Partial<{CurrentPassword:string, NewPassword:string, RepeatNewPassword: string}>){
    return this.httpClient.post<{success:boolean, token:string}>(
      `/api/Identity/ChangePassword`, changePasswordForm
    ).pipe(tap({
      next: res => {
        this.token.set(res.token);
        localStorage.setItem(this.token_StorageKey, res.token);
      },
    }));
  }


}

export class UserProfileModel
{
  constructor(userModel:Partial<UserProfileModel>|null=null){
    this.guid = userModel?.guid ?? "";
    this.username = userModel?.username;
    this.description = userModel?.description;
    this.hasImage = userModel?.hasImage;
    this.integrityVersion = userModel?.integrityVersion;
    this.email = userModel?.email;
    this.displayEmailPublicly = userModel?.displayEmailPublicly;
    this.roles = userModel?.roles?.map(r=>r);
  }
  guid: string = null!;
  username?: string;
  description?: string;
  hasImage?:boolean;
  integrityVersion?:number;
  email?: string;
  displayEmailPublicly?: boolean;
  roles?: string[];
}