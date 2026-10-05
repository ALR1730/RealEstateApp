import React, { useState, useEffect } from 'react';
import { adminService } from '../../api/services';
import { DashboardKPIs } from '../../types';
import { Loader } from '../../components/common/Loader';
import { ChartCard } from '../../components/common/ChartCard';
import {
  PieChart as PieChartIcon,
  BarChart3,
  Building2, 
  Home, 
  CheckCircle, 
  Users, 
  ShieldCheck, 
  Tag, 
  Code
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
import { Link } from 'react-router-dom';
import { useLanguage } from '../../context/LanguageContext';

export const AdminDashboard: React.FC = () => {
  const { t } = useLanguage();
  const [kpis, setKpis] = useState<DashboardKPIs | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchKPIs = async () => {
      try {
        setIsLoading(true);
        const data = await adminService.getDashboardKPIs();
        setKpis(data);
      } catch (err) {
        console.error("Error loading admin KPIs:", err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchKPIs();
  }, []);

  if (isLoading) {
    return <Loader text={t('admin.loading', 'Generando cuadro de mando ejecutivo...')} />;
  }

  return (
    <div className="space-y-8 pb-12">
      
      {/* Header */}
      <div className="bg-gradient-to-r from-navy-950 to-slate-900 text-white p-6 sm:p-8 rounded-3xl shadow-xl flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <span className="text-royal-400 text-xs font-extrabold uppercase tracking-widest">
            {t('admin.badge', 'Dirección Ejecutiva')}
          </span>
          <h1 className="text-2xl sm:text-3xl font-extrabold tracking-tight mt-1">
            {t('admin.title', 'Dashboard de KPIs y Control Global')}
          </h1>
          <p className="text-xs text-slate-300 mt-1">
            {t('admin.subtitle', 'Métricas de inventario inmobiliario, rendimiento de agentes y usuarios en tiempo real.')}
          </p>
        </div>

        <div className="flex gap-2">
          <Link
            to="/admin/agents"
            className="px-4 py-2 bg-royal-600 hover:bg-royal-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
          >
            {t('admin.manageAgents', 'Gestión de Agentes')}
          </Link>
          <Link
            to="/admin/property-types"
            className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-white font-bold text-xs rounded-xl transition-all"
          >
            {t('admin.coreCatalogs', 'Catálogos Núcleo')}
          </Link>
        </div>
      </div>

      {/* KPI Cards Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        
        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-brand-50 dark:bg-slate-800 text-brand-600 dark:text-brand-400 border border-transparent dark:border-slate-700 flex items-center justify-center font-bold">
            <Home className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase">{t('admin.availableProperties', 'Inmuebles Disponibles')}</span>
            <p className="text-2xl font-extrabold text-slate-900 dark:text-white font-mono mt-0.5">{kpis?.totalAvailableProperties || 0}</p>
          </div>
        </div>

        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-emerald-50 dark:bg-slate-800 text-emerald-600 dark:text-emerald-400 border border-transparent dark:border-slate-700 flex items-center justify-center font-bold">
            <CheckCircle className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase">{t('admin.soldProperties', 'Inmuebles Vendidos')}</span>
            <p className="text-2xl font-extrabold text-emerald-600 dark:text-emerald-400 font-mono mt-0.5">{kpis?.totalSoldProperties || 0}</p>
          </div>
        </div>

        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-indigo-50 dark:bg-slate-800 text-royal-600 dark:text-indigo-400 border border-transparent dark:border-slate-700 flex items-center justify-center font-bold">
            <Users className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase">{t('admin.activeAgents', 'Agentes Activos')}</span>
            <p className="text-2xl font-extrabold text-royal-600 dark:text-indigo-400 font-mono mt-0.5">
              {kpis?.totalActiveAgents || 0}
              <span className="text-xs text-slate-400 dark:text-slate-500 font-normal font-sans ml-1">({kpis?.totalInactiveAgents || 0} {t('admin.inactive', 'inactivos')})</span>
            </p>
          </div>
        </div>

        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-amber-50 dark:bg-slate-800 text-amber-600 dark:text-amber-400 border border-transparent dark:border-slate-700 flex items-center justify-center font-bold">
            <ShieldCheck className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 dark:text-slate-400 font-semibold uppercase">{t('admin.buyerClients', 'Clientes Compradores')}</span>
            <p className="text-2xl font-extrabold text-amber-600 dark:text-amber-400 font-mono mt-0.5">{kpis?.totalClients || 0}</p>
          </div>
        </div>
      </div>

      {/* Interactive Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <ChartCard
          title={t('admin.statusDistribution', 'Distribución por Estado')}
          subtitle={`${t('admin.total', 'Total')}: ${kpis?.totalProperties || 0}`}
          icon={<PieChartIcon className="w-5 h-5 text-royal-600" />}
        >
          {(kpis?.totalProperties ?? 0) > 0 ? (
            <ResponsiveContainer width="100%" height={260}>
              <PieChart>
                <Pie
                  data={[
                    { name: t('admin.statusAvailable', 'Disponibles'), value: kpis?.totalAvailableProperties || 0 },
                    { name: t('admin.statusReserved', 'Reservadas'), value: kpis?.totalReservedProperties || 0 },
                    { name: t('admin.statusSold', 'Vendidas'), value: kpis?.totalSoldProperties || 0 },
                  ]}
                  dataKey="value"
                  nameKey="name"
                  cx="50%"
                  cy="50%"
                  innerRadius={55}
                  outerRadius={85}
                  paddingAngle={2}
                >
                  <Cell fill="#10b981" />
                  <Cell fill="#f59e0b" />
                  <Cell fill="#475569" />
                </Pie>
                <Tooltip
                  formatter={(value: number) => [`${value}`, t('admin.properties', 'Propiedades')]}
                />
                <Legend wrapperStyle={{ fontSize: 12, fontWeight: 600 }} />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-xs text-slate-400 py-10 text-center">{t('admin.noProperties', 'Aún no hay propiedades registradas.')}</p>
          )}
        </ChartCard>

        <ChartCard
          title={t('admin.propertiesByType', 'Propiedades por Tipo')}
          subtitle={t('admin.units', 'Unidades')}
          icon={<BarChart3 className="w-5 h-5 text-brand-600" />}
        >
          {(kpis?.propertiesByType ?? []).length > 0 ? (
            <ResponsiveContainer width="100%" height={260}>
              <BarChart data={kpis?.propertiesByType || []}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis
                  dataKey="typeName"
                  tick={{ fontSize: 11, fontWeight: 700 }}
                  interval={0}
                  angle={-18}
                  textAnchor="end"
                  height={40}
                />
                <YAxis allowDecimals={false} width={32} tick={{ fontSize: 11 }} />
                <Tooltip formatter={(value: number) => [`${value}`, t('admin.properties', 'Propiedades')]} />
                <Bar dataKey="count" name={t('admin.properties', 'Propiedades')} fill="#0ea5e9" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-xs text-slate-400 py-10 text-center">{t('admin.noDistributionData', 'No hay datos de distribución disponibles.')}</p>
          )}
        </ChartCard>
      </div>

      {/* Quick Management Shortcuts */}
      <div className="bg-white dark:bg-slate-900 p-6 sm:p-8 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs space-y-4">
        <h3 className="font-extrabold text-base text-slate-900 dark:text-white">
          {t('admin.governanceTitle', 'Accesos Rápidos de Gobernanza')}
        </h3>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <Link
            to="/admin/agents"
            className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-800 hover:bg-brand-50 dark:hover:bg-slate-700/60 border border-slate-200 dark:border-slate-700 hover:border-brand-300 dark:hover:border-brand-500/50 transition-all text-left group"
          >
            <Users className="w-5 h-5 text-brand-600 dark:text-brand-400 mb-2 group-hover:scale-110 transition-transform" />
            <h4 className="font-bold text-xs text-slate-900 dark:text-white">{t('admin.manageAgents', 'Gestión de Agentes')}</h4>
            <p className="text-[11px] text-slate-500 dark:text-slate-400 mt-0.5">{t('admin.agentsDesc', 'Activar, inactivar y reasignar cartera')}</p>
          </Link>

          <Link
            to="/admin/users"
            className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-800 hover:bg-indigo-50 dark:hover:bg-slate-700/60 border border-slate-200 dark:border-slate-700 hover:border-indigo-300 dark:hover:border-indigo-500/50 transition-all text-left group"
          >
            <Code className="w-5 h-5 text-royal-600 dark:text-royal-400 mb-2 group-hover:scale-110 transition-transform" />
            <h4 className="font-bold text-xs text-slate-900 dark:text-white">{t('admin.usersTitle', 'Admins & Developers')}</h4>
            <p className="text-[11px] text-slate-500 dark:text-slate-400 mt-0.5">{t('admin.usersDesc', 'Crear credenciales y accesos REST')}</p>
          </Link>

          <Link
            to="/admin/property-types"
            className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-800 hover:bg-emerald-50 dark:hover:bg-slate-700/60 border border-slate-200 dark:border-slate-700 hover:border-emerald-300 dark:hover:border-emerald-500/50 transition-all text-left group"
          >
            <Building2 className="w-5 h-5 text-emerald-600 dark:text-emerald-400 mb-2 group-hover:scale-110 transition-transform" />
            <h4 className="font-bold text-xs text-slate-900 dark:text-white">{t('admin.propertyTypesTitle', 'Tipos de Inmuebles')}</h4>
            <p className="text-[11px] text-slate-500 dark:text-slate-400 mt-0.5">{t('admin.propertyTypesDesc', 'Mantenimiento y conteo')}</p>
          </Link>

          <Link
            to="/admin/improvements"
            className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-800 hover:bg-amber-50 dark:hover:bg-slate-700/60 border border-slate-200 dark:border-slate-700 hover:border-amber-300 dark:hover:border-amber-500/50 transition-all text-left group"
          >
            <Tag className="w-5 h-5 text-amber-600 dark:text-amber-400 mb-2 group-hover:scale-110 transition-transform" />
            <h4 className="font-bold text-xs text-slate-900 dark:text-white">{t('admin.improvementsTitle', 'Amenidades y Mejoras')}</h4>
            <p className="text-[11px] text-slate-500 dark:text-slate-400 mt-0.5">{t('admin.improvementsDesc', 'Catálogo de amenidades')}</p>
          </Link>
        </div>
      </div>
    </div>
  );
};
