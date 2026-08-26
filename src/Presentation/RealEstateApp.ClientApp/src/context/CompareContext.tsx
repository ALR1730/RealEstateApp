import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { Property } from '../types';

interface CompareContextType {
  compareIds: number[];
  compareProperties: Property[];
  addToCompare: (property: Property) => void;
  removeFromCompare: (propertyId: number) => void;
  isInCompare: (propertyId: number) => boolean;
  clearCompare: () => void;
  compareCount: number;
}

const CompareContext = createContext<CompareContextType | undefined>(undefined);

const STORAGE_KEY = 'realestate_compare';
const MAX_COMPARE = 4;

export const CompareProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [compareIds, setCompareIds] = useState<number[]>(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      return saved ? JSON.parse(saved) : [];
    } catch {
      return [];
    }
  });
  const [compareProperties, setCompareProperties] = useState<Property[]>([]);

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(compareIds));
  }, [compareIds]);

  const addToCompare = useCallback((property: Property) => {
    setCompareIds((prev) => {
      if (prev.includes(property.id)) return prev;
      if (prev.length >= MAX_COMPARE) {
        return [...prev.slice(1), property.id];
      }
      return [...prev, property.id];
    });
    setCompareProperties((prev) => {
      if (prev.find((p) => p.id === property.id)) return prev;
      const updated = [...prev, property];
      if (updated.length > MAX_COMPARE) {
        return updated.slice(1);
      }
      return updated;
    });
  }, []);

  const removeFromCompare = useCallback((propertyId: number) => {
    setCompareIds((prev) => prev.filter((id) => id !== propertyId));
    setCompareProperties((prev) => prev.filter((p) => p.id !== propertyId));
  }, []);

  const isInCompare = useCallback(
    (propertyId: number) => compareIds.includes(propertyId),
    [compareIds]
  );

  const clearCompare = useCallback(() => {
    setCompareIds([]);
    setCompareProperties([]);
  }, []);

  return (
    <CompareContext.Provider
      value={{
        compareIds,
        compareProperties,
        addToCompare,
        removeFromCompare,
        isInCompare,
        clearCompare,
        compareCount: compareIds.length,
      }}
    >
      {children}
    </CompareContext.Provider>
  );
};

export const useCompare = (): CompareContextType => {
  const ctx = useContext(CompareContext);
  if (!ctx) throw new Error('useCompare debe usarse dentro de CompareProvider');
  return ctx;
};
