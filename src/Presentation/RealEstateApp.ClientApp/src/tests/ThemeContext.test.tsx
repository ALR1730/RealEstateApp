import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import React from 'react';
import { ThemeProvider, useTheme } from '../context/ThemeContext';

const wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <ThemeProvider>{children}</ThemeProvider>
);

describe('ThemeContext', () => {
  beforeEach(() => {
    localStorage.removeItem('realestate_theme');
    document.documentElement.classList.remove('dark');
  });

  it('defaults to light or system preference', () => {
    const { result } = renderHook(() => useTheme(), { wrapper });
    expect(['light', 'dark']).toContain(result.current.theme);
  });

  it('toggles between light and dark', () => {
    const { result } = renderHook(() => useTheme(), { wrapper });
    const initial = result.current.theme;
    act(() => result.current.toggleTheme());
    expect(result.current.theme).toBe(initial === 'light' ? 'dark' : 'light');
  });

  it('persists theme to localStorage', () => {
    const { result } = renderHook(() => useTheme(), { wrapper });
    act(() => result.current.setTheme('dark'));
    expect(localStorage.getItem('realestate_theme')).toBe('dark');
  });

  it('applies dark class to document element', () => {
    const { result } = renderHook(() => useTheme(), { wrapper });
    act(() => result.current.setTheme('dark'));
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  it('removes dark class when set to light', () => {
    const { result } = renderHook(() => useTheme(), { wrapper });
    act(() => result.current.setTheme('dark'));
    act(() => result.current.setTheme('light'));
    expect(document.documentElement.classList.contains('dark')).toBe(false);
  });
});
