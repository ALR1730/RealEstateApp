import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Property, PropertyType, SaleType, AiSearchInterpretation, User } from '../../types';
import { propertiesService, catalogsService, agentsService } from '../../api/services';
import { PropertyCard } from '../../components/properties/PropertyCard';
import { MortgageCalculator } from '../../components/simulator/MortgageCalculator';
import { Loader } from '../../components/common/Loader';
import { AiSearchBar } from '../../components/properties/AiSearchBar';
import { 
  Search, 
  Sparkles, 
  ShieldCheck, 
  Calculator, 
  TrendingUp, 
  ArrowRight, 
  Users, 
  Flame
} from 'lucide-react';

export const HomePage: React.FC = () => {
  const navigate = useNavigate();
  const [featuredProperties, setFeaturedProperties] = useState<Property[]>([]);
  const [propertyTypes, setPropertyTypes] = useState<PropertyType[]>([]);
  const [saleTypes, setSaleTypes] = useState<SaleType[]>([]);
  const [agents, setAgents] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // Search mode: 'ai' | 'standard'
  const [searchMode, setSearchMode] = useState<'ai' | 'standard'>('ai');

  // Quick search state
  const [selectedType, setSelectedType] = useState<string>('');
  const [selectedSaleType, setSelectedSaleType] = useState<string>('');
  const [keyword, setKeyword] = useState<string>('');

  const handleAiSearchComplete = (result: AiSearchInterpretation) => {
    if (!result || !result.parsedFilter) return;
    const params = new URLSearchParams();
    const f = result.parsedFilter;
    if (f.propertyTypeId) params.set('propertyTypeId', String(f.propertyTypeId));
    if (f.saleTypeId) params.set('saleTypeId', String(f.saleTypeId));
    if (f.provinceId) params.set('provinceId', String(f.provinceId));
    if (f.municipalityId) params.set('municipalityId', String(f.municipalityId));
    if (f.sector) params.set('sector', f.sector);
    if (f.minRooms) params.set('minRooms', String(f.minRooms));
    if (f.minBathrooms) params.set('minBathrooms', String(f.minBathrooms));
    if (f.minPrice) params.set('minPrice', String(f.minPrice));
    if (f.maxPrice) params.set('maxPrice', String(f.maxPrice));
    if (f.onlyWithVirtualTour) params.set('onlyWithVirtualTour', 'true');
    if (f.onlyFinanciable) params.set('onlyFinanciable', 'true');
    if (f.onlyFeatured) params.set('onlyFeatured', 'true');
    if (Array.isArray(f.improvementIds)) {
      f.improvementIds.forEach((id: number) => params.append('improvementIds', String(id)));
    }
    navigate(`/catalog?${params.toString()}`);
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        const [propsData, typesData, salesData, agentsData] = await Promise.all([
          propertiesService.getAll(),
          catalogsService.getPropertyTypes(),
          catalogsService.getSaleTypes(),
          agentsService.getPublicAgents().catch(() => []),
        ]);

        setFeaturedProperties(propsData.slice(0, 6));
        setPropertyTypes(typesData);
        setSaleTypes(salesData);
        setAgents(agentsData.slice(0, 4));
      } catch (err) {
        console.error("Error loading homepage data:", err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchData();
  }, []);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const params = new URLSearchParams();
    if (selectedType) params.append('propertyTypeId', selectedType);
    if (selectedSaleType) params.append('saleTypeId', selectedSaleType);
    if (keyword) params.append('code', keyword);
    navigate(`/catalog?${params.toString()}`);
  };

  return (
    <div className="space-y-16 sm:space-y-24 pb-20">
      
      {/* HERO SECTION */}
      <section className="relative min-h-[580px] sm:min-h-[640px] rounded-3xl sm:rounded-[2.5rem] overflow-hidden mx-4 sm:mx-8 lg:mx-12 mt-4 bg-navy-950 flex items-center shadow-2xl">
        
        {/* Background Image with Dark Vignette */}
        <div className="absolute inset-0 z-0">
          <img
            src="https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?auto=format&fit=crop&w=2000&q=80"
            alt="Luxury Real Estate Dominican Republic"
            className="w-full h-full object-cover object-center opacity-35 scale-105 animate-in fade-in duration-1000"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-navy-950 via-navy-950/60 to-transparent" />
          <div className="absolute inset-0 bg-gradient-to-r from-navy-950 via-navy-950/80 to-transparent" />
        </div>

        {/* Hero Content */}
        <div className="relative z-10 max-w-5xl mx-auto px-6 sm:px-12 py-16 text-center sm:text-left space-y-8">
          
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-brand-500/10 border border-brand-500/30 text-brand-400 text-xs font-extrabold uppercase tracking-widest backdrop-blur-xs">
            <Sparkles className="w-4 h-4 text-brand-400 animate-spin-slow" />
            Ecosistema Inmobiliario de Alto Rendimiento en RD$
          </div>

          <div className="space-y-4 max-w-3xl">
            <h1 className="text-3xl sm:text-5xl lg:text-6xl font-extrabold text-white tracking-tight leading-[1.15]">
              Encuentra, Negocia y Cierra tu Inmueble Soñado en <span className="bg-gradient-to-r from-emerald-400 via-brand-400 to-teal-300 bg-clip-text text-transparent">República Dominicana</span>
            </h1>
            <p className="text-sm sm:text-lg text-slate-300 font-normal leading-relaxed">
              Catálogo verificado con transacciones garantizadas, simulación hipotecaria en cuotas fijas bajo el sistema francés y cierre atómico de ofertas.
            </p>
          </div>

          {/* Mode Switcher */}
          <div className="flex flex-wrap items-center justify-center sm:justify-start gap-2">
            <button
              type="button"
              onClick={() => setSearchMode('ai')}
              className={`flex items-center gap-2 px-3.5 sm:px-4 py-2 rounded-xl sm:rounded-2xl text-xs sm:text-sm font-extrabold transition-all cursor-pointer ${
                searchMode === 'ai'
                  ? 'bg-gradient-to-r from-brand-600 to-teal-500 text-white shadow-lg shadow-brand-500/25 border border-brand-400/40'
                  : 'bg-slate-900/60 text-slate-300 hover:text-white border border-slate-700/60'
              }`}
            >
              <Sparkles className="w-3.5 h-3.5 text-teal-300 animate-spin-slow" />
              <span>Búsqueda Inteligente con IA (NLP)</span>
            </button>
            <button
              type="button"
              onClick={() => setSearchMode('standard')}
              className={`flex items-center gap-2 px-3.5 sm:px-4 py-2 rounded-xl sm:rounded-2xl text-xs sm:text-sm font-extrabold transition-all cursor-pointer ${
                searchMode === 'standard'
                  ? 'bg-brand-600 text-white shadow-lg shadow-brand-600/25 border border-brand-400/40'
                  : 'bg-slate-900/60 text-slate-300 hover:text-white border border-slate-700/60'
              }`}
            >
              <Search className="w-3.5 h-3.5" />
              <span>Búsqueda Estándar</span>
            </button>
          </div>

          {searchMode === 'ai' ? (
            /* AI Natural Language Search Bar */
            <AiSearchBar
              variant="hero"
              onSearchComplete={handleAiSearchComplete}
            />
          ) : (
            /* Quick Search Floating Bar */
            <form
              onSubmit={handleSearchSubmit}
              className="glass-card-dark p-3 sm:p-4 rounded-2xl sm:rounded-3xl shadow-2xl border border-white/15 max-w-4xl grid grid-cols-1 sm:grid-cols-4 gap-3 text-left"
            >
              {/* Field 1: Property Type */}
              <div>
                <label className="block text-[11px] font-extrabold text-slate-300 uppercase tracking-wider mb-1">
                  Tipo de Propiedad
                </label>
                <select
                  value={selectedType}
                  onChange={(e) => setSelectedType(e.target.value)}
                  className="w-full px-3 py-2.5 bg-slate-800/80 border border-slate-700 text-white rounded-xl text-xs sm:text-sm focus:outline-none focus:ring-2 focus:ring-brand-500"
                >
                  <option value="">Todos los tipos</option>
                  {propertyTypes.map((pt) => (
                    <option key={pt.id} value={pt.id}>
                      {pt.name}
                    </option>
                  ))}
                </select>
              </div>

              {/* Field 2: Sale Type */}
              <div>
                <label className="block text-[11px] font-extrabold text-slate-300 uppercase tracking-wider mb-1">
                  Modalidad
                </label>
                <select
                  value={selectedSaleType}
                  onChange={(e) => setSelectedSaleType(e.target.value)}
                  className="w-full px-3 py-2.5 bg-slate-800/80 border border-slate-700 text-white rounded-xl text-xs sm:text-sm focus:outline-none focus:ring-2 focus:ring-brand-500"
                >
                  <option value="">Cualquier modalidad</option>
                  {saleTypes.map((st) => (
                    <option key={st.id} value={st.id}>
                      {st.name}
                    </option>
                  ))}
                </select>
              </div>

              {/* Field 3: Code / Keyword */}
              <div>
                <label className="block text-[11px] font-extrabold text-slate-300 uppercase tracking-wider mb-1">
                  Código o Sector
                </label>
                <input
                  type="text"
                  placeholder="Ej: 104928, Piantini..."
                  value={keyword}
                  onChange={(e) => setKeyword(e.target.value)}
                  className="w-full px-3 py-2.5 bg-slate-800/80 border border-slate-700 text-white placeholder-slate-400 rounded-xl text-xs sm:text-sm focus:outline-none focus:ring-2 focus:ring-brand-500"
                />
              </div>

              {/* Search Button */}
              <div className="flex items-end">
                <button
                  type="submit"
                  className="w-full h-[42px] bg-brand-600 hover:bg-brand-500 text-white font-extrabold text-xs sm:text-sm rounded-xl flex items-center justify-center gap-2 shadow-lg shadow-brand-600/30 transition-all cursor-pointer"
                >
                  <Search className="w-4 h-4" />
                  Buscar Inmuebles
                </button>
              </div>
            </form>
          )}

          {/* Quick Metrics Chips */}
          <div className="flex flex-wrap items-center justify-center sm:justify-start gap-4 sm:gap-8 pt-2 text-white">
            <div className="flex items-center gap-2">
              <div className="w-2.5 h-2.5 rounded-full bg-emerald-400 animate-pulse" />
              <span className="text-xs font-semibold text-slate-300">Portafolio en Pesos Dominicanos (RD$)</span>
            </div>
            <div className="flex items-center gap-2">
              <ShieldCheck className="w-4 h-4 text-emerald-400" />
              <span className="text-xs font-semibold text-slate-300">Cierre Garantizado sin Duplicidad</span>
            </div>
          </div>
        </div>
      </section>

      {/* FEATURED PROPERTIES SECTION */}
      <section className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 space-y-8">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-end gap-4">
          <div>
            <div className="flex items-center gap-2 text-brand-600 font-extrabold text-xs tracking-wider uppercase mb-1">
              <Flame className="w-4 h-4" />
              Oportunidades Destacadas
            </div>
            <h2 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
              Propiedades Disponibles en RD$
            </h2>
          </div>
          <Link
            to="/catalog"
            className="flex items-center gap-1.5 text-xs sm:text-sm font-bold text-brand-600 hover:text-brand-700 group"
          >
            <span>Ver todo el catálogo</span>
            <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
          </Link>
        </div>

        {isLoading ? (
          <Loader text="Cargando propiedades destacadas..." />
        ) : featuredProperties.length === 0 ? (
          <div className="p-12 text-center bg-white rounded-3xl border border-slate-200 text-slate-500">
            No hay propiedades registradas aún. Inicia sesión como Agente para publicar la primera.
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 sm:gap-8">
            {featuredProperties.map((prop) => (
              <PropertyCard key={prop.id} property={prop} />
            ))}
          </div>
        )}
      </section>

      {/* VALUE PROPOSITION / PILLARS */}
      <section className="bg-gradient-to-b from-slate-900 to-navy-950 text-white py-16 sm:py-24 rounded-3xl sm:rounded-[3rem] mx-4 sm:mx-8 px-6 sm:px-12">
        <div className="max-w-7xl mx-auto space-y-16">
          <div className="text-center max-w-3xl mx-auto space-y-3">
            <span className="text-brand-400 text-xs font-extrabold tracking-widest uppercase">
              ¿Por qué RealEstateApp?
            </span>
            <h2 className="text-2xl sm:text-4xl font-extrabold tracking-tight">
              Innovación Transaccional para la Industria Inmobiliaria
            </h2>
            <p className="text-sm sm:text-base text-slate-400">
              Diseñado desde cero para resolver las ineficiencias del mercado tradicional dominicano.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            
            {/* Pillar 1 */}
            <div className="p-8 rounded-3xl bg-white/5 border border-white/10 backdrop-blur-xs space-y-4 hover:border-brand-500/50 transition-colors">
              <div className="w-12 h-12 rounded-2xl bg-emerald-500/20 text-emerald-400 flex items-center justify-center font-bold">
                <TrendingUp className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white">Regla Atómica de Cierre</h3>
              <p className="text-xs sm:text-sm text-slate-300 leading-relaxed">
                Al aceptar una propuesta económica, el inmueble se marca instantáneamente como Vendido, rechazando automáticamente ofertas pendientes en cascada.
              </p>
            </div>

            {/* Pillar 2 */}
            <div className="p-8 rounded-3xl bg-white/5 border border-white/10 backdrop-blur-xs space-y-4 hover:border-brand-500/50 transition-colors">
              <div className="w-12 h-12 rounded-2xl bg-brand-500/20 text-brand-400 flex items-center justify-center font-bold">
                <Calculator className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white">Simulador Hipotecario Francés</h3>
              <p className="text-xs sm:text-sm text-slate-300 leading-relaxed">
                Cálculo instantáneo de cuota mensual en Pesos Dominicanos (RD$), tabla de amortización completa y descarga de desglose en PDF antes de ofertar.
              </p>
            </div>

            {/* Pillar 3 */}
            <div className="p-8 rounded-3xl bg-white/5 border border-white/10 backdrop-blur-xs space-y-4 hover:border-brand-500/50 transition-colors">
              <div className="w-12 h-12 rounded-2xl bg-royal-500/20 text-indigo-400 flex items-center justify-center font-bold">
                <Users className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-bold text-white">Comunicación Directa & Citas</h3>
              <p className="text-xs sm:text-sm text-slate-300 leading-relaxed">
                Mensajería en tiempo real por propiedad entre cliente y agente, agendamiento de visitas presenciales y trazabilidad sin fuga de prospectos.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* EMBEDDED MORTGAGE CALCULATOR SECTION */}
      <section className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <MortgageCalculator initialPrice={6500000} />
      </section>

      {/* TOP AGENTS SECTION */}
      {agents.length > 0 && (
        <section className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 space-y-8">
          <div className="flex justify-between items-end">
            <div>
              <span className="text-xs font-extrabold uppercase tracking-widest text-brand-600">
                Fuerza de Ventas Verificada
              </span>
              <h2 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight mt-1">
                Nuestros Corredores Inmobiliarios
              </h2>
            </div>
            <Link to="/agents" className="text-xs sm:text-sm font-bold text-brand-600 hover:text-brand-700 flex items-center gap-1">
              <span>Ver todos</span>
              <ArrowRight className="w-4 h-4" />
            </Link>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {agents.map((ag) => (
              <div key={ag.id} className="bg-white rounded-3xl p-6 border border-slate-200 text-center space-y-4 hover:shadow-lg transition-all">
                <div className="w-20 h-20 mx-auto rounded-full bg-gradient-to-tr from-navy-800 to-slate-700 text-white flex items-center justify-center font-bold text-xl overflow-hidden shadow-md">
                  {ag.photoUrl ? (
                    <img src={ag.photoUrl} alt="" className="w-full h-full object-cover" />
                  ) : (
                    ag.firstName ? ag.firstName[0] : 'A'
                  )}
                </div>
                <div>
                  <h4 className="font-bold text-slate-900 text-base">
                    {ag.firstName} {ag.lastName}
                  </h4>
                  <p className="text-xs text-slate-500">{ag.email}</p>
                  {ag.phone && <p className="text-xs font-semibold text-brand-600 mt-1">{ag.phone}</p>}
                </div>
                <div className="pt-2 border-t border-slate-100 flex justify-center gap-2">
                  <span className="text-[11px] font-bold px-2.5 py-1 bg-slate-100 text-slate-700 rounded-lg">
                    {ag.propertiesCount || 0} Inmuebles
                  </span>
                </div>
              </div>
            ))}
          </div>
        </section>
      )}

      {/* CALL TO ACTION BANNER */}
      <section className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="bg-gradient-to-r from-brand-600 to-emerald-700 rounded-3xl sm:rounded-[2.5rem] p-8 sm:p-14 text-white flex flex-col md:flex-row items-center justify-between gap-8 shadow-xl">
          <div className="space-y-3 text-center md:text-left max-w-xl">
            <h3 className="text-2xl sm:text-3xl font-extrabold tracking-tight">
              ¿Listo para publicar tus propiedades o encontrar tu próximo hogar?
            </h3>
            <p className="text-xs sm:text-sm text-emerald-100 leading-relaxed">
              Crea tu cuenta gratuita hoy mismo o contacta a nuestros asesores inmobiliarios certificados.
            </p>
          </div>
          <div className="flex flex-wrap gap-3">
            <Link
              to="/register"
              className="px-6 py-3 bg-white text-slate-900 hover:bg-slate-100 font-extrabold text-xs sm:text-sm rounded-xl shadow-lg transition-all"
            >
              Crear Cuenta de Cliente
            </Link>
            <Link
              to="/register-agent"
              className="px-6 py-3 bg-emerald-900 hover:bg-emerald-950 text-white font-extrabold text-xs sm:text-sm rounded-xl transition-all"
            >
              Registro como Agente
            </Link>
          </div>
        </div>
      </section>

    </div>
  );
};
