import { describe, it, expect, vi, beforeEach } from 'vitest';
import { aiSearchService } from '../api/services';
import { apiClient } from '../api/axiosClient';

describe('AI Conversational Search Service (F-17)', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('searchByAi envia la consulta y adapta las propiedades devueltas', async () => {
    const mockResponse = {
      data: {
        originalQuery: 'Apartamento en Bella Vista',
        explanation: 'Buscando Apartamento en el sector Bella Vista.',
        confidenceScore: 0.95,
        extractedEntities: {
          propertyType: 'Apartamento',
          sector: 'Bella Vista',
          improvements: [],
        },
        parsedFilter: {
          propertyTypeId: 1,
          sector: 'Bella Vista',
        },
        matchedPropertiesCount: 1,
        properties: [
          {
            id: 10,
            name: 'Torre Vista',
            status: 'Disponible',
            images: ['https://example.com/foto.jpg'],
          },
        ],
      },
    };

    vi.spyOn(apiClient, 'post').mockResolvedValue(mockResponse);

    const result = await aiSearchService.searchByAi('Apartamento en Bella Vista');

    expect(apiClient.post).toHaveBeenCalledWith('/aisearch/query', {
      query: 'Apartamento en Bella Vista',
    });
    expect(result.explanation).toContain('Bella Vista');
    expect(result.confidenceScore).toBe(0.95);
    expect(result.properties).toHaveLength(1);
    expect(result.properties[0].status).toBe('Available');
  });

  it('interpret envia la consulta y retorna la interpretacion semantica sin propiedades', async () => {
    const mockResponse = {
      data: {
        originalQuery: 'Casa con piscina',
        explanation: 'Buscando Casa que incluya Piscina.',
        confidenceScore: 0.85,
        extractedEntities: {
          propertyType: 'Casa',
          improvements: ['Piscina'],
        },
        parsedFilter: {
          propertyTypeId: 2,
          improvementIds: [10],
        },
        matchedPropertiesCount: 0,
        properties: [],
      },
    };

    vi.spyOn(apiClient, 'post').mockResolvedValue(mockResponse);

    const result = await aiSearchService.interpret('Casa con piscina');

    expect(apiClient.post).toHaveBeenCalledWith('/aisearch/interpret', {
      query: 'Casa con piscina',
    });
    expect(result.extractedEntities.propertyType).toBe('Casa');
    expect(result.extractedEntities.improvements).toContain('Piscina');
  });

  it('getSuggestions obtiene sugerencias guiadas para el usuario', async () => {
    const mockResponse = {
      data: {
        suggestions: [
          'Apartamento en Bella Vista por menos de 8M',
          'Villa en Punta Cana con piscina',
        ],
      },
    };

    vi.spyOn(apiClient, 'get').mockResolvedValue(mockResponse);

    const suggestions = await aiSearchService.getSuggestions();

    expect(apiClient.get).toHaveBeenCalledWith('/aisearch/suggestions');
    expect(suggestions).toHaveLength(2);
    expect(suggestions[0]).toContain('Bella Vista');
  });
});
