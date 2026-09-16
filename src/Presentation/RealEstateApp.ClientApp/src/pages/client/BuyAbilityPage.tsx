import React, { useState, useEffect } from 'react';
import { BuyAbilityResult, BuyAbilityRequest } from '../../types';
import { buyAbilityService } from '../../api/services';
import { formatCurrencyRD } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import {
  Wallet,
  Calculator,
  CheckCircle2,
  XCircle,
  MinusCircle,
  Landmark,
  Home,
  TrendingUp,
  Percent,
  Clock,
} from 'lucide-react';

const emptyForm: BuyAbilityRequest = {
  monthlyGrossIncome: 0,
  monthlyNetIncome: 0,
  monthlyDebtPayments: 0,
  availableDownPayment: 0,
  currency: 'DOP',
};

const resultStyles: Record<string, { label: string; className: string; Icon: React.ElementType }> = {
  'Aprobado': { label: 'Aprobado', className: 'bg-emerald-50 text-emerald-700 border-emerald-200', Icon: CheckCircle2 },
  'Pre-Aprobado': { label: 'Pre-Aprobado', className: 'bg-amber-50 text-amber-700 border-amber-200', Icon: MinusCircle },
  'No Aprobado': { label: 'No Aprobado', className: 'bg-rose-50 text-rose-700 border-rose-200', Icon: XCircle },
};

