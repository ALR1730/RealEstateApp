import { describe, it, expect } from 'vitest';
import { calculateFrenchAmortization } from '../utils/formatters';

describe('Mortgage French Amortization Calculator', () => {
  it('calculates monthly installment correctly with French system', () => {
    // 5,000,000 property price, 1,000,000 down payment (4,000,000 loan), 11.5% annual rate, 20 years
    const result = calculateFrenchAmortization(5000000, 1000000, 11.5, 20);

    expect(result.loanAmount).toBe(4000000);
    expect(result.totalMonths).toBe(240);
    expect(result.monthlyInstallment).toBeGreaterThan(42000);
    expect(result.monthlyInstallment).toBeLessThan(43000);
    expect(result.schedule.length).toBe(240);
    expect(result.totalInterest).toBeGreaterThan(0);
  });

  it('calculates 100% down payment correctly without error', () => {
    const result = calculateFrenchAmortization(3000000, 3000000, 10, 15);
    expect(result.loanAmount).toBe(0);
    expect(result.monthlyInstallment).toBe(0);
  });
});
