import React, { useState, useEffect } from 'react';
import { subscriptionsService } from '../../api/services';
import { SubscriptionPlan, AgentSubscription } from '../../types';
import { formatCurrencyRD } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import { Award, Check, Zap, Sparkles, CreditCard, ShieldCheck } from 'lucide-react';

export const AgentSubscriptionPage: React.FC = () => {
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [currentSub, setCurrentSub] = useState<AgentSubscription | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isUpgrading, setIsUpgrading] = useState(false);
  const [selectedPlanId, setSelectedPlanId] = useState<number | null>(null);
  const [showCheckoutModal, setShowCheckoutModal] = useState(false);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  const loadData = async () => {
    try {
      const [plansData, subData] = await Promise.all([
        subscriptionsService.getPlans(),
        subscriptionsService.getMySubscription(),
      ]);
      setPlans(plansData);
      setCurrentSub(subData);
    } catch (err) {
      console.error("Error loading subscriptions:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadData()).catch(console.error);
  }, []);

  const handleSelectPlan = (planId: number) => {
    setSelectedPlanId(planId);
    setShowCheckoutModal(true);
  };

  const handleConfirmUpgrade = async () => {
    if (!selectedPlanId) return;
    try {
      setIsUpgrading(true);
      await subscriptionsService.upgrade(selectedPlanId, "mock_pm_card_rd");
      setSuccessMsg("¡Plan actualizado y activado exitosamente!");
      setShowCheckoutModal(false);
      await loadData();
    } catch (err) {
      console.error("Error upgrading plan:", err);
    } finally {
      setIsUpgrading(false);
    }
  };

  if (isLoading) return <Loader text="Cargando planes de membresía..." />;

  const selectedPlan = plans.find((p) => p.id === selectedPlanId);

  return (
    <div className="max-w-5xl mx-auto space-y-8 pb-16">
      
      <div className="text-center max-w-2xl mx-auto space-y-2">
        <span className="text-xs font-extrabold uppercase tracking-widest text-brand-600 bg-brand-50 px-3 py-1 rounded-full">
          Planes y Membresías
        </span>
        <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">
          Impulsa tus Ventas Inmobiliarias
        </h1>
        <p className="text-xs sm:text-sm text-slate-500">
          Elige el plan ideal para destacar más propiedades, recibir clientes prioritarios y maximizar tus cierres.
        </p>
      </div>

      {currentSub && (
        <div className="bg-gradient-to-r from-brand-900 to-navy-950 text-white p-6 rounded-3xl shadow-xl flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
          <div>
            <span className="text-xs text-brand-200 font-bold uppercase tracking-wider">Tu Plan Activo</span>
            <h3 className="text-xl font-extrabold text-white mt-0.5">{currentSub.planName || 'Plan Gratuito'}</h3>
            <p className="text-xs text-slate-300 mt-1">
              Destacados permitidos: <strong>{currentSub.maxFeaturedProperties || 0} inmuebles</strong> | Vence: {new Date(currentSub.endDate).toLocaleDateString()}
            </p>
          </div>
          <span className="px-3.5 py-1.5 bg-emerald-500 text-white font-extrabold text-xs rounded-full shadow-xs flex items-center gap-1.5">
            <Check className="w-3.5 h-3.5" />
            <span>Membresía Activa</span>
          </span>
        </div>
      )}

      {successMsg && (
        <div className="p-4 bg-emerald-50 text-emerald-800 text-xs font-bold rounded-2xl border border-emerald-200">
          {successMsg}
        </div>
      )}

      {/* Plans Pricing Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {plans.map((plan) => {
          const isCurrent = currentSub?.subscriptionPlanId === plan.id;
          const isPro = plan.name.toLowerCase().includes('pro') || plan.name.toLowerCase().includes('profesional');

          return (
            <div
              key={plan.id}
              className={`rounded-3xl p-6 sm:p-8 flex flex-col justify-between transition-all relative ${
                isPro
                  ? 'bg-gradient-to-b from-brand-950 to-navy-950 text-white ring-2 ring-brand-500 shadow-2xl scale-105'
                  : 'bg-white text-slate-900 border border-slate-200 shadow-sm'
              }`}
            >
              {isPro && (
                <div className="absolute -top-3 left-1/2 -translate-x-1/2 bg-gradient-to-r from-amber-500 to-amber-600 text-navy-950 font-extrabold text-[10px] tracking-wider uppercase px-3 py-1 rounded-full shadow-md flex items-center gap-1">
                  <Sparkles className="w-3 h-3" />
                  <span>Más Popular</span>
                </div>
              )}

              <div className="space-y-4">
                <div>
                  <h3 className="font-extrabold text-lg tracking-tight">{plan.name}</h3>
                  <p className={`text-xs mt-1 ${isPro ? 'text-slate-300' : 'text-slate-500'}`}>{plan.description}</p>
                </div>

                <div className="pt-2">
                  <span className="text-3xl font-extrabold font-mono tracking-tight">
                    {plan.monthlyPrice === 0 ? 'Gratis' : formatCurrencyRD(plan.monthlyPrice)}
                  </span>
                  {plan.monthlyPrice > 0 && (
                    <span className={`text-xs font-medium ml-1.5 ${isPro ? 'text-slate-400' : 'text-slate-500'}`}>/mes</span>
                  )}
                </div>

                <div className="space-y-2.5 pt-4 border-t border-slate-100/20 text-xs">
                  <div className="flex items-center gap-2 font-bold">
                    <Check className={`w-4 h-4 ${isPro ? 'text-amber-400' : 'text-brand-600'}`} />
                    <span>Hasta {plan.maxFeaturedProperties} inmuebles destacados</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Check className={`w-4 h-4 ${isPro ? 'text-amber-400' : 'text-brand-600'}`} />
                    <span>Insignia de Corredor Certificado</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Check className={`w-4 h-4 ${isPro ? 'text-amber-400' : 'text-brand-600'}`} />
                    <span>Chat SignalR y WhatsApp directo</span>
                  </div>
                  {isPro && (
                    <div className="flex items-center gap-2 font-semibold text-amber-300">
                      <Zap className="w-4 h-4 text-amber-400" />
                      <span>Prioridad en resultados de búsqueda</span>
                    </div>
                  )}
                </div>
              </div>

              <div className="pt-8">
                {isCurrent ? (
                  <button
                    disabled
                    className="w-full py-2.5 bg-slate-200 text-slate-600 rounded-xl font-bold text-xs cursor-default"
                  >
                    Plan Actual
                  </button>
                ) : (
                  <button
                    onClick={() => handleSelectPlan(plan.id)}
                    className={`w-full py-2.5 rounded-xl font-extrabold text-xs transition-all shadow-md ${
                      isPro
                        ? 'bg-amber-500 hover:bg-amber-600 text-navy-950'
                        : 'bg-brand-600 hover:bg-brand-700 text-white'
                    }`}
                  >
                    {plan.monthlyPrice === 0 ? 'Seleccionar Plan' : 'Mejorar a este Plan'}
                  </button>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {/* Checkout Modal */}
      {showCheckoutModal && selectedPlan && (
        <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl p-6 sm:p-8 max-w-md w-full shadow-2xl space-y-6">
            <div className="flex justify-between items-center pb-3 border-b border-slate-100">
              <h3 className="font-extrabold text-lg text-slate-900 flex items-center gap-2">
                <CreditCard className="w-5 h-5 text-brand-600" />
                Confirmar Suscripción
              </h3>
              <button
                onClick={() => setShowCheckoutModal(false)}
                className="text-slate-400 hover:text-slate-600 text-sm font-bold"
              >
                ✕
              </button>
            </div>

            <div className="p-4 bg-slate-50 rounded-2xl space-y-2 border border-slate-200">
              <div className="flex justify-between text-xs text-slate-600">
                <span>Plan seleccionado:</span>
                <strong className="text-slate-900">{selectedPlan.name}</strong>
              </div>
              <div className="flex justify-between text-xs text-slate-600">
                <span>Inmuebles destacados:</span>
                <strong className="text-slate-900">{selectedPlan.maxFeaturedProperties}</strong>
              </div>
              <div className="flex justify-between text-sm font-extrabold pt-2 border-t border-slate-200 text-brand-600">
                <span>Total a facturar:</span>
                <span className="font-mono">{formatCurrencyRD(selectedPlan.monthlyPrice)} / mes</span>
              </div>
            </div>

            <div className="p-3 bg-amber-50 rounded-xl border border-amber-200 text-[11px] text-amber-800 flex items-center gap-2">
              <ShieldCheck className="w-4 h-4 text-amber-600 shrink-0" />
              <span>Transacción cifrada y protegida por pasarela de pago segura.</span>
            </div>

            <div className="flex gap-2">
              <button
                onClick={() => setShowCheckoutModal(false)}
                className="flex-1 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-xs font-bold"
              >
                Cancelar
              </button>
              <button
                onClick={handleConfirmUpgrade}
                disabled={isUpgrading}
                className="flex-1 py-2.5 bg-brand-600 hover:bg-brand-700 text-white rounded-xl text-xs font-extrabold shadow-md"
              >
                {isUpgrading ? 'Procesando...' : 'Confirmar y Pagar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
