import React, { useState, useEffect } from 'react';
import { PropertyType, SaleType, Improvement, Province, FilterState } from '../../types';
import { catalogsService, provincesService, adminService } from '../../api/services';
import { Search, Filter, RotateCcw, Building2, Tag, DollarSign, Bed, Bath, MapPin, Maximize2, ShieldCheck, Banknote, Eye, Wrench, Navigation } from 'lucide-react';

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
  const [provinces, setProvinces] = useState<Province[]>([]);
  const [municipalities, setMunicipalities] = useState<{ id: number; name: string }[]>([]);
  const [improvements, setImprovements] = useState<Improvement[]>([]);
  const [agents, setAgents] = useState<any[]>([]);
  const [selectedProvinceId, setSelectedProvinceId] = useState<number | undefined>(filters.provinceId);

  useEffect(() => {
    Promise.all([
      provincesService.getAll().catch(() => []),
      catalogsService.getImprovements().catch(() => []),
      adminService.getAgents().catch(() => []),
    ]).then(([provs, imps, ags]) => {
      setProvinces(provs);
      setImprovements(imps);
      setAgents(ags);
    });
  }, []);

  useEffect(() => {
    if (selectedProvinceId) {
      provincesService.getMunicipalities(selectedProvinceId).then(setMunicipalities).catch(() => setMunicipalities([]));
    }
  }, [selectedProvinceId]);

  const resolvedMunicipalities = selectedProvinceId ? municipalities : [];

  const handleChange = (field: keyof FilterState, value: any) => {
    onChange({
      ...filters,
      [field]: value === '' || value === null ? undefined : value,
    });
  };

  const handleProvinceChange = (provinceId: number | undefined) => {
    setSelectedProvinceId(provinceId);
    handleChange('provinceId', provinceId);
    handleChange('municipalityId', undefined);
  };

  const toggleImprovement = (id: number) => {
    const current = filters.improvementIds || [];
    const next = current.includes(id) ? current.filter((i) => i !== id) : [...current, id];
    handleChange('improvementIds', next.length > 0 ? next : undefined);
  };

  const handleNearMe = () => {
    if (!navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        onChange({
          ...filters,
          userLat: pos.coords.latitude,
          userLng: pos.coords.longitude,
        });
      },
      () => {
        console.warn('No se pudo obtener la ubicación');
      }
    );
  };

  return (
    <div className="bg-white rounded-3xl p-5 sm:p-6 border border-slate-200 shadow-xs space-y-6 max-h-[calc(100vh-8rem)] overflow-y-auto">

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

        {/* Código */}
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

        {/* Provincia → Municipio → Sector */}
        <div>
          <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
            <MapPin className="w-3.5 h-3.5 text-brand-600" />
            Ubicación
          </label>
          <div className="space-y-2">
            <select
              value={filters.provinceId || ''}
              onChange={(e) => handleProvinceChange(e.target.value ? Number(e.target.value) : undefined)}
              className="w-full px-3 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-white"
            >
              <option value="">Todas las Provincias</option>
              {provinces.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
            {resolvedMunicipalities.length > 0 && (
              <select
                value={filters.municipalityId || ''}
                onChange={(e) => handleChange('municipalityId', e.target.value ? Number(e.target.value) : undefined)}
                className="w-full px-3 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-white"
              >
                <option value="">Todos los Municipios</option>
                {resolvedMunicipalities.map((m) => (
                  <option key={m.id} value={m.id}>{m.name}</option>
                ))}
              </select>
            )}
            <input
              type="text"
              placeholder="Sector / Barrio"
              value={filters.sector || ''}
              onChange={(e) => handleChange('sector', e.target.value)}
              className="w-full px-3 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-white"
            />
          </div>
        </div>

        {/* Cerca de mí */}
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={handleNearMe}
            className="flex items-center gap-1.5 px-3 py-2 text-xs font-semibold rounded-xl border border-brand-200 bg-brand-50 text-brand-700 hover:bg-brand-100 transition-colors"
          >
            <Navigation className="w-3.5 h-3.5" />
            Cerca de Mí
          </button>
          {filters.userLat && (
            <div className="flex items-center gap-2">
              <input
                type="number"
                placeholder="km"
                value={filters.maxDistanceKm || ''}
                onChange={(e) => handleChange('maxDistanceKm', e.target.value ? Number(e.target.value) : undefined)}
                className="w-20 px-2 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20"
              />
              <span className="text-[11px] text-slate-500">km</span>
            </div>
          )}
        </div>

        {/* Rango de Precios */}
        <div>
          <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
            <DollarSign className="w-3.5 h-3.5 text-brand-600" />
            Rango de Precios (RD$)
          </label>
          <div className="grid grid-cols-2 gap-2">
            <input
              type="number"
              placeholder="Mínimo"
              value={filters.minPrice || ''}
              onChange={(e) => handleChange('minPrice', e.target.value ? Number(e.target.value) : undefined)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
            />
            <input
              type="number"
              placeholder="Máximo"
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
              value={filters.minRooms || ''}
              onChange={(e) => handleChange('minRooms', e.target.value ? Number(e.target.value) : undefined)}
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
              value={filters.minBathrooms || ''}
              onChange={(e) => handleChange('minBathrooms', e.target.value ? Number(e.target.value) : undefined)}
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

        {/* Tamaño */}
        <div>
          <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
            <Maximize2 className="w-3.5 h-3.5 text-brand-600" />
            Tamaño (m²)
          </label>
          <div className="grid grid-cols-2 gap-2">
            <input
              type="number"
              placeholder="Mín m²"
              value={filters.minSizeInMeters || ''}
              onChange={(e) => handleChange('minSizeInMeters', e.target.value ? Number(e.target.value) : undefined)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20"
            />
            <input
              type="number"
              placeholder="Máx m²"
              value={filters.maxSizeInMeters || ''}
              onChange={(e) => handleChange('maxSizeInMeters', e.target.value ? Number(e.target.value) : undefined)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20"
            />
          </div>
        </div>

        {/* Filtro por Agente */}
        {agents.length > 0 && (
          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
              Agente
            </label>
            <select
              value={filters.agentId || ''}
              onChange={(e) => handleChange('agentId', e.target.value || undefined)}
              className="w-full px-3 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-white"
            >
              <option value="">Todos los Agentes</option>
              {agents.map((ag) => (
                <option key={ag.id} value={ag.id}>
                  {ag.firstName} {ag.lastName}
                </option>
              ))}
            </select>
          </div>
        )}

        {/* Toggles especiales */}
        <div className="space-y-2.5 pt-2">
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={filters.onlyFeatured || false}
              onChange={(e) => handleChange('onlyFeatured', e.target.checked || undefined)}
              className="w-4 h-4 rounded text-brand-600 focus:ring-brand-500 border-slate-300"
            />
            <span className="text-xs font-bold text-slate-700">Solo Destacados</span>
          </label>
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={filters.onlyVerifiedAgents || false}
              onChange={(e) => handleChange('onlyVerifiedAgents', e.target.checked || undefined)}
              className="w-4 h-4 rounded text-brand-600 focus:ring-brand-500 border-slate-300"
            />
            <span className="text-xs font-bold text-slate-700 flex items-center gap-1">
              <ShieldCheck className="w-3 h-3 text-emerald-600" />
              Agentes Verificados
            </span>
          </label>
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={filters.onlyFinanciable || false}
              onChange={(e) => handleChange('onlyFinanciable', e.target.checked || undefined)}
              className="w-4 h-4 rounded text-brand-600 focus:ring-brand-500 border-slate-300"
            />
            <span className="text-xs font-bold text-slate-700 flex items-center gap-1">
              <Banknote className="w-3 h-3 text-amber-600" />
              Financiable
            </span>
          </label>
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={filters.onlyWithVirtualTour || false}
              onChange={(e) => handleChange('onlyWithVirtualTour', e.target.checked || undefined)}
              className="w-4 h-4 rounded text-brand-600 focus:ring-brand-500 border-slate-300"
            />
            <span className="text-xs font-bold text-slate-700 flex items-center gap-1">
              <Eye className="w-3 h-3 text-indigo-600" />
              Con Tour Virtual
            </span>
          </label>
        </div>

        {/* Mejoras / Amenidades */}
        {improvements.length > 0 && (
          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
              <Wrench className="w-3.5 h-3.5 text-brand-600" />
              Amenidades
            </label>
            <div className="flex flex-wrap gap-1.5">
              {improvements.map((imp) => {
                const isActive = (filters.improvementIds || []).includes(imp.id);
                return (
                  <button
                    key={imp.id}
                    type="button"
                    onClick={() => toggleImprovement(imp.id)}
                    className={`px-2.5 py-1 text-[11px] font-semibold rounded-lg border transition-colors ${
                      isActive
                        ? 'bg-brand-600 text-white border-brand-600'
                        : 'bg-slate-50 text-slate-600 border-slate-200 hover:border-brand-300'
                    }`}
                  >
                    {imp.name}
                  </button>
                );
              })}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
