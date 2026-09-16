import React, { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { 
  LayoutDashboard, 
  Home, 
  PlusCircle, 
  Tag, 
  Calendar, 
  MessageSquare, 
  Heart, 
  BookmarkCheck, 
  UserCheck, 
  Shield, 
  Layers, 
  Building, 
  Sparkles,
  User,
  Sliders,
  ShieldCheck,
  Award,
  Building2,
  Kanban,
  Calculator,
  Wallet,
  Star,
  FileText,
  Terminal,
  ChevronDown,
  ChevronUp
} from 'lucide-react';

export const Sidebar: React.FC = () => {
  const { isAdmin, isAgent, isClient, hasRole, user } = useAuth();
  const isOwner = hasRole('Owner');
  const isDeveloper = hasRole('Developer');
  const [isMobileOpen, setIsMobileOpen] = useState(false);

  const navClass = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-xs sm:text-sm font-semibold transition-all ${
      isActive
        ? 'bg-brand-600 text-white shadow-md shadow-brand-600/20'
        : 'text-slate-600 dark:text-slate-300 hover:text-slate-900 dark:hover:text-white hover:bg-slate-100 dark:hover:bg-slate-800'
    }`;

  return (
    <>
      {/* Mobile Top Bar */}
      <div className="md:hidden flex items-center justify-between p-3 bg-white dark:bg-slate-900 border-b border-slate-200 dark:border-slate-800">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-tr from-navy-900 to-slate-700 text-white flex items-center justify-center font-bold text-xs">
            {user?.userName ? user.userName.substring(0, 2).toUpperCase() : 'US'}
          </div>
          <span className="text-xs font-bold text-slate-800 dark:text-slate-200">
            {isAdmin ? 'Panel Admin' : isAgent ? 'Panel Agente' : isOwner ? 'Panel Propietario' : isDeveloper ? 'Panel Dev' : 'Panel Cliente'}
          </span>
        </div>
        <button
          onClick={() => setIsMobileOpen(!isMobileOpen)}
          className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-200 hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors"
        >
          <span>Menú Panel</span>
          {isMobileOpen ? <ChevronUp className="w-3.5 h-3.5" /> : <ChevronDown className="w-3.5 h-3.5" />}
        </button>
      </div>

      <aside className={`${isMobileOpen ? 'flex' : 'hidden'} md:flex w-full md:w-64 bg-white dark:bg-slate-900 border-b md:border-b-0 md:border-r border-slate-200 dark:border-slate-800 md:min-h-[calc(100vh-4rem)] p-4 flex-col justify-between shrink-0 transition-all`}>
        <div className="space-y-6">
          
          {/* User Card Header */}
          <div className="p-3 bg-slate-50 dark:bg-slate-800/50 border border-slate-100 dark:border-slate-800 rounded-2xl flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-navy-900 to-slate-700 text-white flex items-center justify-center font-bold text-sm">
              {user?.userName ? user.userName.substring(0, 2).toUpperCase() : 'US'}
            </div>
            <div className="overflow-hidden">
              <p className="text-xs font-bold text-slate-800 dark:text-slate-200 truncate">{user?.userName}</p>
              <span className="inline-block text-[10px] font-bold px-1.5 py-0.5 rounded bg-brand-100 dark:bg-brand-950/70 text-brand-800 dark:text-brand-300 uppercase tracking-wider">
                {isAdmin ? 'Administrador' : isAgent ? 'Agente' : isOwner ? 'Propietario' : isDeveloper ? 'Desarrollador' : 'Cliente'}
              </span>
            </div>
          </div>

          {/* Navigation Sections */}
        <div className="space-y-1">
          
          {/* CLIENT MENU */}
          {isClient && (
            <>
              <div className="text-[10px] font-extrabold uppercase tracking-wider text-slate-400 px-3 py-1">
                Portal del Cliente
              </div>
              <NavLink to="/client" end className={navClass}>
                <LayoutDashboard className="w-4 h-4" />
                Resumen
              </NavLink>
              <NavLink to="/client/favorites" className={navClass}>
                <Heart className="w-4 h-4" />
                Mis Favoritos
              </NavLink>
              <NavLink to="/client/offers" className={navClass}>
                <Tag className="w-4 h-4" />
                Mis Ofertas
              </NavLink>
              <NavLink to="/client/appointments" className={navClass}>
                <Calendar className="w-4 h-4" />
                Mis Citas
              </NavLink>
              <NavLink to="/client/capacidad-compra" className={navClass}>
                <Wallet className="w-4 h-4" />
                Capacidad de Compra
              </NavLink>
              <NavLink to="/client/saved-searches" className={navClass}>
                <BookmarkCheck className="w-4 h-4" />
                Búsquedas Guardadas
              </NavLink>
              <NavLink to="/client/chats" className={navClass}>
                <MessageSquare className="w-4 h-4" />
                Mensajes Directos
              </NavLink>
              <NavLink to="/client/profile" className={navClass}>
                <User className="w-4 h-4" />
                Mi Perfil
              </NavLink>
            </>
          )}

          {/* OWNER MENU */}
          {isOwner && (
            <>
              <div className="text-[10px] font-extrabold uppercase tracking-wider text-slate-400 px-3 py-1">
                Propietario Directo
              </div>
              <NavLink to="/owner" end className={navClass}>
                <Home className="w-4 h-4" />
                Mis Inmuebles
              </NavLink>
              <NavLink to="/owner/properties/create" className={navClass}>
                <PlusCircle className="w-4 h-4" />
                Publicar Directo
              </NavLink>
              <NavLink to="/owner/offers" className={navClass}>
                <Tag className="w-4 h-4" />
                Ofertas Recibidas
              </NavLink>
              <NavLink to="/owner/profile" className={navClass}>
                <User className="w-4 h-4" />
                Mi Perfil
              </NavLink>
            </>
          )}

          {/* AGENT MENU */}
          {isAgent && (
            <>
              <div className="text-[10px] font-extrabold uppercase tracking-wider text-slate-400 px-3 py-1">
                Portal del Agente
              </div>
              <NavLink to="/agent" end className={navClass}>
                <LayoutDashboard className="w-4 h-4" />
                Panel de Agente
              </NavLink>
              <NavLink to="/agent/properties" end className={navClass}>
                <Home className="w-4 h-4" />
                Mis Propiedades
              </NavLink>
              <NavLink to="/agent/properties/create" className={navClass}>
                <PlusCircle className="w-4 h-4" />
                Publicar Inmueble
              </NavLink>
              <NavLink to="/agent/avm" className={navClass}>
                <Calculator className="w-4 h-4" />
                Valuación (AVM)
              </NavLink>
              <NavLink to="/agent/leads" className={navClass}>
                <Kanban className="w-4 h-4" />
                Pipeline de Leads
              </NavLink>
              <NavLink to="/agent/offers" className={navClass}>
                <Tag className="w-4 h-4" />
                Ofertas Recibidas
              </NavLink>
              <NavLink to="/agent/commissions" className={navClass}>
                <Wallet className="w-4 h-4" />
                Mis Comisiones
              </NavLink>
              <NavLink to="/agent/appointments" className={navClass}>
                <Calendar className="w-4 h-4" />
                Gestión de Citas
              </NavLink>
              <NavLink to="/agent/chats" className={navClass}>
                <MessageSquare className="w-4 h-4" />
                Mensajes con Clientes
              </NavLink>
              <NavLink to="/agent/verification" className={navClass}>
                <ShieldCheck className="w-4 h-4" />
                Verificación KYC
              </NavLink>
              <NavLink to="/agent/subscription" className={navClass}>
                <Award className="w-4 h-4" />
                Membresías & Planes
              </NavLink>
              <NavLink to="/agent/profile" className={navClass}>
                <User className="w-4 h-4" />
                Mi Perfil de Agente
              </NavLink>
            </>
          )}

          {/* DEVELOPER MENU */}
          {isDeveloper && (
            <>
              <div className="text-[10px] font-extrabold uppercase tracking-wider text-slate-400 px-3 py-1">
                Developer Portal
              </div>
              <NavLink to="/developer" end className={navClass}>
                <Terminal className="w-4 h-4" />
                Panel de Desarrollo
              </NavLink>
              <NavLink to="/developer/property-types" className={navClass}>
                <Building className="w-4 h-4" />
                Tipos de Propiedad
              </NavLink>
              <NavLink to="/developer/sale-types" className={navClass}>
                <Layers className="w-4 h-4" />
                Tipos de Venta
              </NavLink>
              <NavLink to="/developer/improvements" className={navClass}>
                <Sparkles className="w-4 h-4" />
                Amenidades / Mejoras
              </NavLink>
              <NavLink to="/developer/profile" className={navClass}>
                <User className="w-4 h-4" />
                Mi Perfil
              </NavLink>
            </>
          )}

          {/* ADMIN MENU */}
          {isAdmin && (
            <>
              <div className="text-[10px] font-extrabold uppercase tracking-wider text-slate-400 px-3 py-1">
                Administración Global
              </div>
              <NavLink to="/admin" end className={navClass}>
                <LayoutDashboard className="w-4 h-4" />
                Dashboard KPIs
              </NavLink>
              <NavLink to="/admin/all-properties" className={navClass}>
                <Building2 className="w-4 h-4" />
                Catálogo Global
              </NavLink>
              <NavLink to="/admin/verifications" className={navClass}>
                <ShieldCheck className="w-4 h-4" />
                Validación KYC
              </NavLink>
              <NavLink to="/admin/subscriptions" className={navClass}>
                <Award className="w-4 h-4" />
                Membresías & Planes
              </NavLink>
              <NavLink to="/admin/reviews" className={navClass}>
                <Star className="w-4 h-4" />
                Reseñas de Agentes
              </NavLink>
              <NavLink to="/admin/commissions" className={navClass}>
                <Wallet className="w-4 h-4" />
                Comisiones por Venta
              </NavLink>
              <NavLink to="/admin/documents" className={navClass}>
                <FileText className="w-4 h-4" />
                Documentos Legales
              </NavLink>
              <NavLink to="/admin/agents" className={navClass}>
                <UserCheck className="w-4 h-4" />
                Gestión de Agentes
              </NavLink>
              <NavLink to="/admin/users" className={navClass}>
                <Shield className="w-4 h-4" />
                Admins & Developers
              </NavLink>

              <div className="text-[10px] font-extrabold uppercase tracking-wider text-slate-400 px-3 pt-3 pb-1">
                Mantenimientos Núcleo
              </div>
              <NavLink to="/admin/property-types" className={navClass}>
                <Building className="w-4 h-4" />
                Tipos de Propiedad
              </NavLink>
              <NavLink to="/admin/sale-types" className={navClass}>
                <Layers className="w-4 h-4" />
                Tipos de Venta
              </NavLink>
              <NavLink to="/admin/improvements" className={navClass}>
                <Sparkles className="w-4 h-4" />
                Amenidades / Mejoras
              </NavLink>
              <NavLink to="/admin/profile" className={navClass}>
                <User className="w-4 h-4" />
                Mi Perfil
              </NavLink>
            </>
          )}
        </div>
      </div>

      {/* Quick link to public catalog */}
      <div className="pt-4 border-t border-slate-100 dark:border-slate-800">
        <NavLink to="/catalog" className="flex items-center gap-2 px-3 py-2 text-xs font-semibold text-slate-500 dark:text-slate-400 hover:text-slate-800 dark:hover:text-slate-200">
          <Sliders className="w-4 h-4" />
          Ver Catálogo Público
        </NavLink>
      </div>
    </aside>
    </>
  );
};
