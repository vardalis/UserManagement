import { samePasswordValidator } from "./same-password.validator";
import { FormGroup, FormBuilder } from "@angular/forms";

describe('SamePasswordValidator', () => {


  it('should create an instance', () => {
    let fb = new FormBuilder();
    let group =fb.group({
      password: ['aPassword'],
      confirmPassword: ['aPassword'],
    });
    const validator = samePasswordValidator(group);
    expect(validator).toBeNull();
  });
});
