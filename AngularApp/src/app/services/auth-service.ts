import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { map, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private tokenStorageKey = "jwt_token";
  private httpClient = inject(HttpClient);

  login(usernameOrEmail: string, password: string){
    return this.httpClient.post<{token:string}>("/api/Identity/login", {usernameOrEmail, password}).pipe(
      tap({
        next: res => localStorage.setItem(this.tokenStorageKey, res.token),
      }),
    );
  }

  logout(){
    localStorage.removeItem(this.tokenStorageKey);
  }

  isLogin():boolean{
    if(localStorage.getItem(this.tokenStorageKey)){
      return true;
    }
    return false;
  }

  getToken(){
    return localStorage.getItem(this.tokenStorageKey);
  }

}
