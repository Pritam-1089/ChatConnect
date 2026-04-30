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
  styleUrl: './chat.component.scss',
  host: { '(document:click)': 'onDocClick()' }
})
export class ChatComponent implements OnInit, OnDestroy {
  conversations: ConversationDto[] = [];
  selectedConvo: ConversationDto | null = null;
  messages: MessageDto[] = [];
  newMessage = '';
  searchQuery = '';
  searchResults: UserDto[] = [];
  typingUser: string | null = null;
  showEmojiPicker = false;
  previewImage: string | null = null;
  pendingImage: string | null = null;
  imageCaption = '';
  toastMessage: string | null = null;
  msgMenuId: number | null = null;
  emojis = [
    '😀','😂','🤣','😍','🥰','😘','😎','🤩','😊','🙂','😉','😋','🤔','🤗','🤭',
    '😐','😑','😶','🙄','😏','😣','😥','😮','🤐','😯','😪','😫','🥱','😴','😌',
    '👍','👎','👏','🙌','🤝','💪','🎉','🎊','❤️','🔥','⭐','💯','✅','❌','⚡',
    '🚀','💻','📱','🎯','📌','💡','🔑','📝','📂','🛒','💰','🎁','🏆','🌟','👋'
  ];
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

  addEmoji(emoji: string) {
    this.newMessage += emoji;
    this.showEmojiPicker = false;
  }

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file || !this.selectedConvo) return;
    const reader = new FileReader();
    reader.onload = () => {
      this.pendingImage = reader.result as string;
      this.imageCaption = '';
      this.cdr.markForCheck();
    };
    reader.readAsDataURL(file);
    (event.target as HTMLInputElement).value = '';
  }

  sendImage() {
    if (!this.pendingImage || !this.selectedConvo) return;
    if (this.imageCaption.trim()) {
      this.signalR.sendMessage(this.selectedConvo.id, this.imageCaption);
    }
    this.signalR.sendMessage(this.selectedConvo.id, this.pendingImage);
    this.pendingImage = null;
    this.imageCaption = '';
  }

  onDocClick() { this.msgMenuId = null; }

  onMsgRightClick(event: Event, msg: MessageDto) {
    event.preventDefault();
    this.msgMenuId = this.msgMenuId === msg.id ? null : msg.id;
    this.cdr.markForCheck();
  }

  copyMessage(msg: MessageDto) {
    navigator.clipboard.writeText(msg.content);
    this.msgMenuId = null;
    this.showToast('Message copied!');
  }

  unsendMessage(msg: MessageDto) {
    if (!this.selectedConvo) return;
    this.chatService.unsendMessage(this.selectedConvo.id, msg.id).subscribe(() => {
      const idx = this.messages.findIndex(m => m.id === msg.id);
      if (idx >= 0) {
        this.messages[idx] = { ...msg, content: 'This message was deleted' };
        this.messages = [...this.messages];
      }
      this.msgMenuId = null;
      this.showToast('Message unsent');
      this.cdr.markForCheck();
    });
  }

  showToast(msg: string) {
    this.toastMessage = msg;
    this.cdr.markForCheck();
    setTimeout(() => { this.toastMessage = null; this.cdr.markForCheck(); }, 2000);
  }

  cancelImage() {
    this.pendingImage = null;
    this.imageCaption = '';
  }

  downloadImage(dataUrl: string, msgId: number) {
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = `ChatConnect-image-${msgId || Date.now()}.png`;
    link.click();
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

  convoAvatar(convo: ConversationDto): string | undefined {
    if (convo.isGroup) return undefined;
    const me = this.auth.user()?.userId;
    return convo.members.find(m => m.id !== me)?.avatarUrl;
  }

  onAvatarSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    (event.target as HTMLInputElement).value = '';
    if (!file) return;
    if (!file.type.startsWith('image/')) { this.showToast('Please pick an image'); return; }
    if (file.size > 2 * 1024 * 1024) { this.showToast('Image must be under 2 MB'); return; }
    const reader = new FileReader();
    reader.onload = () => {
      const dataUrl = reader.result as string;
      this.chatService.updateAvatar(dataUrl).subscribe(res => {
        this.auth.updateAvatar(res.avatarUrl);
        const myId = this.auth.user()?.userId;
        this.conversations.forEach(c => c.members.forEach(m => { if (m.id === myId) m.avatarUrl = res.avatarUrl ?? undefined; }));
        this.showToast('Profile photo updated');
        this.cdr.markForCheck();
      });
    };
    reader.readAsDataURL(file);
  }
}
