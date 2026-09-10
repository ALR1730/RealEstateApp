import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { propertiesService } from '../../api/services';
import { Property } from '../../types';
import { useCurrency } from '../../context/CurrencyContext';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Home, PlusCircle, Edit3, Trash2, Eye, ExternalLink, Image as ImageIcon, FileText } from 'lucide-react';

export const MyPropertiesPage: React.FC = () => {
  const { formatPrice } = useCurrency();
  const [properties, setProperties] = useState<Property[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const loadProperties = async () => {
    try {
      const data = await propertiesService.getMyProperties();
      setProperties(data || []);
    } catch (err) {
      console.error("Error loading agent properties:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadProperties()).catch(console.error);
  }, []);

  const handleDelete = async (id: number) => {
    if (!window.confirm("¿Seguro que deseas eliminar esta propiedad? Esta acción no se puede deshacer.")) return;
    try {
      await propertiesService.delete(id);
      loadProperties();
    } catch (err) {
      console.error("Error deleting property:", err);
      alert("Error al eliminar la propiedad.");
    }
  };

  return (
    <div className="space-y-6">
      
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Home className="w-6 h-6 text-brand-600" />
            Mis Propiedades Publicadas
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Administra tu portafolio, edita detalles y sube hasta 15 fotografías por inmueble.
          </p>
        </div>

        <Link
          to="/agent/properties/create"
          className="flex items-center gap-2 px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white font-extrabold text-xs rounded-xl shadow-md transition-all"
        >
          <PlusCircle className="w-4 h-4" />
          <span>Publicar Nuevo Inmueble</span>
        </Link>
      </div>

      {isLoading ? (
        <Loader text="Cargando tu portafolio de propiedades..." />
      ) : properties.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-4">
          <Home className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">No tienes propiedades publicadas aún</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Comienza a publicar inmuebles residenciales o comerciales para recibir ofertas y solicitudes de visitas.
          </p>
          <Link
            to="/agent/properties/create"
            className="inline-block px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl shadow-md"
          >
            Publicar Inmueble Ahora
          </Link>
        </div>
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
                  <th className="py-3.5 px-4">Inmueble</th>
                  <th className="py-3.5 px-4">Tipo / Venta</th>
                  <th className="py-3.5 px-4">Precio (RD$)</th>
                  <th className="py-3.5 px-4">Características</th>
                  <th className="py-3.5 px-4">Estado</th>
                  <th className="py-3.5 px-4 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {properties.map((prop) => (
                  <tr key={prop.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3 px-4">
                      <div className="flex items-center gap-3">
                        <div className="w-12 h-12 rounded-xl bg-slate-100 overflow-hidden shrink-0 border border-slate-200">
                          {prop.images && prop.images.length > 0 ? (
                            <img src={typeof prop.images[0] === 'string' ? prop.images[0] : prop.images[0].imageUrl} alt="" className="w-full h-full object-cover" />
                          ) : (
                            <div className="w-full h-full flex items-center justify-center text-slate-400">
                              <ImageIcon className="w-4 h-4" />
                            </div>
                          )}
                        </div>
                        <div className="overflow-hidden">
                          <span className="font-bold text-slate-900 font-mono">#{prop.code}</span>
                          <p className="text-[11px] text-slate-500 truncate max-w-xs">{prop.description}</p>
                        </div>
                      </div>
                    </td>
                    <td className="py-3 px-4 text-slate-700 font-semibold">
                      {prop.propertyTypeName || 'Inmueble'} <br />
                      <span className="text-[10px] text-slate-500 font-normal">{prop.saleTypeName || 'Venta'}</span>
                    </td>
                    <td className="py-3 px-4 font-mono font-bold text-emerald-600 text-sm">
                      {formatPrice(prop.price)}
                    </td>
                    <td className="py-3 px-4 text-slate-600">
                      {prop.bedrooms ?? prop.rooms ?? 0} hab • {prop.bathrooms} bñ • {prop.landSizeMeters ?? prop.sizeInMeters ?? 0} m²
                    </td>
                    <td className="py-3 px-4">
                      <Badge status={prop.status} />
                    </td>
                    <td className="py-3 px-4 text-right space-x-1">
                      <Link
                        to={`/property/${prop.id}`}
                        className="inline-block p-2 text-slate-600 hover:text-brand-600 hover:bg-slate-100 rounded-lg transition-colors"
                        title="Ver en Catálogo"
                      >
                        <ExternalLink className="w-4 h-4" />
                      </Link>
                      <Link
                        to={`/agent/documents/${prop.id}`}
                        className="inline-block p-2 text-slate-600 hover:text-brand-600 hover:bg-slate-100 rounded-lg transition-colors"
                        title="Documentos Legales"
                      >
                        <FileText className="w-4 h-4" />
                      </Link>
                      <button
                        onClick={() => handleDelete(prop.id)}
                        className="p-2 text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
                        title="Eliminar Inmueble"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};
