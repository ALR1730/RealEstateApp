import React from 'react';
import { PropertyType, SaleType } from '../../types';
import { Search, Filter, RotateCcw, Building2, Tag, DollarSign, Bed, Bath } from 'lucide-react';

interface FilterState {
  propertyTypeId?: number;
  saleTypeId?: number;
  minPrice?: number;
  maxPrice?: number;
  bedrooms?: number;
  bathrooms?: number;
  code?: string;
  isFeaturedOnly?: boolean;
}

interface PropertyFilterProps {
  filters: FilterState;
  onChange: (filters: FilterState) => void;
  onReset: () => void;
  propertyTypes: PropertyType[];
  saleTypes: SaleType[];
}

export const PropertyFilter: React.FC<PropertyFilterProps> = ({
  filters,
  onChange,
  onReset,
  propertyTypes,
  saleTypes,
}) => {
  const handleChange = (field: keyof FilterState, value: any) => {
    onChange({
      ...filters,
      [field]: value === '' ? undefined : value,
    });
  };

  return (
    <div className="bg-white rounded-3xl p-5 sm:p-6 border border-slate-200 shadow-xs space-y-6">
      
      {/* Header */}
      <div className="flex items-center justify-between pb-3 border-b border-slate-100">
        <div className="flex items-center gap-2">
          <Filter className="w-5 h-5 text-brand-600" />
          <h3 className="font-bold text-base text-slate-900">Filtros de Búsqueda</h3>
        </div>
        <button
          onClick={onReset}
          className="flex items-center gap-1 text-xs font-semibold text-slate-500 hover:text-rose-600 transition-colors"
        >
          <RotateCcw className="w-3.5 h-3.5" />
          Limpiar
        </button>
      </div>

      <div className="space-y-4">
        
        {/* Código de Propiedad */}
        <div>
          <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
            Código Único
          </label>
          <div className="relative">
            <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
            <input
              type="text"
              placeholder="Ej: 104928"
              value={filters.code || ''}
              onChange={(e) => handleChange('code', e.target.value)}
              className="w-full pl-9 pr-3 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-slate-50/50"
            />
          </div>
        </div>

        {/* Tipo de Propiedad */}
        <div>
          <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
            <Building2 className="w-3.5 h-3.5 text-brand-600" />
            Tipo de Propiedad
          </label>
          <select
            value={filters.propertyTypeId || ''}
            onChange={(e) => handleChange('propertyTypeId', e.target.value ? Number(e.target.value) : undefined)}
            className="w-full px-3 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-white"
          >
            <option value="">Todos los Tipos</option>
            {propertyTypes.map((pt) => (
              <option key={pt.id} value={pt.id}>
                {pt.name} {pt.propertiesCount !== undefined ? `(${pt.propertiesCount})` : ''}
              </option>
            ))}
          </select>
        </div>

        {/* Tipo de Venta */}
        <div>
          <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
            <Tag className="w-3.5 h-3.5 text-brand-600" />
            Modalidad de Negocio
          </label>
          <select
            value={filters.saleTypeId || ''}
            onChange={(e) => handleChange('saleTypeId', e.target.value ? Number(e.target.value) : undefined)}
            className="w-full px-3 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-white"
          >
            <option value="">Todas las Modalidades</option>
            {saleTypes.map((st) => (
              <option key={st.id} value={st.id}>
                {st.name} {st.propertiesCount !== undefined ? `(${st.propertiesCount})` : ''}
              </option>
            ))}
          </select>
        </div>

        {/* Rango de Precios RD$ */}
        <div>
          <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
            <DollarSign className="w-3.5 h-3.5 text-brand-600" />
            Rango de Precios (RD$)
          </label>
          <div className="grid grid-cols-2 gap-2">
            <input
              type="number"
              placeholder="Mínimo RD$"
              value={filters.minPrice || ''}
              onChange={(e) => handleChange('minPrice', e.target.value ? Number(e.target.value) : undefined)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
            />
            <input
              type="number"
              placeholder="Máximo RD$"
              value={filters.maxPrice || ''}
              onChange={(e) => handleChange('maxPrice', e.target.value ? Number(e.target.value) : undefined)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
            />
          </div>
        </div>

        {/* Habitaciones y Baños */}
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1">
              <Bed className="w-3.5 h-3.5 text-brand-600" />
              Habitaciones
            </label>
            <select
              value={filters.bedrooms || ''}
              onChange={(e) => handleChange('bedrooms', e.target.value ? Number(e.target.value) : undefined)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-white"
            >
              <option value="">Cualquiera</option>
              <option value="1">1+</option>
              <option value="2">2+</option>
              <option value="3">3+</option>
              <option value="4">4+</option>
              <option value="5">5+</option>
            </select>
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1">
              <Bath className="w-3.5 h-3.5 text-brand-600" />
              Baños
            </label>
            <select
              value={filters.bathrooms || ''}
              onChange={(e) => handleChange('bathrooms', e.target.value ? Number(e.target.value) : undefined)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-white"
            >
              <option value="">Cualquiera</option>
              <option value="1">1+</option>
              <option value="2">2+</option>
              <option value="3">3+</option>
              <option value="4">4+</option>
            </select>
          </div>
        </div>

        {/* Solo Destacadas Switch */}
        <div className="pt-2">
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={filters.isFeaturedOnly || false}
              onChange={(e) => handleChange('isFeaturedOnly', e.target.checked)}
              className="w-4 h-4 rounded text-brand-600 focus:ring-brand-500 border-slate-300"
            />
            <span className="text-xs font-bold text-slate-700">Solo Inmuebles Destacados ⭐</span>
          </label>
        </div>
      </div>
    </div>
  );
};
