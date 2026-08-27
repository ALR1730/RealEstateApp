import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Property, PropertyType, SaleType, FilterState } from '../../types';
import { propertiesService, catalogsService, favoritesService, savedSearchesService } from '../../api/services';
import { PropertyCard } from '../../components/properties/PropertyCard';
import { PropertyFilter } from '../../components/properties/PropertyFilter';
import { Loader } from '../../components/common/Loader';
import { useAuth } from '../../context/AuthContext';
import { LayoutGrid, List, SlidersHorizontal, ArrowUpDown, BookmarkPlus, X } from 'lucide-react';

const FILTER_URL_KEYS: (keyof FilterState)[] = [
  'code', 'propertyTypeId', 'saleTypeId', 'minPrice', 'maxPrice',
  'minRooms', 'minBathrooms', 'minSizeInMeters', 'maxSizeInMeters',
  'provinceId', 'municipalityId', 'sector', 'agentId',
  'onlyFeatured', 'onlyVerifiedAgents', 'onlyFinanciable', 'onlyWithVirtualTour',
  'maxDistanceKm',
];

const FILTER_LABELS: Record<string, string> = {
  code: 'Código',
  propertyTypeId: 'Tipo',
  saleTypeId: 'Modalidad',
  minPrice: 'Precio Mín',
  maxPrice: 'Precio Máx',
  minRooms: 'Habitaciones',
  minBathrooms: 'Baños',
  minSizeInMeters: 'Tamaño Mín',
  maxSizeInMeters: 'Tamaño Máx',
  provinceId: 'Provincia',
  municipalityId: 'Municipio',
  sector: 'Sector',
  agentId: 'Agente',
  onlyFeatured: 'Destacados',
  onlyVerifiedAgents: 'Verificados',
  onlyFinanciable: 'Financiable',
  onlyWithVirtualTour: 'Tour Virtual',
  maxDistanceKm: 'Distancia',
};

function readFiltersFromURL(sp: URLSearchParams): FilterState {
  const f: FilterState = {};
  const num = (v: string | null) => v ? Number(v) : undefined;
  const str = (v: string | null) => v || undefined;
  const bool = (v: string | null) => v === 'true';

  f.code = str(sp.get('code'));
  f.propertyTypeId = num(sp.get('propertyTypeId'));
  f.saleTypeId = num(sp.get('saleTypeId'));
  f.minPrice = num(sp.get('minPrice'));
  f.maxPrice = num(sp.get('maxPrice'));
  f.minRooms = num(sp.get('minRooms'));
  f.minBathrooms = num(sp.get('minBathrooms'));
  f.minSizeInMeters = num(sp.get('minSizeInMeters'));
  f.maxSizeInMeters = num(sp.get('maxSizeInMeters'));
  f.provinceId = num(sp.get('provinceId'));
  f.municipalityId = num(sp.get('municipalityId'));
  f.sector = str(sp.get('sector'));
  f.agentId = str(sp.get('agentId'));
  f.onlyFeatured = bool(sp.get('onlyFeatured'));
  f.onlyVerifiedAgents = bool(sp.get('onlyVerifiedAgents'));
  f.onlyFinanciable = bool(sp.get('onlyFinanciable'));
  f.onlyWithVirtualTour = bool(sp.get('onlyWithVirtualTour'));
  f.maxDistanceKm = num(sp.get('maxDistanceKm'));
  f.userLat = num(sp.get('userLat'));
  f.userLng = num(sp.get('userLng'));
  return f;
}

function writeFiltersToURL(filters: FilterState): URLSearchParams {
  const params = new URLSearchParams();
  FILTER_URL_KEYS.forEach((k) => {
    const v = filters[k];
    if (v !== undefined && v !== '' && v !== null) {
      params.set(k, String(v));
    }
  });
  if (filters.userLat) params.set('userLat', String(filters.userLat));
  if (filters.userLng) params.set('userLng', String(filters.userLng));
  return params;
}

