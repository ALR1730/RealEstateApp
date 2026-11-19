import React from 'react';
import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import { VideoPlayer } from '../components/properties/VideoPlayer';
import { VirtualTourEmbed } from '../components/properties/VirtualTourEmbed';

describe('EmbedSecurity - VideoPlayer & VirtualTourEmbed', () => {
  const dangerousUrls = [
    'javascript:alert(1)',
    'https://evil.example.com/phish',
    'data:text/html,<script>alert(1)</script>',
    'http://youtube.com/watch?v=123',
    'https://youtube.com.evil.com/watch?v=123',
    '',
  ];

  describe('VideoPlayer', () => {
    it.each(dangerousUrls)(
      'NO renderiza ningún <iframe> ante URL peligrosa o no autorizada: %s',
      (dangerousUrl) => {
        // Arrange & Act
        const { container } = render(<VideoPlayer url={dangerousUrl} />);

        // Assert
        expect(container.querySelector('iframe')).toBeNull();
        expect(container.innerHTML).not.toContain('javascript:');
        expect(container.innerHTML).not.toContain('<script');
      }
    );

    it('renderiza <iframe> endurecido con sandbox y referrerpolicy="no-referrer" para YouTube HTTPS', () => {
      // Arrange
      const safeVideoUrl = 'https://www.youtube.com/watch?v=dQw4w9WgXcQ';

      // Act
      const { container } = render(<VideoPlayer url={safeVideoUrl} />);
      const iframe = container.querySelector('iframe');

      // Assert
      expect(iframe).not.toBeNull();
      expect(iframe).toHaveAttribute('sandbox');
      expect(iframe?.getAttribute('sandbox')?.trim().length).toBeGreaterThan(0);
      expect(iframe).toHaveAttribute('referrerpolicy', 'no-referrer');
      expect(iframe?.getAttribute('src')).toMatch(/^https:\/\//);
    });
  });

  describe('VirtualTourEmbed', () => {
    it.each(dangerousUrls)(
      'NO renderiza ningún <iframe> ante URL peligrosa o no autorizada: %s',
      (dangerousUrl) => {
        // Arrange & Act
        const { container } = render(<VirtualTourEmbed url={dangerousUrl} />);

        // Assert
        expect(container.querySelector('iframe')).toBeNull();
        expect(container.innerHTML).not.toContain('javascript:');
        expect(container.innerHTML).not.toContain('<script');
      }
    );

    it('renderiza <iframe> endurecido con sandbox y referrerpolicy="no-referrer" para Matterport HTTPS', () => {
      // Arrange
      const safeTourUrl = 'https://my.matterport.com/show/?m=abc';

      // Act
      const { container } = render(<VirtualTourEmbed url={safeTourUrl} />);
      const iframe = container.querySelector('iframe');

      // Assert
      expect(iframe).not.toBeNull();
      expect(iframe).toHaveAttribute('sandbox');
      expect(iframe?.getAttribute('sandbox')?.trim().length).toBeGreaterThan(0);
      expect(iframe).toHaveAttribute('referrerpolicy', 'no-referrer');
      expect(iframe?.getAttribute('src')).toMatch(/^https:\/\//);
      expect(iframe?.getAttribute('src')).toContain('matterport.com');
    });
  });
});
