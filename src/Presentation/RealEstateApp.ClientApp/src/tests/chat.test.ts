import { describe, it, expect } from 'vitest';
import { mapConversations } from '../utils/chat';

const RAW = [
  {
    id: 3,
    clienteId: 'cliente-guid',
    clienteName: 'Laura Pena',
    agenteId: 'agente-guid',
    agenteName: 'Ana Gomez',
    propertyId: 7,
    propertyCode: 'APT101',
    messageContent: 'Hola, tengo una pregunta sobre el inmueble.',
    senderId: 'cliente-guid',
  },
];

describe('mapConversations', () => {
  it('Map_DeberiaElegirAlAgenteComoDestinatario_CuandoElUsuarioEsCliente', () => {
    const out = mapConversations(RAW, { roles: ['Client'] });
    expect(out[0].recipientId).toBe('agente-guid');
    expect(out[0].recipientName).toBe('Ana Gomez');
  });

  it('Map_DeberiaElegirAlClienteComoDestinatario_CuandoElUsuarioEsAgente', () => {
    const out = mapConversations(RAW, { roles: ['Agent'] });
    expect(out[0].recipientId).toBe('cliente-guid');
    expect(out[0].recipientName).toBe('Laura Pena');
  });

  it('Map_DeberiaRetornarListaVacia_CuandoNoHayConversaciones', () => {
    const out = mapConversations(null, { roles: ['Client'] });
    expect(out).toHaveLength(0);
  });
});