export const BuyAbilityPage: React.FC = () => {
  const [form, setForm] = useState<BuyAbilityRequest>(emptyForm);
  const [result, setResult] = useState<BuyAbilityResult | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isEvaluating, setIsEvaluating] = useState<boolean>(false);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    const load = async () => {
      try {
        const last = await buyAbilityService.getLast();
        if (last) setResult(last);
      } catch {
        // ignore (no prior evaluation or not authorized)
      } finally {
        setIsLoading(false);
      }
    };
    Promise.resolve().then(() => load()).catch(console.error);
  }, []);

  const handleChange = (field: keyof BuyAbilityRequest) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [field]: Number(e.target.value) || 0 });
  };

  const handleEvaluate = async () => {
    if (form.monthlyGrossIncome <= 0 || form.monthlyNetIncome <= 0) {
      setError('Por favor completa tus ingresos mensuales (bruto y neto).');
      return;
    }
    setIsEvaluating(true);
    setError('');
    try {
      const res = await buyAbilityService.evaluate(form);
      setResult(res);
    } catch (err: unknown) {
      const message = err && typeof err === 'object' && 'response' in err
        ? (err as { response?: { data?: { error?: string } } }).response?.data?.error
        : undefined;
      setError(message || 'No se pudo evaluar tu capacidad de compra.');
    } finally {
      setIsEvaluating(false);
    }
  };

  if (isLoading) {
    return <Loader text="Cargando tu perfil financiero..." size="lg" />;
  }

  const cfg = result ? resultStyles[result.evaluationResult] || resultStyles['No Aprobado'] : null;

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="rounded-3xl bg-gradient-to-r from-navy-950 to-slate-900 p-8 text-white flex flex-wrap items-center justify-between gap-4 shadow-lg">
        <div className="space-y-1">
          <span className="text-[11px] font-extrabold uppercase tracking-widest text-brand-400 flex items-center gap-1.5">
            <Wallet className="w-4 h-4" /> F-09
          </span>
          <h1 className="text-2xl font-extrabold">Mi Capacidad de Compra</h1>
          <p className="text-xs text-slate-300 max-w-xl">
            Calcula cuánto puedes financiar y qué rango de propiedades está a tu alcance según tu perfil financiero.
          </p>
        </div>
        <div className="text-4xl">
          <Calculator className="text-brand-400 w-14 h-14" />
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Form */}
        <div className="bg-white rounded-3xl p-6 border border-slate-200 shadow-xs space-y-5">
          <div className="flex items-center gap-2">
            <Landmark className="w-5 h-5 text-brand-600" />
            <h2 className="font-extrabold text-slate-900">Perfil Financiero</h2>
          </div>

          <div className="space-y-1.5">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Ingreso Bruto Mensual (RD$)</label>
            <input
              type="number"
              min={0}
              value={form.monthlyGrossIncome || ''}
              onChange={handleChange('monthlyGrossIncome')}
              className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
              placeholder="0"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Ingreso Neto Mensual (RD$)</label>
            <input
              type="number"
              min={0}
              value={form.monthlyNetIncome || ''}
              onChange={handleChange('monthlyNetIncome')}
              className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
              placeholder="0"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Pagos de Deudas Mensuales (RD$)</label>
            <input
              type="number"
              min={0}
              value={form.monthlyDebtPayments || ''}
              onChange={handleChange('monthlyDebtPayments')}
              className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
              placeholder="0"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Pie / Pago Inicial Disponible (RD$)</label>
            <input
              type="number"
              min={0}
              value={form.availableDownPayment || ''}
              onChange={handleChange('availableDownPayment')}
              className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
              placeholder="0"
            />
          </div>

          {error && (
            <div className="p-3.5 bg-rose-50 border border-rose-200 rounded-xl text-xs font-semibold text-rose-700">
              {error}
            </div>
          )}

          <button
            onClick={handleEvaluate}
            disabled={isEvaluating}
            className="w-full py-3 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 disabled:cursor-not-allowed text-white font-extrabold text-xs rounded-2xl shadow-lg shadow-brand-600/30 flex items-center justify-center gap-2 transition-all"
          >
            <Calculator className="w-4 h-4" />
            {isEvaluating ? 'Evaluando...' : 'Evaluar Capacidad de Compra'}
          </button>
        </div>

        {/* Results */}
        <div className="space-y-6">
          {!result ? (
            <div className="bg-white rounded-3xl p-10 border border-slate-200 shadow-xs text-center space-y-3">
              <Wallet className="w-10 h-10 text-brand-300 mx-auto" />
              <p className="text-sm font-bold text-slate-700">Aún no has evaluado tu capacidad</p>
              <p className="text-xs text-slate-500">
                Completa el formulario con tu perfil financiero y presiona «Evaluar Capacidad de Compra» para ver tu rango de vivienda.
              </p>
            </div>
          ) : (
            <>
              {/* Result banner */}
              <div className={`rounded-3xl p-8 border shadow-lg text-center space-y-2 ${cfg!.className}`}>
                <cfg.Icon className="w-12 h-12 mx-auto" />
                <h2 className="text-2xl font-extrabold uppercase tracking-wide">{cfg!.label}</h2>
                <p className="text-sm font-semibold">
                  {formatCurrencyRD(result.maxPropertyPrice)}
                </p>
                <p className="text-[11px] font-bold opacity-80">Precio máximo de propiedad que puedes financiar</p>
              </div>

              {/* Key stats */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="bg-white p-5 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
                  <div className="w-12 h-12 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
                    <Home className="w-6 h-6" />
                  </div>
                  <div>
                    <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-400">Máx. Hipoteca</p>
                    <p className="text-lg font-extrabold font-mono text-slate-900">{formatCurrencyRD(result.maxMortgageAmount)}</p>
                  </div>
                </div>
                <div className="bg-white p-5 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
                  <div className="w-12 h-12 rounded-2xl bg-indigo-50 text-indigo-600 flex items-center justify-center">
                    <Clock className="w-6 h-6" />
                  </div>
                  <div>
                    <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-400">Cuota Mensual Máx.</p>
                    <p className="text-lg font-extrabold font-mono text-slate-900">{formatCurrencyRD(result.maxMonthlyPayment)}</p>
                  </div>
                </div>
                <div className="bg-white p-5 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
                  <div className="w-12 h-12 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center">
                    <Percent className="w-6 h-6" />
                  </div>
                  <div>
                    <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-400">Deuda / Ingreso</p>
                    <p className="text-lg font-extrabold text-slate-900">{(result.debtToIncomeRatio * 100).toFixed(1)}%</p>
                  </div>
                </div>
                <div className="bg-white p-5 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
                  <div className="w-12 h-12 rounded-2xl bg-brand-50 text-brand-600 flex items-center justify-center">
                    <TrendingUp className="w-6 h-6" />
                  </div>
                  <div>
                    <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-400">Tasa Anual / Plazo</p>
                    <p className="text-lg font-extrabold text-slate-900">
                      {result.estimatedAnnualRate}% · {result.recommendedTermYears} años
                    </p>
                  </div>
                </div>
              </div>

              {result.observations && (
                <div className="bg-sky-50 border border-sky-200 rounded-2xl p-4 text-xs font-semibold text-sky-800">
                  {result.observations}
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};
