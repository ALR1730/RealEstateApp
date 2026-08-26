import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { favoritesService, offersService, appointmentsService } from '../../api/services';
import { Offer, Appointment } from '../../types';
import { formatCurrencyRD, formatDate } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { 
  Heart, 
  Tag, 
  Calendar, 
  MessageSquare, 
  ArrowRight, 
  Search, 
  Building2 
} from 'lucide-react';

export const ClientDashboard: React.FC = () => {
  const { user } = useAuth();
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

  return (
    <div className="space-y-8">
      
      {/* Welcome Banner */}
      <div className="bg-gradient-to-r from-navy-950 to-slate-900 text-white p-6 sm:p-8 rounded-3xl shadow-xl flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <span className="text-brand-400 text-xs font-extrabold uppercase tracking-widest">
            Portal del Comprador
          </span>
          <h1 className="text-2xl sm:text-3xl font-extrabold tracking-tight mt-1">
            Bienvenido, {user?.userName}
          </h1>
          <p className="text-xs text-slate-300 mt-1">
            Gestiona tus propuestas económicas, citas agendadas y propiedades favoritas.
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
          className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs hover:shadow-lg transition-all flex items-center gap-4 group"
        >
          <div className="w-12 h-12 rounded-2xl bg-rose-50 text-rose-600 flex items-center justify-center font-bold group-hover:scale-105 transition-transform">
            <Heart className="w-6 h-6 fill-rose-500" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase tracking-wider">Favoritos</span>
            <p className="text-2xl font-extrabold text-slate-900 font-mono mt-0.5">{favoritesCount}</p>
          </div>
        </Link>

        <Link
          to="/client/offers"
          className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs hover:shadow-lg transition-all flex items-center gap-4 group"
        >
          <div className="w-12 h-12 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center font-bold group-hover:scale-105 transition-transform">
            <Tag className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase tracking-wider">Mis Ofertas</span>
            <p className="text-2xl font-extrabold text-slate-900 font-mono mt-0.5">{offers.length}</p>
          </div>
        </Link>

        <Link
          to="/client/appointments"
          className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs hover:shadow-lg transition-all flex items-center gap-4 group"
        >
          <div className="w-12 h-12 rounded-2xl bg-indigo-50 text-royal-600 flex items-center justify-center font-bold group-hover:scale-105 transition-transform">
            <Calendar className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase tracking-wider">Citas Agendadas</span>
            <p className="text-2xl font-extrabold text-slate-900 font-mono mt-0.5">{appointments.length}</p>
          </div>
        </Link>
      </div>

      {/* Recents Section: Offers & Appointments */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        
        {/* Recent Offers */}
        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex justify-between items-center pb-3 border-b border-slate-100">
            <h3 className="font-extrabold text-base text-slate-900 flex items-center gap-2">
              <Tag className="w-4 h-4 text-emerald-600" />
              Últimas Ofertas Enviadas
            </h3>
            <Link to="/client/offers" className="text-xs font-bold text-brand-600 hover:underline">
              Ver todas
            </Link>
          </div>

          {offers.length === 0 ? (
            <p className="text-xs text-slate-400 py-6 text-center">No has enviado ninguna oferta económica aún.</p>
          ) : (
            <div className="divide-y divide-slate-100">
              {offers.slice(0, 4).map((offer) => (
                <div key={offer.id} className="py-3 flex items-center justify-between gap-2">
                  <div>
                    <span className="font-bold text-xs text-slate-800">Propiedad #{offer.propertyCode || offer.propertyId}</span>
                    <p className="text-[11px] text-slate-500 font-mono font-bold text-emerald-600">
                      {formatCurrencyRD(offer.amount)}
                    </p>
                  </div>
                  <Badge status={offer.status} />
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Recent Appointments */}
        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex justify-between items-center pb-3 border-b border-slate-100">
            <h3 className="font-extrabold text-base text-slate-900 flex items-center gap-2">
              <Calendar className="w-4 h-4 text-royal-600" />
              Próximas Visitas Agendadas
            </h3>
            <Link to="/client/appointments" className="text-xs font-bold text-brand-600 hover:underline">
              Ver todas
            </Link>
          </div>

          {appointments.length === 0 ? (
            <p className="text-xs text-slate-400 py-6 text-center">No tienes citas de visita programadas.</p>
          ) : (
            <div className="divide-y divide-slate-100">
              {appointments.slice(0, 4).map((appt) => (
                <div key={appt.id} className="py-3 flex items-center justify-between gap-2">
                  <div>
                    <span className="font-bold text-xs text-slate-800">Propiedad #{appt.propertyCode || appt.propertyId}</span>
                    <p className="text-[11px] text-slate-500">
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
