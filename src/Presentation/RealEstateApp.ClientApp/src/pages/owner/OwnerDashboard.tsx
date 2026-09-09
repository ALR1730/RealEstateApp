import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { ownersService } from '../../api/services';
import { Property } from '../../types';
import { formatCurrencyRD } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Home, PlusCircle, Trash2, ExternalLink, AlertCircle, Pencil, BedDouble, Bath, Ruler, MapPin } from 'lucide-react';

type StatusFilter = 'All' | 'Available' | 'Reserved' | 'Sold';

export const OwnerDashboard: React.FC = () => {
  const [properties, setProperties] = useState<Property[]>([]);
  const [canCreate, setCanCreate] = useState(true);
  const [maxAllowed, setMaxAllowed] = useState(2);
  const [isLoading, setIsLoading] = useState(true);
  const [filter, setFilter] = useState<StatusFilter>('All');

  const loadData = async () => {
    try {
      const data = await ownersService.getMyProperties();
      setProperties(data.properties || []);
      setCanCreate(data.canCreate);
      setMaxAllowed(data.maxAllowed || 2);
    } catch (err) {
      console.error("Error loading owner dashboard:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadData()).catch(console.error);
  }, []);

  const handleDelete = async (id: number) => {
    if (!window.confirm("¿Seguro que deseas retirar este inmueble directo?")) return;
    try {
      await ownersService.deleteProperty(id);
      loadData();
    } catch (err) {
      console.error("Error deleting property:", err);
    }
  };

  const filteredProperties = filter === 'All'
    ? properties
    : properties.filter((p) => p.status === filter);

  const filters: { value: StatusFilter; label: string }[] = [
    { value: 'All', label: 'Todas' },
    { value: 'Available', label: 'Disponibles' },
    { value: 'Reserved', label: 'Reservadas' },
    { value: 'Sold', label: 'Vendidas' },
  ];

  if (isLoading) return <Loader text="Cargando panel de propietario directo..." />;

  return (
    <div className="space-y-8">

      {/* Header Banner */}
      <div className="bg-gradient-to-r from-navy-950 to-slate-900 text-white p-6 sm:p-8 rounded-3xl shadow-xl flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <span className="text-amber-400 text-xs font-extrabold uppercase tracking-widest">
            Portal de Propietario Directo
          </span>
          <h1 className="text-2xl sm:text-3xl font-extrabold tracking-tight mt-1">
            Mis Inmuebles en Venta Directa
          </h1>
          <p className="text-xs text-slate-300 mt-1">
            Publica hasta 2 propiedades sin comisión de corretaje para recibir compradores calificados.
          </p>
        </div>

        {canCreate ? (
          <Link
            to="/owner/properties/create"
            className="px-5 py-2.5 bg-amber-500 hover:bg-amber-600 text-navy-950 font-extrabold text-xs rounded-xl shadow-lg transition-all flex items-center gap-2"
          >
            <PlusCircle className="w-4 h-4" />
            <span>Publicar Inmueble Directo</span>
          </Link>
        ) : (
          <div className="px-4 py-2 bg-slate-800 rounded-xl text-xs font-semibold text-slate-300 flex items-center gap-1.5 border border-slate-700">
            <AlertCircle className="w-4 h-4 text-amber-400" />
            <span>Límite de {maxAllowed} inmuebles alcanzado</span>
          </div>
        )}
      </div>

      {/* Status Filters */}
      {properties.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {filters.map((f) => (
            <button
              key={f.value}
              onClick={() => setFilter(f.value)}
              className={`px-4 py-1.5 rounded-full text-xs font-bold transition-all border ${
                filter === f.value
                  ? 'bg-navy-950 text-white border-navy-950'
                  : 'bg-white text-slate-600 border-slate-200 hover:border-slate-300'
              }`}
            >
              {f.label}
            </button>
          ))}
        </div>
      )}

      {/* Properties List */}
      {properties.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-4">
          <Home className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">No tienes inmuebles directos activos</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Comienza a publicar tu propiedad residencial o comercial de forma directa para recibir ofertas.
          </p>
          <Link
            to="/owner/properties/create"
            className="inline-block px-5 py-2.5 bg-amber-500 text-navy-950 font-extrabold text-xs rounded-xl shadow-md"
          >
            Publicar Inmueble Ahora
          </Link>
        </div>
      ) : filteredProperties.length === 0 ? (
        <div className="bg-white rounded-3xl p-10 text-center border border-slate-200">
          <p className="text-xs text-slate-500">No hay inmuebles en este estado.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {filteredProperties.map((prop) => (
            <div key={prop.id} className="bg-white rounded-3xl overflow-hidden border border-slate-200 shadow-xs">
              {/* Image thumbnail */}
              <div className="h-44 bg-slate-100 relative">
                {prop.images && prop.images.length > 0 ? (
                  <img src={prop.images[0].imageUrl} alt={prop.name || prop.code} className="w-full h-full object-cover" />
                ) : (
                  <div className="w-full h-full flex items-center justify-center text-slate-300">
                    <Home className="w-10 h-10" />
                  </div>
                )}
                <div className="absolute top-3 left-3">
                  <Badge status={prop.status} />
                </div>
              </div>

              <div className="p-6 space-y-4">
                <div className="flex justify-between items-start">
                  <div>
                    <span className="font-mono text-xs font-bold text-slate-500">#{prop.code}</span>
                    <h3 className="font-bold text-base text-slate-900">{prop.name || `${prop.propertyTypeName} en ${prop.saleTypeName}`}</h3>
                    <p className="text-sm font-extrabold font-mono text-emerald-600">{formatCurrencyRD(prop.price)}</p>
                  </div>
                </div>

                {prop.provinceName && (
                  <p className="text-[11px] text-slate-500 flex items-center gap-1">
                    <MapPin className="w-3.5 h-3.5" />
                    {[prop.provinceName, prop.municipalityName, prop.sector].filter(Boolean).join(' · ')}
                  </p>
                )}

                <div className="flex items-center gap-4 text-[11px] font-semibold text-slate-600">
                  <span className="flex items-center gap-1">
                    <BedDouble className="w-4 h-4 text-slate-400" /> {prop.bedrooms} Hab.
                  </span>
                  <span className="flex items-center gap-1">
                    <Bath className="w-4 h-4 text-slate-400" /> {prop.bathrooms} Baños
                  </span>
                  <span className="flex items-center gap-1">
                    <Ruler className="w-4 h-4 text-slate-400" /> {prop.landSizeMeters} m²
                  </span>
                </div>

                <p className="text-xs text-slate-600 line-clamp-2">{prop.description}</p>

                <div className="flex items-center justify-between pt-3 border-t border-slate-100">
                  <Link
                    to={`/property/${prop.id}`}
                    className="inline-flex items-center gap-1 text-xs font-bold text-brand-600 hover:text-brand-700"
                  >
                    <span>Ver Ficha Pública</span>
                    <ExternalLink className="w-3.5 h-3.5" />
                  </Link>

                  <div className="flex items-center gap-2">
                    <Link
                      to={`/owner/properties/edit/${prop.id}`}
                      className="p-2 text-slate-500 hover:bg-brand-50 hover:text-brand-700 rounded-lg transition-colors"
                      title="Editar Inmueble"
                    >
                      <Pencil className="w-4 h-4" />
                    </Link>
                    <button
                      onClick={() => handleDelete(prop.id)}
                      className="p-2 text-rose-500 hover:bg-rose-50 rounded-lg transition-colors"
                      title="Retirar Inmueble"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
