export interface SignalRHubEvents {
  UserJoined: (user: User) => void;
  UserLeft: (userId: string) => void;
  VotingStarted: () => void;
  VoteSubmitted: (userId: string, hasVoted: boolean) => void;
  VotesRevealed: (votes: { [userId: string]: Vote }, statistics: VoteStatistics) => void;
  SessionEnded: () => void;
  ScrumMasterChanged: (newScrumMasterId: string) => void;
  UserKicked: (userId: string) => void;
  SessionUpdated: (session: Session) => void;
  ConnectionStatusChanged: (status: ConnectionStatus) => void;
}

export interface SignalRHubMethods {
  JoinSession: (sessionId: string, user: UserPreferences) => Promise<boolean>;
  LeaveSession: (sessionId: string) => Promise<void>;
  SubmitVote: (sessionId: string, vote: VoteValue) => Promise<boolean>;
  StartVoting: (sessionId: string) => Promise<boolean>;
  RevealVotes: (sessionId: string) => Promise<boolean>;
  TransferScrumMaster: (sessionId: string, newScrumMasterId: string) => Promise<boolean>;
  KickUser: (sessionId: string, userId: string) => Promise<boolean>;
  EndSession: (sessionId: string) => Promise<boolean>;
}

export type ConnectionStatus = 'connecting' | 'connected' | 'disconnected' | 'reconnecting';

import { User, UserPreferences } from './user.model';
import { Vote, VoteValue, VoteStatistics } from './vote.model';
import { Session } from './session.model';
