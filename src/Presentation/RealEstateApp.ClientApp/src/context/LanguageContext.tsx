import React, { createContext, useContext, useState, useCallback } from 'react';

export type Language = 'es' | 'en';

export interface LanguageContextType {
  language: Language;
  toggleLanguage: () => void;
  setLanguage: (lang: Language) => void;
  t: (key: string, fallback?: string) => string;
}

const STORAGE_KEY = 'realestate_language';

const TRANSLATIONS: Record<Language, Record<string, string>> = {
  es: {
    // Navigation
    'nav.catalog': 'Catálogo',
    'nav.agents': 'Agentes',
    'nav.mortgage': 'Simulador RD$',
    'nav.map': 'Mapa',
    'nav.compare': 'Comparador',
    'nav.myPanel': 'Mi Panel',
    'nav.login': 'Iniciar Sesión',
    'nav.register': 'Registrarse',
    'nav.logout': 'Cerrar Sesión',

    // AI Search
    'ai.searchMode': 'Búsqueda Inteligente con IA (NLP)',
    'ai.standardMode': 'Búsqueda Estándar',
    'ai.button': 'Buscar con IA',
    'ai.analyzing': 'Interpretando...',
    'ai.placeholder': "Escribe libremente: Ej. 'Apartamento de 3 habitaciones en Bella Vista con balcón por menos de 8M'...",
    'ai.suggestions': 'Sugerencias:',

    // Catalog & Filters
    'catalog.title': 'Propiedades Exclusivas en RD$',
    'catalog.subtitle': 'Filtra por tipo, ubicación, amenidades, habitaciones y rango de precios con cálculo hipotecario en tiempo real.',
    'catalog.filters': 'Filtros',
    'catalog.activeFilters': 'Filtros activos:',
    'catalog.clearFilters': 'Limpiar filtros',
    'catalog.noResults': 'No se encontraron propiedades con los criterios seleccionados.',
    'catalog.sortFeatured': 'Destacados primero',
    'catalog.sortPriceAsc': 'Precio: Menor a Mayor',
    'catalog.sortPriceDesc': 'Precio: Mayor a Menor',
    'catalog.sortBedrooms': 'Más habitaciones',

    // Property Card & Details
    'property.featured': 'Destacado',
    'property.rooms': 'Hab.',
    'property.bathrooms': 'Baños',
    'property.size': 'm²',
    'property.viewDetails': 'Ver Detalle',
    'property.makeOffer': 'Hacer Oferta',
    'property.bookAppointment': 'Agendar Cita',
    'property.virtualTour': 'Tour 360°',

    // Statuses
    'status.available': 'Disponible',
    'status.reserved': 'Reservada',
    'status.sold': 'Vendida',
    'status.pending': 'Pendiente',
    'status.accepted': 'Aceptada',
    'status.rejected': 'Rechazada',

    // Common
    'common.loading': 'Cargando...',
    'common.save': 'Guardar',
    'common.cancel': 'Cancelar',
    'common.delete': 'Eliminar',
    'common.edit': 'Editar',
    'common.search': 'Buscar',
    'common.close': 'Cerrar',
  },
  en: {
    // Navigation
    'nav.catalog': 'Catalog',
    'nav.agents': 'Agents',
    'nav.mortgage': 'Mortgage (RD$)',
    'nav.map': 'Map',
    'nav.compare': 'Compare',
    'nav.myPanel': 'Dashboard',
    'nav.login': 'Sign In',
    'nav.register': 'Register',
    'nav.logout': 'Sign Out',

    // AI Search
    'ai.searchMode': 'AI Natural Language Search',
    'ai.standardMode': 'Standard Search',
    'ai.button': 'Search with AI',
    'ai.analyzing': 'Analyzing...',
    'ai.placeholder': "Type freely: E.g. '3 bedroom apartment in Bella Vista with balcony under 8M'...",
    'ai.suggestions': 'Suggestions:',

    // Catalog & Filters
    'catalog.title': 'Exclusive Properties in DR',
    'catalog.subtitle': 'Filter by type, location, amenities, bedrooms and price ranges with real-time mortgage calculations.',
    'catalog.filters': 'Filters',
    'catalog.activeFilters': 'Active filters:',
    'catalog.clearFilters': 'Clear filters',
    'catalog.noResults': 'No properties found matching the selected criteria.',
    'catalog.sortFeatured': 'Featured first',
    'catalog.sortPriceAsc': 'Price: Low to High',
    'catalog.sortPriceDesc': 'Price: High to Low',
    'catalog.sortBedrooms': 'Most bedrooms',

    // Property Card & Details
    'property.featured': 'Featured',
    'property.rooms': 'Beds',
    'property.bathrooms': 'Baths',
    'property.size': 'sqm',
    'property.viewDetails': 'View Details',
    'property.makeOffer': 'Make Offer',
    'property.bookAppointment': 'Schedule Tour',
    'property.virtualTour': '360° Tour',

    // Statuses
    'status.available': 'Available',
    'status.reserved': 'Reserved',
    'status.sold': 'Sold',
    'status.pending': 'Pending',
    'status.accepted': 'Accepted',
    'status.rejected': 'Rejected',

    // Common
    'common.loading': 'Loading...',
    'common.save': 'Save',
    'common.cancel': 'Cancel',
    'common.delete': 'Delete',
    'common.edit': 'Edit',
    'common.search': 'Search',
    'common.close': 'Close',
  },
};

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

export const LanguageProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [language, setLanguageState] = useState<Language>(() => {
    const saved = localStorage.getItem(STORAGE_KEY);
    return saved === 'en' || saved === 'es' ? saved : 'es';
  });

  const setLanguage = useCallback((lang: Language) => {
    setLanguageState(lang);
    localStorage.setItem(STORAGE_KEY, lang);
  }, []);

  const toggleLanguage = useCallback(() => {
    setLanguageState((prev) => {
      const next = prev === 'es' ? 'en' : 'es';
      localStorage.setItem(STORAGE_KEY, next);
      return next;
    });
  }, []);

  const t = useCallback(
    (key: string, fallback?: string): string => {
      const dict = TRANSLATIONS[language];
      if (dict && dict[key] !== undefined) {
        return dict[key];
      }
      // Si falta en el idioma actual, buscar en español
      if (TRANSLATIONS.es[key] !== undefined) {
        return TRANSLATIONS.es[key];
      }
      return fallback || key;
    },
    [language]
  );

  return (
    <LanguageContext.Provider value={{ language, toggleLanguage, setLanguage, t }}>
      {children}
    </LanguageContext.Provider>
  );
};

export const useLanguage = (): LanguageContextType => {
  const ctx = useContext(LanguageContext);
  if (!ctx) {
    throw new Error('useLanguage debe usarse dentro de un LanguageProvider');
  }
  return ctx;
};
