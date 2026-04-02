import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { MessageDto } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hub?: signalR.HubConnection;
  message$ = new Subject<{ conversationId: number; message: MessageDto }>();
  typing$ = new Subject<{ conversationId: number; userName: string }>();
  userOnline$ = new Subject<number>();
  userOffline$ = new Subject<number>();

  constructor(private auth: AuthService) {}

  connect() {
    const token = this.auth.getToken();
    if (!token) return;
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7002/hubs/chat', { accessTokenFactory: () => token })
      .withAutomaticReconnect().build();
    this.hub.start();
    this.hub.on('ReceiveMessage', (convoId: number, msg: MessageDto) => this.message$.next({ conversationId: convoId, message: msg }));
    this.hub.on('UserTyping', (convoId: number, userName: string) => this.typing$.next({ conversationId: convoId, userName }));
    this.hub.on('UserOnline', (userId: number) => this.userOnline$.next(userId));
    this.hub.on('UserOffline', (userId: number) => this.userOffline$.next(userId));
  }

  joinConversation(id: number) { this.hub?.invoke('JoinConversation', id); }
  leaveConversation(id: number) { this.hub?.invoke('LeaveConversation', id); }
  sendMessage(convoId: number, content: string) { this.hub?.invoke('SendMessage', convoId, content); }
  typing(convoId: number) { this.hub?.invoke('Typing', convoId); }
  disconnect() { this.hub?.stop(); }
}
