import React, { useState, useEffect } from 'react';
import { Download } from 'lucide-react';

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed'; platform: string }>;
}

export const InstallAppButton: React.FC = () => {
  const [promptEvent, setPromptEvent] = useState<BeforeInstallPromptEvent | null>(null);
  const [installed, setInstalled] = useState(false);

  useEffect(() => {
    const isStandalone = window.matchMedia('(display-mode: standalone)').matches;
    const isIos = /iphone|ipad|ipod/i.test(navigator.userAgent) && !(window as any).MSStream;
    if (isStandalone || isIos) return;

    const onPrompt = (e: Event) => {
      e.preventDefault();
      setPromptEvent(e as BeforeInstallPromptEvent);
    };
    const onInstalled = () => setInstalled(true);

    window.addEventListener('beforeinstallprompt', onPrompt);
    window.addEventListener('appinstalled', onInstalled);
    return () => {
      window.removeEventListener('beforeinstallprompt', onPrompt);
      window.removeEventListener('appinstalled', onInstalled);
    };
  }, []);

  if (!promptEvent || installed) return null;

  const handleInstall = async () => {
    if (!promptEvent) return;
    await promptEvent.prompt();
    const choice = await promptEvent.userChoice;
    if (choice.outcome === 'accepted') setInstalled(true);
    setPromptEvent(null);
  };

  return (
    <button
      onClick={handleInstall}
      className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold rounded-lg text-royal-600 border border-royal-200 bg-royal-50 hover:bg-royal-100 hover:border-royal-300 transition-colors"
      title="Instalar la aplicación en tu dispositivo"
    >
      <Download className="w-3.5 h-3.5" />
      Instalar App
    </button>
  );
};