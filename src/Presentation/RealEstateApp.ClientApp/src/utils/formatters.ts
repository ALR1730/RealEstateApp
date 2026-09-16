import { MortgageSimulationResult, AmortizationScheduleItem } from '../types';

/**
 * Formatea un número como Moneda de República Dominicana (RD$).
 * Ejemplo: 1500000 -> "RD$ 1,500,000.00"
 */
export function formatCurrencyRD(amount: number | null | undefined): string {
  if (amount === null || amount === undefined || isNaN(amount)) {
    return 'RD$ 0.00';
  }
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount).replace('DOP', 'RD$');
}

/**
 * Formatea una fecha ISO o string a formato legible en español.
 */
export function formatDate(dateString?: string): string {
  if (!dateString) return '';
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return dateString;
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  }).format(date);
}

/**
 * Realiza el cálculo del simulador hipotecario bajo el Sistema Francés en RD$.
 */
export function calculateFrenchAmortization(
  propertyPrice: number,
  downPayment: number,
  annualRate: number,
  termInYears: number
): MortgageSimulationResult {
  const loanAmount = Math.max(0, propertyPrice - downPayment);
  const totalMonths = termInYears * 12;
  const monthlyRate = (annualRate / 100) / 12;
  const downPaymentPercentage = propertyPrice > 0 ? (downPayment / propertyPrice) * 100 : 0;

  let monthlyInstallment = 0;
  if (loanAmount > 0 && monthlyRate > 0 && totalMonths > 0) {
    const factor = Math.pow(1 + monthlyRate, totalMonths);
    monthlyInstallment = loanAmount * (monthlyRate * factor) / (factor - 1);
  } else if (loanAmount > 0 && totalMonths > 0) {
    monthlyInstallment = loanAmount / totalMonths;
  }

  const schedule: AmortizationScheduleItem[] = [];
  let remainingBalance = loanAmount;
  let totalInterest = 0;

  for (let period = 1; period <= totalMonths; period++) {
    const interest = remainingBalance * monthlyRate;
    const principal = monthlyInstallment - interest;
    remainingBalance = Math.max(0, remainingBalance - principal);
    totalInterest += interest;

    schedule.push({
      period,
      installment: Math.round(monthlyInstallment * 100) / 100,
      interest: Math.round(interest * 100) / 100,
      principal: Math.round(principal * 100) / 100,
      remainingBalance: Math.round(remainingBalance * 100) / 100,
    });
  }

  return {
    propertyPrice,
    downPayment,
    downPaymentPercentage: Math.round(downPaymentPercentage * 10) / 10,
    loanAmount,
    annualRate,
    termInYears,
    totalMonths,
    monthlyInstallment: Math.round(monthlyInstallment * 100) / 100,
    totalInterest: Math.round(totalInterest * 100) / 100,
    totalCost: Math.round((loanAmount + totalInterest) * 100) / 100,
    schedule,
  };
}
