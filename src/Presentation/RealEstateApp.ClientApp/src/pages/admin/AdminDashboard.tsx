import React, { useState, useEffect } from 'react';
import { adminService } from '../../api/services';
import { DashboardKPIs } from '../../types';
import { Loader } from '../../components/common/Loader';
import { 
  Building2, 
  Home, 
  CheckCircle, 
  Users, 
  ShieldCheck, 
  TrendingUp, 
  PieChart, 
  Tag, 
  Code
} from 'lucide-react';
import { Link } from 'react-router-dom';

export const AdminDashboard: React.FC = () => {
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
    return <Loader text="Generando cuadro de mando ejecutivo..." />;
  }

  return (
    <div className="space-y-8 pb-12">
      
      {/* Header */}
      <div className="bg-gradient-to-r from-navy-950 to-slate-900 text-white p-6 sm:p-8 rounded-3xl shadow-xl flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <span className="text-royal-400 text-xs font-extrabold uppercase tracking-widest">
            Dirección Ejecutiva
          </span>
          <h1 className="text-2xl sm:text-3xl font-extrabold tracking-tight mt-1">
            Dashboard de KPIs y Control Global
          </h1>
          <p className="text-xs text-slate-300 mt-1">
            Métricas de inventario inmobiliario, rendimiento de agentes y usuarios en tiempo real.
          </p>
        </div>

        <div className="flex gap-2">
          <Link
            to="/admin/agents"
            className="px-4 py-2 bg-royal-600 hover:bg-royal-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
          >
            Gestión de Agentes
          </Link>
          <Link
            to="/admin/property-types"
            className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-white font-bold text-xs rounded-xl transition-all"
          >
            Catálogos Núcleo
          </Link>
        </div>
      </div>

      {/* KPI Cards Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        
        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-brand-50 text-brand-600 flex items-center justify-center font-bold">
            <Home className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Inmuebles Disponibles</span>
            <p className="text-2xl font-extrabold text-slate-900 font-mono mt-0.5">{kpis?.totalAvailableProperties || 0}</p>
          </div>
        </div>

        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center font-bold">
            <CheckCircle className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Inmuebles Vendidos</span>
            <p className="text-2xl font-extrabold text-emerald-600 font-mono mt-0.5">{kpis?.totalSoldProperties || 0}</p>
          </div>
        </div>

        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-indigo-50 text-royal-600 flex items-center justify-center font-bold">
            <Users className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Agentes Activos</span>
            <p className="text-2xl font-extrabold text-royal-600 font-mono mt-0.5">
              {kpis?.totalActiveAgents || 0}
              <span className="text-xs text-slate-400 font-normal font-sans ml-1">({kpis?.totalInactiveAgents || 0} inactivos)</span>
            </p>
          </div>
        </div>

        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center font-bold">
            <ShieldCheck className="w-6 h-6" />
          </div>
          <div>
            <span className="text-xs text-slate-500 font-semibold uppercase">Clientes Compradores</span>
            <p className="text-2xl font-extrabold text-amber-600 font-mono mt-0.5">{kpis?.totalClients || 0}</p>
          </div>
        </div>
      </div>

      {/* Distribution by Category / Types */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        
        {/* Properties by Type Breakdown */}
        <div className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex items-center justify-between pb-3 border-b border-slate-100">
            <h3 className="font-extrabold text-base text-slate-900 flex items-center gap-2">
              <PieChart className="w-5 h-5 text-royal-600" />
              Distribución por Tipo de Propiedad
            </h3>
            <span className="text-xs font-mono font-bold text-slate-500">
              Total: {kpis?.totalProperties || 0}
            </span>
          </div>

          <div className="space-y-3">
            {kpis?.propertiesByType && kpis.propertiesByType.length > 0 ? (
              kpis.propertiesByType.map((item, idx) => {
                const percent = kpis.totalProperties > 0 ? Math.round((item.count / kpis.totalProperties) * 100) : 0;
                return (
                  <div key={idx} className="space-y-1">
                    <div className="flex justify-between text-xs font-bold text-slate-700">
                      <span>{item.typeName}</span>
                      <span className="font-mono">{item.count} ({percent}%)</span>
                    </div>
                    <div className="w-full h-2.5 bg-slate-100 rounded-full overflow-hidden">
                      <div
                        className="h-full bg-brand-600 rounded-full transition-all duration-500"
                        style={{ width: `${percent}%` }}
                      />
                    </div>
                  </div>
                );
              })
            ) : (
              <p className="text-xs text-slate-400 py-6 text-center">No hay datos de distribución disponibles.</p>
            )}
          </div>
        </div>

        {/* Quick Management Shortcuts */}
        <div className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <h3 className="font-extrabold text-base text-slate-900">
            Accesos Rápidos de Gobernanza
          </h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <Link
              to="/admin/agents"
              className="p-4 rounded-2xl bg-slate-50 hover:bg-brand-50 border border-slate-200 hover:border-brand-300 transition-all text-left group"
            >
              <Users className="w-5 h-5 text-brand-600 mb-2 group-hover:scale-110 transition-transform" />
              <h4 className="font-bold text-xs text-slate-900">Gestión de Agentes</h4>
              <p className="text-[11px] text-slate-500 mt-0.5">Activar, inactivar y reasignar cartera</p>
            </Link>

            <Link
              to="/admin/users"
              className="p-4 rounded-2xl bg-slate-50 hover:bg-indigo-50 border border-slate-200 hover:border-indigo-300 transition-all text-left group"
            >
              <Code className="w-5 h-5 text-royal-600 mb-2 group-hover:scale-110 transition-transform" />
              <h4 className="font-bold text-xs text-slate-900">Admins & Developers</h4>
              <p className="text-[11px] text-slate-500 mt-0.5">Crear credenciales y accesos REST</p>
            </Link>

            <Link
              to="/admin/property-types"
              className="p-4 rounded-2xl bg-slate-50 hover:bg-emerald-50 border border-slate-200 hover:border-emerald-300 transition-all text-left group"
            >
              <Building2 className="w-5 h-5 text-emerald-600 mb-2 group-hover:scale-110 transition-transform" />
              <h4 className="font-bold text-xs text-slate-900">Tipos de Inmuebles</h4>
              <p className="text-[11px] text-slate-500 mt-0.5">Mantenimiento y conteo</p>
            </Link>

            <Link
              to="/admin/improvements"
              className="p-4 rounded-2xl bg-slate-50 hover:bg-amber-50 border border-slate-200 hover:border-amber-300 transition-all text-left group"
            >
              <Tag className="w-5 h-5 text-amber-600 mb-2 group-hover:scale-110 transition-transform" />
              <h4 className="font-bold text-xs text-slate-900">Amenidades y Mejoras</h4>
              <p className="text-[11px] text-slate-500 mt-0.5">Catálogo de amenidades</p>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};
