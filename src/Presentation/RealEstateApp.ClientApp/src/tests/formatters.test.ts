import { describe, it, expect } from 'vitest';
import { formatCurrencyRD, formatDate, calculateFrenchAmortization, getApiErrorMessage } from '../utils/formatters';

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

  describe('getApiErrorMessage', () => {
    it('returns default fallback when error is null or undefined', () => {
      expect(getApiErrorMessage(null)).toBe('Ha ocurrido un error inesperado');
      expect(getApiErrorMessage(undefined, 'Custom fallback')).toBe('Custom fallback');
    });

    it('returns error directly if it is a non-empty string', () => {
      expect(getApiErrorMessage('Error directo del servidor')).toBe('Error directo del servidor');
      expect(getApiErrorMessage('   ')).toBe('Ha ocurrido un error inesperado');
    });

    it('extracts data.error if present', () => {
      const err = { response: { data: { error: 'Acceso denegado' } } };
      expect(getApiErrorMessage(err)).toBe('Acceso denegado');
    });

    it('extracts data.message if present', () => {
      const err = { response: { data: { message: 'Propiedad no encontrada' } } };
      expect(getApiErrorMessage(err)).toBe('Propiedad no encontrada');
    });

    it('extracts data.title if ProblemDetails RFC 7807', () => {
      const err = { response: { data: { title: 'One or more validation errors occurred.' } } };
      expect(getApiErrorMessage(err)).toBe('One or more validation errors occurred.');
    });

    it('extracts validation errors array or dictionary', () => {
      const errArray = { response: { data: { errors: ['El precio debe ser positivo', 'La descripción es obligatoria'] } } };
      expect(getApiErrorMessage(errArray)).toBe('El precio debe ser positivo, La descripción es obligatoria');

      const errDict = {
        response: {
          data: {
            errors: {
              Price: ['El precio es inválido'],
              Title: ['El título es requerido'],
            },
          },
        },
      };
      expect(getApiErrorMessage(errDict)).toBe('El precio es inválido, El título es requerido');
    });

    it('falls back to error.message when response.data is absent', () => {
      const err = new Error('Network Error');
      expect(getApiErrorMessage(err)).toBe('Network Error');
    });

    it('returns fallback if object has no recognizable error message', () => {
      expect(getApiErrorMessage({})).toBe('Ha ocurrido un error inesperado');
      expect(getApiErrorMessage({ response: { data: {} } }, 'Falló la petición')).toBe('Falló la petición');
    });
  });
});

