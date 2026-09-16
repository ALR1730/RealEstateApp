import React, { useState, useEffect } from 'react';
import { Property } from '../../types';
import { propertiesService } from '../../api/services';
import { PropertyMap } from '../../components/properties/PropertyMap';
import { PropertyMapCard } from '../../components/properties/PropertyMapCard';
import { Loader } from '../../components/common/Loader';
import { Map } from 'lucide-react';

export const MapPage: React.FC = () => {
  const [properties, setProperties] = useState<Property[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    const loadProperties = async () => {
      try {
        setIsLoading(true);
        const data = await propertiesService.getAll();
        setProperties(data);
      } catch (err) {
        console.error('Error loading properties for map:', err);
      } finally {
        setIsLoading(false);
      }
    };
    loadProperties();
  }, []);

  if (isLoading) {
    return <Loader text="Cargando propiedades en el mapa..." />;
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      {/* Header Banner */}
      <div className="bg-navy-950 text-white rounded-3xl p-6 sm:p-10 relative overflow-hidden shadow-xl">
        <div className="relative z-10 max-w-2xl space-y-2">
          <span className="text-brand-400 font-extrabold text-xs uppercase tracking-widest">
            Explora en Mapa
          </span>
          <h1 className="text-2xl sm:text-4xl font-extrabold tracking-tight">
            Mapa de Propiedades en RD$
          </h1>
          <p className="text-xs sm:text-sm text-slate-300">
            Visualiza todas las propiedades disponibles en la República Dominicana. Haz clic en un marcador para ver los detalles.
          </p>
        </div>
      </div>

      {/* Map + Sidebar Layout */}
      <div className="flex flex-col lg:flex-row gap-6" style={{ height: '600px' }}>
        {/* Map - 60% on desktop */}
        <div className="w-full lg:w-[60%] h-full">
          <PropertyMap properties={properties} />
        </div>

        {/* Sidebar - 40% on desktop */}
        <div className="w-full lg:w-[40%] overflow-y-auto custom-scrollbar space-y-3">
          <div className="flex items-center gap-2 mb-2">
            <Map className="w-4 h-4 text-brand-600" />
            <span className="text-xs font-bold text-slate-700 uppercase tracking-wide">
              {properties.length} propiedades en el mapa
            </span>
          </div>
          {properties.map((property) => (
            <PropertyMapCard key={property.id} property={property} />
          ))}
        </div>
      </div>
    </div>
  );
};
