import React, { useState, useEffect } from 'react';
import { subscriptionsService } from '../../api/services';
import { SubscriptionPlan } from '../../types';
import { formatCurrencyRD } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import { Award, Zap, CheckCircle2, TrendingUp, Users } from 'lucide-react';

export const ManageSubscriptionsPage: React.FC = () => {
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadPlans = async () => {
      try {
        setIsLoading(true);
        const data = await subscriptionsService.getPlans();
        setPlans(data || []);
      } catch (err) {
        console.error("Error loading subscription plans:", err);
      } finally {
        setIsLoading(false);
      }
    };
    loadPlans();
  }, []);

  if (isLoading) return <Loader text="Cargando planes de suscripción..." />;

  return (
    <div className="space-y-8 pb-16">
      
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Award className="w-7 h-7 text-brand-600" />
            Membresías y Planes de Agentes
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Configuración de tarifas, límites de inmuebles destacados y beneficios para corredores asociados.
          </p>
        </div>
      </div>

      {/* KPI summary */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs space-y-1">
          <span className="text-xs font-bold text-slate-400 uppercase">Planes Activos</span>
          <p className="text-2xl font-extrabold font-mono text-slate-900">{plans.length}</p>
        </div>
        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs space-y-1">
          <span className="text-xs font-bold text-slate-400 uppercase">Modelo de Ingresos</span>
          <p className="text-2xl font-extrabold font-mono text-emerald-600">SaaS Recurrente</p>
        </div>
        <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs space-y-1">
          <span className="text-xs font-bold text-slate-400 uppercase">Moneda Base</span>
          <p className="text-2xl font-extrabold font-mono text-brand-600">Pesos Dominicanos (RD$)</p>
        </div>
      </div>

      {/* Plans List Table */}
      <div className="bg-white rounded-3xl border border-slate-200 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-extrabold uppercase tracking-wider">
              <tr>
                <th className="px-6 py-4">Nombre del Plan</th>
                <th className="px-6 py-4">Descripción</th>
                <th className="px-6 py-4">Precio Mensual</th>
                <th className="px-6 py-4">Inmuebles Destacados</th>
                <th className="px-6 py-4">Beneficios Incluidos</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {plans.map((plan) => (
                <tr key={plan.id} className="hover:bg-slate-50/50">
                  <td className="px-6 py-4 font-bold text-slate-900 text-sm">
                    {plan.name}
                  </td>
                  <td className="px-6 py-4 text-slate-500 max-w-xs">
                    {plan.description}
                  </td>
                  <td className="px-6 py-4 font-mono font-bold text-slate-900 text-sm">
                    {plan.monthlyPrice === 0 ? 'Gratuito' : formatCurrencyRD(plan.monthlyPrice)}
                  </td>
                  <td className="px-6 py-4 font-mono font-bold text-amber-600">
                    Hasta {plan.maxFeaturedProperties} propiedades
                  </td>
                  <td className="px-6 py-4 space-y-1">
                    <span className="inline-flex items-center gap-1 text-[11px] font-bold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-md">
                      <CheckCircle2 className="w-3 h-3" /> Verificación KYC
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
