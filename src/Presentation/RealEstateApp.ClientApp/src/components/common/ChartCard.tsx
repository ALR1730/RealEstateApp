import React from 'react';

interface ChartCardProps {
  title: string;
  subtitle?: string;
  icon?: React.ReactNode;
  children: React.ReactNode;
}

export const ChartCard: React.FC<ChartCardProps> = ({ title, subtitle, icon, children }) => (
  <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs space-y-4">
    <div className="flex items-center justify-between pb-3 border-b border-slate-100 dark:border-slate-800">
      <h3 className="font-extrabold text-base text-slate-900 dark:text-white flex items-center gap-2">
        {icon}
        {title}
      </h3>
      {subtitle && (
        <span className="text-[11px] font-mono font-bold text-slate-500 dark:text-slate-400">{subtitle}</span>
      )}
    </div>
    {children}
  </div>
);