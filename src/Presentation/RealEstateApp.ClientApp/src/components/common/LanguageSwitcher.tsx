import React from 'react';
import { useLanguage } from '../../context/LanguageContext';
import { Globe } from 'lucide-react';

export const LanguageSwitcher: React.FC = () => {
  const { language, toggleLanguage } = useLanguage();

  return (
    <button
      onClick={toggleLanguage}
      className="flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg text-xs font-bold transition-all border border-slate-200/80 hover:border-brand-400 bg-white hover:bg-brand-50 text-slate-700 hover:text-brand-700 dark:bg-slate-800 dark:border-slate-700 dark:text-slate-200 dark:hover:bg-slate-700 cursor-pointer"
      title={language === 'es' ? 'Switch to English' : 'Cambiar a Español'}
    >
      <Globe className="w-3.5 h-3.5 text-brand-500" />
      <span>{language === 'es' ? 'ES' : 'EN'}</span>
    </button>
  );
};
