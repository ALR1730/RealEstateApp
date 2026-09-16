import React, { useState, useEffect, useRef } from 'react';
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
  Loader2,
  AlertCircle,
  RefreshCw,
} from 'lucide-react';

const MAX_NUMERIC_AMOUNT = 999_999_999;

const resultStyles: Record<string, { label: string; className: string; Icon: React.ElementType }> = {
  'Aprobado': { label: 'Aprobado', className: 'bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800', Icon: CheckCircle2 },
  'Pre-Aprobado': { label: 'Pre-Aprobado', className: 'bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800', Icon: MinusCircle },
  'No Aprobado': { label: 'No Aprobado', className: 'bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/40 dark:text-rose-300 dark:border-rose-800', Icon: XCircle },
};

function formatThousands(val: number | string): string {
  if (val === '' || val === null || val === undefined) return '';
  const numStr = String(val).replace(/\D/g, '');
  if (!numStr) return '';
  return numStr.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
}

export const BuyAbilityPage: React.FC = () => {
  const [form, setForm] = useState<BuyAbilityRequest>({
    monthlyGrossIncome: 0,
    monthlyNetIncome: 0,
    monthlyDebtPayments: 0,
    availableDownPayment: 0,
    currency: 'DOP',
  });

  const [rawInputs, setRawInputs] = useState<Record<string, string>>({
    monthlyGrossIncome: '',
    monthlyNetIncome: '',
    monthlyDebtPayments: '',
    availableDownPayment: '',
  });

  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [result, setResult] = useState<BuyAbilityResult | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isEvaluating, setIsEvaluating] = useState<boolean>(false);
  const [isSlowResponse, setIsSlowResponse] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const slowTimerRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const last = await buyAbilityService.getLast();
        if (last) {
          setResult(last);
          setForm({
            monthlyGrossIncome: last.monthlyGrossIncome || 0,
            monthlyNetIncome: last.monthlyNetIncome || 0,
            monthlyDebtPayments: last.monthlyDebtPayments || 0,
            availableDownPayment: last.availableDownPayment || 0,
            currency: last.currency || 'DOP',
          });
          setRawInputs({
            monthlyGrossIncome: last.monthlyGrossIncome ? formatThousands(last.monthlyGrossIncome) : '',
            monthlyNetIncome: last.monthlyNetIncome ? formatThousands(last.monthlyNetIncome) : '',
            monthlyDebtPayments: last.monthlyDebtPayments ? formatThousands(last.monthlyDebtPayments) : '',
            availableDownPayment: last.availableDownPayment ? formatThousands(last.availableDownPayment) : '',
          });
        }
      } catch {
        // ignore (no prior evaluation or not authorized)
      } finally {
        setIsLoading(false);
      }
    };
    Promise.resolve().then(() => load()).catch(console.error);

    return () => {
      if (slowTimerRef.current) clearTimeout(slowTimerRef.current);
    };
  }, []);

  const handleInputChange = (field: keyof BuyAbilityRequest) => (e: React.ChangeEvent<HTMLInputElement>) => {
    const rawVal = e.target.value;
    const cleanDigits = rawVal.replace(/\D/g, '').slice(0, 10);
    const formatted = cleanDigits ? cleanDigits.replace(/\B(?=(\d{3})+(?!\d))/g, ',') : '';
    const numericVal = cleanDigits ? Math.min(Number(cleanDigits), MAX_NUMERIC_AMOUNT) : 0;

    const newForm = { ...form, [field]: numericVal };
    const newRaw = { ...rawInputs, [field]: formatted };

    setRawInputs(newRaw);
    setForm(newForm);

    // Reactive error clearing
    setFieldErrors((prev) => {
      const updated = { ...prev };
      delete updated[field];

      // Cross-validation of gross vs net
      if (field === 'monthlyGrossIncome' || field === 'monthlyNetIncome') {
        const gross = field === 'monthlyGrossIncome' ? numericVal : newForm.monthlyGrossIncome;
        const net = field === 'monthlyNetIncome' ? numericVal : newForm.monthlyNetIncome;

        if (gross > 0 && net > 0 && net > gross) {
          updated.monthlyNetIncome = 'El ingreso neto mensual no puede ser mayor al ingreso bruto mensual.';
        } else if (updated.monthlyNetIncome === 'El ingreso neto mensual no puede ser mayor al ingreso bruto mensual.') {
          delete updated.monthlyNetIncome;
        }
      }
      return updated;
    });

    if (error) setError('');
  };

  const handleBlur = (field: keyof BuyAbilityRequest) => () => {
    const numericVal = form[field] as number;
    const errors: Record<string, string> = { ...fieldErrors };

    if (field === 'monthlyGrossIncome') {
      if (numericVal <= 0) {
        errors.monthlyGrossIncome = 'El ingreso bruto mensual es requerido y debe ser mayor a 0.';
      } else {
        delete errors.monthlyGrossIncome;
      }
    }

    if (field === 'monthlyNetIncome') {
      if (numericVal <= 0) {
        errors.monthlyNetIncome = 'El ingreso neto mensual es requerido y debe ser mayor a 0.';
      } else if (form.monthlyGrossIncome > 0 && numericVal > form.monthlyGrossIncome) {
        errors.monthlyNetIncome = 'El ingreso neto mensual no puede ser mayor al ingreso bruto mensual.';
      } else {
        delete errors.monthlyNetIncome;
      }
    }

    setFieldErrors(errors);
  };

  const validateAll = (): boolean => {
    const errors: Record<string, string> = {};

    if (form.monthlyGrossIncome <= 0) {
      errors.monthlyGrossIncome = 'El ingreso bruto mensual es requerido y debe ser mayor a 0.';
    }

    if (form.monthlyNetIncome <= 0) {
      errors.monthlyNetIncome = 'El ingreso neto mensual es requerido y debe ser mayor a 0.';
    } else if (form.monthlyGrossIncome > 0 && form.monthlyNetIncome > form.monthlyGrossIncome) {
      errors.monthlyNetIncome = 'El ingreso neto mensual no puede ser mayor al ingreso bruto mensual.';
    }

    if (form.monthlyDebtPayments < 0) {
      errors.monthlyDebtPayments = 'El total de deudas mensuales no puede ser negativo.';
    }

    if (form.availableDownPayment < 0) {
      errors.availableDownPayment = 'El pago inicial disponible no puede ser negativo.';
    }

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleEvaluate = async () => {
    if (!validateAll()) {
      setError('Por favor revisa los campos señalados con error antes de continuar.');
      return;
    }

    setIsEvaluating(true);
    setIsSlowResponse(false);
    setError('');

    // Cold start notification timer for Render or slow connections (>3.5s)
    slowTimerRef.current = setTimeout(() => {
      setIsSlowResponse(true);
    }, 3500);

    try {
      const res = await buyAbilityService.evaluate(form);
      setResult(res);
      setFieldErrors({});
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'response' in err
          ? (err as { response?: { data?: { error?: string } } }).response?.data?.error
          : undefined;
      setError(
        message ||
          'No se pudo evaluar tu capacidad de compra. Verifica tu conexión e inténtalo nuevamente.'
      );
    } finally {
      if (slowTimerRef.current) clearTimeout(slowTimerRef.current);
      setIsEvaluating(false);
      setIsSlowResponse(false);
    }
  };

  if (isLoading) {
    return <Loader text="Cargando tu perfil financiero..." size="lg" />;
  }

  const cfg = result ? resultStyles[result.evaluationResult] || resultStyles['No Aprobado'] : null;

  return (
    <div className="space-y-8 max-w-7xl mx-auto">
      {/* Header */}
      <div className="rounded-3xl bg-gradient-to-r from-navy-950 to-slate-900 p-6 sm:p-8 text-white flex flex-wrap items-center justify-between gap-4 shadow-lg">
        <div className="space-y-1">
          <span className="text-[11px] font-extrabold uppercase tracking-widest text-brand-400 flex items-center gap-1.5">
            <Wallet className="w-4 h-4" /> Capacidad Financiera
          </span>
          <h1 className="text-xl sm:text-2xl font-extrabold">Mi Capacidad de Compra</h1>
          <p className="text-xs text-slate-300 max-w-xl">
            Calcula cuánto financiamiento hipotecario puedes obtener y el rango de propiedades a tu alcance según tu perfil de ingresos y deudas.
          </p>
        </div>
        <div className="text-4xl hidden sm:block">
          <Calculator className="text-brand-400 w-12 h-12 sm:w-14 sm:h-14" />
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 lg:gap-8 items-start">
        {/* Form */}
        <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 border border-slate-200 dark:border-slate-800 shadow-xs space-y-5">
          <div className="flex items-center gap-2">
            <Landmark className="w-5 h-5 text-brand-600 dark:text-brand-400" />
            <h2 className="font-extrabold text-slate-900 dark:text-white">Perfil Financiero</h2>
          </div>

          {/* Ingreso Bruto */}
          <div className="space-y-1.5">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500 dark:text-slate-400">
              Ingreso Bruto Mensual (RD$) <span className="text-rose-500">*</span>
            </label>
            <div className="relative">
              <input
                type="text"
                inputMode="numeric"
                value={rawInputs.monthlyGrossIncome}
                onChange={handleInputChange('monthlyGrossIncome')}
                onBlur={handleBlur('monthlyGrossIncome')}
                className={`w-full px-3.5 py-2.5 rounded-xl border text-sm font-semibold focus:outline-hidden focus:ring-2 dark:bg-slate-800 dark:text-white transition-colors ${
                  fieldErrors.monthlyGrossIncome
                    ? 'border-rose-400 focus:ring-rose-400 bg-rose-50/20'
                    : 'border-slate-300 dark:border-slate-700 focus:ring-brand-500'
                }`}
                placeholder="Ej. 65,000"
              />
            </div>
            {fieldErrors.monthlyGrossIncome && (
              <p className="text-xs font-semibold text-rose-600 dark:text-rose-400 flex items-center gap-1">
                <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" />
                {fieldErrors.monthlyGrossIncome}
              </p>
            )}
          </div>

          {/* Ingreso Neto */}
          <div className="space-y-1.5">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500 dark:text-slate-400">
              Ingreso Neto Mensual (RD$) <span className="text-rose-500">*</span>
            </label>
            <div className="relative">
              <input
                type="text"
                inputMode="numeric"
                value={rawInputs.monthlyNetIncome}
                onChange={handleInputChange('monthlyNetIncome')}
                onBlur={handleBlur('monthlyNetIncome')}
                className={`w-full px-3.5 py-2.5 rounded-xl border text-sm font-semibold focus:outline-hidden focus:ring-2 dark:bg-slate-800 dark:text-white transition-colors ${
                  fieldErrors.monthlyNetIncome
                    ? 'border-rose-400 focus:ring-rose-400 bg-rose-50/20'
                    : 'border-slate-300 dark:border-slate-700 focus:ring-brand-500'
                }`}
                placeholder="Ej. 50,000"
              />
            </div>
            {fieldErrors.monthlyNetIncome && (
              <p className="text-xs font-semibold text-rose-600 dark:text-rose-400 flex items-center gap-1">
                <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" />
                {fieldErrors.monthlyNetIncome}
              </p>
            )}
          </div>

          {/* Deudas Mensuales */}
          <div className="space-y-1.5">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500 dark:text-slate-400">
              Pagos de Deudas Mensuales (RD$)
            </label>
            <div className="relative">
              <input
                type="text"
                inputMode="numeric"
                value={rawInputs.monthlyDebtPayments}
                onChange={handleInputChange('monthlyDebtPayments')}
                onBlur={handleBlur('monthlyDebtPayments')}
                className={`w-full px-3.5 py-2.5 rounded-xl border text-sm font-semibold focus:outline-hidden focus:ring-2 dark:bg-slate-800 dark:text-white transition-colors ${
                  fieldErrors.monthlyDebtPayments
                    ? 'border-rose-400 focus:ring-rose-400 bg-rose-50/20'
                    : 'border-slate-300 dark:border-slate-700 focus:ring-brand-500'
                }`}
                placeholder="Ej. 4,500 (préstamos, tarjetas)"
              />
            </div>
            {fieldErrors.monthlyDebtPayments && (
              <p className="text-xs font-semibold text-rose-600 dark:text-rose-400 flex items-center gap-1">
                <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" />
                {fieldErrors.monthlyDebtPayments}
              </p>
            )}
          </div>

          {/* Inicial / Separación */}
          <div className="space-y-1.5">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500 dark:text-slate-400">
              Inicial / Separación Disponible (RD$)
            </label>
            <div className="relative">
              <input
                type="text"
                inputMode="numeric"
                value={rawInputs.availableDownPayment}
                onChange={handleInputChange('availableDownPayment')}
                onBlur={handleBlur('availableDownPayment')}
                className={`w-full px-3.5 py-2.5 rounded-xl border text-sm font-semibold focus:outline-hidden focus:ring-2 dark:bg-slate-800 dark:text-white transition-colors ${
                  fieldErrors.availableDownPayment
                    ? 'border-rose-400 focus:ring-rose-400 bg-rose-50/20'
                    : 'border-slate-300 dark:border-slate-700 focus:ring-brand-500'
                }`}
                placeholder="Ej. 300,000 (ahorros disponibles)"
              />
            </div>
            {fieldErrors.availableDownPayment && (
              <p className="text-xs font-semibold text-rose-600 dark:text-rose-400 flex items-center gap-1">
                <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" />
                {fieldErrors.availableDownPayment}
              </p>
            )}
          </div>

          {/* Slow response / cold start info banner */}
          {isSlowResponse && (
            <div className="p-3.5 bg-amber-50 dark:bg-amber-950/40 border border-amber-200 dark:border-amber-800 rounded-xl text-xs font-semibold text-amber-800 dark:text-amber-200 flex items-center gap-2 animate-pulse">
              <Loader2 className="w-4 h-4 animate-spin flex-shrink-0" />
              <span>Conectando con el servidor... Las instancias en reposo pueden tardar unos segundos en iniciar.</span>
            </div>
          )}

          {/* Global error banner */}
          {error && (
            <div className="p-3.5 bg-rose-50 dark:bg-rose-950/40 border border-rose-200 dark:border-rose-800 rounded-xl text-xs font-semibold text-rose-700 dark:text-rose-300 flex items-center justify-between gap-3">
              <div className="flex items-center gap-2">
                <AlertCircle className="w-4 h-4 flex-shrink-0" />
                <span>{error}</span>
              </div>
              <button
                type="button"
                onClick={handleEvaluate}
                disabled={isEvaluating}
                className="px-2.5 py-1 bg-rose-600 hover:bg-rose-700 text-white rounded-lg text-[11px] font-bold inline-flex items-center gap-1 flex-shrink-0 transition-colors"
              >
                <RefreshCw className="w-3 h-3" /> Reintentar
              </button>
            </div>
          )}

          <button
            type="button"
            onClick={handleEvaluate}
            disabled={isEvaluating}
            className="w-full py-3 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 disabled:cursor-not-allowed text-white font-extrabold text-xs rounded-2xl shadow-lg shadow-brand-600/30 flex items-center justify-center gap-2 transition-all cursor-pointer"
          >
            {isEvaluating ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                <span>Evaluando capacidad...</span>
              </>
            ) : (
              <>
                <Calculator className="w-4 h-4" />
                <span>Evaluar Capacidad de Compra</span>
              </>
            )}
          </button>
        </div>

        {/* Results */}
        <div className="space-y-6">
          {!result ? (
            <div className="bg-white dark:bg-slate-900 rounded-3xl p-8 sm:p-10 border border-slate-200 dark:border-slate-800 shadow-xs text-center space-y-3">
              <Wallet className="w-10 h-10 text-brand-300 mx-auto" />
              <p className="text-sm font-bold text-slate-700 dark:text-slate-200">Aún no has evaluado tu capacidad</p>
              <p className="text-xs text-slate-500 dark:text-slate-400">
                Completa el formulario con tu perfil financiero y presiona «Evaluar Capacidad de Compra» para ver tu rango de vivienda recomendado.
              </p>
            </div>
          ) : (
            <>
              {/* Result banner */}
              <div className={`rounded-3xl p-6 sm:p-8 border shadow-lg text-center space-y-2 break-words ${cfg!.className}`}>
                <cfg.Icon className="w-12 h-12 mx-auto" />
                <h2 className="text-2xl font-extrabold uppercase tracking-wide">{cfg!.label}</h2>
                <p className="text-xl sm:text-2xl font-extrabold font-mono">
                  {formatCurrencyRD(result.maxPropertyPrice)}
                </p>
                <p className="text-[11px] font-bold opacity-90">Precio máximo de propiedad que puedes financiar</p>
              </div>

              {/* Key stats cards - symmetric padding & responsive */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {/* Máx. Hipoteca */}
                <div className="bg-white dark:bg-slate-900 p-5 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs flex items-center gap-4 min-w-0">
                  <div className="w-12 h-12 rounded-2xl bg-emerald-50 dark:bg-emerald-950/50 text-emerald-600 dark:text-emerald-400 flex items-center justify-center flex-shrink-0">
                    <Home className="w-6 h-6" />
                  </div>
                  <div className="min-w-0 flex-1 overflow-hidden">
                    <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-400 dark:text-slate-500 truncate">
                      Máx. Hipoteca
                    </p>
                    <p className="text-base sm:text-lg font-extrabold font-mono text-slate-900 dark:text-white truncate">
                      {formatCurrencyRD(result.maxMortgageAmount)}
                    </p>
                  </div>
                </div>

                {/* Cuota Mensual Máx. */}
                <div className="bg-white dark:bg-slate-900 p-5 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs flex items-center gap-4 min-w-0">
                  <div className="w-12 h-12 rounded-2xl bg-indigo-50 dark:bg-indigo-950/50 text-indigo-600 dark:text-indigo-400 flex items-center justify-center flex-shrink-0">
                    <Clock className="w-6 h-6" />
                  </div>
                  <div className="min-w-0 flex-1 overflow-hidden">
                    <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-400 dark:text-slate-500 truncate">
                      Cuota Mensual Máx.
                    </p>
                    <p className="text-base sm:text-lg font-extrabold font-mono text-slate-900 dark:text-white truncate">
                      {formatCurrencyRD(result.maxMonthlyPayment)}
                    </p>
                  </div>
                </div>

                {/* Deuda / Ingreso (DTI) */}
                <div className="bg-white dark:bg-slate-900 p-5 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs flex items-center gap-4 min-w-0">
                  <div className="w-12 h-12 rounded-2xl bg-amber-50 dark:bg-amber-950/50 text-amber-600 dark:text-amber-400 flex items-center justify-center flex-shrink-0">
                    <Percent className="w-6 h-6" />
                  </div>
                  <div className="min-w-0 flex-1 overflow-hidden">
                    <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-400 dark:text-slate-500 truncate">
                      Deuda / Ingreso
                    </p>
                    <p className="text-base sm:text-lg font-extrabold font-mono text-slate-900 dark:text-white truncate">
                      {Number.isFinite(result.debtToIncomeRatio) ? result.debtToIncomeRatio.toFixed(1) : '0.0'}%
                    </p>
                  </div>
                </div>

                {/* Tasa Anual / Plazo */}
                <div className="bg-white dark:bg-slate-900 p-5 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs flex items-center gap-4 min-w-0">
                  <div className="w-12 h-12 rounded-2xl bg-brand-50 dark:bg-brand-950/50 text-brand-600 dark:text-brand-400 flex items-center justify-center flex-shrink-0">
                    <TrendingUp className="w-6 h-6" />
                  </div>
                  <div className="min-w-0 flex-1 overflow-hidden">
                    <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-400 dark:text-slate-500 truncate">
                      Tasa Anual / Plazo
                    </p>
                    <p className="text-base sm:text-lg font-extrabold text-slate-900 dark:text-white truncate">
                      {Number.isFinite(result.estimatedAnnualRate) ? result.estimatedAnnualRate.toFixed(1) : '0.0'}% · {result.recommendedTermYears || 0} años
                    </p>
                  </div>
                </div>
              </div>

              {/* Dynamic observations block */}
              {result.observations && (
                <div className="bg-sky-50 dark:bg-sky-950/40 border border-sky-200 dark:border-sky-800/60 rounded-2xl p-4 text-xs font-semibold text-sky-900 dark:text-sky-200 leading-relaxed break-words">
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
