import React, { useState, useEffect, useRef } from 'react';
import { ChatMessage } from '../../types';
import { chatsService } from '../../api/services';
import { useAuth } from '../../context/AuthContext';
import { useNotifications } from '../../context/NotificationContext';
import { Send, MessageSquare, Loader2, User } from 'lucide-react';

interface ChatBoxProps {
  propertyId?: number;
  recipientId: string;
  recipientName?: string;
  propertyCode?: string;
}

export const ChatBox: React.FC<ChatBoxProps> = ({
  propertyId,
  recipientId,
  recipientName = 'Usuario',
  propertyCode,
}) => {
  const { user } = useAuth();
  const { chatHubConnection } = useNotifications();
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputText, setInputText] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSending, setIsSending] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const loadMessages = async () => {
    if (!user) return;
    try {
      const isClientRole = user.roles?.includes('Client');
      const clientId = isClientRole ? user.id : recipientId;
      const agentId = isClientRole ? recipientId : user.id;

      const data = await chatsService.getThread(clientId, agentId, propertyId);
      setMessages(data || []);
    } catch (err) {
      console.error("Error loading chat messages:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadMessages()).catch(console.error);
  }, [propertyId, recipientId]);

  useEffect(() => {
    if (chatHubConnection && propertyId) {
      chatHubConnection.invoke('JoinThread', propertyId, recipientId).catch(console.error);

      const handleReceive = (payload: any) => {
        if (payload.propertyId === propertyId) {
          const newMsg: ChatMessage = {
            id: Date.now(),
            propertyId: payload.propertyId,
            senderId: payload.senderId,
            recipientId: payload.recipientId,
            messageContent: payload.messageContent,
            sentAt: new Date().toISOString(),
            sentAtFormatted: payload.sentAtFormatted || 'Ahora',
          };
          setMessages((prev) => [...prev, newMsg]);
        }
      };

      chatHubConnection.on('ReceiveMessage', handleReceive);

      return () => {
        chatHubConnection.off('ReceiveMessage', handleReceive);
        chatHubConnection.invoke('LeaveThread', propertyId, recipientId).catch(console.error);
      };
    }
  }, [chatHubConnection, propertyId, recipientId]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputText.trim() || !user || isSending) return;

    const content = inputText.trim();
    setInputText('');
    setIsSending(true);

    try {
      if (chatHubConnection && chatHubConnection.state === 'Connected' && propertyId) {
        await chatHubConnection.invoke('SendMessage', propertyId, recipientId, content);
      } else {
        await chatsService.sendMessage({
          propertyId,
          recipientId,
          messageContent: content,
        });
        loadMessages();
      }
    } catch (err) {
      console.error("Error sending message:", err);
      // Fallback REST
      try {
        await chatsService.sendMessage({ propertyId, recipientId, messageContent: content });
        loadMessages();
      } catch (e2) {
        console.error("Fallback REST failed:", e2);
      }
    } finally {
      setIsSending(false);
    }
  };

  return (
    <div className="flex flex-col h-[500px] bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
      
      {/* Header */}
      <div className="p-3.5 bg-slate-900 text-white flex items-center justify-between">
        <div className="flex items-center gap-2.5">
          <div className="w-8 h-8 rounded-full bg-brand-500 text-white flex items-center justify-center font-bold text-xs">
            {recipientName ? recipientName[0] : 'U'}
          </div>
          <div>
            <p className="text-xs font-bold truncate">{recipientName}</p>
            {propertyCode && (
              <span className="text-[10px] text-emerald-400 font-mono">
                Propiedad #{propertyCode}
              </span>
            )}
          </div>
        </div>
      </div>

      {/* Message History */}
      <div className="flex-1 p-4 overflow-y-auto space-y-3 bg-slate-50/50 custom-scrollbar">
        {isLoading ? (
          <div className="flex items-center justify-center h-full text-slate-400 gap-2 text-xs">
            <Loader2 className="w-4 h-4 animate-spin text-brand-600" />
            <span>Cargando conversación...</span>
          </div>
        ) : messages.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full text-slate-400 text-center px-4 space-y-2">
            <MessageSquare className="w-8 h-8 text-slate-300" />
            <p className="text-xs font-semibold text-slate-500">Inicia la conversación</p>
            <p className="text-[11px] text-slate-400 max-w-xs">
              Envía tus preguntas sobre la propiedad, precio o condiciones de cierre.
            </p>
          </div>
        ) : (
          messages.map((m, i) => {
            const isMe = m.senderId === user?.id;
            return (
              <div
                key={m.id || i}
                className={`flex flex-col ${isMe ? 'items-end' : 'items-start'}`}
              >
                <div
                  className={`max-w-[80%] rounded-2xl px-4 py-2.5 text-xs shadow-2xs ${
                    isMe
                      ? 'bg-brand-600 text-white rounded-br-xs'
                      : 'bg-white text-slate-800 border border-slate-200/80 rounded-bl-xs'
                  }`}
                >
                  <p className="leading-relaxed whitespace-pre-wrap">{m.messageContent}</p>
                </div>
                <span className="text-[10px] text-slate-400 mt-1 px-1">
                  {m.sentAtFormatted || (m.sentAt ? new Date(m.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '')}
                </span>
              </div>
            );
          })
        )}
        <div ref={messagesEndRef} />
      </div>

      {/* Input Box */}
      <form onSubmit={handleSend} className="p-3 bg-white border-t border-slate-100 flex items-center gap-2">
        <input
          type="text"
          placeholder="Escribe un mensaje aquí..."
          value={inputText}
          onChange={(e) => setInputText(e.target.value)}
          className="flex-1 px-3.5 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
        />
        <button
          type="submit"
          disabled={!inputText.trim() || isSending}
          className="p-2 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 text-white rounded-xl transition-all shadow-sm"
        >
          <Send className="w-4 h-4" />
        </button>
      </form>
    </div>
  );
};
