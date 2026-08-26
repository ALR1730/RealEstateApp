import { describe, it, expect } from 'vitest';
import { formatCurrencyRD, formatDate, calculateFrenchAmortization } from '../utils/formatters';

describe('Formatters & Utilities', () => {
  it('formatCurrencyRD formats amounts in Dominican Pesos RD$', () => {
    const formatted = formatCurrencyRD(1500000);
    expect(formatted).toContain('RD$');
    expect(formatted).toContain('1,500,000');
  });

  it('formatCurrencyRD handles zero and null gracefully', () => {
    expect(formatCurrencyRD(0)).toContain('0.00');
    expect(formatCurrencyRD(null)).toBe('RD$ 0.00');
  });

  it('formatDate formats ISO dates properly', () => {
    const formatted = formatDate('2026-08-24T12:00:00Z');
    expect(formatted).toBeTruthy();
  });
});
