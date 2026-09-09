import React from 'react';

interface ChartCardProps {
  title: string;
  subtitle?: string;
  icon?: React.ReactNode;
  children: React.ReactNode;
}

export const ChartCard: React.FC<ChartCardProps> = ({ title, subtitle, icon, children }) => (
  <div className="bg-white p-6 rounded-3xl border border-slate-200 shadow-xs space-y-4">
    <div className="flex items-center justify-between pb-3 border-b border-slate-100">
      <h3 className="font-extrabold text-base text-slate-900 flex items-center gap-2">
        {icon}
        {title}
      </h3>
      {subtitle && (
        <span className="text-[11px] font-mono font-bold text-slate-500">{subtitle}</span>
      )}
    </div>
    {children}
  </div>
);