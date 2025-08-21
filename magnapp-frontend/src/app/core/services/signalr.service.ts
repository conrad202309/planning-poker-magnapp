import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { SignalRHubEvents, SignalRHubMethods, ConnectionStatus } from '../models/signalr-events.model';
import { UserPreferences } from '../models/user.model';
import { VoteValue } from '../models/vote.model';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private connection: HubConnection | null = null;
  private connectionStatusSubject = new BehaviorSubject<ConnectionStatus>('disconnected');
  private eventSubjects: { [K in keyof SignalRHubEvents]: Subject<Parameters<SignalRHubEvents[K]>> } = {
    UserJoined: new Subject(),
    UserLeft: new Subject(),
    VotingStarted: new Subject(),
    VoteSubmitted: new Subject(),
    VotesRevealed: new Subject(),
    SessionEnded: new Subject(),
    ScrumMasterChanged: new Subject(),
    UserKicked: new Subject(),
    SessionUpdated: new Subject(),
    ConnectionStatusChanged: new Subject()
  };

  public connectionStatus$ = this.connectionStatusSubject.asObservable();

  constructor() {
    this.initializeConnection();
  }

  private initializeConnection(): void {
    this.connection = new HubConnectionBuilder()
      .withUrl('http://localhost:5000/hub/session')
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
          return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
        }
      })
      .configureLogging(LogLevel.Information)
      .build();

    this.setupEventHandlers();
    this.setupConnectionHandlers();
  }

  private setupEventHandlers(): void {
    if (!this.connection) return;

    Object.keys(this.eventSubjects).forEach(eventName => {
      this.connection!.on(eventName, (...args: any[]) => {
        this.eventSubjects[eventName as keyof SignalRHubEvents].next(args as any);
      });
    });
  }

  private setupConnectionHandlers(): void {
    if (!this.connection) return;

    this.connection.onclose(() => {
      this.connectionStatusSubject.next('disconnected');
    });

    this.connection.onreconnecting(() => {
      this.connectionStatusSubject.next('reconnecting');
    });

    this.connection.onreconnected(() => {
      this.connectionStatusSubject.next('connected');
    });
  }

  public async startConnection(): Promise<void> {
    if (!this.connection) return;

    try {
      this.connectionStatusSubject.next('connecting');
      await this.connection.start();
      this.connectionStatusSubject.next('connected');
    } catch (error) {
      this.connectionStatusSubject.next('disconnected');
      console.error('SignalR connection failed:', error);
      throw error;
    }
  }

  public async stopConnection(): Promise<void> {
    if (!this.connection) return;

    try {
      await this.connection.stop();
      this.connectionStatusSubject.next('disconnected');
    } catch (error) {
      console.error('Error stopping SignalR connection:', error);
    }
  }

  public on<K extends keyof SignalRHubEvents>(
    eventName: K
  ): Observable<Parameters<SignalRHubEvents[K]>> {
    return this.eventSubjects[eventName].asObservable();
  }

  public async invoke<K extends keyof SignalRHubMethods>(
    methodName: K,
    ...args: Parameters<SignalRHubMethods[K]>
  ): Promise<ReturnType<SignalRHubMethods[K]>> {
    if (!this.connection || this.connection.state !== 'Connected') {
      throw new Error('SignalR connection is not established');
    }

    return this.connection.invoke(methodName, ...args);
  }

  public async joinSession(sessionId: string, user: UserPreferences): Promise<boolean> {
    return this.invoke('JoinSession', sessionId, user);
  }

  public async leaveSession(sessionId: string): Promise<void> {
    return this.invoke('LeaveSession', sessionId);
  }

  public async submitVote(sessionId: string, vote: VoteValue): Promise<boolean> {
    return this.invoke('SubmitVote', sessionId, vote);
  }

  public async startVoting(sessionId: string): Promise<boolean> {
    return this.invoke('StartVoting', sessionId);
  }

  public async revealVotes(sessionId: string): Promise<boolean> {
    return this.invoke('RevealVotes', sessionId);
  }

  public async transferScrumMaster(sessionId: string, newScrumMasterId: string): Promise<boolean> {
    return this.invoke('TransferScrumMaster', sessionId, newScrumMasterId);
  }

  public async kickUser(sessionId: string, userId: string): Promise<boolean> {
    return this.invoke('KickUser', sessionId, userId);
  }

  public async endSession(sessionId: string): Promise<boolean> {
    return this.invoke('EndSession', sessionId);
  }

  public isConnected(): boolean {
    return this.connection?.state === 'Connected' ?? false;
  }
}
