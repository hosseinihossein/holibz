import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export function compareTwoInputs(inputOneName:string, inputTwoName:string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        return control.get(inputOneName)?.value !== control.get(inputTwoName)?.value ? 
        {notEqual: {message: `${inputOneName} and ${inputTwoName} are Not equal!`}} : 
        null;
    };
}