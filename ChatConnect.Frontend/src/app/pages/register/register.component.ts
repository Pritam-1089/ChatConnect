import { Component, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <div class="logo">ChatConnect</div>
        <h2>Create Account</h2>
        @if (error) { <div class="error">{{ error }}</div> }
        <form (ngSubmit)="onSubmit()">
          <input type="text" [(ngModel)]="fullName" name="fullName" placeholder="Full Name" required />
          <input type="email" [(ngModel)]="email" name="email" placeholder="Email" required />
          <input type="password" [(ngModel)]="password" name="password" placeholder="Password" required />
          <button type="submit" [disabled]="loading">{{ loading ? 'Creating...' : 'Create Account' }}</button>
        </form>
        <p class="link">Have an account? <a routerLink="/login">Sign In</a></p>
      </div>
    </div>
  `,
  styles: [`
    .auth-page { display: flex; justify-content: center; align-items: center; min-height: 100vh; background: #0f0f23; }
    .auth-card { background: #1a1a2e; padding: 40px; border-radius: 16px; width: 380px; color: #fff; }
    .logo { font-size: 24px; font-weight: 700; color: #7c3aed; margin-bottom: 24px; }
    h2 { margin: 0 0 20px; font-size: 20px; }
    .error { background: rgba(239,68,68,0.15); color: #ef4444; padding: 10px; border-radius: 8px; margin-bottom: 16px; font-size: 14px; }
    input { width: 100%; padding: 12px 16px; background: #16213e; border: 1px solid #2a2a4a; border-radius: 8px; color: #fff; font-size: 14px; margin-bottom: 12px; box-sizing: border-box; }
    input:focus { outline: none; border-color: #7c3aed; }
    button { width: 100%; padding: 12px; background: #7c3aed; color: #fff; border: none; border-radius: 8px; font-size: 15px; font-weight: 600; cursor: pointer; }
    button:hover { background: #6d28d9; }
    button:disabled { opacity: 0.7; }
    .link { text-align: center; margin-top: 16px; font-size: 14px; color: #64748b; }
    .link a { color: #7c3aed; text-decoration: none; }
  `]
})
export class RegisterComponent {
  fullName = ''; email = ''; password = ''; error = ''; loading = false;
  constructor(private auth: AuthService, private router: Router, private cdr: ChangeDetectorRef) {}
  onSubmit() {
    this.loading = true; this.error = '';
    this.auth.register(this.fullName, this.email, this.password).subscribe({
      next: () => this.router.navigate(['/chat']),
      error: (e) => { this.error = e.error?.message || 'Registration failed.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }
}
