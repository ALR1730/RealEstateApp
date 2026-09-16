import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Loader } from '../../components/common/Loader';
import { catalogsService } from '../../api/services';
import {
  Code,
  Building,
  Layers,
  Sparkles,
  ArrowUpRight,
  KeyRound,
  FileJson,
  Globe
} from 'lucide-react';

export const DeveloperDashboard: React.FC = () => {
  const [counts, setCounts] = useState<{ propertyTypes: number; saleTypes: number; improvements: number } | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const [pt, st, imp] = await Promise.all([
          catalogsService.getPropertyTypes(),
          catalogsService.getSaleTypes(),
          catalogsService.getImprovements(),
        ]);
        setCounts({ propertyTypes: pt.length, saleTypes: st.length, improvements: imp.length });
      } catch (err) {
        console.error('Error cargando catálogos:', err);
      } finally {
        setIsLoading(false);
      }
    })();
  }, []);

  if (isLoading) {
    return <Loader text="Cargando portal de desarrollo..." />;
  }

  const crudCards = [
    {
      to: '/developer/property-types',
      icon: Building,
      title: 'Tipos de Propiedad',
      desc: 'Lista maestra de tipos de inmueble usados por la API.',
      count: counts?.propertyTypes ?? 0,
      accent: 'from-royal-600 to-indigo-600',
    },
    {
      to: '/developer/sale-types',
      icon: Layers,
      title: 'Tipos de Venta',
      desc: 'Modalidades de operación (venta, alquiler, etc.).',
      count: counts?.saleTypes ?? 0,
      accent: 'from-emerald-600 to-teal-600',
    },
    {
      to: '/developer/improvements',
      icon: Sparkles,
      title: 'Amenidades / Mejoras',
      desc: 'Atributos y mejoras disponibles en las propiedades.',
      count: counts?.improvements ?? 0,
      accent: 'from-amber-600 to-orange-600',
    },
  ];

  const endpoints = [
    { method: 'GET', path: '/api/v1/propertytypes', roles: 'Público' },
    { method: 'POST/PUT/DELETE', path: '/api/v1/propertytypes', roles: 'Admin, Developer' },
    { method: 'GET', path: '/api/v1/saletypes', roles: 'Público' },
    { method: 'POST/PUT/DELETE', path: '/api/v1/saletypes', roles: 'Admin, Developer' },
    { method: 'GET', path: '/api/v1/improvements', roles: 'Público' },
    { method: 'POST/PUT/DELETE', path: '/api/v1/improvements', roles: 'Admin, Developer' },
    { method: 'GET', path: '/api/v1/provinces', roles: 'Público' },
    { method: 'GET', path: '/api/v1/subscriptions/plans', roles: 'Público' },
  ];

  return (
    <div className="space-y-8 pb-12">

      <div className="bg-gradient-to-r from-slate-900 to-navy-950 text-white p-6 sm:p-8 rounded-3xl shadow-xl flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <span className="text-royal-400 text-xs font-extrabold uppercase tracking-widest flex items-center gap-2">
            <Code className="w-4 h-4" /> Developer Portal
          </span>
          <h1 className="text-2xl sm:text-3xl font-extrabold tracking-tight mt-1">
            Panel de Desarrollo y Catálogos
          </h1>
          <p className="text-xs text-slate-300 mt-1">
            Gestione los catálogos maestros que alimentan la API pública de inmuebles.
          </p>
        </div>
        <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-slate-800/80 text-[11px] font-bold text-slate-200">
          <KeyRound className="w-4 h-4 text-royal-400" />
          Roles: Admin, Developer
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
        {crudCards.map((c) => (
          <Link
            key={c.to}
            to={c.to}
            className="group bg-white rounded-3xl border border-slate-200 p-5 shadow-sm hover:shadow-lg hover:-translate-y-0.5 transition-all space-y-3"
          >
            <div className={`w-11 h-11 rounded-2xl bg-gradient-to-tr ${c.accent} text-white flex items-center justify-center`}>
              <c.icon className="w-5 h-5" />
            </div>
            <div>
              <p className="text-sm font-extrabold text-slate-900 flex items-center gap-1">
                {c.title}
                <ArrowUpRight className="w-4 h-4 text-slate-300 group-hover:text-brand-600 transition-colors" />
              </p>
              <p className="text-xs text-slate-500 mt-1 leading-relaxed">{c.desc}</p>
            </div>
            <div className="flex items-center justify-between text-[11px] font-bold">
              <span className="text-slate-400">{c.count} registros</span>
              <span className="text-brand-600">Gestionar CRUD</span>
            </div>
          </Link>
        ))}
      </div>

      <div className="bg-white rounded-3xl border border-slate-200 p-6 shadow-sm">
        <div className="flex items-center gap-2 pb-4 border-b border-slate-100">
          <FileJson className="w-5 h-5 text-royal-600" />
          <h2 className="text-sm font-extrabold text-slate-900 uppercase tracking-wider">
            Referencia rápida de la API
          </h2>
        </div>
        <div className="overflow-x-auto mt-4">
          <table className="w-full text-left text-xs">
            <thead>
              <tr className="text-[10px] uppercase tracking-wider text-slate-400 border-b border-slate-100">
                <th className="py-2 pr-4">Método</th>
                <th className="py-2 pr-4">Ruta</th>
                <th className="py-2">Acceso</th>
              </tr>
            </thead>
            <tbody>
              {endpoints.map((e) => (
                <tr key={e.path + e.method} className="border-b border-slate-50 last:border-0">
                  <td className="py-2.5 pr-4">
                    <span className={`px-2 py-1 rounded-lg font-extrabold ${
                      e.method === 'GET'
                        ? 'bg-sky-50 text-sky-700'
                        : 'bg-amber-50 text-amber-700'
                    }`}>
                      {e.method}
                    </span>
                  </td>
                  <td className="py-2.5 pr-4 font-mono text-slate-700">{e.path}</td>
                  <td className="py-2.5">
                    <span className="inline-flex items-center gap-1 text-slate-500">
                      <Globe className="w-3.5 h-3.5" /> {e.roles}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};