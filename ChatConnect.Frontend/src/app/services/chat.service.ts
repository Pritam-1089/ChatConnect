import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConversationDto, MessageDto, UserDto } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private api = 'http://10.93.149.246:5276/api';
  constructor(private http: HttpClient) {}

  getConversations(): Observable<ConversationDto[]> { return this.http.get<ConversationDto[]>(this.api + '/conversations'); }
  getMessages(convoId: number, skip = 0): Observable<MessageDto[]> { return this.http.get<MessageDto[]>(this.api + '/conversations/' + convoId + '/messages?skip=' + skip); }
  sendMessage(convoId: number, content: string): Observable<MessageDto> { return this.http.post<MessageDto>(this.api + '/conversations/' + convoId + '/messages', { content }); }
  startPrivateChat(userId: number): Observable<ConversationDto> { return this.http.post<ConversationDto>(this.api + '/conversations/private/' + userId, {}); }
  createGroup(name: string, memberIds: number[]): Observable<ConversationDto> { return this.http.post<ConversationDto>(this.api + '/conversations/group', { name, memberIds }); }
  markAsRead(convoId: number): Observable<void> { return this.http.post<void>(this.api + '/conversations/' + convoId + '/read', {}); }
  searchUsers(q: string): Observable<UserDto[]> { return this.http.get<UserDto[]>(this.api + '/users/search?q=' + q); }
  getOnlineUsers(): Observable<UserDto[]> { return this.http.get<UserDto[]>(this.api + '/users/online'); }
  unsendMessage(convoId: number, msgId: number): Observable<void> { return this.http.delete<void>(this.api + '/conversations/' + convoId + '/messages/' + msgId); }
}
