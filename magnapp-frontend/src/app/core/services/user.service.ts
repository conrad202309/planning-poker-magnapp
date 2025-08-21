import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { User, UserPreferences } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly STORAGE_KEY_PREFERENCES = 'magnapp_user_preferences';
  private readonly STORAGE_KEY_USER_ID = 'magnapp_user_id';
  
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private userPreferencesSubject = new BehaviorSubject<UserPreferences | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public userPreferences$ = this.userPreferencesSubject.asObservable();

  constructor() {
    this.loadUserPreferences();
  }

  private loadUserPreferences(): void {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY_PREFERENCES);
      if (stored) {
        const preferences: UserPreferences = JSON.parse(stored);
        this.userPreferencesSubject.next(preferences);
      }
    } catch (error) {
      console.error('Error loading user preferences:', error);
    }
  }

  public saveUserPreferences(preferences: UserPreferences): void {
    try {
      localStorage.setItem(this.STORAGE_KEY_PREFERENCES, JSON.stringify(preferences));
      this.userPreferencesSubject.next(preferences);
    } catch (error) {
      console.error('Error saving user preferences:', error);
    }
  }

  public getUserPreferences(): UserPreferences | null {
    return this.userPreferencesSubject.value;
  }

  public hasUserPreferences(): boolean {
    const preferences = this.getUserPreferences();
    return preferences !== null && !!preferences.name && !!preferences.avatar;
  }

  public clearUserPreferences(): void {
    try {
      localStorage.removeItem(this.STORAGE_KEY_PREFERENCES);
      this.userPreferencesSubject.next(null);
    } catch (error) {
      console.error('Error clearing user preferences:', error);
    }
  }

  public generateUserId(): string {
    let userId = localStorage.getItem(this.STORAGE_KEY_USER_ID);
    if (!userId) {
      userId = this.createUniqueId();
      localStorage.setItem(this.STORAGE_KEY_USER_ID, userId);
    }
    return userId;
  }

  public getCurrentUserId(): string | null {
    return localStorage.getItem(this.STORAGE_KEY_USER_ID);
  }

  public setCurrentUser(user: User): void {
    this.currentUserSubject.next(user);
  }

  public getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  public clearCurrentUser(): void {
    this.currentUserSubject.next(null);
  }

  public createUserFromPreferences(preferences: UserPreferences): User {
    const userId = this.generateUserId();
    return {
      id: userId,
      name: preferences.name,
      avatar: preferences.avatar,
      isOnline: true,
      isScrumMaster: false,
      hasVoted: false,
      joinedAt: new Date()
    };
  }

  public updateUserPreferences(updates: Partial<UserPreferences>): void {
    const current = this.getUserPreferences();
    if (current) {
      const updated = { ...current, ...updates };
      this.saveUserPreferences(updated);
    }
  }

  private createUniqueId(): string {
    return 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
  }

  public isUserSetupComplete(): boolean {
    const preferences = this.getUserPreferences();
    return preferences !== null && 
           preferences.name.trim().length >= 2 && 
           preferences.name.trim().length <= 20 &&
           preferences.avatar.length > 0;
  }
}
