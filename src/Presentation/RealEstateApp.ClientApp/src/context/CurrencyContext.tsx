import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { currencyService } from '../api/services';

type Currency = 'DOP' | 'USD';

interface CurrencyContextType {
  currency: Currency;
  exchangeRate: number;
  toggleCurrency: () => void;
  setCurrency: (c: Currency) => void;
  formatPrice: (amountDOP: number) => string;
  convertToDisplay: (amountDOP: number) => number;
}

const CurrencyContext = createContext<CurrencyContextType | undefined>(undefined);

const STORAGE_KEY = 'realestate_currency';
const DEFAULT_RATE = 58.5;

export const CurrencyProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [currency, setCurrencyState] = useState<Currency>(() => {
    const saved = localStorage.getItem(STORAGE_KEY);
    return saved === 'USD' || saved === 'DOP' ? saved : 'DOP';
  });
  const [exchangeRate, setExchangeRate] = useState<number>(DEFAULT_RATE);

  useEffect(() => {
    const fetchRate = async () => {
      try {
        const data = await currencyService.getRates();
        if (data && data.rate) {
          setExchangeRate(data.rate);
        }
      } catch {
        // keep default rate
      }
    };
    fetchRate();
  }, []);

  const toggleCurrency = useCallback(() => {
    setCurrencyState((prev) => {
      const next = prev === 'DOP' ? 'USD' : 'DOP';
      localStorage.setItem(STORAGE_KEY, next);
      return next;
    });
  }, []);

  const setCurrency = useCallback((c: Currency) => {
    setCurrencyState(c);
    localStorage.setItem(STORAGE_KEY, c);
  }, []);

  const convertToDisplay = useCallback(
    (amountDOP: number): number => {
      if (currency === 'USD') {
        return Math.round((amountDOP / exchangeRate) * 100) / 100;
      }
      return amountDOP;
    },
    [currency, exchangeRate]
  );

  const formatPrice = useCallback(
    (amountDOP: number): string => {
      const converted = convertToDisplay(amountDOP);
      if (currency === 'USD') {
        return new Intl.NumberFormat('en-US', {
          style: 'currency',
          currency: 'USD',
          minimumFractionDigits: 0,
          maximumFractionDigits: 0,
        }).format(converted);
      }
      return new Intl.NumberFormat('es-DO', {
        style: 'currency',
        currency: 'DOP',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0,
      }).format(converted).replace('DOP', 'RD$');
    },
    [currency, convertToDisplay]
  );

  return (
    <CurrencyContext.Provider
      value={{ currency, exchangeRate, toggleCurrency, setCurrency, formatPrice, convertToDisplay }}
    >
      {children}
    </CurrencyContext.Provider>
  );
};

export const useCurrency = (): CurrencyContextType => {
  const ctx = useContext(CurrencyContext);
  if (!ctx) throw new Error('useCurrency debe usarse dentro de CurrencyProvider');
  return ctx;
};
