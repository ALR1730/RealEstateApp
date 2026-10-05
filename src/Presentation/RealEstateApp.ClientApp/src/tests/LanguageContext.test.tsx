import React from 'react';
import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { LanguageProvider, useLanguage } from '../context/LanguageContext';

describe('LanguageContext (F-13)', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('inicia con idioma español por defecto', () => {
    const wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
      <LanguageProvider>{children}</LanguageProvider>
    );

    const { result } = renderHook(() => useLanguage(), { wrapper });

    expect(result.current.language).toBe('es');
    expect(result.current.t('nav.catalog')).toBe('Catálogo');
  });

  it('toggleLanguage cambia entre español e inglés y persiste en localStorage', () => {
    const wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
      <LanguageProvider>{children}</LanguageProvider>
    );

    const { result } = renderHook(() => useLanguage(), { wrapper });

    act(() => {
      result.current.toggleLanguage();
    });

    expect(result.current.language).toBe('en');
    expect(result.current.t('nav.catalog')).toBe('Catalog');
    expect(localStorage.getItem('realestate_language')).toBe('en');

    act(() => {
      result.current.toggleLanguage();
    });

    expect(result.current.language).toBe('es');
    expect(result.current.t('nav.catalog')).toBe('Catálogo');
    expect(localStorage.getItem('realestate_language')).toBe('es');
  });

  it('setLanguage establece el idioma directamente', () => {
    const wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
      <LanguageProvider>{children}</LanguageProvider>
    );

    const { result } = renderHook(() => useLanguage(), { wrapper });

    act(() => {
      result.current.setLanguage('en');
    });

    expect(result.current.language).toBe('en');
    expect(result.current.t('catalog.filters')).toBe('Filters');
    expect(result.current.t('property.viewDetails')).toBe('View Details');
  });

  it('traduce namespaces extendidos (home, sidebar, filter, status, mortgage, ai) en ES y EN', () => {
    const wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
      <LanguageProvider>{children}</LanguageProvider>
    );

    const { result } = renderHook(() => useLanguage(), { wrapper });

    // Spanish assertions
    expect(result.current.t('home.featuredTitle')).toBe('Propiedades Destacadas');
    expect(result.current.t('sidebar.properties')).toBe('Mis Propiedades');
    expect(result.current.t('filter.title')).toBe('Filtros Avanzados');
    expect(result.current.t('status.available')).toBe('Disponible');
    expect(result.current.t('mortgage.title')).toBe('Simulador Hipotecario (RD$)');
    expect(result.current.t('ai.button')).toBe('Buscar con IA');

    // Switch to English
    act(() => {
      result.current.setLanguage('en');
    });

    expect(result.current.t('home.featuredTitle')).toBe('Featured Properties');
    expect(result.current.t('sidebar.properties')).toBe('My Properties');
    expect(result.current.t('filter.title')).toBe('Advanced Filters');
    expect(result.current.t('status.available')).toBe('Available');
    expect(result.current.t('mortgage.title')).toBe('Mortgage Simulator (RD$)');
    expect(result.current.t('ai.button')).toBe('Search with AI');
    expect(document.documentElement.lang).toBe('en');
  });

  it('t retorna la clave o el fallback si la traducción no existe', () => {
    const wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
      <LanguageProvider>{children}</LanguageProvider>
    );

    const { result } = renderHook(() => useLanguage(), { wrapper });

    expect(result.current.t('non.existent.key', 'Texto por defecto')).toBe('Texto por defecto');
    expect(result.current.t('non.existent.key')).toBe('non.existent.key');
  });
});
