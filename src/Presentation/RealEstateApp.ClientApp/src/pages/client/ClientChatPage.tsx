import React, { useState, useEffect } from 'react';
import { chatsService } from '../../api/services';
import { ChatMessage } from '../../types';
import { ChatBox } from '../../components/chat/ChatBox';
import { Loader } from '../../components/common/Loader';
import { useAuth } from '../../context/AuthContext';
import { mapConversations } from '../../utils/chat';
import { MessageSquare, User, Building } from 'lucide-react';

export const ClientChatPage: React.FC = () => {
  const { user } = useAuth();
  const [conversations, setConversations] = useState<ChatMessage[]>([]);
  const [selectedChat, setSelectedChat] = useState<ChatMessage | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchChats = async () => {
      try {
        setIsLoading(true);
        const data = await chatsService.getConversations();
        const mapped = mapConversations(data, user);
        setConversations(mapped);
        if (mapped.length > 0) {
          setSelectedChat(mapped[0]);
        }
      } catch (err) {
        console.error("Error loading client chats:", err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchChats();
  }, [user]);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <MessageSquare className="w-6 h-6 text-brand-600" />
            Centro de Mensajería con Agentes
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Conversaciones en tiempo real asociadas a tus consultas por inmueble.
          </p>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Cargando tus mensajes..." />
      ) : conversations.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-4">
          <MessageSquare className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">No tienes conversaciones activas</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Haz clic en &quot;Chatear con el Agente&quot; en cualquier inmueble del catálogo para iniciar un hilo de conversación.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          
          {/* Conversations List */}
          <div className="lg:col-span-1 bg-white rounded-3xl p-4 border border-slate-200 shadow-xs space-y-2 max-h-[600px] overflow-y-auto custom-scrollbar">
            <span className="text-xs font-bold text-slate-400 uppercase tracking-wider px-2">
              Hilos de Conversación
            </span>
            {conversations.map((c, i) => (
              <button
                key={c.id || i}
                onClick={() => setSelectedChat(c)}
                className={`w-full p-3.5 rounded-2xl text-left transition-all border flex items-center gap-3 ${
                  selectedChat?.propertyId === c.propertyId && selectedChat?.recipientId === c.recipientId
                    ? 'bg-brand-50 border-brand-200 text-brand-900 shadow-2xs'
                    : 'bg-slate-50/50 border-transparent hover:bg-slate-100 text-slate-700'
                }`}
              >
                <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-navy-800 to-slate-700 text-white flex items-center justify-center font-bold text-xs shrink-0">
                  {c.recipientName ? c.recipientName[0] : 'A'}
                </div>
                <div className="overflow-hidden flex-1">
                  <p className="font-bold text-xs truncate text-slate-900">{c.recipientName || 'Agente'}</p>
                  <p className="text-[11px] text-slate-500 truncate">{c.messageContent}</p>
                  {c.propertyCode && (
                    <span className="text-[10px] text-brand-600 font-mono">#{c.propertyCode}</span>
                  )}
                </div>
              </button>
            ))}
          </div>

          {/* Active Chat Thread */}
          <div className="lg:col-span-2">
            {selectedChat ? (
              <ChatBox
                propertyId={selectedChat.propertyId}
                recipientId={selectedChat.recipientId}
                recipientName={selectedChat.recipientName}
                propertyCode={selectedChat.propertyCode}
              />
            ) : (
              <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 text-slate-400">
                Selecciona una conversación para ver los mensajes.
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
