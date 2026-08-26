import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import React from 'react';
import { CompareProvider, useCompare } from '../context/CompareContext';
import { Property } from '../types';

const mockProperty1: Property = {
  id: 1,
  code: 'P001',
  propertyTypeId: 1,
  saleTypeId: 1,
  price: 1500000,
  landSizeMeters: 120,
  bedrooms: 3,
  bathrooms: 2,
  description: 'Test property 1',
  agentId: 'agent1',
  status: 'Available',
  images: [],
  improvements: [],
};

const mockProperty2: Property = {
  ...mockProperty1,
  id: 2,
  code: 'P002',
  price: 2000000,
};

const wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <CompareProvider>{children}</CompareProvider>
);

describe('CompareContext', () => {
  beforeEach(() => {
    localStorage.removeItem('realestate_compare');
  });

  it('starts with empty compare list', () => {
    const { result } = renderHook(() => useCompare(), { wrapper });
    expect(result.current.compareCount).toBe(0);
    expect(result.current.compareIds).toEqual([]);
  });

  it('adds property to compare', () => {
    const { result } = renderHook(() => useCompare(), { wrapper });
    act(() => result.current.addToCompare(mockProperty1));
    expect(result.current.compareCount).toBe(1);
    expect(result.current.isInCompare(1)).toBe(true);
  });

  it('removes property from compare', () => {
    const { result } = renderHook(() => useCompare(), { wrapper });
    act(() => result.current.addToCompare(mockProperty1));
    act(() => result.current.removeFromCompare(1));
    expect(result.current.compareCount).toBe(0);
    expect(result.current.isInCompare(1)).toBe(false);
  });

  it('does not add duplicate properties', () => {
    const { result } = renderHook(() => useCompare(), { wrapper });
    act(() => result.current.addToCompare(mockProperty1));
    act(() => result.current.addToCompare(mockProperty1));
    expect(result.current.compareCount).toBe(1);
  });

  it('limits to 4 properties', () => {
    const { result } = renderHook(() => useCompare(), { wrapper });
    const props = [
      mockProperty1,
      mockProperty2,
      { ...mockProperty1, id: 3, code: 'P003' },
      { ...mockProperty1, id: 4, code: 'P004' },
      { ...mockProperty1, id: 5, code: 'P005' },
    ];
    props.forEach((p) => act(() => result.current.addToCompare(p)));
    expect(result.current.compareCount).toBe(4);
    expect(result.current.isInCompare(1)).toBe(false);
  });

  it('clears all properties', () => {
    const { result } = renderHook(() => useCompare(), { wrapper });
    act(() => result.current.addToCompare(mockProperty1));
    act(() => result.current.addToCompare(mockProperty2));
    act(() => result.current.clearCompare());
    expect(result.current.compareCount).toBe(0);
  });

  it('persists to localStorage', () => {
    const { result } = renderHook(() => useCompare(), { wrapper });
    act(() => result.current.addToCompare(mockProperty1));
    const saved = JSON.parse(localStorage.getItem('realestate_compare') || '[]');
    expect(saved).toContain(1);
  });
});
