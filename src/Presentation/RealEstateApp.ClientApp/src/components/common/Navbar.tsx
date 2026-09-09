import React, { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useNotifications } from '../../context/NotificationContext';
import { useCurrency } from '../../context/CurrencyContext';
import { useTheme } from '../../context/ThemeContext';
import { CurrencySwitcher } from './CurrencySwitcher';
import { ThemeToggle } from './ThemeToggle';
import { InstallAppButton } from './InstallAppButton';
import {
  Building2,
  Search,
  Calculator,
  Users,
  Heart,
  Bell,
  LogOut,
  User,
  Menu,
  X,
  LayoutDashboard,
  Map,
} from 'lucide-react';

export const Navbar: React.FC = () => {
  const { user, isAuthenticated, logout, isAdmin, isAgent, isClient } = useAuth();
  const { unreadCount, notifications } = useNotifications();
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [userDropdownOpen, setUserDropdownOpen] = useState(false);
  const [notifDropdownOpen, setNotifDropdownOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/');
    setUserDropdownOpen(false);
  };

  const isActive = (path: string) => location.pathname === path;

  return (
    <header className="sticky top-0 z-40 w-full bg-white/90 backdrop-blur-md border-b border-slate-200/80 shadow-xs transition-all">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          
          {/* Logo & Brand */}
          <Link to="/" className="flex items-center gap-2.5 group">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-brand-600 to-emerald-400 flex items-center justify-center text-white shadow-md shadow-brand-500/20 group-hover:scale-105 transition-transform">
              <Building2 className="w-6 h-6" />
            </div>
            <div>
              <span className="font-extrabold text-xl tracking-tight bg-gradient-to-r from-navy-900 to-slate-700 bg-clip-text text-transparent">
                RealEstate<span className="text-brand-600">App</span>
              </span>
              <span className="hidden sm:inline-block ml-2 text-[10px] uppercase font-bold tracking-widest px-1.5 py-0.5 rounded bg-brand-100 text-brand-700">
                RD$
              </span>
            </div>
          </Link>

          {/* Desktop Navigation */}
          <nav className="hidden md:flex items-center gap-1 lg:gap-2">
            <Link
              to="/catalog"
              className={`flex items-center gap-1.5 px-3.5 py-2 rounded-lg text-sm font-medium transition-colors ${
                isActive('/catalog')
                  ? 'bg-brand-50 text-brand-700 font-semibold'
                  : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100/70'
              }`}
            >
              <Search className="w-4 h-4" />
              Catálogo
            </Link>

            <Link
              to="/simulator"
              className={`flex items-center gap-1.5 px-3.5 py-2 rounded-lg text-sm font-medium transition-colors ${
                isActive('/simulator')
                  ? 'bg-brand-50 text-brand-700 font-semibold'
                  : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100/70'
              }`}
            >
              <Calculator className="w-4 h-4" />
              Simulador Hipotecario
            </Link>

            <Link
              to="/agents"
              className={`flex items-center gap-1.5 px-3.5 py-2 rounded-lg text-sm font-medium transition-colors ${
                isActive('/agents')
                  ? 'bg-brand-50 text-brand-700 font-semibold'
                  : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100/70'
              }`}
            >
              <Users className="w-4 h-4" />
              Agentes
            </Link>

            <Link
              to="/map"
              className={`flex items-center gap-1.5 px-3.5 py-2 rounded-lg text-sm font-medium transition-colors ${
                isActive('/map')
                  ? 'bg-brand-50 text-brand-700 font-semibold'
                  : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100/70'
              }`}
            >
              <Map className="w-4 h-4" />
              Mapa
            </Link>

            {/* Quick Link to Portals */}
            {isAuthenticated && (
              <Link
                to={isAdmin ? '/admin' : isAgent ? '/agent' : '/client'}
                className="flex items-center gap-1.5 px-3.5 py-2 rounded-lg text-sm font-medium text-royal-600 bg-indigo-50 hover:bg-indigo-100 transition-colors ml-2 font-semibold"
              >
                <LayoutDashboard className="w-4 h-4" />
                Mi Panel
              </Link>
            )}

            {/* Install App (Desktop) */}
            <div className="ml-1">
              <InstallAppButton />
            </div>
          </nav>

          {/* Right Action Icons & User Menu */}
          <div className="flex items-center gap-2 sm:gap-3">
            <CurrencySwitcher />
            <ThemeToggle />

            {isAuthenticated ? (
              <>
                {/* Client Favorites Link */}
                {isClient && (
                  <Link
                    to="/client/favorites"
                    className="p-2 rounded-lg text-slate-600 hover:text-rose-600 hover:bg-rose-50 transition-colors relative"
                    title="Mis Favoritos"
                  >
                    <Heart className="w-5 h-5" />
                  </Link>
                )}

                {/* Notifications Bell */}
                <div className="relative">
                  <button
                    onClick={() => setNotifDropdownOpen(!notifDropdownOpen)}
                    className="p-2 rounded-lg text-slate-600 hover:text-brand-600 hover:bg-brand-50 transition-colors relative"
                    title="Notificaciones"
                  >
                    <Bell className="w-5 h-5" />
                    {unreadCount > 0 && (
                      <span className="absolute top-1.5 right-1.5 w-2.5 h-2.5 bg-rose-500 rounded-full ring-2 ring-white animate-pulse" />
                    )}
                  </button>

                  {/* Notification Dropdown */}
                  {notifDropdownOpen && (
                    <div className="absolute right-0 mt-2 w-80 bg-white rounded-2xl shadow-xl border border-slate-200 py-3 z-50 animate-in fade-in zoom-in-95">
                      <div className="px-4 pb-2 border-b border-slate-100 flex justify-between items-center">
                        <span className="font-bold text-sm text-slate-800">Notificaciones</span>
                        <span className="text-xs bg-brand-100 text-brand-700 font-semibold px-2 py-0.5 rounded-full">
                          {unreadCount} nuevas
                        </span>
                      </div>
                      <div className="max-h-64 overflow-y-auto px-2 py-1 custom-scrollbar">
                        {notifications.length === 0 ? (
                          <div className="py-6 text-center text-xs text-slate-400">
                            No tienes notificaciones pendientes.
                          </div>
                        ) : (
                          notifications.map((n) => (
                            <div key={n.id} className="p-2.5 rounded-xl hover:bg-slate-50 transition-colors border-b border-slate-100/50">
                              <p className="text-xs font-bold text-slate-800">{n.title}</p>
                              <p className="text-xs text-slate-600 mt-0.5">{n.message}</p>
                              <span className="text-[10px] text-slate-400 mt-1 block">{n.timestamp}</span>
                            </div>
                          ))
                        )}
                      </div>
                    </div>
                  )}
                </div>

                {/* User Dropdown */}
                <div className="relative">
                  <button
                    onClick={() => setUserDropdownOpen(!userDropdownOpen)}
                    className="flex items-center gap-2 p-1.5 rounded-xl hover:bg-slate-100 transition-colors border border-slate-200/80 bg-white"
                  >
                    <div className="w-8 h-8 rounded-lg bg-gradient-to-tr from-navy-800 to-slate-700 text-white flex items-center justify-center font-bold text-xs">
                      {user?.userName ? user.userName.substring(0, 2).toUpperCase() : 'US'}
                    </div>
                    <span className="hidden md:block text-xs font-semibold text-slate-700 max-w-[100px] truncate">
                      {user?.userName}
                    </span>
                  </button>

                  {userDropdownOpen && (
                    <div className="absolute right-0 mt-2 w-56 bg-white rounded-2xl shadow-xl border border-slate-200 py-2 z-50">
                      <div className="px-4 py-2.5 border-b border-slate-100">
                        <p className="text-xs font-bold text-slate-900 truncate">{user?.userName}</p>
                        <p className="text-[11px] text-slate-500 truncate">{user?.email}</p>
                        <span className="inline-block mt-1 text-[10px] font-bold px-2 py-0.5 rounded-md bg-slate-100 text-slate-700">
                          Rol: {user?.roles?.join(', ')}
                        </span>
                      </div>

                      <div className="py-1">
                        <Link
                          to={isAdmin ? '/admin' : isAgent ? '/agent' : '/client'}
                          onClick={() => setUserDropdownOpen(false)}
                          className="flex items-center gap-2 px-4 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 hover:text-brand-600"
                        >
                          <LayoutDashboard className="w-4 h-4" />
                          Panel de Control
                        </Link>

                        <Link
                          to={isAdmin ? '/admin/profile' : isAgent ? '/agent/profile' : '/client/profile'}
                          onClick={() => setUserDropdownOpen(false)}
                          className="flex items-center gap-2 px-4 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 hover:text-brand-600"
                        >
                          <User className="w-4 h-4" />
                          Mi Perfil
                        </Link>
                      </div>

                      <div className="pt-1 border-t border-slate-100">
                        <button
                          onClick={handleLogout}
                          className="w-full flex items-center gap-2 px-4 py-2 text-xs font-medium text-rose-600 hover:bg-rose-50 text-left"
                        >
                          <LogOut className="w-4 h-4" />
                          Cerrar Sesión
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              </>
            ) : (
              <div className="flex items-center gap-2">
                <Link
                  to="/login"
                  className="text-xs sm:text-sm font-semibold text-slate-700 hover:text-slate-900 px-3 py-2 rounded-lg hover:bg-slate-100 transition-colors"
                >
                  Iniciar Sesión
                </Link>
                <Link
                  to="/register"
                  className="text-xs sm:text-sm font-semibold text-white bg-brand-600 hover:bg-brand-700 px-4 py-2 rounded-xl shadow-md shadow-brand-600/20 hover:shadow-lg transition-all"
                >
                  Registrarse
                </Link>
              </div>
            )}

            {/* Mobile Menu Button */}
            <button
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              className="md:hidden p-2 rounded-lg text-slate-600 hover:text-slate-900 hover:bg-slate-100"
            >
              {mobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
            </button>
          </div>
        </div>
      </div>

      {/* Mobile Drawer */}
      {mobileMenuOpen && (
        <div className="md:hidden border-t border-slate-200 bg-white px-4 pt-3 pb-6 space-y-2">
          <Link
            to="/catalog"
            onClick={() => setMobileMenuOpen(false)}
            className="flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-100"
          >
            <Search className="w-4 h-4 text-brand-600" />
            Catálogo de Propiedades
          </Link>
          <Link
            to="/simulator"
            onClick={() => setMobileMenuOpen(false)}
            className="flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-100"
          >
            <Calculator className="w-4 h-4 text-brand-600" />
            Simulador Hipotecario (RD$)
          </Link>
          <Link
            to="/agents"
            onClick={() => setMobileMenuOpen(false)}
            className="flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-100"
          >
            <Users className="w-4 h-4 text-brand-600" />
            Directorio de Agentes
          </Link>

          {isAuthenticated && (
            <Link
              to={isAdmin ? '/admin' : isAgent ? '/agent' : '/client'}
              onClick={() => setMobileMenuOpen(false)}
              className="flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium text-royal-600 bg-indigo-50 font-bold"
            >
              <LayoutDashboard className="w-4 h-4" />
              Mi Panel de Control
            </Link>
          )}

          <div className="pt-2 border-t border-slate-100">
            <InstallAppButton />
          </div>
        </div>
      )}
    </header>
  );
};
