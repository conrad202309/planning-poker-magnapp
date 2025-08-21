export interface User {
  id: string;
  name: string;
  avatar: string;
  isOnline: boolean;
  isScrumMaster: boolean;
  hasVoted: boolean;
  joinedAt: Date;
}

export interface UserPreferences {
  name: string;
  avatar: string;
  audioEnabled: boolean;
}
