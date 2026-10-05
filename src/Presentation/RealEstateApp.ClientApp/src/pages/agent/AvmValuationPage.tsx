import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Property, ValuationResult } from '../../types';
import { propertiesService, avmService } from '../../api/services';
import { formatCurrencyRD } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import { useLanguage } from '../../context/LanguageContext';
import {
  Sparkles,
  TrendingUp,
  TrendingDown,
  Scale,
  MapPin,
  RefreshCw,
  Filter,
  Building2,
} from 'lucide-react';

export const AvmValuationPage: React.FC = () => {
  const { t, language } = useLanguage();
  const [properties, setProperties] = useState<Property[]>([]);
  const [selectedId, setSelectedId] = useState<number | ''>('');
  const [radius, setRadius] = useState<number>(5);
  const [valuation, setValuation] = useState<ValuationResult | null>(null);
  const [isLoadingProps, setIsLoadingProps] = useState<boolean>(true);
  const [isLoadingAvm, setIsLoadingAvm] = useState<boolean>(false);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    const load = async () => {
      try {
        const props = await propertiesService.getMyProperties();
        setProperties(props);
      } catch (err) {
        console.error('Error cargando propiedades:', err);
      } finally {
        setIsLoadingProps(false);
      }
    };
    Promise.resolve().then(() => load()).catch(console.error);
  }, []);

  const handlePropertyChange = async (id: number) => {
    setSelectedId(id);
    setError('');
    setValuation(null);
    if (id === 0) return;
    try {
      const last = await avmService.getLast(id);
      if (last) setValuation(last);
    } catch {
      // ignore
    }
  };

  const handleCalculate = async () => {
    if (selectedId === '') return;
    setIsLoadingAvm(true);
    setError('');
    try {
      const result = await avmService.calculate(Number(selectedId), radius);
      setValuation(result);
    } catch (err: unknown) {
      const message = err && typeof err === 'object' && 'response' in err
        ? (err as { response?: { data?: { error?: string } } }).response?.data?.error
        : undefined;
      setError(message || t('common.error', 'No se pudo calcular la valuación de esta propiedad.'));
    } finally {
      setIsLoadingAvm(false);
    }
  };

  const getRatingConfig = (rating: string) => {
    if (rating === 'Sobrevalorada') {
      return {
        label: t('agent.avm.overvalued', 'Sobrevalorada'),
        className: 'bg-rose-50 dark:bg-rose-950/40 text-rose-700 dark:text-rose-400 border-rose-200 dark:border-rose-800/60',
        icon: 'down' as const,
      };
    }
    if (rating === 'Subvalorada') {
      return {
        label: t('agent.avm.undervalued', 'Subvalorada'),
        className: 'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-400 border-emerald-200 dark:border-emerald-800/60',
        icon: 'up' as const,
      };
    }
    return {
      label: t('agent.avm.fair', 'Justamente Valorada'),
      className: 'bg-sky-50 dark:bg-sky-950/40 text-sky-700 dark:text-sky-400 border-sky-200 dark:border-sky-800/60',
      icon: 'fair' as const,
    };
  };

  const renderRating = () => {
    if (!valuation) return null;
    const cfg = getRatingConfig(valuation.valuationRating);
    const Icon = cfg.icon === 'up' ? TrendingUp : cfg.icon === 'down' ? TrendingDown : Scale;
    return (
      <span className={`inline-flex items-center gap-2 px-4 py-1.5 rounded-full border font-extrabold text-xs uppercase tracking-wider ${cfg.className}`}>
        <Icon className="w-4 h-4" />
        {cfg.label} · {valuation.priceDifferencePercentage > 0 ? '+' : ''}{valuation.priceDifferencePercentage.toFixed(1)}%
      </span>
    );
  };

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="rounded-3xl bg-gradient-to-r from-navy-950 to-slate-900 p-8 text-white flex flex-wrap items-center justify-between gap-4 shadow-lg">
        <div className="space-y-1">
          <span className="text-[11px] font-extrabold uppercase tracking-widest text-brand-400 flex items-center gap-1.5">
            <Sparkles className="w-4 h-4" /> {t('agent.avm.tag', "F-01 · AVM")}
          </span>
          <h1 className="text-2xl font-extrabold">{t('agent.avm.title', "Valuación Automatizada")}</h1>
          <p className="text-xs text-slate-300 max-w-xl">
            {t('agent.avm.subtitle', "Estima el valor de mercado de una propiedad comparándola con inmuebles similares en un radio de búsqueda configurable.")}
          </p>
        </div>
        <div className="text-4xl">
          <Building2 className="text-brand-400 w-14 h-14" />
        </div>
      </div>

      {/* Controls */}
      <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 border border-slate-200 dark:border-slate-800 shadow-xs space-y-5">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="space-y-1.5 md:col-span-1">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500 dark:text-slate-400">{t('agent.avm.property', "Propiedad")}</label>
            <select
              value={selectedId}
              onChange={(e) => handlePropertyChange(e.target.value ? Number(e.target.value) : 0)}
              className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
            >
              <option value="">{t('agent.avm.selectProperty', "Selecciona una propiedad...")}</option>
              {properties.map((p) => (
                <option key={p.id} value={p.id}>
                  #{p.code} · {p.propertyTypeName || t('agent.avm.property', 'Inmueble')} · {formatCurrencyRD(p.price)}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-1.5">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500 dark:text-slate-400 flex items-center gap-1">
              <Filter className="w-3.5 h-3.5" /> {t('agent.avm.searchRadius', "Radio de búsqueda (km)")}
            </label>
            <input
              type="number"
              min={1}
              max={50}
              value={radius}
              onChange={(e) => setRadius(Number(e.target.value))}
              className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
            />
          </div>

          <div className="flex items-end">
            <button
              onClick={handleCalculate}
              disabled={selectedId === '' || isLoadingAvm}
              className="w-full py-2.5 px-4 flex items-center justify-center gap-2 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 disabled:cursor-not-allowed text-white font-extrabold text-xs rounded-xl shadow-lg shadow-brand-600/30 transition-all"
            >
              <RefreshCw className={`w-4 h-4 ${isLoadingAvm ? 'animate-spin' : ''}`} />
              {isLoadingAvm ? t('agent.avm.calculating', 'Calculando...') : t('agent.avm.btnCalculate', 'Calcular Valuación')}
            </button>
          </div>
        </div>

        {error && (
          <div className="p-3.5 bg-rose-50 border border-rose-200 rounded-xl text-xs font-semibold text-rose-700">
            {error}
          </div>
        )}
      </div>

      {/* Results */}
      {isLoadingProps ? (
        <Loader text={t('agent.avm.loadingProps', "Cargando propiedades...")} />
      ) : selectedId === '' ? (
        <div className="bg-white dark:bg-slate-900 rounded-3xl p-10 border border-slate-200 dark:border-slate-800 shadow-xs text-center space-y-2">
          <Sparkles className="w-10 h-10 text-brand-300 mx-auto" />
          <p className="text-sm font-bold text-slate-700 dark:text-slate-300">{t('agent.avm.selectPrompt', "Selecciona una propiedad para ver su valuación")}</p>
          <p className="text-xs text-slate-500 dark:text-slate-400">
            {t('agent.avm.noPropsNotice', 'Si ninguna aparece, ve a')}{' '}
            <Link to="/agent/properties" className="text-brand-600 dark:text-brand-400 font-bold">{t('agent.avm.myPropsLink', 'Mis Propiedades')}</Link>{' '}
            {t('agent.avm.toPublishFirst', 'para publicar primero.')}
          </p>
        </div>
      ) : valuation ? (
        <div className="space-y-6">
          {/* Summary cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs">
              <span className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400 dark:text-slate-500">{t('agent.avm.currentPrice', "Precio Actual")}</span>
              <p className="mt-2 text-xl font-extrabold font-mono text-slate-900 dark:text-white">{formatCurrencyRD(valuation.currentPrice)}</p>
              <p className="text-[11px] text-slate-500 dark:text-slate-400">{valuation.propertyName} · #{valuation.propertyCode}</p>
            </div>
            <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs">
              <span className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400 dark:text-slate-500">{t('agent.avm.estimatedValue', "Valor Estimado")}</span>
              <p className="mt-2 text-xl font-extrabold font-mono text-emerald-600 dark:text-emerald-400">{formatCurrencyRD(valuation.estimatedTotalPrice)}</p>
              <p className="text-[11px] text-slate-500 dark:text-slate-400">≈ {formatCurrencyRD(valuation.estimatedPricePerSqm)}/m²</p>
            </div>
            <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs">
              <span className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400 dark:text-slate-500">{t('agent.avm.comparables', "Comparables")}</span>
              <p className="mt-2 text-xl font-extrabold text-slate-900 dark:text-white">{valuation.comparableCount}</p>
              <p className="text-[11px] text-slate-500 dark:text-slate-400">{t('agent.avm.range', 'Rango')} {formatCurrencyRD(valuation.minPricePerSqm)}–{formatCurrencyRD(valuation.maxPricePerSqm)}/m²</p>
            </div>
            <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs">
              <span className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400 dark:text-slate-500">{t('agent.avm.confidence', "Confianza")}</span>
              <p className="mt-2 text-xl font-extrabold text-slate-900 dark:text-white">
                {valuation.confidenceScore}
                <span className="text-sm text-slate-400 dark:text-slate-500 font-bold">/100</span>
              </p>
              <p className="text-[11px] text-slate-500 dark:text-slate-400">{t('agent.avm.evaluatedOn', 'Evaluado el')} {new Date(valuation.calculatedAt).toLocaleString(language === 'en' ? 'en-US' : 'es-DO')}</p>
            </div>
          </div>

          {/* Rating + price difference */}
          <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 border border-slate-200 dark:border-slate-800 shadow-xs flex flex-wrap items-center justify-between gap-4">
            <div className="space-y-1">
              <span className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500 dark:text-slate-400">{t('agent.avm.opinionTitle', "Dictamen de la Valuación")}</span>
              <div className="mt-1">{renderRating()}</div>
            </div>
            <div className="text-right space-y-1">
              <span className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500 dark:text-slate-400">{t('agent.avm.marketDiff', "Diferencia respecto al mercado")}</span>
              <p className="text-lg font-extrabold font-mono text-emerald-600 dark:text-emerald-400">
                {formatCurrencyRD(Math.abs(valuation.priceDifference))}
              </p>
              <p className="text-[11px] text-slate-500 dark:text-slate-400">{t('agent.avm.stdDev', 'Desviación estándar')} {formatCurrencyRD(valuation.standardDeviation)}/m² · {t('agent.avm.radiusLabel', 'Radio')} {valuation.searchRadiusKm} km</p>
            </div>
          </div>

          {/* Comparables table */}
          <div className="bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 overflow-hidden shadow-xs">
            <div className="px-6 py-4 border-b border-slate-100 dark:border-slate-800">
              <h2 className="font-extrabold text-slate-900 dark:text-white">{t('agent.avm.compTitle', "Propiedades Comparables")}</h2>
              <p className="text-xs text-slate-500 dark:text-slate-400">{valuation.comparableCount} {t('agent.avm.compSubtitle', 'inmuebles similares encontrados en el radio de')} {valuation.searchRadiusKm} km</p>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-slate-50 dark:bg-slate-800/70 text-slate-700 dark:text-slate-300 font-extrabold uppercase tracking-wider text-[11px]">
                    <th className="py-3 px-4 text-left">{t('agent.avm.colProperty', "Propiedad")}</th>
                    <th className="py-3 px-4 text-left">{t('agent.avm.colSector', "Sector")}</th>
                    <th className="py-3 px-4 text-right">{t('agent.avm.colPrice', "Precio")}</th>
                    <th className="py-3 px-4 text-right">{t('agent.avm.colSqm', "m²")}</th>
                    <th className="py-3 px-4 text-right">{t('agent.avm.colPriceSqm', "Precio/m²")}</th>
                    <th className="py-3 px-4 text-right">{t('agent.avm.colDistance', "Distancia")}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                  {valuation.comparables.map((c) => (
                    <tr key={c.propertyId} className="hover:bg-slate-50/80 dark:hover:bg-slate-800/40 transition-colors">
                      <td className="py-3 px-4">
                        <Link to={`/property/${c.propertyId}`} className="font-bold text-slate-800 dark:text-slate-200 hover:text-brand-600 dark:hover:text-brand-400">
                          #{c.code} · {c.name}
                        </Link>
                        <p className="text-[11px] text-slate-500 dark:text-slate-400">{c.rooms} {t('property.rooms', 'hab')} · {c.bathrooms} {t('property.bathrooms', 'baños')}</p>
                      </td>
                      <td className="py-3 px-4 text-slate-600 dark:text-slate-400 flex items-center gap-1">
                        <MapPin className="w-3.5 h-3.5 text-brand-500" /> {c.sector || c.municipalityName || '—'}
                      </td>
                      <td className="py-3 px-4 text-right font-mono font-bold text-slate-900 dark:text-white">{formatCurrencyRD(c.price)}</td>
                      <td className="py-3 px-4 text-right text-slate-600 dark:text-slate-300">{c.sizeInMeters} m²</td>
                      <td className="py-3 px-4 text-right font-mono font-semibold text-emerald-600 dark:text-emerald-400">{formatCurrencyRD(c.pricePerSqm)}</td>
                      <td className="py-3 px-4 text-right text-slate-600 dark:text-slate-300">{c.distanceKm.toFixed(2)} km</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      ) : (
        <div className="bg-white dark:bg-slate-900 rounded-3xl p-10 border border-slate-200 dark:border-slate-800 shadow-xs text-center space-y-2">
          <Sparkles className="w-10 h-10 text-brand-300 mx-auto" />
          <p className="text-sm font-bold text-slate-700 dark:text-slate-300">{t('agent.avm.noValuationTitle', "No hay una valuación guardada para esta propiedad todavía")}</p>
          <p className="text-xs text-slate-500 dark:text-slate-400">{t('agent.avm.noValuationDesc', "Usa el botón «Calcular Valuación» para generar el dictamen AVM.")}</p>
        </div>
      )}
    </div>
  );
};
