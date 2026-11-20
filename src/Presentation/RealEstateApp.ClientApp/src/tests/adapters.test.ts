import { describe, it, expect, vi, beforeEach } from 'vitest';
import { adaptProperty, adaptProperties, adaptVerification, adaptPagedProperties, propertiesService } from '../api/services';
import { apiClient } from '../api/axiosClient';

describe('Schema Adapters (services.ts)', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

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
    expect(adaptProperties(null as unknown as Parameters<typeof adaptProperties>[0])).toEqual([]);
  });

  it('adaptVerification normalizes KYC status correctly', () => {
    expect(adaptVerification({ id: 1, status: 'Pendiente' }).status).toBe('Pending');
    expect(adaptVerification({ id: 2, status: 'Aprobado' }).status).toBe('Approved');
    expect(adaptVerification({ id: 3, status: 'Rechazado' }).status).toBe('Rejected');
    expect(adaptVerification(null)).toBeNull();
  });

  it('adaptPagedProperties normaliza respuesta PagedResult y arrays planos', () => {
    const pagedPayload = {
      items: [{ id: 1, status: 'Disponible', rooms: 2, sizeInMeters: 90, images: [] }],
      page: 2,
      pageSize: 12,
      totalCount: 25,
      totalPages: 3,
      hasPreviousPage: true,
      hasNextPage: true,
    };

    const paged = adaptPagedProperties(pagedPayload);
    expect(paged.items).toHaveLength(1);
    expect(paged.items[0].status).toBe('Available');
    expect(paged.page).toBe(2);
    expect(paged.pageSize).toBe(12);
    expect(paged.totalCount).toBe(25);
    expect(paged.totalPages).toBe(3);
    expect(paged.hasPreviousPage).toBe(true);
    expect(paged.hasNextPage).toBe(true);

    const flat = adaptPagedProperties([{ id: 7, status: 'Disponible', images: [] }]);
    expect(flat.items).toHaveLength(1);
    expect(flat.totalCount).toBe(1);
    expect(flat.totalPages).toBe(1);
  });

  it('propertiesService.getPaged y getAll desempaquetan PagedResult correctamente', async () => {
    vi.spyOn(apiClient, 'get').mockResolvedValue({
      data: {
        items: [{ id: 10, status: 'Disponible', images: ['https://example.com/a.jpg'] }],
        page: 1,
        pageSize: 50,
        totalCount: 1,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      },
    });

    const paged = await propertiesService.getPaged({ page: 1, pageSize: 50 });
    expect(paged.items).toHaveLength(1);
    expect(paged.pageSize).toBe(50);

    const all = await propertiesService.getAll();
    expect(all).toHaveLength(1);
    expect(all[0].id).toBe(10);
  });
});
