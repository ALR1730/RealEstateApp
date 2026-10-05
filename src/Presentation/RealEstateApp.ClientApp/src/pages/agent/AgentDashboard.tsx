import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useCurrency } from '../../context/CurrencyContext';
import { useLanguage } from '../../context/LanguageContext';
import { propertiesService, offersService, appointmentsService, commissionsService } from '../../api/services';
import { Property, Offer, Appointment, CommissionSummary } from '../../types';
import { formatDate } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { ChartCard } from '../../components/common/ChartCard';
import {
  Home, 
  Tag, 
  Calendar, 
  PlusCircle, 
  ShieldCheck, 
  CheckCircle,
  Wallet,
  CheckSquare,
  Square
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
  const { formatPrice } = useCurrency();
  const { t } = useLanguage();
  const [properties, setProperties] = useState<Property[]>([]);
  const [receivedOffers, setReceivedOffers] = useState<Offer[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [commissionSummary, setCommissionSummary] = useState<CommissionSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // F-06: Agent Daily Follow-up Tasks (AlterEstate style)
  const [tasks, setTasks] = useState<{ id: number; text: string; completed: boolean; priority: 'alta' | 'media' | 'baja' }[]>(() => {
    const saved = localStorage.getItem('agent_daily_tasks');
    if (saved) {
      try { return JSON.parse(saved); } catch { /* ignore */ }
    }
    return [
      { id: 1, text: 'Confirmar solicitudes de visita pendientes', completed: false, priority: 'alta' },
      { id: 2, text: 'Revisar y responder contraofertas en negociación', completed: false, priority: 'alta' },
      { id: 3, text: 'Verificar fotos de alta resolución en propiedades activas', completed: true, priority: 'media' },
      { id: 4, text: 'Generar valuación AVM para nueva captación en Santo Domingo', completed: false, priority: 'media' },
    ];
  });

  const getTaskLabel = (task: { id: number; text: string }) => {
    switch (task.id) {
      case 1: return t('agent.dash.task1', task.text);
      case 2: return t('agent.dash.task2', task.text);
      case 3: return t('agent.dash.task3', task.text);
      case 4: return t('agent.dash.task4', task.text);
      default: return task.text;
    }
  };

  const toggleTask = (id: number) => {
    const updated = tasks.map(t => t.id === id ? { ...t, completed: !t.completed } : t);
    setTasks(updated);
    localStorage.setItem('agent_daily_tasks', JSON.stringify(updated));
  };

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
    return <Loader text={t('agent.dash.loading', 'Cargando tu panel de agente...')} />;
  }

  const availableCount = properties.filter((p) => p.status === 'Available' || p.status === 'Disponible').length;
  const soldCount = properties.filter((p) => p.status === 'Sold' || p.status === 'Vendida').length;
  const pendingOffersCount = receivedOffers.filter((o) => o.status === 'Pending' || o.status === 'Pendiente').length;

  return (
    <div className="space-y-8">
      
      {/* Header Banner */}
      <div className="bg-gradient-to-r from-navy-950 to-slate-900 text-white p-6 sm:p-8 rounded-3xl shadow-xl flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 border dark:border-slate-800">
        <div>
          <span className="text-emerald-400 text-xs font-extrabold uppercase tracking-widest flex items-center gap-1.5">
            <ShieldCheck className="w-3.5 h-3.5" />
            {t('agent.dash.badge', 'Panel de Agente Inmobiliario')}
          </span>
          <h1 className="text-2xl sm:text-3xl font-extrabold tracking-tight mt-1">
            {t('agent.dash.welcome', 'Bienvenido')}, {user?.userName}
          </h1>
          <p className="text-xs text-slate-300 mt-1">
            {t('agent.dash.subtitle', 'Gestiona tus inmuebles asignados, negocia propuestas económicas y atiende solicitudes de visita con conversión de moneda automática.')}
          </p>
        </div>

        <Link
          to="/agent/properties/create"
          className="px-5 py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white font-extrabold text-xs rounded-xl shadow-lg transition-all flex items-center gap-2"
        >
          <PlusCircle className="w-4 h-4" />
          <span>{t('agent.dash.publishBtn', 'Publicar Inmueble')}</span>
        </Link>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-brand-50 dark:bg-slate-800 text-brand-600 dark:text-brand-400 flex items-center justify-center font-bold border border-transparent dark:border-slate-700">
            <Home className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase">{t('agent.dash.activeProps', 'Propiedades Activas')}</span>
            <p className="text-2xl font-extrabold text-slate-900 dark:text-white font-mono mt-0.5">{availableCount}</p>
          </div>
        </div>

        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-emerald-50 dark:bg-emerald-950/40 text-emerald-600 dark:text-emerald-400 flex items-center justify-center font-bold">
            <CheckCircle className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase">{t('agent.dash.closedSales', 'Ventas Cerradas')}</span>
            <p className="text-2xl font-extrabold text-emerald-600 dark:text-emerald-400 font-mono mt-0.5">{soldCount}</p>
          </div>
        </div>

        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-amber-50 dark:bg-amber-950/40 text-amber-600 dark:text-amber-400 flex items-center justify-center font-bold">
            <Tag className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase">{t('agent.dash.pendingOffers', 'Ofertas Pendientes')}</span>
            <p className="text-2xl font-extrabold text-amber-600 dark:text-amber-400 font-mono mt-0.5">{pendingOffersCount}</p>
          </div>
        </div>

        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-indigo-50 dark:bg-indigo-950/40 text-royal-600 dark:text-indigo-400 flex items-center justify-center font-bold">
            <Calendar className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase">{t('agent.dash.scheduledAppts', 'Citas Agendadas')}</span>
            <p className="text-2xl font-extrabold text-royal-600 dark:text-indigo-400 font-mono mt-0.5">{appointments.length}</p>
          </div>
        </div>
      </div>

      {/* F-06: Daily Follow-up Tasks & Reminders (AlterEstate style) */}
      <div className="bg-white dark:bg-slate-900 p-6 sm:p-7 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 pb-4 border-b border-slate-100 dark:border-slate-800">
          <div>
            <span className="text-[10px] uppercase font-bold text-emerald-600 dark:text-emerald-400 tracking-wider">
              {t('agent.dash.dailyTasksTag', 'Gestión Operativa de Corretaje')}
            </span>
            <h2 className="text-lg font-extrabold text-slate-900 dark:text-white flex items-center gap-2">
              <CheckSquare className="w-5 h-5 text-emerald-600" />
              {t('agent.dash.dailyTasksTitle', 'Tareas & Seguimientos Diarios')}
            </h2>
          </div>
          <span className="text-xs font-semibold text-slate-500 dark:text-slate-400">
            {tasks.filter(t => t.completed).length} de {tasks.length} {t('agent.dash.dailyTasksProgress', 'completadas')}
          </span>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          {tasks.map(task => (
            <div
              key={task.id}
              onClick={() => toggleTask(task.id)}
              className={`p-3.5 rounded-2xl border transition-all cursor-pointer flex items-center justify-between gap-3 ${
                task.completed
                  ? 'bg-slate-50 dark:bg-slate-800/30 border-slate-200 dark:border-slate-800/60 opacity-60'
                  : 'bg-white dark:bg-slate-800/60 border-slate-200 dark:border-slate-700 hover:border-emerald-500'
              }`}
            >
              <div className="flex items-center gap-3">
                {task.completed ? (
                  <CheckSquare className="w-4 h-4 text-emerald-600 shrink-0" />
                ) : (
                  <Square className="w-4 h-4 text-slate-400 shrink-0" />
                )}
                <span className={`text-xs font-semibold ${task.completed ? 'line-through text-slate-400' : 'text-slate-800 dark:text-slate-200'}`}>
                  {getTaskLabel(task)}
                </span>
              </div>
              <span className={`text-[10px] font-bold px-2 py-0.5 rounded-md uppercase tracking-wider shrink-0 ${
                task.priority === 'alta'
                  ? 'bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-300'
                  : 'bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300'
              }`}>
                {task.priority === 'alta' ? t('agent.dash.priorityHigh', 'alta') : t('agent.dash.priorityMedium', 'media')}
              </span>
            </div>
          ))}
        </div>
      </div>

      {/* Interactive Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-1">
          <ChartCard
            title={t('agent.dash.chartCommissions', 'Mis Comisiones')}
            subtitle={commissionSummary ? formatPrice(commissionSummary.totalAmount) : '—'}
            icon={<Wallet className="w-5 h-5 text-emerald-600" />}
          >
            {(commissionSummary?.totalAmount ?? 0) > 0 ? (
              <ResponsiveContainer width="100%" height={250}>
                <PieChart>
                  <Pie
                    data={[
                      { name: t('agent.dash.pending', 'Pendientes'), value: commissionSummary?.pendingAmount || 0 },
                      { name: t('agent.dash.paid', 'Pagadas'), value: commissionSummary?.paidAmount || 0 },
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
                  <Tooltip formatter={(value: number) => formatPrice(value)} />
                  <Legend wrapperStyle={{ fontSize: 12, fontWeight: 600 }} />
                </PieChart>
              </ResponsiveContainer>
            ) : (
              <p className="text-xs text-slate-400 py-10 text-center">
                {t('agent.dash.chartCommissionsEmpty', 'Aún no se han generado comisiones por ventas cerradas.')}
              </p>
            )}
          </ChartCard>
        </div>

        <div className="lg:col-span-2">
          <ChartCard
            title={t('agent.dash.chartAppts', 'Citas por Estado')}
            subtitle={`${appointments.length} ${t('agent.dash.chartApptsVisits', 'visitas')}`}
            icon={<Calendar className="w-5 h-5 text-royal-600" />}
          >
            {appointments.length > 0 ? (
              <ResponsiveContainer width="100%" height={250}>
                <BarChart
                  data={[
                    {
                      name: t('agent.dash.pending', 'Pendiente'),
                      Cantidad: appointments.filter((a) => (a.status as string) === 'Pending' || (a.status as string) === 'Pendiente').length,
                    },
                    {
                      name: t('agent.dash.confirmed', 'Confirmada'),
                      Cantidad: appointments.filter((a) => (a.status as string) === 'Confirmed' || (a.status as string) === 'Confirmada').length,
                    },
                    {
                      name: t('agent.dash.completed', 'Completada'),
                      Cantidad: appointments.filter((a) => (a.status as string) === 'Completed' || (a.status as string) === 'Completada').length,
                    },
                    {
                      name: t('agent.dash.cancelled', 'Cancelada'),
                      Cantidad: appointments.filter((a) => (a.status as string) === 'Cancelled' || (a.status as string) === 'Cancelada').length,
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
                {t('agent.dash.chartApptsEmpty', 'No tienes citas agendadas todavía.')}
              </p>
            )}
          </ChartCard>
        </div>
      </div>

      {/* Recents: Offers to review & Appointments */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        
        {/* Offers Received */}
        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <div className="flex justify-between items-center pb-3 border-b border-slate-100 dark:border-slate-800">
            <h3 className="font-extrabold text-base text-slate-900 dark:text-white flex items-center gap-2">
              <Tag className="w-4 h-4 text-emerald-600" />
              {t('agent.dash.recentsOffers', 'Ofertas Recibidas en tus Inmuebles')}
            </h3>
            <Link to="/agent/offers" className="text-xs font-bold text-emerald-600 dark:text-emerald-400 hover:underline">
              {t('agent.dash.manageAll', 'Gestionar Todas')}
            </Link>
          </div>

          {receivedOffers.length === 0 ? (
            <p className="text-xs text-slate-400 dark:text-slate-500 py-6 text-center">{t('agent.dash.noOffers', 'No hay ofertas recibidas pendientes de revisión.')}</p>
          ) : (
            <div className="divide-y divide-slate-100 dark:divide-slate-800">
              {receivedOffers.slice(0, 4).map((offer) => (
                <div key={offer.id} className="py-3 flex items-center justify-between gap-2">
                  <div>
                    <span className="font-bold text-xs text-slate-800 dark:text-slate-200">{t('agent.dash.propertyNumber', 'Propiedad')} #{offer.propertyCode || offer.propertyId}</span>
                    <p className="text-[11px] font-mono font-bold text-emerald-600 dark:text-emerald-400">
                      {formatPrice(offer.amount)} • {t('agent.dash.byClient', 'Por')} {offer.clientName || t('agent.dash.clientDefault', 'Cliente')}
                    </p>
                  </div>
                  <Badge status={offer.status} />
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Appointments to Confirm */}
        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <div className="flex justify-between items-center pb-3 border-b border-slate-100 dark:border-slate-800">
            <h3 className="font-extrabold text-base text-slate-900 dark:text-white flex items-center gap-2">
              <Calendar className="w-4 h-4 text-royal-600 dark:text-indigo-400" />
              {t('agent.dash.recentsAppts', 'Solicitudes de Visitas')}
            </h3>
            <Link to="/agent/appointments" className="text-xs font-bold text-emerald-600 dark:text-emerald-400 hover:underline">
              {t('agent.dash.viewCalendar', 'Ver Calendario')}
            </Link>
          </div>

          {appointments.length === 0 ? (
            <p className="text-xs text-slate-400 dark:text-slate-500 py-6 text-center">{t('agent.dash.noAppts', 'No tienes solicitudes de visitas pendientes.')}</p>
          ) : (
            <div className="divide-y divide-slate-100 dark:divide-slate-800">
              {appointments.slice(0, 4).map((appt) => (
                <div key={appt.id} className="py-3 flex items-center justify-between gap-2">
                  <div>
                    <span className="font-bold text-xs text-slate-800 dark:text-slate-200">{t('agent.dash.propertyNumber', 'Propiedad')} #{appt.propertyCode || appt.propertyId}</span>
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
