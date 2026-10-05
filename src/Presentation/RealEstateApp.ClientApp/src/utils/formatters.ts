import { MortgageSimulationResult, AmortizationScheduleItem } from '../types';

/**
 * Formatea un número como Moneda de República Dominicana (RD$).
 * Ejemplo: 1500000 -> "RD$1,500,000.00"
 */
export function formatCurrencyRD(amount: number | null | undefined): string {
  if (amount === null || amount === undefined || !Number.isFinite(amount)) {
    return 'RD$0.00';
  }
  const isNegative = amount < 0;
  const absVal = Math.abs(amount);
  const parts = absVal.toFixed(2).split('.');
  const integerPart = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
  const decimalPart = parts[1];
  return `${isNegative ? '-' : ''}RD$${integerPart}.${decimalPart}`;
}

/**
 * Formatea una fecha ISO o string a formato legible según el locale especificado (por defecto es-DO).
 */
export function formatDate(dateString?: string, locale: string = 'es-DO'): string {
  if (!dateString) return '';
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return dateString;
  return new Intl.DateTimeFormat(locale, {
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

/**
 * Extrae de forma segura el mensaje de error de una respuesta de API (Axios),
 * soportando ProblemDetails (RFC 7807), diccionarios de validación de ASP.NET Core,
 * excepciones estándar de JavaScript y fallbacks personalizados.
 */
export function getApiErrorMessage(error: unknown, fallback: string = 'Ha ocurrido un error inesperado'): string {
  if (!error) return fallback;

  if (typeof error === 'string') {
    const trimmed = error.trim();
    return trimmed.length > 0 ? trimmed : fallback;
  }

  if (typeof error === 'object') {
    const err = error as Record<string, unknown>;
    const response = err.response as Record<string, unknown> | undefined;
    const responseData = response?.data;

    if (responseData !== undefined && responseData !== null) {
      if (typeof responseData === 'string' && responseData.trim().length > 0) {
        return responseData.trim();
      }

      if (typeof responseData === 'object') {
        const dataObj = responseData as Record<string, unknown>;

        // 1. data.error
        if (typeof dataObj.error === 'string' && dataObj.error.trim().length > 0) {
          return dataObj.error.trim();
        }

        // 2. data.message
        if (typeof dataObj.message === 'string' && dataObj.message.trim().length > 0) {
          return dataObj.message.trim();
        }

        // 3. data.title (ProblemDetails RFC 7807)
        if (typeof dataObj.title === 'string' && dataObj.title.trim().length > 0) {
          return dataObj.title.trim();
        }

        // 4. data.errors (ASP.NET Core validation dictionary o array de strings)
        if (dataObj.errors) {
          if (Array.isArray(dataObj.errors) && dataObj.errors.length > 0) {
            const list = dataObj.errors
              .filter((e: unknown): e is string => typeof e === 'string' && e.trim().length > 0)
              .map((e: string) => e.trim());
            if (list.length > 0) return list.join(', ');
          } else if (typeof dataObj.errors === 'object') {
            const errorsDict = dataObj.errors as Record<string, unknown>;
            const messages: string[] = [];
            for (const key of Object.keys(errorsDict)) {
              const val = errorsDict[key];
              if (Array.isArray(val)) {
                for (const subVal of val) {
                  if (typeof subVal === 'string' && subVal.trim().length > 0) {
                    messages.push(subVal.trim());
                  }
                }
              } else if (typeof val === 'string' && val.trim().length > 0) {
                messages.push(val.trim());
              }
            }
            if (messages.length > 0) {
              return messages.join(', ');
            }
          }
        }
      }
    }

    // 5. err.message (Error estándar o AxiosError)
    if (typeof err.message === 'string' && err.message.trim().length > 0) {
      return err.message.trim();
    }
  }

  return fallback;
}

