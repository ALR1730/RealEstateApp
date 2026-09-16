import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Property, ValuationResult } from '../../types';
import { propertiesService, avmService } from '../../api/services';
import { formatCurrencyRD } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
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

const ratingStyles: Record<string, { label: string; className: string; icon: 'up' | 'down' | 'fair' }> = {
  'Sobrevalorada': { label: 'Sobrevalorada', className: 'bg-rose-50 text-rose-700 border-rose-200', icon: 'down' },
  'Subvalorada': { label: 'Subvalorada', className: 'bg-emerald-50 text-emerald-700 border-emerald-200', icon: 'up' },
  'Justa': { label: 'Justamente Valorada', className: 'bg-sky-50 text-sky-700 border-sky-200', icon: 'fair' },
};

export const AvmValuationPage: React.FC = () => {
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
      setError(message || 'No se pudo calcular la valuación de esta propiedad.');
    } finally {
      setIsLoadingAvm(false);
    }
  };

  const renderRating = () => {
    if (!valuation) return null;
    const cfg = ratingStyles[valuation.valuationRating] || ratingStyles['Justa'];
    const Icon = cfg.icon === 'up' ? TrendingUp : cfg.icon === 'down' ? TrendingDown : Scale;
    return (
      <span className={`inline-flex items-center gap-2 px-4 py-1.5 rounded-full border font-extrabold text-xs uppercase tracking-wider ${cfg.className}`}>
        <Icon className="w-4 h-4" />
        {cfg.label} · Déficit / Excedente {valuation.priceDifferencePercentage > 0 ? '+' : ''}{valuation.priceDifferencePercentage.toFixed(1)}%
      </span>
    );
  };

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="rounded-3xl bg-gradient-to-r from-navy-950 to-slate-900 p-8 text-white flex flex-wrap items-center justify-between gap-4 shadow-lg">
        <div className="space-y-1">
          <span className="text-[11px] font-extrabold uppercase tracking-widest text-brand-400 flex items-center gap-1.5">
            <Sparkles className="w-4 h-4" /> F-01 · AVM
          </span>
          <h1 className="text-2xl font-extrabold">Valuación Automatizada</h1>
          <p className="text-xs text-slate-300 max-w-xl">
            Estima el valor de mercado de una propiedad comparándola con inmuebles similares en un radio de búsqueda configurable.
          </p>
        </div>
        <div className="text-4xl">
          <Building2 className="text-brand-400 w-14 h-14" />
        </div>
      </div>

      {/* Controls */}
      <div className="bg-white rounded-3xl p-6 border border-slate-200 shadow-xs space-y-5">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="space-y-1.5 md:col-span-1">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Propiedad</label>
            <select
              value={selectedId}
              onChange={(e) => handlePropertyChange(e.target.value ? Number(e.target.value) : 0)}
              className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 bg-white text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
            >
              <option value="">Selecciona una propiedad...</option>
              {properties.map((p) => (
                <option key={p.id} value={p.id}>
                  #{p.code} · {p.propertyTypeName || 'Inmueble'} · {formatCurrencyRD(p.price)}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-1.5">
            <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500 flex items-center gap-1">
              <Filter className="w-3.5 h-3.5" /> Radio de búsqueda (km)
            </label>
            <input
              type="number"
              min={1}
              max={50}
              value={radius}
              onChange={(e) => setRadius(Number(e.target.value))}
              className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 bg-white text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
            />
          </div>

          <div className="flex items-end">
            <button
              onClick={handleCalculate}
              disabled={selectedId === '' || isLoadingAvm}
              className="w-full py-2.5 px-4 flex items-center justify-center gap-2 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 disabled:cursor-not-allowed text-white font-extrabold text-xs rounded-xl shadow-lg shadow-brand-600/30 transition-all"
            >
              <RefreshCw className={`w-4 h-4 ${isLoadingAvm ? 'animate-spin' : ''}`} />
              {isLoadingAvm ? 'Calculando...' : 'Calcular Valuación'}
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
        <Loader text="Cargando propiedades..." />
      ) : selectedId === '' ? (
        <div className="bg-white rounded-3xl p-10 border border-slate-200 shadow-xs text-center space-y-2">
          <Sparkles className="w-10 h-10 text-brand-300 mx-auto" />
          <p className="text-sm font-bold text-slate-700">Selecciona una propiedad para ver su valuación</p>
          <p className="text-xs text-slate-500">Si ninguna aparece, ve a <Link to="/agent/properties" className="text-brand-600 font-bold">Mis Propiedades</Link> para publicar primero.</p>
        </div>
      ) : valuation ? (
        <div className="space-y-6">
          {/* Summary cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs">
              <span className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400">Precio Actual</span>
              <p className="mt-2 text-xl font-extrabold font-mono text-slate-900">{formatCurrencyRD(valuation.currentPrice)}</p>
              <p className="text-[11px] text-slate-500">{valuation.propertyName} · #{valuation.propertyCode}</p>
            </div>
            <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs">
              <span className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400">Valor Estimado</span>
              <p className="mt-2 text-xl font-extrabold font-mono text-emerald-600">{formatCurrencyRD(valuation.estimatedTotalPrice)}</p>
              <p className="text-[11px] text-slate-500">≈ {formatCurrencyRD(valuation.estimatedPricePerSqm)}/m²</p>
            </div>
            <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs">
              <span className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400">Comparables</span>
              <p className="mt-2 text-xl font-extrabold text-slate-900">{valuation.comparableCount}</p>
              <p className="text-[11px] text-slate-500">Rango {formatCurrencyRD(valuation.minPricePerSqm)}–{formatCurrencyRD(valuation.maxPricePerSqm)}/m²</p>
            </div>
            <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs">
              <span className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400">Confianza</span>
              <p className="mt-2 text-xl font-extrabold text-slate-900">
                {valuation.confidenceScore}
                <span className="text-sm text-slate-400 font-bold">/100</span>
              </p>
              <p className="text-[11px] text-slate-500">Evaluado el {new Date(valuation.calculatedAt).toLocaleString('es-DO')}</p>
            </div>
          </div>

          {/* Rating + price difference */}
          <div className="bg-white rounded-3xl p-6 border border-slate-200 shadow-xs flex flex-wrap items-center justify-between gap-4">
            <div className="space-y-1">
              <span className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Dictamen de la Valuación</span>
              <div className="mt-1">{renderRating()}</div>
            </div>
            <div className="text-right space-y-1">
              <span className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Diferencia respecto al mercado</span>
              <p className="text-lg font-extrabold font-mono text-emerald-600">
                {formatCurrencyRD(Math.abs(valuation.priceDifference))}
              </p>
              <p className="text-[11px] text-slate-500">Desviación estándar {formatCurrencyRD(valuation.standardDeviation)}/m² · Radio {valuation.searchRadiusKm} km</p>
            </div>
          </div>

          {/* Comparables table */}
          <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
            <div className="px-6 py-4 border-b border-slate-100">
              <h2 className="font-extrabold text-slate-900">Propiedades Comparables</h2>
              <p className="text-xs text-slate-500">{valuation.comparableCount} inmuebles similares encontrados en el radio de {valuation.searchRadiusKm} km</p>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider text-[11px]">
                    <th className="py-3 px-4 text-left">Propiedad</th>
                    <th className="py-3 px-4 text-left">Sector</th>
                    <th className="py-3 px-4 text-right">Precio</th>
                    <th className="py-3 px-4 text-right">m²</th>
                    <th className="py-3 px-4 text-right">Precio/m²</th>
                    <th className="py-3 px-4 text-right">Distancia</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {valuation.comparables.map((c) => (
                    <tr key={c.propertyId} className="hover:bg-slate-50/80 transition-colors">
                      <td className="py-3 px-4">
                        <Link to={`/property/${c.propertyId}`} className="font-bold text-slate-800 hover:text-brand-600">
                          #{c.code} · {c.name}
                        </Link>
                        <p className="text-[11px] text-slate-500">{c.rooms} hab · {c.bathrooms} baños</p>
                      </td>
                      <td className="py-3 px-4 text-slate-600 flex items-center gap-1">
                        <MapPin className="w-3.5 h-3.5 text-brand-500" /> {c.sector || c.municipalityName || '—'}
                      </td>
                      <td className="py-3 px-4 text-right font-mono font-bold text-slate-900">{formatCurrencyRD(c.price)}</td>
                      <td className="py-3 px-4 text-right text-slate-600">{c.sizeInMeters} m²</td>
                      <td className="py-3 px-4 text-right font-mono font-semibold text-emerald-600">{formatCurrencyRD(c.pricePerSqm)}</td>
                      <td className="py-3 px-4 text-right text-slate-600">{c.distanceKm.toFixed(2)} km</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      ) : (
        <div className="bg-white rounded-3xl p-10 border border-slate-200 shadow-xs text-center space-y-2">
          <Sparkles className="w-10 h-10 text-brand-300 mx-auto" />
          <p className="text-sm font-bold text-slate-700">No hay una valuación guardada para esta propiedad todavía</p>
          <p className="text-xs text-slate-500">Usa el botón «Calcular Valuación» para generar el dictamen AVM.</p>
        </div>
      )}
    </div>
  );
};
