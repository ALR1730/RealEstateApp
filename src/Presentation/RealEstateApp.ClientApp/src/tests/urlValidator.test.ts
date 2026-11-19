import { describe, it, expect } from 'vitest';
import { isSafeEmbedUrl } from '../utils/urlValidator';

describe('urlValidator - isSafeEmbedUrl', () => {
  describe('URLs seguras de proveedores permitidos', () => {
    it.each([
      ['YouTube estándar', 'https://www.youtube.com/watch?v=dQw4w9WgXcQ'],
      ['YouTube acortado (youtu.be)', 'https://youtu.be/dQw4w9WgXcQ'],
      ['Vimeo', 'https://vimeo.com/123456'],
      ['Matterport', 'https://my.matterport.com/show/?m=abc'],
      ['Kuula', 'https://kuula.co/share/abc'],
    ])('acepta URL válida de %s: %s', (_label, url) => {
      // Arrange & Act
      const result = isSafeEmbedUrl(url);

      // Assert
      expect(result).toBe(true);
    });
  });

  describe('Rechazo estricto de URLs peligrosas o no autorizadas', () => {
    it.each([
      ['esquema javascript:', 'javascript:alert(1)'],
      ['esquema JAVASCRIPT en mayúsculas', 'JAVASCRIPT:alert(1)'],
      ['esquema data: HTML con script', 'data:text/html,<script>alert(1)</script>'],
      ['esquema vbscript:', 'vbscript:msgbox(1)'],
      ['protocolo inseguro http://', 'http://youtube.com/watch?v=123'],
      ['protocolo inseguro http:// con www', 'http://www.youtube.com/watch?v=123'],
      ['dominio permitido solo en el path', 'https://evil.com/youtube.com'],
      ['dominio permitido solo en query string', 'https://evil.com/?redirect=https://youtube.com'],
      ['cadena vacía', ''],
      ['solo espacios en blanco', '   \t\n  '],
      ['subdominio falsificado (prefix attack)', 'https://youtube.com.evil.com'],
      ['subdominio falsificado con path', 'https://youtube.com.evil.com/watch?v=dQw4w9WgXcQ'],
      ['sufijo de dominio falsificado', 'https://evil-youtube.com/watch?v=123'],
      ['URL malformada sin esquema', 'not-a-valid-url'],
      ['URL con protocolo relativo', '//www.youtube.com/watch?v=dQw4w9WgXcQ'],
    ])('rechaza %s (%s)', (_label, maliciousUrl) => {
      // Arrange & Act
      const result = isSafeEmbedUrl(maliciousUrl);

      // Assert
      expect(result).toBe(false);
    });

    it('rechaza valores null o undefined en tiempo de ejecución', () => {
      // Arrange
      const nullUrl = null as unknown as string;
      const undefinedUrl = undefined as unknown as string;

      // Act & Assert
      expect(isSafeEmbedUrl(nullUrl)).toBe(false);
      expect(isSafeEmbedUrl(undefinedUrl)).toBe(false);
    });
  });
});
