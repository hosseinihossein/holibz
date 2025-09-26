import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { map, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private token_StorageKey = "jwt_token";
  private tokenExpiration_StorageKey = "token_expire";
  private httpClient = inject(HttpClient);

  signup(username: string, email:string, password: string){
    return this.httpClient.post(
      "/api/Identity/signup", 
      {username, email, password}
    );
  }

  login(usernameOrEmail: string, password: string){
    return this.httpClient.post<{token:string, expiresInHours:string}>(
      "/api/Identity/login", 
      {usernameOrEmail, password}
    ).pipe(
      tap({
        next: res => {
          localStorage.setItem(this.token_StorageKey, res.token);
          let expireDate = new Date(Date.now());
          expireDate.setHours(expireDate.getHours() + Number(res.expiresInHours));
          localStorage.setItem(this.tokenExpiration_StorageKey, expireDate.toString());
        },
      }),
    );
  }

  logout(){
    localStorage.removeItem(this.token_StorageKey);
    localStorage.removeItem(this.tokenExpiration_StorageKey);
  }

  isAuthenticated():boolean{
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

}
