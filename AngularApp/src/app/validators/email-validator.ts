import { HttpClient } from "@angular/common/http";
import { inject } from "@angular/core";
import { AbstractControl, AsyncValidatorFn, ValidationErrors } from "@angular/forms";
import { catchError, debounceTime, Observable, of, switchMap } from "rxjs";

export function validateEmail(): AsyncValidatorFn{
    let httpClient = inject(HttpClient);
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
        if(control.value){
            return httpClient.get<{isTaken:boolean}>(
                "/api/Identity/CheckEmail",
                { params:{email: `${control.value}`},}
            ).pipe(
                debounceTime(500),
                switchMap(res => res.isTaken ? of({emailTaken: true}) : of(null)),
                catchError(()=>of(null))
            );
        }
        return of(null);
    }
}