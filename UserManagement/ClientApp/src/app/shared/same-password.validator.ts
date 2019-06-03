import { FormGroup, ValidatorFn, ValidationErrors } from '@angular/forms';

export const samePasswordValidator: ValidatorFn = (group: FormGroup): ValidationErrors | null =>
{
  const password = group.get('password');
  const confirmPassword = group.get('confirmPassword');

  return password && confirmPassword && password.value === confirmPassword.value ? null : { samePassword: false };
}
