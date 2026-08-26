import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Property, PropertyType, SaleType } from '../../types';
import { propertiesService, catalogsService, favoritesService } from '../../api/services';
import { PropertyCard } from '../../components/properties/PropertyCard';
import { PropertyFilter } from '../../components/properties/PropertyFilter';
import { Loader } from '../../components/common/Loader';
import { useAuth } from '../../context/AuthContext';
import { LayoutGrid, List, SlidersHorizontal, ArrowUpDown, BookmarkPlus } from 'lucide-react';

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

  // Filters State synced from URL params
  const [filters, setFilters] = useState<{
    propertyTypeId?: number;
    saleTypeId?: number;
    minPrice?: number;
    maxPrice?: number;
    bedrooms?: number;
    bathrooms?: number;
    code?: string;
    isFeaturedOnly?: boolean;
  }>({
    propertyTypeId: searchParams.get('propertyTypeId') ? Number(searchParams.get('propertyTypeId')) : undefined,
    saleTypeId: searchParams.get('saleTypeId') ? Number(searchParams.get('saleTypeId')) : undefined,
    minPrice: searchParams.get('minPrice') ? Number(searchParams.get('minPrice')) : undefined,
    maxPrice: searchParams.get('maxPrice') ? Number(searchParams.get('maxPrice')) : undefined,
    bedrooms: searchParams.get('bedrooms') ? Number(searchParams.get('bedrooms')) : undefined,
    bathrooms: searchParams.get('bathrooms') ? Number(searchParams.get('bathrooms')) : undefined,
    code: searchParams.get('code') || undefined,
    isFeaturedOnly: searchParams.get('isFeaturedOnly') === 'true',
  });

  const loadData = async () => {
    try {
      setIsLoading(true);
      const [props, types, sales] = await Promise.all([
        propertiesService.getAll(filters),
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
        } catch (e) {
          // ignore favorites error if unauthorized
        }
      }
    } catch (err) {
      console.error("Error loading catalog:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [filters]);

  const handleFilterChange = (newFilters: typeof filters) => {
    setFilters(newFilters);
    const params = new URLSearchParams();
    Object.entries(newFilters).forEach(([k, v]) => {
      if (v !== undefined && v !== '') params.set(k, String(v));
    });
    setSearchParams(params);
  };

  const handleResetFilters = () => {
    setFilters({});
    setSearchParams({});
  };

  // Sorting
  const sortedProperties = [...properties].sort((a, b) => {
    if (sortBy === 'price-asc') return a.price - b.price;
    if (sortBy === 'price-desc') return b.price - a.price;
    if (sortBy === 'bedrooms-desc') return b.bedrooms - a.bedrooms;
    if (sortBy === 'featured') return (b.isFeatured ? 1 : 0) - (a.isFeatured ? 1 : 0);
    return b.id - a.id;
  });

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      
      {/* Header Banner */}
      <div className="bg-navy-950 text-white rounded-3xl p-6 sm:p-10 relative overflow-hidden shadow-xl">
        <div className="relative z-10 max-w-2xl space-y-2">
          <span className="text-brand-400 font-extrabold text-xs uppercase tracking-widest">
            Catálogo Oficial de Inmuebles
          </span>
          <h1 className="text-2xl sm:text-4xl font-extrabold tracking-tight">
            Propiedades Exclusivas en RD$
          </h1>
          <p className="text-xs sm:text-sm text-slate-300">
            Filtra por tipo de propiedad, amenidades, habitaciones y rango de precios con cálculo hipotecario en tiempo real.
          </p>
        </div>
      </div>

      {/* Main Layout: Filters Sidebar + Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
        
        {/* Sidebar Filters */}
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

        {/* Catalog Grid View */}
        <div className="lg:col-span-3 space-y-6">
          
          {/* Controls Bar: Total counts, sorting, view mode */}
          <div className="bg-white rounded-2xl p-4 border border-slate-200 shadow-xs flex flex-wrap items-center justify-between gap-4 text-xs font-semibold text-slate-700">
            <div className="flex items-center gap-2">
              <SlidersHorizontal className="w-4 h-4 text-brand-600" />
              <span>Mostrando <strong>{sortedProperties.length}</strong> inmuebles disponibles</span>
            </div>

            <div className="flex items-center gap-3">
              {/* Sort Selector */}
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

              {/* View Mode Toggle */}
              <div className="flex border border-slate-200 rounded-lg overflow-hidden">
                <button
                  onClick={() => setViewMode('grid')}
                  className={`p-1.5 ${viewMode === 'grid' ? 'bg-slate-100 text-brand-600' : 'text-slate-400 hover:text-slate-700'}`}
                  title="Vista Cuadrícula"
                >
                  <LayoutGrid className="w-4 h-4" />
                </button>
                <button
                  onClick={() => setViewMode('list')}
                  className={`p-1.5 ${viewMode === 'list' ? 'bg-slate-100 text-brand-600' : 'text-slate-400 hover:text-slate-700'}`}
                  title="Vista Lista"
                >
                  <List className="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>

          {/* Properties List */}
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
    </div>
  );
};
