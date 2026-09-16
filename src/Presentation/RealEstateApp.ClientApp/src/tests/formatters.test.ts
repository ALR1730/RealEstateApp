import { describe, it, expect } from 'vitest';
import { formatCurrencyRD, formatDate, calculateFrenchAmortization } from '../utils/formatters';

describe('Formatters & Utilities', () => {
  it('formatCurrencyRD formats amounts in Dominican Pesos RD$', () => {
    const formatted = formatCurrencyRD(1500000);
    expect(formatted).toBe('RD$1,500,000.00');
    expect(formatCurrencyRD(1021310.85)).toBe('RD$1,021,310.85');
  });

  it('formatCurrencyRD handles zero, null, undefined and NaN gracefully', () => {
    expect(formatCurrencyRD(0)).toBe('RD$0.00');
    expect(formatCurrencyRD(null)).toBe('RD$0.00');
    expect(formatCurrencyRD(undefined)).toBe('RD$0.00');
    expect(formatCurrencyRD(NaN)).toBe('RD$0.00');
  });

  it('formatDate formats ISO dates properly', () => {
    const formatted = formatDate('2026-08-24T12:00:00Z');
    expect(formatted).toBeTruthy();
  });

  it('calculateFrenchAmortization computes french mortgage installment properly', () => {
    const sim = calculateFrenchAmortization(5000000, 1000000, 12, 20);
    expect(sim.loanAmount).toBe(4000000);
    expect(sim.totalMonths).toBe(240);
    expect(sim.monthlyInstallment).toBeGreaterThan(40000);
    expect(sim.schedule).toHaveLength(240);
  });
});
