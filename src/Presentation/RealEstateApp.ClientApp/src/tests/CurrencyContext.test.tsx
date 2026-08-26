import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import React from 'react';
import { CurrencyProvider, useCurrency } from '../context/CurrencyContext';

const wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <CurrencyProvider>{children}</CurrencyProvider>
);

describe('CurrencyContext', () => {
  beforeEach(() => {
    localStorage.removeItem('realestate_currency');
  });

  it('defaults to DOP', () => {
    const { result } = renderHook(() => useCurrency(), { wrapper });
    expect(result.current.currency).toBe('DOP');
  });

  it('toggles between DOP and USD', () => {
    const { result } = renderHook(() => useCurrency(), { wrapper });
    act(() => result.current.toggleCurrency());
    expect(result.current.currency).toBe('USD');
    act(() => result.current.toggleCurrency());
    expect(result.current.currency).toBe('DOP');
  });

  it('persists currency to localStorage', () => {
    const { result } = renderHook(() => useCurrency(), { wrapper });
    act(() => result.current.setCurrency('USD'));
    expect(localStorage.getItem('realestate_currency')).toBe('USD');
  });

  it('formatPrice returns RD$ for DOP', () => {
    const { result } = renderHook(() => useCurrency(), { wrapper });
    const formatted = result.current.formatPrice(1500000);
    expect(formatted).toContain('RD$');
  });

  it('formatPrice returns $ for USD', () => {
    const { result } = renderHook(() => useCurrency(), { wrapper });
    act(() => result.current.setCurrency('USD'));
    const formatted = result.current.formatPrice(58500);
    expect(formatted).toContain('$');
    expect(formatted).not.toContain('RD$');
  });

  it('convertToDisplay divides by rate for USD', () => {
    const { result } = renderHook(() => useCurrency(), { wrapper });
    act(() => result.current.setCurrency('USD'));
    const converted = result.current.convertToDisplay(58500);
    expect(converted).toBeCloseTo(1000, 0);
  });
});
