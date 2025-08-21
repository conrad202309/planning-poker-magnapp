import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { CommonModule } from '@angular/common';
import { UserService } from '../../core/services/user.service';
import { UserPreferences } from '../../core/models/user.model';

@Component({
  selector: 'app-user-setup',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule
  ],
  templateUrl: './user-setup.component.html',
  styleUrl: './user-setup.component.scss'
})
export class UserSetupComponent implements OnInit {
  userSetupForm: FormGroup;
  availableAvatars: string[] = [
    '👨‍💼', '👩‍💼', '👨‍💻', '👩‍💻', '🧑‍💻',
    '👨‍🎨', '👩‍🎨', '👨‍🔬', '👩‍🔬', '🧑‍🎓',
    '👨‍⚕️', '👩‍⚕️', '👨‍🏫', '👩‍🏫', '🧑‍🍳'
  ];

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private router: Router
  ) {
    this.userSetupForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(20)]],
      avatar: ['👨‍💼', Validators.required],
      audioEnabled: [true]
    });
  }

  ngOnInit(): void {
    const existingPreferences = this.userService.getUserPreferences();
    if (existingPreferences) {
      this.userSetupForm.patchValue(existingPreferences);
    }
  }

  onSubmit(): void {
    if (this.userSetupForm.valid) {
      const preferences: UserPreferences = this.userSetupForm.value;
      this.userService.saveUserPreferences(preferences);
      this.router.navigate(['/sessions']);
    } else {
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.userSetupForm.controls).forEach(key => {
      const control = this.userSetupForm.get(key);
      control?.markAsTouched();
    });
  }

  getErrorMessage(fieldName: string): string {
    const control = this.userSetupForm.get(fieldName);
    if (control?.hasError('required')) {
      return `${fieldName.charAt(0).toUpperCase() + fieldName.slice(1)} is required`;
    }
    if (control?.hasError('minlength')) {
      return `Name must be at least 2 characters long`;
    }
    if (control?.hasError('maxlength')) {
      return `Name cannot exceed 20 characters`;
    }
    return '';
  }
}
