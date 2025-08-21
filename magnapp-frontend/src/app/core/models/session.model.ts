export interface Session {
  id: string;
  name: string;
  scrumMasterId: string;
  users: User[];
  votingState: VotingState;
  currentVotes: { [userId: string]: Vote };
  createdAt: Date;
  lastActivity: Date;
  isActive: boolean;
}

export type VotingState = 'idle' | 'voting' | 'revealing' | 'revealed';

export interface SessionSummary {
  id: string;
  name: string;
  userCount: number;
  isActive: boolean;
  scrumMasterName: string;
}

import { User } from './user.model';
import { Vote } from './vote.model';
