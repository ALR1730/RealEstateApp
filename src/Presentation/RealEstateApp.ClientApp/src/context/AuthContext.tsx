import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { User, AuthResponse } from '../types';
import { authService } from '../api/services';

interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, pass: string) => Promise<AuthResponse>;
  logout: () => void;
  hasRole: (role: string) => boolean;
  isAdmin: boolean;
  isAgent: boolean;
  isClient: boolean;
  isDeveloper: boolean;
  isOwner: boolean;
  refreshProfile: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    try {
      const rawUser = localStorage.getItem('realestate_user');
      return rawUser ? (JSON.parse(rawUser) as User) : null;
    } catch {
      localStorage.removeItem('realestate_user');
      localStorage.removeItem('realestate_jwt_token');
      return null;
    }
  });
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('realestate_jwt_token'));
  const [isLoading, setIsLoading] = useState<boolean>(() => Boolean(localStorage.getItem('realestate_jwt_token')));

  const logout = useCallback(() => {
    setToken(null);
    setUser(null);
    localStorage.removeItem('realestate_jwt_token');
    localStorage.removeItem('realestate_user');
  }, []);

  useEffect(() => {
    const handleLogout = () => logout();
    window.addEventListener('auth:logout', handleLogout);
    return () => window.removeEventListener('auth:logout', handleLogout);
  }, [logout]);

  useEffect(() => {
    let active = true;
    const validateToken = async () => {
      const storedToken = localStorage.getItem('realestate_jwt_token');
      if (!storedToken) {
        if (active) setIsLoading(false);
        return;
      }
      try {
        const profile = await authService.getProfile();
        if (!active) return;
        setUser((current) => {
          if (!current) return current;
          return {
            ...current,
            firstName: profile?.firstName,
            lastName: profile?.lastName,
            phone: profile?.phone,
            photoUrl: profile?.profilePictureUrl,
          };
        });
      } catch {
        if (!active) return;
        setToken(null);
        setUser(null);
        localStorage.removeItem('realestate_jwt_token');
        localStorage.removeItem('realestate_user');
      } finally {
        if (active) setIsLoading(false);
      }
    };
    validateToken();
    return () => {
      active = false;
    };
  }, []);

  const login = async (email: string, pass: string): Promise<AuthResponse> => {
    const response = await authService.login(email, pass);
    if (!response.hasError && response.jwToken) {
      const userData: User = {
        id: response.id,
        userName: response.userName,
        email: response.email,
        roles: response.roles || [],
        isVerified: response.isVerified,
        jwToken: response.jwToken,
      };
      setToken(response.jwToken);
      setUser(userData);
      localStorage.setItem('realestate_jwt_token', response.jwToken);
      localStorage.setItem('realestate_user', JSON.stringify(userData));
    }
    return response;
  };

  const refreshProfile = async () => {
    try {
      const profile = await authService.getProfile();
      if (profile && user) {
        const updatedUser = {
          ...user,
          firstName: profile.firstName,
          lastName: profile.lastName,
          phone: profile.phone,
          photoUrl: profile.profilePictureUrl,
        };
        setUser(updatedUser);
        localStorage.setItem('realestate_user', JSON.stringify(updatedUser));
      }
    } catch (err) {
      console.error("Error al actualizar perfil:", err);
    }
  };

  const hasRole = (role: string): boolean => {
    if (!user || !user.roles) return false;
    return user.roles.some((r) => r.toLowerCase() === role.toLowerCase());
  };

  const isAdmin = hasRole('Admin');
  const isAgent = hasRole('Agent');
  const isClient = hasRole('Client');
  const isDeveloper = hasRole('Developer');
  const isOwner = hasRole('Owner');

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: !!user && !!token,
        isLoading,
        login,
        logout,
        hasRole,
        isAdmin,
        isAgent,
        isClient,
        isDeveloper,
        isOwner,
        refreshProfile,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth debe ser utilizado dentro de un AuthProvider');
  }
  return context;
};
