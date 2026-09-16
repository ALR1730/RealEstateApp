import React, { useState, useEffect } from 'react';
import { subscriptionsService } from '../../api/services';
import { SubscriptionPlan, AgentSubscription } from '../../types';
import { useCurrency } from '../../context/CurrencyContext';
import { Loader } from '../../components/common/Loader';
import { Award, Check, Zap, Sparkles, CreditCard, Lock } from 'lucide-react';

export const AgentSubscriptionPage: React.FC = () => {
  const { formatPrice } = useCurrency();
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [currentSub, setCurrentSub] = useState<AgentSubscription | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isUpgrading, setIsUpgrading] = useState(false);
  const [selectedPlanId, setSelectedPlanId] = useState<number | null>(null);
  const [showCheckoutModal, setShowCheckoutModal] = useState(false);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  // B6: Mock Payment Form State
  const [cardHolder, setCardHolder] = useState('');
  const [cardNumber, setCardNumber] = useState('');
  const [cardExpiry, setCardExpiry] = useState('');
  const [cardCvc, setCardCvc] = useState('');
  const [cardError, setCardError] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;
    const fetchSubscriptions = async () => {
      try {
        const [plansData, subData] = await Promise.all([
          subscriptionsService.getPlans(),
          subscriptionsService.getMySubscription(),
        ]);
        if (isMounted) {
          setPlans(plansData);
          setCurrentSub(subData);
        }
      } catch (err) {
        console.error("Error loading subscriptions:", err);
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    fetchSubscriptions();
    return () => {
      isMounted = false;
    };
  }, [refreshKey]);

  const handleSelectPlan = (planId: number) => {
    setSelectedPlanId(planId);
    setCardHolder('');
    setCardNumber('');
    setCardExpiry('');
    setCardCvc('');
    setCardError(null);
    setShowCheckoutModal(true);
  };

  const handleCardNumberChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const raw = e.target.value.replace(/\D/g, '').slice(0, 16);
    const formatted = raw.replace(/(\d{4})(?=\d)/g, '$1 ');
    setCardNumber(formatted);
  };

  const handleExpiryChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    let raw = e.target.value.replace(/\D/g, '').slice(0, 4);
    if (raw.length >= 3) {
      raw = raw.slice(0, 2) + '/' + raw.slice(2);
    }
    setCardExpiry(raw);
  };

  const handleConfirmUpgrade = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedPlanId) return;

    const cleanCard = cardNumber.replace(/\s/g, '');
    if (cleanCard.length < 15) {
      setCardError("Ingresa un número de tarjeta válido (15-16 dígitos).");
      return;
    }
    if (cardExpiry.length < 5) {
      setCardError("Ingresa una fecha de expiración válida (MM/AA).");
      return;
    }
    if (cardCvc.length < 3) {
      setCardError("Ingresa un código CVC válido.");
      return;
    }
    if (!cardHolder.trim()) {
      setCardError("Ingresa el nombre del titular como figura en la tarjeta.");
      return;
    }

    try {
      setIsUpgrading(true);
      setCardError(null);
      const mockPaymentToken = `tok_cardnet_${cleanCard.slice(-4)}_${Date.now()}`;
      await subscriptionsService.upgrade(selectedPlanId, mockPaymentToken);
      setSuccessMsg("¡Plan actualizado y activado exitosamente! Tu límite de inmuebles destacados ha sido ampliado.");
      setShowCheckoutModal(false);
      setRefreshKey((k) => k + 1);
    } catch (err: unknown) {
      console.error("Error upgrading plan:", err);
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setCardError(msg || "Error al procesar el pago con la pasarela.");
    } finally {
      setIsUpgrading(false);
    }
  };

  if (isLoading) return <Loader text="Cargando planes de membresía..." />;

  const selectedPlan = plans.find((p) => p.id === selectedPlanId);

  return (
    <div className="max-w-5xl mx-auto space-y-8 pb-16">
      
      <div className="text-center max-w-2xl mx-auto space-y-2">
        <span className="text-xs font-extrabold uppercase tracking-widest text-brand-600 dark:text-brand-400 bg-brand-50 dark:bg-brand-950/60 px-3 py-1 rounded-full">
          Planes y Membresías
        </span>
        <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white tracking-tight">
          Impulsa tus Ventas Inmobiliarias
        </h1>
        <p className="text-xs sm:text-sm text-slate-500 dark:text-slate-400">
          Elige el plan ideal para destacar más propiedades, recibir clientes prioritarios y maximizar tus cierres.
        </p>
      </div>

      {currentSub && (
        <div className="bg-gradient-to-r from-brand-900 to-navy-950 text-white p-6 rounded-3xl shadow-xl flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 border dark:border-slate-800">
          <div>
            <span className="text-xs text-brand-200 font-bold uppercase tracking-wider">Tu Plan Activo</span>
            <h3 className="text-xl font-extrabold text-white mt-0.5">{currentSub.planName || 'Plan Gratuito'}</h3>
            <p className="text-xs text-slate-300 mt-1">
              Destacados permitidos: <strong>{currentSub.maxFeaturedProperties || 0} inmuebles</strong> | Vence: {new Date(currentSub.endDate).toLocaleDateString()}
            </p>
          </div>
          <span className="px-3.5 py-1.5 bg-emerald-500 text-white font-extrabold text-xs rounded-full shadow-sm flex items-center gap-1.5">
            <Award className="w-4 h-4" />
            <span>Membresía Activa</span>
          </span>
        </div>
      )}

      {successMsg && (
        <div className="p-4 bg-emerald-50 dark:bg-emerald-950/40 text-emerald-800 dark:text-emerald-300 text-xs font-bold rounded-2xl border border-emerald-200 dark:border-emerald-800/60">
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
                  : 'bg-white dark:bg-slate-900 text-slate-900 dark:text-white border border-slate-200 dark:border-slate-800 shadow-sm'
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
                  <p className={`text-xs mt-1 ${isPro ? 'text-slate-300' : 'text-slate-500 dark:text-slate-400'}`}>{plan.description}</p>
                </div>

                <div className="pt-2">
                  <span className="text-3xl font-extrabold font-mono tracking-tight">
                    {plan.monthlyPrice === 0 ? 'Gratis' : formatPrice(plan.monthlyPrice)}
                  </span>
                  {plan.monthlyPrice > 0 && (
                    <span className={`text-xs font-medium ml-1.5 ${isPro ? 'text-slate-400' : 'text-slate-500 dark:text-slate-400'}`}>/mes</span>
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
                    <span>Chat SignalR y mensajería directa</span>
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
                    className="w-full py-2.5 bg-slate-200 dark:bg-slate-800 text-slate-600 dark:text-slate-400 rounded-xl font-bold text-xs cursor-default"
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

      {/* B6: Interactive Checkout Modal with Card Fields */}
      {showCheckoutModal && selectedPlan && (
        <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 sm:p-8 max-w-md w-full shadow-2xl space-y-5 border border-slate-100 dark:border-slate-800 animate-in zoom-in-95">
            <div className="flex justify-between items-center pb-3 border-b border-slate-100 dark:border-slate-800">
              <h3 className="font-extrabold text-lg text-slate-900 dark:text-white flex items-center gap-2">
                <CreditCard className="w-5 h-5 text-brand-600 dark:text-brand-400" />
                Pasarela de Pago Segura
              </h3>
              <button
                onClick={() => setShowCheckoutModal(false)}
                className="text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 text-sm font-bold"
              >
                ✕
              </button>
            </div>

            <div className="p-4 bg-slate-50 dark:bg-slate-800/60 rounded-2xl space-y-2 border border-slate-200 dark:border-slate-700">
              <div className="flex justify-between text-xs text-slate-600 dark:text-slate-300">
                <span>Plan seleccionado:</span>
                <strong className="text-slate-900 dark:text-white">{selectedPlan.name}</strong>
              </div>
              <div className="flex justify-between text-xs text-slate-600 dark:text-slate-300">
                <span>Inmuebles destacados:</span>
                <strong className="text-slate-900 dark:text-white">{selectedPlan.maxFeaturedProperties}</strong>
              </div>
              <div className="flex justify-between text-sm font-extrabold pt-2 border-t border-slate-200 dark:border-slate-700 text-brand-600 dark:text-brand-400">
                <span>Total mensual:</span>
                <span className="font-mono">{formatPrice(selectedPlan.monthlyPrice)} / mes</span>
              </div>
            </div>

            {/* Credit Card Interactive Form */}
            <form onSubmit={handleConfirmUpgrade} className="space-y-3">
              <div>
                <label className="block text-[11px] font-bold text-slate-600 dark:text-slate-300 mb-1">
                  Titular de la Tarjeta
                </label>
                <input
                  type="text"
                  required
                  placeholder="Ej. Juan Pérez"
                  value={cardHolder}
                  onChange={(e) => setCardHolder(e.target.value)}
                  className="w-full px-3.5 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
                />
              </div>

              <div>
                <label className="block text-[11px] font-bold text-slate-600 dark:text-slate-300 mb-1">
                  Número de Tarjeta (Visa, Mastercard)
                </label>
                <input
                  type="text"
                  required
                  placeholder="4000 1234 5678 9010"
                  value={cardNumber}
                  onChange={handleCardNumberChange}
                  className="w-full px-3.5 py-2 text-xs font-mono rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-[11px] font-bold text-slate-600 dark:text-slate-300 mb-1">
                    Vencimiento (MM/AA)
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="12/28"
                    value={cardExpiry}
                    onChange={handleExpiryChange}
                    className="w-full px-3.5 py-2 text-xs font-mono rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
                  />
                </div>
                <div>
                  <label className="block text-[11px] font-bold text-slate-600 dark:text-slate-300 mb-1">
                    CVC / CVV
                  </label>
                  <input
                    type="password"
                    required
                    maxLength={4}
                    placeholder="123"
                    value={cardCvc}
                    onChange={(e) => setCardCvc(e.target.value.replace(/\D/g, '').slice(0, 4))}
                    className="w-full px-3.5 py-2 text-xs font-mono rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
                  />
                </div>
              </div>

              {cardError && (
                <p className="text-xs font-semibold text-rose-600 dark:text-rose-400 bg-rose-50 dark:bg-rose-950/40 p-2.5 rounded-xl border border-rose-200 dark:border-rose-800/60">
                  {cardError}
                </p>
              )}

              <div className="p-3 bg-slate-50 dark:bg-slate-800/40 rounded-xl border border-slate-200 dark:border-slate-700 text-[11px] text-slate-600 dark:text-slate-400 flex items-center gap-2">
                <Lock className="w-4 h-4 text-emerald-600 shrink-0" />
                <span>Simulación de pasarela segura CardNet / Azul (RD$).</span>
              </div>

              <div className="flex gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowCheckoutModal(false)}
                  className="flex-1 py-2.5 bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-300 rounded-xl text-xs font-bold transition-colors"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={isUpgrading}
                  className="flex-1 py-2.5 bg-brand-600 hover:bg-brand-700 text-white rounded-xl text-xs font-extrabold shadow-md transition-all flex items-center justify-center gap-1.5"
                >
                  {isUpgrading ? 'Procesando Pago...' : 'Pagar y Activar'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
