import { ChatMessage } from '../types';

export interface RawConversation {
  id?: number;
  clienteId?: string;
  clienteName?: string;
  agenteId?: string;
  agenteName?: string;
  propertyId?: number;
  propertyCode?: string;
  messageContent?: string;
  senderId?: string;
  sentAt?: string;
  sentAtFormatted?: string;
}

export function mapConversations(data: RawConversation[] | null, user: { roles?: string[] } | null): ChatMessage[] {
  const isClient = user?.roles?.includes('Client');
  return (data || []).map((c) => ({
    ...c,
    id: c.id || 0,
    propertyId: c.propertyId || 0,
    senderId: c.senderId || '',
    messageContent: c.messageContent || '',
    sentAt: c.sentAt || '',
    recipientId: isClient ? (c.agenteId || '') : (c.clienteId || ''),
    recipientName: isClient ? (c.agenteName || 'Agente') : (c.clienteName || 'Cliente'),
  }));
}