export const PropertiesCatalogPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const { isAuthenticated, isClient } = useAuth();
  const [properties, setProperties] = useState<Property[]>([]);
  const [propertyTypes, setPropertyTypes] = useState<PropertyType[]>([]);
  const [saleTypes, setSaleTypes] = useState<SaleType[]>([]);
  const [favoriteIds, setFavoriteIds] = useState<Set<number>>(new Set());
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [sortBy, setSortBy] = useState<string>('featured');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [showSaveModal, setShowSaveModal] = useState(false);
  const [saveName, setSaveName] = useState('');
  const [saveAlerts, setSaveAlerts] = useState(false);

  const [filters, setFilters] = useState<FilterState>(() => readFiltersFromURL(searchParams));

  const loadData = async () => {
    try {
      const cleanParams: Record<string, any> = {};
      Object.entries(filters).forEach(([k, v]) => {
        if (v !== undefined && v !== '' && v !== null) {
          cleanParams[k] = v;
        }
      });
      const [props, types, sales] = await Promise.all([
        propertiesService.getAll(cleanParams),
        catalogsService.getPropertyTypes(),
        catalogsService.getSaleTypes(),
      ]);

      setProperties(props);
      setPropertyTypes(types);
      setSaleTypes(sales);

      if (isAuthenticated && isClient) {
        try {
          const favs = await favoritesService.getAll();
          setFavoriteIds(new Set(favs.map((f) => f.propertyId)));
        } catch {
          // ignore
        }
      }
    } catch (err) {
      console.error("Error loading catalog:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadData()).catch(console.error);
  }, [filters]);

  const handleFilterChange = (newFilters: FilterState) => {
    setFilters(newFilters);
    setSearchParams(writeFiltersToURL(newFilters));
  };

  const handleResetFilters = () => {
    setFilters({});
    setSearchParams({});
  };

  const handleRemoveFilter = (key: keyof FilterState) => {
    const next = { ...filters };
    delete next[key];
    setFilters(next);
    setSearchParams(writeFiltersToURL(next));
  };

  const handleSaveSearch = async () => {
    if (!saveName.trim()) return;
    try {
      await savedSearchesService.save({
        name: saveName.trim(),
        ...filters,
        emailAlertsEnabled: saveAlerts,
      });
      setShowSaveModal(false);
      setSaveName('');
      setSaveAlerts(false);
    } catch (err) {
      console.error("Error saving search:", err);
    }
  };

  const sortedProperties = [...properties].sort((a, b) => {
    if (sortBy === 'price-asc') return a.price - b.price;
    if (sortBy === 'price-desc') return b.price - a.price;
    if (sortBy === 'bedrooms-desc') return b.bedrooms - a.bedrooms;
    if (sortBy === 'featured') return (b.isFeatured ? 1 : 0) - (a.isFeatured ? 1 : 0);
    return b.id - a.id;
  });

  const activeFilters = Object.entries(filters).filter(
    ([k, v]) => v !== undefined && v !== '' && v !== null && !['userLat', 'userLng'].includes(k)
  );

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">

      <div className="bg-navy-950 text-white rounded-3xl p-6 sm:p-10 relative overflow-hidden shadow-xl">
        <div className="relative z-10 max-w-2xl space-y-2">
          <span className="text-brand-400 font-extrabold text-xs uppercase tracking-widest">
            Catálogo Oficial de Inmuebles
          </span>
          <h1 className="text-2xl sm:text-4xl font-extrabold tracking-tight">
            Propiedades Exclusivas en RD$
          </h1>
          <p className="text-xs sm:text-sm text-slate-300">
            Filtra por tipo, ubicación, amenidades, habitaciones y rango de precios con cálculo hipotecario en tiempo real.
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">

        <div className="lg:col-span-1">
          <div className="sticky top-24">
            <PropertyFilter
              filters={filters}
              onChange={handleFilterChange}
              onReset={handleResetFilters}
              propertyTypes={propertyTypes}
              saleTypes={saleTypes}
            />
          </div>
        </div>

        <div className="lg:col-span-3 space-y-6">

          {/* Active Filter Tags */}
          {activeFilters.length > 0 && (
            <div className="flex flex-wrap items-center gap-2">
              <span className="text-xs font-bold text-slate-500">Filtros activos:</span>
              {activeFilters.map(([key, value]) => (
                <span
                  key={key}
                  className="inline-flex items-center gap-1 px-2.5 py-1 text-[11px] font-semibold bg-brand-50 text-brand-700 rounded-full border border-brand-200"
                >
                  {FILTER_LABELS[key] || key}: {typeof value === 'boolean' ? 'Sí' : String(value)}
                  <button onClick={() => handleRemoveFilter(key as keyof FilterState)} className="hover:text-rose-600">
                    <X className="w-3 h-3" />
                  </button>
                </span>
              ))}
              <button
                onClick={handleResetFilters}
                className="text-[11px] font-bold text-rose-500 hover:text-rose-700 ml-1"
              >
                Limpiar todo
              </button>
            </div>
          )}

          {/* Controls Bar */}
          <div className="bg-white rounded-2xl p-4 border border-slate-200 shadow-xs flex flex-wrap items-center justify-between gap-4 text-xs font-semibold text-slate-700">
            <div className="flex items-center gap-2">
              <SlidersHorizontal className="w-4 h-4 text-brand-600" />
              <span>Mostrando <strong>{sortedProperties.length}</strong> inmuebles</span>
            </div>

            <div className="flex items-center gap-3">
              {isAuthenticated && isClient && (
                <button
                  onClick={() => setShowSaveModal(true)}
                  className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold text-brand-600 border border-brand-200 rounded-lg hover:bg-brand-50 transition-colors"
                >
                  <BookmarkPlus className="w-3.5 h-3.5" />
                  Guardar Búsqueda
                </button>
              )}

              <div className="flex items-center gap-1.5">
                <ArrowUpDown className="w-3.5 h-3.5 text-slate-400" />
                <select
                  value={sortBy}
                  onChange={(e) => setSortBy(e.target.value)}
                  className="px-2.5 py-1.5 rounded-lg border border-slate-200 text-xs font-medium focus:ring-2 focus:ring-brand-500 bg-white"
                >
                  <option value="featured">Destacadas Primero</option>
                  <option value="price-asc">Precio: Menor a Mayor</option>
                  <option value="price-desc">Precio: Mayor a Menor</option>
                  <option value="bedrooms-desc">Más Habitaciones</option>
                  <option value="recent">Más Recientes</option>
                </select>
              </div>

              <div className="flex border border-slate-200 rounded-lg overflow-hidden">
                <button
                  onClick={() => setViewMode('grid')}
                  className={`p-1.5 ${viewMode === 'grid' ? 'bg-slate-100 text-brand-600' : 'text-slate-400 hover:text-slate-700'}`}
                >
                  <LayoutGrid className="w-4 h-4" />
                </button>
                <button
                  onClick={() => setViewMode('list')}
                  className={`p-1.5 ${viewMode === 'list' ? 'bg-slate-100 text-brand-600' : 'text-slate-400 hover:text-slate-700'}`}
                >
                  <List className="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>

          {isLoading ? (
            <Loader text="Consultando catálogo en vivo..." />
          ) : sortedProperties.length === 0 ? (
            <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-4">
              <div className="w-16 h-16 rounded-full bg-slate-100 text-slate-400 flex items-center justify-center mx-auto">
                <SlidersHorizontal className="w-8 h-8" />
              </div>
              <h3 className="text-base font-bold text-slate-800">
                No se encontraron propiedades con los filtros seleccionados
              </h3>
              <p className="text-xs text-slate-500 max-w-md mx-auto">
                Intenta ajustar los criterios de búsqueda, ampliar el rango de precios en RD$ o limpiar los filtros.
              </p>
              <button
                onClick={handleResetFilters}
                className="px-5 py-2 bg-brand-600 text-white font-bold text-xs rounded-xl shadow-md"
              >
                Limpiar Filtros
              </button>
            </div>
          ) : (
            <div className={viewMode === 'grid' ? 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6' : 'space-y-4'}>
              {sortedProperties.map((prop) => (
                <PropertyCard
                  key={prop.id}
                  property={prop}
                  isFavoriteInitial={favoriteIds.has(prop.id)}
                />
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Save Search Modal */}
      {showSaveModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
          <div className="bg-white rounded-2xl p-6 w-full max-w-md shadow-2xl space-y-4">
            <h3 className="text-base font-bold text-slate-900">Guardar Búsqueda</h3>
            <input
              type="text"
              placeholder="Nombre de la búsqueda"
              value={saveName}
              onChange={(e) => setSaveName(e.target.value)}
              className="w-full px-3 py-2 text-sm rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
            />
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={saveAlerts}
                onChange={(e) => setSaveAlerts(e.target.checked)}
                className="w-4 h-4 rounded text-brand-600"
              />
              <span className="text-xs font-semibold text-slate-700">Recibir alertas por correo</span>
            </label>
            <div className="flex gap-3 pt-2">
              <button
                onClick={() => { setShowSaveModal(false); setSaveName(''); setSaveAlerts(false); }}
                className="flex-1 px-4 py-2 text-xs font-bold text-slate-600 border border-slate-200 rounded-xl hover:bg-slate-50"
              >
                Cancelar
              </button>
              <button
                onClick={handleSaveSearch}
                disabled={!saveName.trim()}
                className="flex-1 px-4 py-2 text-xs font-bold text-white bg-brand-600 rounded-xl hover:bg-brand-500 disabled:opacity-50"
              >
                Guardar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
