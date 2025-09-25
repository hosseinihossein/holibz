import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private httpClient = inject(HttpClient);

  login(usernameOrEmail: string, password: string){
    return this.httpClient.post<{token:string}>("/api/Identity/login", {usernameOrEmail, password})
      .subscribe({
        next: res => localStorage.setItem("jwt_token", res.token),
        error: err => 
      })
  }

  private saveJwtToken(token:string){
    localStorage.setItem("jwt_token", token);
  }
}
