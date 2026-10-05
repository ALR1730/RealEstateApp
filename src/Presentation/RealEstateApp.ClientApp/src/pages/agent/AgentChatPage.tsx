import React, { useState, useEffect } from 'react';
import { chatsService } from '../../api/services';
import { ChatMessage } from '../../types';
import { ChatBox } from '../../components/chat/ChatBox';
import { Loader } from '../../components/common/Loader';
import { useAuth } from '../../context/AuthContext';
import { useLanguage } from '../../context/LanguageContext';
import { mapConversations } from '../../utils/chat';
import { MessageSquare } from 'lucide-react';

export const AgentChatPage: React.FC = () => {
  const { t } = useLanguage();
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
        console.error("Error loading agent chats:", err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchChats();
  }, [user]);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-4 border-b border-slate-200 dark:border-slate-800">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight flex items-center gap-2">
            <MessageSquare className="w-6 h-6 text-brand-600 dark:text-brand-400" />
            {t('agent.chat.title', "Mensajes Directos con Clientes Compradores")}
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            {t('agent.chat.subtitle', "Atención en tiempo real y asesoramiento de clientes por inmueble.")}
          </p>
        </div>
      </div>

      {isLoading ? (
        <Loader text={t('agent.chat.loading', "Cargando tus mensajes...")} />
      ) : conversations.length === 0 ? (
        <div className="bg-white dark:bg-slate-900 rounded-3xl p-12 text-center border border-slate-200 dark:border-slate-800 space-y-4">
          <MessageSquare className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto" />
          <h3 className="text-base font-bold text-slate-800 dark:text-white">{t('agent.chat.emptyTitle', "No hay mensajes recientes")}</h3>
          <p className="text-xs text-slate-500 dark:text-slate-400 max-w-sm mx-auto">
            {t('agent.chat.emptySubtitle', "Cuando los clientes te envíen preguntas sobre tus inmuebles, aparecerán aquí de inmediato.")}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          
          <div className="lg:col-span-1 bg-white dark:bg-slate-900 rounded-3xl p-4 border border-slate-200 dark:border-slate-800 shadow-xs space-y-2 max-h-[600px] overflow-y-auto custom-scrollbar">
            <span className="text-xs font-bold text-slate-400 uppercase tracking-wider px-2">
              {t('agent.chat.interestedClients', "Clientes Interesados")}
            </span>
            {conversations.map((c, i) => (
              <button
                key={c.id || i}
                onClick={() => setSelectedChat(c)}
                className={`w-full p-3.5 rounded-2xl text-left transition-all border flex items-center gap-3 ${
                  selectedChat?.propertyId === c.propertyId && selectedChat?.recipientId === c.recipientId
                    ? 'bg-brand-50 dark:bg-brand-950/40 border-brand-200 dark:border-brand-800/60 text-brand-900 dark:text-brand-300 shadow-2xs'
                    : 'bg-slate-50/50 dark:bg-slate-800/50 border-transparent hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300'
                }`}
              >
                <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-brand-600 to-emerald-500 text-white flex items-center justify-center font-bold text-xs shrink-0">
                  {c.recipientName ? c.recipientName[0] : 'C'}
                </div>
                <div className="overflow-hidden flex-1">
                  <p className="font-bold text-xs truncate text-slate-900 dark:text-white">{c.recipientName || 'Cliente'}</p>
                  <p className="text-[11px] text-slate-500 dark:text-slate-400 truncate">{c.messageContent}</p>
                  {c.propertyCode && (
                    <span className="text-[10px] text-brand-600 dark:text-brand-400 font-mono">#{c.propertyCode}</span>
                  )}
                </div>
              </button>
            ))}
          </div>

          <div className="lg:col-span-2">
            {selectedChat ? (
              <ChatBox
                propertyId={selectedChat.propertyId}
                recipientId={selectedChat.recipientId}
                recipientName={selectedChat.recipientName}
                propertyCode={selectedChat.propertyCode}
              />
            ) : (
              <div className="bg-white dark:bg-slate-900 rounded-3xl p-12 text-center border border-slate-200 dark:border-slate-800 text-slate-400">
                {t('agent.chat.selectConversation', "Selecciona una conversación para responder.")}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
