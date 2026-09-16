import React, { useState, useEffect } from 'react';
import { Sparkles, Search, Loader2, ArrowRight, X, Bot, CheckCircle2 } from 'lucide-react';
import { aiSearchService } from '../../api/services';
import { AiSearchInterpretation } from '../../types';

interface AiSearchBarProps {
  onSearchComplete?: (result: AiSearchInterpretation) => void;
  onApplyFilters?: (filter: Record<string, any>) => void;
  variant?: 'hero' | 'catalog';
  placeholder?: string;
  autoFocus?: boolean;
}

const DEFAULT_SUGGESTIONS = [
  'Apartamento de 3 habitaciones en Bella Vista por menos de RD$ 8M',
  'Villa en Punta Cana con piscina y tour virtual',
  'Casa en Santiago de 4 habitaciones con patio',
  'Penthouse en Piantini con terraza y ascensor',
];

export const AiSearchBar: React.FC<AiSearchBarProps> = ({
  onSearchComplete,
  onApplyFilters,
  variant = 'hero',
  placeholder = "Escribe libremente: Ej. 'Apartamento de 3 habitaciones en Bella Vista con balcón por menos de 8M'...",
  autoFocus = false,
}) => {
  const [query, setQuery] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const [suggestions, setSuggestions] = useState<string[]>(DEFAULT_SUGGESTIONS);
  const [activeResult, setActiveResult] = useState<AiSearchInterpretation | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;
    aiSearchService
      .getSuggestions()
      .then((items) => {
        if (isMounted && items && items.length > 0) {
          setSuggestions(items);
        }
      })
      .catch(() => {
        // Fallback a sugerencias por defecto
      });
    return () => {
      isMounted = false;
    };
  }, []);

  const handleExecuteSearch = async (textToSearch: string) => {
    const targetText = textToSearch.trim();
    if (!targetText || isProcessing) return;

    setIsProcessing(true);
    setErrorMsg(null);

    try {
      const result = await aiSearchService.searchByAi(targetText);
      setActiveResult(result);

      if (onSearchComplete) {
        onSearchComplete(result);
      }

      if (onApplyFilters && result.parsedFilter) {
        onApplyFilters(result.parsedFilter);
      }
    } catch (err: any) {
      console.error('Error al procesar búsqueda con IA:', err);
      setErrorMsg('No se pudo procesar la consulta. Intenta reformularla.');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    handleExecuteSearch(query);
  };

  const handleClear = () => {
    setQuery('');
    setActiveResult(null);
    setErrorMsg(null);
  };

  const handleSuggestionClick = (suggestion: string) => {
    setQuery(suggestion);
    handleExecuteSearch(suggestion);
  };

  const isHero = variant === 'hero';

  return (
    <div className={`w-full transition-all duration-300 ${isHero ? 'max-w-4xl mx-auto' : 'max-w-3xl'}`}>
      {/* Barra principal de búsqueda */}
      <form
        onSubmit={handleSubmit}
        className={`relative rounded-2xl sm:rounded-3xl p-1.5 sm:p-2.5 transition-all shadow-xl ${
          isHero
            ? 'bg-slate-900/90 backdrop-blur-md border border-brand-500/40 shadow-brand-500/10 focus-within:border-brand-400 focus-within:ring-4 focus-within:ring-brand-500/20'
            : 'bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 focus-within:border-brand-500'
        }`}
      >
        <div className="flex items-center gap-2 sm:gap-3 px-2 sm:px-3">
          {/* Icono AI / Sparkles */}
          <div className="flex items-center justify-center w-8 h-8 sm:w-10 sm:h-10 rounded-xl sm:rounded-2xl bg-gradient-to-br from-brand-500 to-teal-400 text-white shrink-0 shadow-md shadow-brand-500/20">
            {isProcessing ? (
              <Loader2 className="w-4 h-4 sm:w-5 sm:h-5 animate-spin" />
            ) : (
              <Sparkles className="w-4 h-4 sm:w-5 sm:h-5 animate-pulse" />
            )}
          </div>

          {/* Campo de texto libre */}
          <input
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={placeholder}
            autoFocus={autoFocus}
            disabled={isProcessing}
            className={`w-full py-2 sm:py-2.5 text-xs sm:text-sm bg-transparent outline-none font-medium placeholder:font-normal transition-colors ${
              isHero
                ? 'text-white placeholder:text-slate-400'
                : 'text-slate-900 dark:text-white placeholder:text-slate-400'
            }`}
          />

          {/* Botón de limpiar si hay texto */}
          {query && !isProcessing && (
            <button
              type="button"
              onClick={handleClear}
              className="p-1 sm:p-1.5 text-slate-400 hover:text-slate-200 dark:hover:text-white rounded-lg hover:bg-slate-800/40 transition-colors"
              title="Limpiar consulta"
            >
              <X className="w-4 h-4" />
            </button>
          )}

          {/* Botón Ejecutar Búsqueda IA */}
          <button
            type="submit"
            disabled={isProcessing || !query.trim()}
            className="flex items-center gap-1.5 px-3 sm:px-5 py-2 sm:py-2.5 rounded-xl sm:rounded-2xl font-bold text-xs sm:text-sm text-white bg-gradient-to-r from-brand-600 via-brand-500 to-teal-500 hover:from-brand-500 hover:to-teal-400 active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed shadow-md shadow-brand-500/25 transition-all shrink-0 cursor-pointer"
          >
            {isProcessing ? (
              <span>Interpretando...</span>
            ) : (
              <>
                <Bot className="w-4 h-4" />
                <span className="hidden sm:inline">Buscar con IA</span>
                <span className="sm:hidden">Buscar</span>
              </>
            )}
          </button>
        </div>
      </form>

      {/* Indicador de procesamiento semántico */}
      {isProcessing && (
        <div className="mt-2.5 flex items-center justify-center gap-2 text-xs font-semibold text-brand-400 animate-pulse">
          <Sparkles className="w-3.5 h-3.5" />
          <span>Extrayendo intenciones, rangos de precio, habitaciones y zonas en tiempo real...</span>
        </div>
      )}

      {/* Mensaje de error si ocurre */}
      {errorMsg && (
        <div className="mt-2 text-xs text-rose-400 px-3 py-1.5 rounded-lg bg-rose-500/10 border border-rose-500/20">
          {errorMsg}
        </div>
      )}

      {/* Tarjeta explicativa con badges de entidades extraídas */}
      {activeResult && !isProcessing && (
        <div className="mt-3 p-3.5 sm:p-4 rounded-2xl bg-slate-900/85 backdrop-blur-md border border-brand-500/30 text-white space-y-2.5 animate-in fade-in slide-in-from-top-2 duration-300">
          <div className="flex items-start justify-between gap-2">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0" />
              <p className="text-xs sm:text-sm font-semibold text-slate-200">
                {activeResult.explanation}
              </p>
            </div>
            <span className="px-2 py-0.5 rounded-full text-[10px] font-extrabold bg-brand-500/20 text-brand-300 border border-brand-500/30 shrink-0">
              Confianza: {Math.round(activeResult.confidenceScore * 100)}%
            </span>
          </div>

          {/* Badges de entidades identificadas */}
          <div className="flex flex-wrap gap-1.5 pt-1">
            {activeResult.extractedEntities.propertyType && (
              <span className="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-slate-800 border border-slate-700 text-teal-300">
                🏢 Tipo: {activeResult.extractedEntities.propertyType}
              </span>
            )}
            {activeResult.extractedEntities.saleType && (
              <span className="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-slate-800 border border-slate-700 text-blue-300">
                🏷️ Modalidad: {activeResult.extractedEntities.saleType}
              </span>
            )}
            {activeResult.extractedEntities.sector && (
              <span className="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-slate-800 border border-slate-700 text-emerald-300">
                📍 Sector: {activeResult.extractedEntities.sector}
              </span>
            )}
            {activeResult.extractedEntities.province && (
              <span className="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-slate-800 border border-slate-700 text-emerald-300">
                📍 Provincia: {activeResult.extractedEntities.province}
              </span>
            )}
            {activeResult.extractedEntities.minRooms && (
              <span className="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-slate-800 border border-slate-700 text-amber-300">
                🛏️ {activeResult.extractedEntities.minRooms}+ Hab.
              </span>
            )}
            {activeResult.extractedEntities.minBathrooms && (
              <span className="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-slate-800 border border-slate-700 text-amber-300">
                🚿 {activeResult.extractedEntities.minBathrooms}+ Baños
              </span>
            )}
            {activeResult.extractedEntities.maxPrice && (
              <span className="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-slate-800 border border-slate-700 text-purple-300">
                💰 Hasta RD$ {activeResult.extractedEntities.maxPrice.toLocaleString()}
              </span>
            )}
            {activeResult.extractedEntities.improvements.map((imp, idx) => (
              <span
                key={idx}
                className="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-slate-800 border border-slate-700 text-pink-300"
              >
                ✨ {imp}
              </span>
            ))}
            {activeResult.extractedEntities.hasVirtualTour && (
              <span className="px-2.5 py-1 rounded-lg text-[11px] font-bold bg-slate-800 border border-slate-700 text-cyan-300">
                🕶️ Tour Virtual 360°
              </span>
            )}
            <span className="px-2.5 py-1 rounded-lg text-[11px] font-extrabold bg-brand-500/30 text-white ml-auto">
              {activeResult.matchedPropertiesCount} {activeResult.matchedPropertiesCount === 1 ? 'coincidencia' : 'coincidencias'}
            </span>
          </div>
        </div>
      )}

      {/* Sugerencias Rápidas / Chips */}
      {!activeResult && suggestions.length > 0 && (
        <div className="mt-3 flex flex-wrap items-center gap-1.5 sm:gap-2">
          <span className="text-[11px] font-semibold text-slate-400 flex items-center gap-1 shrink-0">
            <Sparkles className="w-3 h-3 text-brand-400" />
            Sugerencias:
          </span>
          {suggestions.slice(0, 3).map((item, idx) => (
            <button
              key={idx}
              type="button"
              onClick={() => handleSuggestionClick(item)}
              className={`text-[11px] sm:text-xs px-2.5 py-1 rounded-full transition-all text-left truncate max-w-[280px] sm:max-w-none cursor-pointer ${
                isHero
                  ? 'bg-slate-800/80 text-slate-300 hover:bg-brand-500/20 hover:text-white border border-slate-700/60 hover:border-brand-500/50'
                  : 'bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-brand-50 dark:hover:bg-brand-950/30 hover:text-brand-600 dark:hover:text-brand-400 border border-slate-200 dark:border-slate-700'
              }`}
            >
              {item}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};
