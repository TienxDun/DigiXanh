import { Component } from '@angular/core';
import { FieldType, FieldTypeConfig, FormlyModule } from '@ngx-formly/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
    selector: 'app-formly-field-password',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormlyModule],
    template: `
    <div class="mb-3 position-relative">
      <label *ngIf="props.label" class="form-label" [for]="id">
        {{ props.label }} <span *ngIf="props.required" class="text-danger">*</span>
      </label>
      <div class="input-group">
        <input
          [type]="showPassword ? 'text' : 'password'"
          [formControl]="formControl"
          [formlyAttributes]="field"
          [attr.placeholder]="props.placeholder"
          [class.is-invalid]="showError"
          class="form-control"
          [id]="id"
        />
        <button
          class="btn btn-outline-secondary"
          type="button"
          (click)="togglePassword()"
          tabindex="-1"
        >
          <i class="fa-solid" [ngClass]="showPassword ? 'fa-eye' : 'fa-eye-slash'"></i>
        </button>
      </div>
      <div *ngIf="showError" class="invalid-feedback d-block">
        <formly-validation-message [field]="field"></formly-validation-message>
      </div>
    </div>
  `
})
export class FormlyFieldPasswordComponent extends FieldType<FieldTypeConfig> {
    showPassword = false;

    togglePassword() {
        this.showPassword = !this.showPassword;
    }
}
