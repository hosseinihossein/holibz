import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { map, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class IdentityService {
  private token_StorageKey = "jwt_token";
  private myGuid_StorageKey = "my_guid";
  private tokenExpiration_StorageKey = "token_expire";
  private httpClient = inject(HttpClient);

  signup(formValue:Partial<{username: string; email: string; password: string; CfTurnstileResponse: string;}>){
    return this.httpClient.post<{success:boolean}>(
      "/api/Identity/signup", 
      formValue
    );
  }

  login(formValue:Partial<{UsernameOrEmail: string; password: string; CfTurnstileResponse: string;}>){
    return this.httpClient.post<{token:string, expiresInHours:string, userGuid: string}>(
      "/api/Identity/login", 
      formValue
    ).pipe(
      tap({
        next: res => {
          localStorage.setItem(this.token_StorageKey, res.token);

          localStorage.setItem(this.myGuid_StorageKey, res.userGuid);

          let expireDate = new Date(Date.now());
          expireDate.setHours(expireDate.getHours() + Number(res.expiresInHours));
          localStorage.setItem(this.tokenExpiration_StorageKey, expireDate.toString());

          this.isAuthenticated.set(true);
        },
      }),
    );
  }

  logout(){
    localStorage.removeItem(this.token_StorageKey);
    localStorage.removeItem(this.myGuid_StorageKey);
    localStorage.removeItem(this.tokenExpiration_StorageKey);
    this.isAuthenticated.set(false);
    console.log("user logout!");
  }

  isAuthenticated = signal(this.hasRecord());

  private hasRecord():boolean{
    if(localStorage.getItem(this.token_StorageKey) && 
      localStorage.getItem(this.tokenExpiration_StorageKey)){
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

  getToken(){
    return localStorage.getItem(this.token_StorageKey);
  }

  getMyGuid(){
    return localStorage.getItem(this.myGuid_StorageKey);
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

  getUserImgAddress(userGuid:string | null = null){
    if(!userGuid){
      userGuid = this.getMyGuid();
    }
    return this.httpClient.get<{address:string;}>(
      `api/Identity/UserImageAddress?userGuid=${userGuid}`);
  }

}
