import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import React from 'react';
import { BuyAbilityPage } from '../pages/client/BuyAbilityPage';
import { buyAbilityService } from '../api/services';

vi.mock('../api/services', () => ({
  buyAbilityService: {
    getLast: vi.fn(),
    evaluate: vi.fn(),
  },
}));

describe('BuyAbilityPage Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders initial form with Dominican terminology and empty state', async () => {
    vi.mocked(buyAbilityService.getLast).mockResolvedValue(null);

    render(<BuyAbilityPage />);

    expect(await screen.findByText('Mi Capacidad de Compra')).toBeInTheDocument();
    expect(screen.getByText(/Inicial \/ Separación Disponible \(RD\$\)/i)).toBeInTheDocument();
    expect(screen.getByText('Aún no has evaluado tu capacidad')).toBeInTheDocument();
  });

  it('formats input values with thousands separators in real time and ignores negative signs', async () => {
    vi.mocked(buyAbilityService.getLast).mockResolvedValue(null);

    render(<BuyAbilityPage />);
    await screen.findByText('Mi Capacidad de Compra');

    const grossInput = screen.getByPlaceholderText('Ej. 65,000') as HTMLInputElement;

    // Typing -50000 should format to 50,000 without negative sign
    fireEvent.change(grossInput, { target: { value: '-50000' } });
    expect(grossInput.value).toBe('50,000');

    fireEvent.change(grossInput, { target: { value: '1500000' } });
    expect(grossInput.value).toBe('1,500,000');
  });

  it('validates that Net Income cannot be greater than Gross Income reactively', async () => {
    vi.mocked(buyAbilityService.getLast).mockResolvedValue(null);

    render(<BuyAbilityPage />);
    await screen.findByText('Mi Capacidad de Compra');

    const grossInput = screen.getByPlaceholderText('Ej. 65,000');
    const netInput = screen.getByPlaceholderText('Ej. 50,000');

    // Gross: 50,000, Net: 80,000
    fireEvent.change(grossInput, { target: { value: '50000' } });
    fireEvent.change(netInput, { target: { value: '80000' } });

    expect(
      await screen.findByText('El ingreso neto mensual no puede ser mayor al ingreso bruto mensual.')
    ).toBeInTheDocument();

    // Fix net income to 40,000 -> error clears reactively
    fireEvent.change(netInput, { target: { value: '40000' } });
    expect(
      screen.queryByText('El ingreso neto mensual no puede ser mayor al ingreso bruto mensual.')
    ).not.toBeInTheDocument();
  });

  it('displays correct DTI ratio percentage (9.0% and NOT 9000%) upon evaluation', async () => {
    vi.mocked(buyAbilityService.getLast).mockResolvedValue(null);
    vi.mocked(buyAbilityService.evaluate).mockResolvedValue({
      clientId: 'client-1',
      monthlyGrossIncome: 65000,
      monthlyNetIncome: 50000,
      monthlyDebtPayments: 4500,
      availableDownPayment: 300000,
      maxMonthlyPayment: 15000,
      maxMortgageAmount: 1475000,
      maxPropertyPrice: 1775000,
      debtToIncomeRatio: 9.0, // 9.0%
      creditScoreRating: 'Excelente',
      estimatedAnnualRate: 9.5,
      recommendedTermYears: 25,
      evaluationResult: 'Aprobado',
      currency: 'DOP',
      observations: 'Felicidades. Su capacidad de compra máxima estimada es de RD$1,775,000.00.',
      evaluationDate: new Date().toISOString(),
    });

    render(<BuyAbilityPage />);
    await screen.findByText('Mi Capacidad de Compra');

    const grossInput = screen.getByPlaceholderText('Ej. 65,000');
    const netInput = screen.getByPlaceholderText('Ej. 50,000');
    const evaluateBtn = screen.getByRole('button', { name: /Evaluar Capacidad de Compra/i });

    fireEvent.change(grossInput, { target: { value: '65000' } });
    fireEvent.change(netInput, { target: { value: '50000' } });

    fireEvent.click(evaluateBtn);

    // Verify evaluation succeeded and displayed 9.0%
    await waitFor(() => {
      expect(screen.getByText('9.0%')).toBeInTheDocument();
      expect(screen.queryByText('900.0%')).not.toBeInTheDocument();
      expect(screen.queryByText('9000.0%')).not.toBeInTheDocument();
    });

    // Check standard Dominican currency formatting
    expect(screen.getByText('RD$1,775,000.00')).toBeInTheDocument();
    expect(screen.getByText('RD$1,475,000.00')).toBeInTheDocument();
    expect(screen.getByText('RD$15,000.00')).toBeInTheDocument();
  });
});
