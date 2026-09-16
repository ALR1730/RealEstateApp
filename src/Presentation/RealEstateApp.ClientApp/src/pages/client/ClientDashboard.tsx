import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useCurrency } from '../../context/CurrencyContext';
import { favoritesService, offersService, appointmentsService } from '../../api/services';
import { Offer, Appointment } from '../../types';
import { formatDate } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { 
  Heart, 
  Tag, 
  Calendar, 
  ArrowRight, 
  Search, 
  CheckCircle2, 
  Circle, 
  TrendingUp, 
  ShieldCheck 
} from 'lucide-react';

export const ClientDashboard: React.FC = () => {
  const { user } = useAuth();
  const { formatPrice } = useCurrency();
  const [favoritesCount, setFavoritesCount] = useState<number>(0);
  const [offers, setOffers] = useState<Offer[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    const loadDashboard = async () => {
      try {
        setIsLoading(true);
        const [favs, offs, appts] = await Promise.all([
          favoritesService.getAll().catch(() => []),
          offersService.getMyOffers().catch(() => []),
          appointmentsService.getMyAppointments().catch(() => []),
        ]);
        setFavoritesCount(favs.length);
        setOffers(offs);
        setAppointments(appts);
      } catch (err) {
        console.error("Error loading client dashboard:", err);
      } finally {
        setIsLoading(false);
      }
    };
    loadDashboard();
  }, []);

  if (isLoading) {
    return <Loader text="Cargando tu panel de cliente..." />;
  }

  const hasAcceptedOffer = offers.some(o => o.status === 'Accepted' || o.status === 'Aceptada');

  const milestones = [
    {
      id: 1,
      title: '1. Búsqueda & Favoritos',
      description: 'Explora y guarda tus inmuebles de interés',
      isCompleted: favoritesCount > 0,
      link: '/client/favorites'
    },
    {
      id: 2,
      title: '2. Capacidad de Compra',
      description: 'Calcula tu cuota hipotecaria y poder de pago',
      isCompleted: true,
      link: '/client/capacidad-compra'
    },
    {
      id: 3,
      title: '3. Visita Presencial',
      description: 'Coordina citas y conoce los inmuebles en persona',
      isCompleted: appointments.length > 0,
      link: '/client/appointments'
    },
    {
      id: 4,
      title: '4. Presentar Oferta',
      description: 'Negocia el precio formalmente con el agente',
      isCompleted: offers.length > 0,
      link: '/client/offers'
    },
    {
      id: 5,
      title: '5. Cierre & Promesa',
      description: 'Aceptación de oferta y cierre transaccional',
      isCompleted: hasAcceptedOffer,
      link: '/client/offers'
    }
  ];

  return (
    <div className="space-y-8">
      
      {/* Welcome Banner */}
      <div className="bg-gradient-to-r from-navy-950 to-slate-900 text-white p-6 sm:p-8 rounded-3xl shadow-xl flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 border dark:border-slate-800">
        <div>
          <span className="text-brand-400 text-xs font-extrabold uppercase tracking-widest flex items-center gap-1.5">
            <ShieldCheck className="w-3.5 h-3.5" />
            Portal del Comprador
          </span>
          <h1 className="text-2xl sm:text-3xl font-extrabold tracking-tight mt-1">
            Bienvenido, {user?.userName}
          </h1>
          <p className="text-xs text-slate-300 mt-1">
            Gestiona tus propuestas económicas, citas agendadas y propiedades favoritas con precios en tiempo real.
          </p>
        </div>

        <Link
          to="/catalog"
          className="px-5 py-2.5 bg-brand-600 hover:bg-brand-500 text-white font-extrabold text-xs rounded-xl shadow-lg transition-all flex items-center gap-2"
        >
          <Search className="w-4 h-4" />
          <span>Explorar Catálogo</span>
        </Link>
      </div>

      {/* KPI Cards Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
        
        <Link
          to="/client/favorites"
          className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm hover:shadow-lg transition-all flex items-center gap-4 group"
        >
          <div className="w-12 h-12 rounded-2xl bg-rose-50 dark:bg-rose-950/40 text-rose-600 dark:text-rose-400 flex items-center justify-center font-bold group-hover:scale-105 transition-transform">
            <Heart className="w-6 h-6 fill-rose-500" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase tracking-wider">Favoritos</span>
            <p className="text-2xl font-extrabold text-slate-900 dark:text-white font-mono mt-0.5">{favoritesCount}</p>
          </div>
        </Link>

        <Link
          to="/client/offers"
          className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm hover:shadow-lg transition-all flex items-center gap-4 group"
        >
          <div className="w-12 h-12 rounded-2xl bg-emerald-50 dark:bg-emerald-950/40 text-emerald-600 dark:text-emerald-400 flex items-center justify-center font-bold group-hover:scale-105 transition-transform">
            <Tag className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase tracking-wider">Mis Ofertas</span>
            <p className="text-2xl font-extrabold text-slate-900 dark:text-white font-mono mt-0.5">{offers.length}</p>
          </div>
        </Link>

        <Link
          to="/client/appointments"
          className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm hover:shadow-lg transition-all flex items-center gap-4 group"
        >
          <div className="w-12 h-12 rounded-2xl bg-indigo-50 dark:bg-indigo-950/40 text-royal-600 dark:text-indigo-400 flex items-center justify-center font-bold group-hover:scale-105 transition-transform">
            <Calendar className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase tracking-wider">Citas Agendadas</span>
            <p className="text-2xl font-extrabold text-slate-900 dark:text-white font-mono mt-0.5">{appointments.length}</p>
          </div>
        </Link>
      </div>

      {/* F-10: Buyer Milestones Hub (Ruta de Compra Paso a Paso) */}
      <div className="bg-white dark:bg-slate-900 p-6 sm:p-7 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm space-y-5">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 pb-4 border-b border-slate-100 dark:border-slate-800">
          <div>
            <span className="text-[10px] uppercase font-bold text-brand-600 dark:text-brand-400 tracking-wider">
              Guía Transaccional Dominicana
            </span>
            <h2 className="text-lg font-extrabold text-slate-900 dark:text-white flex items-center gap-2">
              <TrendingUp className="w-5 h-5 text-brand-600" />
              Tu Ruta de Adquisición de Inmueble
            </h2>
          </div>
          <span className="text-xs text-slate-500 dark:text-slate-400">
            {milestones.filter(m => m.isCompleted).length} de {milestones.length} pasos completados
          </span>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-5 gap-3 sm:gap-4">
          {milestones.map((m) => (
            <Link
              key={m.id}
              to={m.link}
              className={`p-4 rounded-2xl border transition-all flex flex-col justify-between ${
                m.isCompleted
                  ? 'bg-emerald-50/60 dark:bg-emerald-950/20 border-emerald-200 dark:border-emerald-800/40 text-emerald-950 dark:text-emerald-300'
                  : 'bg-slate-50/80 dark:bg-slate-800/40 border-slate-200 dark:border-slate-800 text-slate-700 dark:text-slate-300 hover:border-brand-500'
              }`}
            >
              <div>
                <div className="flex items-center justify-between mb-2">
                  <span className="text-xs font-bold font-mono">Paso {m.id}</span>
                  {m.isCompleted ? (
                    <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
                  ) : (
                    <Circle className="w-4 h-4 text-slate-400" />
                  )}
                </div>
                <h3 className="font-bold text-xs">{m.title}</h3>
                <p className="text-[11px] text-slate-500 dark:text-slate-400 mt-1 leading-snug">
                  {m.description}
                </p>
              </div>
              <div className="mt-3 pt-2 border-t border-slate-200/60 dark:border-slate-800 flex items-center text-[10px] font-bold text-brand-600 dark:text-brand-400 gap-1">
                <span>Ir al paso</span>
                <ArrowRight className="w-3 h-3" />
              </div>
            </Link>
          ))}
        </div>
      </div>

      {/* Recents Section: Offers & Appointments */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        
        {/* Recent Offers */}
        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <div className="flex justify-between items-center pb-3 border-b border-slate-100 dark:border-slate-800">
            <h3 className="font-extrabold text-base text-slate-900 dark:text-white flex items-center gap-2">
              <Tag className="w-4 h-4 text-emerald-600" />
              Últimas Ofertas Enviadas
            </h3>
            <Link to="/client/offers" className="text-xs font-bold text-brand-600 dark:text-brand-400 hover:underline">
              Ver todas
            </Link>
          </div>

          {offers.length === 0 ? (
            <p className="text-xs text-slate-400 dark:text-slate-500 py-6 text-center">No has enviado ninguna oferta económica aún.</p>
          ) : (
            <div className="divide-y divide-slate-100 dark:divide-slate-800">
              {offers.slice(0, 4).map((offer) => (
                <div key={offer.id} className="py-3 flex items-center justify-between gap-2">
                  <div>
                    <span className="font-bold text-xs text-slate-800 dark:text-slate-200">Propiedad #{offer.propertyCode || offer.propertyId}</span>
                    <p className="text-[11px] font-mono font-bold text-emerald-600 dark:text-emerald-400">
                      {formatPrice(offer.amount)}
                    </p>
                  </div>
                  <Badge status={offer.status} />
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Recent Appointments */}
        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <div className="flex justify-between items-center pb-3 border-b border-slate-100 dark:border-slate-800">
            <h3 className="font-extrabold text-base text-slate-900 dark:text-white flex items-center gap-2">
              <Calendar className="w-4 h-4 text-royal-600 dark:text-indigo-400" />
              Próximas Visitas Agendadas
            </h3>
            <Link to="/client/appointments" className="text-xs font-bold text-brand-600 dark:text-brand-400 hover:underline">
              Ver todas
            </Link>
          </div>

          {appointments.length === 0 ? (
            <p className="text-xs text-slate-400 dark:text-slate-500 py-6 text-center">No tienes citas de visita programadas.</p>
          ) : (
            <div className="divide-y divide-slate-100 dark:divide-slate-800">
              {appointments.slice(0, 4).map((appt) => (
                <div key={appt.id} className="py-3 flex items-center justify-between gap-2">
                  <div>
                    <span className="font-bold text-xs text-slate-800 dark:text-slate-200">Propiedad #{appt.propertyCode || appt.propertyId}</span>
                    <p className="text-[11px] text-slate-500 dark:text-slate-400">
                      {formatDate(appt.date)} • {appt.timeSlot}
                    </p>
                  </div>
                  <Badge status={appt.status} />
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
