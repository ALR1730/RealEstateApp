import React, { createContext, useContext, useEffect, useState } from 'react';
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
  chatHubConnection: signalR.HubConnection | null;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { token, isAuthenticated } = useAuth();
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [chatHubConnection, setChatHubConnection] = useState<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!isAuthenticated || !token) {
      if (chatHubConnection) {
        chatHubConnection.stop();
        setChatHubConnection(null);
      }
      return;
    }

    const hubUrl = import.meta.env.VITE_HUB_URL || 'http://localhost:5196/hubs/chat';
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    connection.on('NewChatMessageNotification', (data: any) => {
      const newItem: NotificationItem = {
        id: Math.random().toString(),
        title: 'Nuevo Mensaje en Chat',
        message: data.messageContent || 'Has recibido un nuevo mensaje.',
        type: 'info',
        timestamp: data.sentAtFormatted || new Date().toLocaleTimeString(),
      };
      setNotifications((prev) => [newItem, ...prev]);
    });

    connection.start()
      .then(() => {
        setChatHubConnection(connection);
      })
      .catch((err) => {
        console.warn("SignalR ChatHub connection warning:", err.message);
      });

    return () => {
      connection.stop();
    };
  }, [isAuthenticated, token]);

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
