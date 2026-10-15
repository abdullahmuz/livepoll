import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { API_URL } from './api-config';
import { ApiResponse, AuthResponse, LoginRequest, RegisterRequest } from '../models/api.models';

const STORAGE_KEY = 'livepoll_auth';

// reads the "exp" (expiry) claim out of the JWT payload
function isExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
    return payload.exp * 1000 < Date.now();
  } catch {
    return true;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _user = signal<AuthResponse | null>(this.load());

  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._user() !== null);

  get token(): string | null {
    return this._user()?.token ?? null;
  }

  register(dto: RegisterRequest) {
    return this.authenticate('register', dto);
  }

  login(dto: LoginRequest) {
    return this.authenticate('login', dto);
  }

  logout() {
    localStorage.removeItem(STORAGE_KEY);
    this._user.set(null);
    this.router.navigateByUrl('/login');
  }

  private async authenticate(path: 'login' | 'register', body: LoginRequest | RegisterRequest) {
    const res = await firstValueFrom(
      this.http.post<ApiResponse<AuthResponse>>(`${API_URL}/api/auth/${path}`, body));

    if (!res.data) throw new Error(res.message);

    localStorage.setItem(STORAGE_KEY, JSON.stringify(res.data));
    this._user.set(res.data);
  }

  private load(): AuthResponse | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const user = JSON.parse(raw) as AuthResponse;
      return isExpired(user.token) ? null : user;
    } catch {
      return null;
    }
  }
}