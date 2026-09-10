import { describe, it, expect } from 'vitest';
import { adaptProperty, adaptProperties, adaptVerification } from '../api/services';

describe('Schema Adapters (services.ts)', () => {
  it('adaptProperty normalizes raw string images into objects', () => {
    const raw = {
      id: 42,
      name: 'Villa Romana',
      images: ['https://example.com/img1.jpg', 'https://example.com/img2.jpg'],
      rooms: 4,
      sizeInMeters: 350,
      status: 'Disponible',
    };

    const adapted = adaptProperty(raw);

    expect(adapted.images).toHaveLength(2);
    expect(adapted.images[0]).toEqual({
      id: 1,
      propertyId: 42,
      imageUrl: 'https://example.com/img1.jpg',
      isMain: true,
    });
    expect(adapted.bedrooms).toBe(4);
    expect(adapted.rooms).toBe(4);
    expect(adapted.landSizeMeters).toBe(350);
    expect(adapted.sizeInMeters).toBe(350);
    expect(adapted.status).toBe('Available');
  });

  it('adaptProperty normalizes Spanish statuses to standard keys', () => {
    expect(adaptProperty({ id: 1, status: 'Disponible' }).status).toBe('Available');
    expect(adaptProperty({ id: 2, status: 'Reservada' }).status).toBe('Reserved');
    expect(adaptProperty({ id: 3, status: 'Vendida' }).status).toBe('Sold');
  });

  it('adaptProperties handles empty or invalid arrays gracefully', () => {
    expect(adaptProperties([])).toEqual([]);
    expect(adaptProperties(null as any)).toEqual([]);
  });

  it('adaptVerification normalizes KYC status correctly', () => {
    expect(adaptVerification({ id: 1, status: 'Pendiente' }).status).toBe('Pending');
    expect(adaptVerification({ id: 2, status: 'Aprobado' }).status).toBe('Approved');
    expect(adaptVerification({ id: 3, status: 'Rechazado' }).status).toBe('Rejected');
    expect(adaptVerification(null)).toBeNull();
  });
});
