import React, { useState, useEffect } from 'react';
import { savedSearchesService } from '../../api/services';
import { SavedSearch } from '../../types';
import { formatCurrencyRD } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import { BookmarkCheck, Bell, BellOff, Trash2, Search } from 'lucide-react';
import { Link } from 'react-router-dom';

export const SavedSearchesPage: React.FC = () => {
  const [searches, setSearches] = useState<SavedSearch[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const loadSearches = async () => {
    try {
      setIsLoading(true);
      const data = await savedSearchesService.getAll();
      setSearches(data || []);
    } catch (err) {
      console.error("Error loading saved searches:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadSearches();
  }, []);

  const handleToggleAlerts = async (id: number) => {
    try {
      await savedSearchesService.toggleAlerts(id);
      loadSearches();
    } catch (err) {
      console.error("Error toggling alerts:", err);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm("¿Seguro que deseas eliminar esta búsqueda guardada?")) return;
    try {
      await savedSearchesService.delete(id);
      loadSearches();
    } catch (err) {
      console.error("Error deleting search:", err);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <BookmarkCheck className="w-6 h-6 text-brand-600" />
            Búsquedas Guardadas & Alertas
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Recibe notificaciones cuando se publiquen nuevos inmuebles que coincidan con tus criterios.
          </p>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Consultando tus búsquedas guardadas..." />
      ) : searches.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-4">
          <BookmarkCheck className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">No tienes búsquedas guardadas</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Configura filtros en el catálogo de propiedades y guárdalos para recibir alertas automáticas.
          </p>
          <Link
            to="/catalog"
            className="inline-block px-5 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md"
          >
            Ir al Catálogo de Inmuebles
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {searches.map((search) => (
            <div key={search.id} className="bg-white rounded-3xl p-6 border border-slate-200 shadow-xs space-y-4">
              <div className="flex justify-between items-start">
                <div>
                  <h4 className="font-bold text-sm text-slate-900">{search.name || 'Búsqueda Personalizada'}</h4>
                  <p className="text-xs text-slate-500">
                    {search.bedrooms ? `${search.bedrooms} hab min • ` : ''}
                    {search.bathrooms ? `${search.bathrooms} baños min • ` : ''}
                    {search.maxPrice ? `Hasta ${formatCurrencyRD(search.maxPrice)}` : 'Cualquier precio'}
                  </p>
                </div>

                <button
                  onClick={() => handleToggleAlerts(search.id)}
                  className={`p-2 rounded-xl border text-xs font-bold transition-all ${
                    search.emailAlertsEnabled
                      ? 'bg-emerald-50 border-emerald-200 text-emerald-700'
                      : 'bg-slate-50 border-slate-200 text-slate-400'
                  }`}
                  title={search.emailAlertsEnabled ? 'Alertas activas' : 'Alertas inactivas'}
                >
                  {search.emailAlertsEnabled ? <Bell className="w-4 h-4" /> : <BellOff className="w-4 h-4" />}
                </button>
              </div>

              <div className="flex items-center justify-between pt-2 border-t border-slate-100">
                <Link
                  to={`/catalog?propertyTypeId=${search.propertyTypeId || ''}&maxPrice=${search.maxPrice || ''}`}
                  className="inline-flex items-center gap-1 text-xs font-bold text-brand-600 hover:text-brand-700"
                >
                  <Search className="w-3.5 h-3.5" />
                  <span>Ver Resultados</span>
                </Link>

                <button
                  onClick={() => handleDelete(search.id)}
                  className="p-1.5 text-rose-500 hover:text-rose-700 hover:bg-rose-50 rounded-lg transition-colors"
                  title="Eliminar búsqueda"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
