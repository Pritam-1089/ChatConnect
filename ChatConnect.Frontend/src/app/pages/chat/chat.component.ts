import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe, SlicePipe } from '@angular/common';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ChatService } from '../../services/chat.service';
import { SignalRService } from '../../services/signalr.service';
import { ConversationDto, MessageDto, UserDto } from '../../models/api.models';

@Component({
  selector: 'app-chat',
  imports: [FormsModule, DatePipe, SlicePipe],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss'
})
export class ChatComponent implements OnInit, OnDestroy {
  conversations: ConversationDto[] = [];
  selectedConvo: ConversationDto | null = null;
  messages: MessageDto[] = [];
  newMessage = '';
  searchQuery = '';
  searchResults: UserDto[] = [];
  typingUser: string | null = null;
  private subs: Subscription[] = [];
  private typingTimeout: any;

  constructor(
    public auth: AuthService,
    private chatService: ChatService,
    private signalR: SignalRService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadConversations();
    this.signalR.connect();

    this.subs.push(
      this.signalR.message$.subscribe(({ conversationId, message }) => {
        if (this.selectedConvo?.id === conversationId) {
          this.messages = [...this.messages, message];
        }
        const convo = this.conversations.find(c => c.id === conversationId);
        if (convo) {
          convo.lastMessage = message;
          if (this.selectedConvo?.id !== conversationId) convo.unreadCount++;
        }
        this.cdr.markForCheck();
      }),
      this.signalR.typing$.subscribe(({ conversationId, userName }) => {
        if (this.selectedConvo?.id === conversationId) {
          this.typingUser = userName;
          clearTimeout(this.typingTimeout);
          this.typingTimeout = setTimeout(() => { this.typingUser = null; this.cdr.markForCheck(); }, 2000);
          this.cdr.markForCheck();
        }
      })
    );
  }

  ngOnDestroy() {
    this.signalR.disconnect();
    this.subs.forEach(s => s.unsubscribe());
  }

  loadConversations() {
    this.chatService.getConversations().subscribe(c => {
      this.conversations = c;
      this.cdr.markForCheck();
    });
  }

  selectConversation(convo: ConversationDto) {
    if (this.selectedConvo) this.signalR.leaveConversation(this.selectedConvo.id);
    this.selectedConvo = convo;
    this.signalR.joinConversation(convo.id);
    this.chatService.getMessages(convo.id).subscribe(msgs => {
      this.messages = msgs.reverse();
      this.cdr.markForCheck();
    });
    if (convo.unreadCount) {
      this.chatService.markAsRead(convo.id).subscribe();
      convo.unreadCount = 0;
    }
  }

  sendMessage() {
    if (!this.newMessage.trim() || !this.selectedConvo) return;
    this.signalR.sendMessage(this.selectedConvo.id, this.newMessage);
    this.newMessage = '';
  }

  onTyping() {
    if (this.selectedConvo) this.signalR.typing(this.selectedConvo.id);
  }

  onSearch() {
    if (this.searchQuery.length < 2) { this.searchResults = []; return; }
    this.chatService.searchUsers(this.searchQuery).subscribe(u => {
      this.searchResults = u;
      this.cdr.markForCheck();
    });
  }

  startChat(userId: number) {
    this.chatService.startPrivateChat(userId).subscribe(convo => {
      this.searchQuery = '';
      this.searchResults = [];
      if (!this.conversations.find(c => c.id === convo.id)) this.conversations.unshift(convo);
      this.selectConversation(convo);
      this.cdr.markForCheck();
    });
  }
}
