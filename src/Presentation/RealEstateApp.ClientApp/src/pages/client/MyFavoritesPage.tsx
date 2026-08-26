import React, { useState, useEffect } from 'react';
import { favoritesService, propertiesService } from '../../api/services';
import { Property } from '../../types';
import { PropertyCard } from '../../components/properties/PropertyCard';
import { Loader } from '../../components/common/Loader';
import { Heart, Sliders } from 'lucide-react';
import { Link } from 'react-router-dom';

export const MyFavoritesPage: React.FC = () => {
  const [favoriteProperties, setFavoriteProperties] = useState<Property[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const loadFavorites = async () => {
    try {
      setIsLoading(true);
      const favList = await favoritesService.getAll();
      
      // Load detailed properties
      if (favList.length > 0) {
        const propPromises = favList.map((f: any) =>
          propertiesService.getById(f.propertyId).catch(() => null)
        );
        const results = await Promise.all(propPromises);
        setFavoriteProperties(results.filter((p): p is Property => p !== null));
      } else {
        setFavoriteProperties([]);
      }
    } catch (err) {
      console.error("Error loading favorites:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadFavorites();
  }, []);

  const handleFavoriteToggle = (propId: number, isFav: boolean) => {
    if (!isFav) {
      setFavoriteProperties((prev) => prev.filter((p) => p.id !== propId));
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Heart className="w-6 h-6 text-rose-500 fill-rose-500" />
            Mis Propiedades Favoritas
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Portafolio de inmuebles guardados para seguimiento rápido y ofertas.
          </p>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Cargando tus propiedades favoritas..." />
      ) : favoriteProperties.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-4">
          <Heart className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">No tienes favoritos guardados</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Explora el catálogo de propiedades y haz clic en el icono de corazón para agregar tus inmuebles favoritos.
          </p>
          <Link
            to="/catalog"
            className="inline-block px-5 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md"
          >
            Explorar Catálogo de Inmuebles
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {favoriteProperties.map((prop) => (
            <PropertyCard
              key={prop.id}
              property={prop}
              isFavoriteInitial={true}
              onFavoriteToggle={handleFavoriteToggle}
            />
          ))}
        </div>
      )}
    </div>
  );
};
