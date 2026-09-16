import React, { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from './AuthContext';

interface NotificationItem {
  id: string;
  title: string;
  message: string;
  type?: 'info' | 'success' | 'warning' | 'error';
  timestamp: string;
}

interface NotificationContextType {
  notifications: NotificationItem[];
  unreadCount: number;
  clearNotifications: () => void;
  removeNotification: (id: string) => void;
  notificationHubConnection: signalR.HubConnection | null;
  chatHubConnection: signalR.HubConnection | null;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { token, isAuthenticated } = useAuth();
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [notificationHubConnection, setNotificationHubConnection] = useState<signalR.HubConnection | null>(null);
  const [chatHubConnection, setChatHubConnection] = useState<signalR.HubConnection | null>(null);

  const tokenRef = useRef(token);

  useEffect(() => {
    tokenRef.current = token;
  }, [token]);

  const handleReceiveNotification = useCallback((data: any) => {
    const newItem: NotificationItem = {
      id: Math.random().toString(),
      title: data.title || 'Notificación',
      message: data.message || '',
      type: data.type || 'info',
      timestamp: data.timestamp || new Date().toLocaleTimeString(),
    };
    setNotifications((prev) => [newItem, ...prev]);
  }, []);

  const handleNewChatMessage = useCallback((data: any) => {
    const newItem: NotificationItem = {
      id: Math.random().toString(),
      title: 'Nuevo Mensaje en Chat',
      message: data.messageContent || 'Has recibido un nuevo mensaje.',
      type: 'info',
      timestamp: data.sentAtFormatted || new Date().toLocaleTimeString(),
    };
    setNotifications((prev) => [newItem, ...prev]);
  }, []);

  useEffect(() => {
    if (!isAuthenticated || !token) {
      const stopConnections = async () => {
        if (notificationHubConnection) {
          await notificationHubConnection.stop();
          setNotificationHubConnection(null);
        }
        if (chatHubConnection) {
          await chatHubConnection.stop();
          setChatHubConnection(null);
        }
      };
      stopConnections();
      return;
    }

    const baseUrl = import.meta.env.VITE_API_URL
      ? import.meta.env.VITE_API_URL.replace('/api/v1', '').replace(/\/+$/, '')
      : '';

    const notificationConn = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/notifications`, {
        accessTokenFactory: () => tokenRef.current ?? '',
      })
      .withAutomaticReconnect()
      .build();

    notificationConn.on('ReceiveNotification', handleReceiveNotification);

    const chatConn = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/chat`, {
        accessTokenFactory: () => tokenRef.current ?? '',
      })
      .withAutomaticReconnect()
      .build();

    chatConn.on('NewChatMessageNotification', handleNewChatMessage);

    Promise.all([
      notificationConn.start(),
      chatConn.start(),
    ]).then(() => {
      setNotificationHubConnection(notificationConn);
      setChatHubConnection(chatConn);
    }).catch((err) => {
      console.warn("SignalR connection warning:", err.message);
    });

    return () => {
      notificationConn.stop();
      chatConn.stop();
    };
  }, [isAuthenticated, token, handleReceiveNotification, handleNewChatMessage]);

  const clearNotifications = () => setNotifications([]);
  const removeNotification = (id: string) => {
    setNotifications((prev) => prev.filter((n) => n.id !== id));
  };

  return (
    <NotificationContext.Provider
      value={{
        notifications,
        unreadCount: notifications.length,
        clearNotifications,
        removeNotification,
        notificationHubConnection,
        chatHubConnection,
      }}
    >
      {children}
    </NotificationContext.Provider>
  );
};

export const useNotifications = (): NotificationContextType => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotifications debe usarse dentro de NotificationProvider');
  }
  return context;
};
