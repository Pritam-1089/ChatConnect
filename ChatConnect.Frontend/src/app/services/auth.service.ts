import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { AuthResponse } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5276/api/auth';
  private currentUser = signal<AuthResponse | null>(this.getStored());
  user = this.currentUser.asReadonly();
  isLoggedIn = computed(() => !!this.currentUser());

  constructor(private http: HttpClient, private router: Router) {}

  register(fullName: string, email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(this.apiUrl + '/register', { fullName, email, password }).pipe(tap(r => this.setUser(r)));
  }
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(this.apiUrl + '/login', { email, password }).pipe(tap(r => this.setUser(r)));
  }
  logout() { localStorage.removeItem('chat_user'); this.currentUser.set(null); this.router.navigate(['/login']); }
  getToken(): string | null { return this.currentUser()?.token ?? null; }
  updateAvatar(avatarUrl: string | null) {
    const u = this.currentUser();
    if (!u) return;
    const updated: AuthResponse = { ...u, avatarUrl: avatarUrl ?? undefined };
    this.setUser(updated);
  }
  private setUser(u: AuthResponse) { localStorage.setItem('chat_user', JSON.stringify(u)); this.currentUser.set(u); }
  private getStored(): AuthResponse | null { const d = localStorage.getItem('chat_user'); return d ? JSON.parse(d) : null; }
}
