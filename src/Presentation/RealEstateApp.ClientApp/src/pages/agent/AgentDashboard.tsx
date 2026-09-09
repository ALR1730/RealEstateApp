import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { propertiesService, offersService, appointmentsService, commissionsService } from '../../api/services';
import { Property, Offer, Appointment, CommissionSummary } from '../../types';
import { formatCurrencyRD, formatDate } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { ChartCard } from '../../components/common/ChartCard';
import {
  Home, 
  Tag, 
  Calendar, 
  MessageSquare, 
  PlusCircle, 
  TrendingUp, 
  ShieldCheck, 
  CheckCircle,
  Building2,
  Wallet
} from 'lucide-react';
import {
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
} from 'recharts';

export const AgentDashboard: React.FC = () => {
  const { user } = useAuth();
  const [properties, setProperties] = useState<Property[]>([]);
  const [receivedOffers, setReceivedOffers] = useState<Offer[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [commissionSummary, setCommissionSummary] = useState<CommissionSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadDashboard = async () => {
      try {
        setIsLoading(true);
        const [props, offers, appts, commissions] = await Promise.all([
          propertiesService.getMyProperties().catch(() => []),
          offersService.getReceivedOffers().catch(() => []),
          appointmentsService.getMyAppointments().catch(() => []),
          commissionsService.getMySummary().catch(() => null),
        ]);
        setProperties(props);
        setReceivedOffers(offers);
        setAppointments(appts);
        setCommissionSummary(commissions);
      } catch (err) {
        console.error("Error loading agent dashboard:", err);
      } finally {
        setIsLoading(false);
      }
    };
    loadDashboard();
  }, []);

  if (isLoading) {
    return <Loader text="Cargando tu panel de agente..." />;
  }

  const availableCount = properties.filter((p) => p.status === 'Available').length;
  const soldCount = properties.filter((p) => p.status === 'Sold').length;
  const pendingOffersCount = receivedOffers.filter((o) => o.status === 'Pending').length;

  return (
    <div className="space-y-8">
      
      {/* Header Banner */}
      <div className="bg-gradient-to-r from-navy-950 to-slate-900 text-white p-6 sm:p-8 rounded-3xl shadow-xl flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <span className="text-emerald-400 text-xs font-extrabold uppercase tracking-widest">
            Panel de Agente Inmobiliario
          </span>
          <h1 className="text-2xl sm:text-3xl font-extrabold tracking-tight mt-1">
            Bienvenido, {user?.userName}
          </h1>
          <p className="text-xs text-slate-300 mt-1">
            Gestiona tus inmuebles asignados, negocia propuestas económicas y atiende solicitudes de visita.
          </p>
        </div>

        <Link
          to="/agent/properties/create"
          className="px-5 py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white font-extrabold text-xs rounded-xl shadow-lg transition-all flex items-center gap-2"
        >
          <PlusCircle className="w-4 h-4" />
          <span>Publicar Inmueble</span>
        </Link>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-brand-50 text-brand-600 flex items-center justify-center font-bold">
            <Home className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Propiedades Activas</span>
            <p className="text-2xl font-extrabold text-slate-900 font-mono mt-0.5">{availableCount}</p>
          </div>
        </div>

        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center font-bold">
            <CheckCircle className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Ventas Cerradas</span>
            <p className="text-2xl font-extrabold text-emerald-600 font-mono mt-0.5">{soldCount}</p>
          </div>
        </div>

        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center font-bold">
            <Tag className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Ofertas Pendientes</span>
            <p className="text-2xl font-extrabold text-amber-600 font-mono mt-0.5">{pendingOffersCount}</p>
          </div>
        </div>

        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-indigo-50 text-royal-600 flex items-center justify-center font-bold">
            <Calendar className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Citas Agendadas</span>
            <p className="text-2xl font-extrabold text-royal-600 font-mono mt-0.5">{appointments.length}</p>
          </div>
        </div>
      </div>

      {/* Interactive Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-1">
          <ChartCard
            title="Mis Comisiones"
            subtitle={commissionSummary ? formatCurrencyRD(commissionSummary.totalAmount) : '—'}
            icon={<Wallet className="w-5 h-5 text-emerald-600" />}
          >
            {(commissionSummary?.totalAmount ?? 0) > 0 ? (
              <ResponsiveContainer width="100%" height={250}>
                <PieChart>
                  <Pie
                    data={[
                      { name: 'Pendientes', value: commissionSummary?.pendingAmount || 0 },
                      { name: 'Pagadas', value: commissionSummary?.paidAmount || 0 },
                    ]}
                    dataKey="value"
                    nameKey="name"
                    cx="50%"
                    cy="50%"
                    innerRadius={55}
                    outerRadius={85}
                    paddingAngle={2}
                  >
                    <Cell fill="#f59e0b" />
                    <Cell fill="#10b981" />
                  </Pie>
                  <Tooltip formatter={(value: number) => formatCurrencyRD(value)} />
                  <Legend wrapperStyle={{ fontSize: 12, fontWeight: 600 }} />
                </PieChart>
              </ResponsiveContainer>
            ) : (
              <p className="text-xs text-slate-400 py-10 text-center">
                Aún no se han generado comisiones por ventas cerradas.
              </p>
            )}
          </ChartCard>
        </div>

        <div className="lg:col-span-2">
          <ChartCard
            title="Citas por Estado"
            subtitle={`${appointments.length} visitas`}
            icon={<Calendar className="w-5 h-5 text-royal-600" />}
          >
            {appointments.length > 0 ? (
              <ResponsiveContainer width="100%" height={250}>
                <BarChart
                  data={[
                    {
                      name: 'Pendiente',
                      Cantidad: appointments.filter((a) => a.status === 'Pending').length,
                    },
                    {
                      name: 'Confirmada',
                      Cantidad: appointments.filter((a) => a.status === 'Confirmed').length,
                    },
                    {
                      name: 'Completada',
                      Cantidad: appointments.filter((a) => a.status === 'Completed').length,
                    },
                    {
                      name: 'Cancelada',
                      Cantidad: appointments.filter((a) => a.status === 'Cancelled').length,
                    },
                  ]}
                >
                  <CartesianGrid strokeDasharray="3 3" vertical={false} />
                  <XAxis dataKey="name" tick={{ fontSize: 12, fontWeight: 700 }} />
                  <YAxis allowDecimals={false} width={32} tick={{ fontSize: 11 }} />
                  <Tooltip formatter={(value: number) => [`${value}`, 'Citas']} />
                  <Legend wrapperStyle={{ fontSize: 12, fontWeight: 600 }} />
                  <Bar dataKey="Cantidad" fill="#6366f1" radius={[6, 6, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <p className="text-xs text-slate-400 py-10 text-center">
                No tienes citas agendadas todavía.
              </p>
            )}
          </ChartCard>
        </div>
      </div>

      {/* Recents: Offers to review & Appointments */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        
        {/* Offers Received */}
        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex justify-between items-center pb-3 border-b border-slate-100">
            <h3 className="font-extrabold text-base text-slate-900 flex items-center gap-2">
              <Tag className="w-4 h-4 text-emerald-600" />
              Ofertas Recibidas en tus Inmuebles
            </h3>
            <Link to="/agent/offers" className="text-xs font-bold text-emerald-600 hover:underline">
              Gestionar Todas
            </Link>
          </div>

          {receivedOffers.length === 0 ? (
            <p className="text-xs text-slate-400 py-6 text-center">No hay ofertas recibidas pendientes de revisión.</p>
          ) : (
            <div className="divide-y divide-slate-100">
              {receivedOffers.slice(0, 4).map((offer) => (
                <div key={offer.id} className="py-3 flex items-center justify-between gap-2">
                  <div>
                    <span className="font-bold text-xs text-slate-800">Propiedad #{offer.propertyCode || offer.propertyId}</span>
                    <p className="text-[11px] text-slate-500 font-mono font-bold text-emerald-600">
                      {formatCurrencyRD(offer.amount)} • Por {offer.clientName || 'Cliente'}
                    </p>
                  </div>
                  <Badge status={offer.status} />
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Appointments to Confirm */}
        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex justify-between items-center pb-3 border-b border-slate-100">
            <h3 className="font-extrabold text-base text-slate-900 flex items-center gap-2">
              <Calendar className="w-4 h-4 text-royal-600" />
              Solicitudes de Visitas
            </h3>
            <Link to="/agent/appointments" className="text-xs font-bold text-emerald-600 hover:underline">
              Ver Calendario
            </Link>
          </div>

          {appointments.length === 0 ? (
            <p className="text-xs text-slate-400 py-6 text-center">No tienes solicitudes de visitas pendientes.</p>
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
