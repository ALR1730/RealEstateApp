import React, { useState, useEffect } from 'react';
import { authService } from '../../api/services';
import { Loader } from '../../components/common/Loader';
import {
  Activity,
  LogIn,
  LogOut,
  Heart,
  BarChart3,
  MessageCircle,
  Bell,
  Settings,
  Home,
} from 'lucide-react';

interface ActivityItem {
  id?: number;
  description: string;
  timestamp: string;
  type?: string;
}

const activityIcon = (type?: string) => {
  switch (type?.toLowerCase()) {
    case 'login':
      return <LogIn className="w-4 h-4 text-emerald-600" />;
    case 'logout':
      return <LogOut className="w-4 h-4 text-slate-500" />;
    case 'favorite':
    case 'favorited':
      return <Heart className="w-4 h-4 text-rose-500" />;
    case 'offer':
      return <BarChart3 className="w-4 h-4 text-amber-600" />;
    case 'chat':
    case 'message':
      return <MessageCircle className="w-4 h-4 text-brand-600" />;
    case 'notification':
      return <Bell className="w-4 h-4 text-violet-600" />;
    case 'property':
      return <Home className="w-4 h-4 text-blue-600" />;
    case 'profile':
    case 'settings':
      return <Settings className="w-4 h-4 text-slate-500" />;
    default:
      return <Activity className="w-4 h-4 text-brand-600" />;
  }
};

const formatTimestamp = (ts: string) => {
  const d = new Date(ts);
  if (isNaN(d.getTime())) return ts;
  return new Intl.DateTimeFormat('es-DO', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(d);
};

export const ActivityPage: React.FC = () => {
  const [activities, setActivities] = useState<ActivityItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const data = await authService.getActivity();
        setActivities(Array.isArray(data) ? data : []);
      } catch (err) {
        console.error('Error loading activity:', err);
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, []);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Activity className="w-6 h-6 text-brand-600" />
            Registro de Actividad
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Historial completo de acciones realizadas en tu cuenta.
          </p>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Cargando tu actividad reciente..." />
      ) : activities.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-4">
          <Activity className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">Sin actividad registrada</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Cuando realices acciones en la plataforma, tu actividad aparecerá aquí.
          </p>
        </div>
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 shadow-xs overflow-hidden">
          <ul className="divide-y divide-slate-100">
            {activities.map((item, idx) => (
              <li key={item.id ?? idx} className="flex items-start gap-4 p-4 sm:p-5 hover:bg-slate-50/60 transition-colors">
                <div className="shrink-0 w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center mt-0.5">
                  {activityIcon(item.type)}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-xs sm:text-sm font-medium text-slate-800 leading-relaxed">
                    {item.description}
                  </p>
                  <p className="text-[11px] text-slate-400 mt-1 font-semibold">
                    {formatTimestamp(item.timestamp)}
                  </p>
                </div>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};
