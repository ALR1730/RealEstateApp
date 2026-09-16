import { apiClient } from './axiosClient';
import { 
  AuthResponse, 
  Property, 
  Offer, 
  Appointment, 
  ChatMessage, 
  SavedSearch, 
  PropertyType, 
  SaleType, 
  Improvement, 
  DashboardKPIs,
  MortgageSimulationResult,
  User,
  PriceHistory,
  ValuationResult,
  Lead,
  LeadPipelineStats,
  CreateLeadPayload,
  UpdateLeadPayload,
  BuyAbilityResult,
  BuyAbilityRequest,
  Review,
  ReviewSummary,
  Commission,
  CommissionSummary,
  PropertyDocument,
  SubscriptionPlan,
  SubscriptionPlanInput,
  AiSearchInterpretation,
  AiSearchSuggestions
} from '../types';

function combineDateAndTime(date: string, timeSlot: string): string {
  if (!date) return date;
  const timeMatch = timeSlot.match(/^(\d{1,2}):(\d{2})\s*([AP]M)/i);
  if (!timeMatch) {
    return date;
  }
  let hours = parseInt(timeMatch[1], 10);
  const minutes = parseInt(timeMatch[2], 10);
  const ampm = timeMatch[3].toUpperCase();
  if (ampm === 'PM' && hours !== 12) hours += 12;
  if (ampm === 'AM' && hours === 12) hours = 0;
  const iso = `${date}T${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:00`;
  return new Date(iso).toISOString();
}

// ==================== AUTH SERVICE ====================
export const authService = {
  login: async (email: string, password: string): Promise<AuthResponse> => {
    const res = await apiClient.post<AuthResponse>('/account/authenticate', { email, password });
    return res.data;
  },

  registerClient: async (data: any): Promise<any> => {
    const res = await apiClient.post('/account/register-client', data);
    return res.data;
  },

  registerAgent: async (data: any): Promise<any> => {
    const res = await apiClient.post('/account/register-agent', data);
    return res.data;
  },

  confirmEmail: async (userId: string, token: string): Promise<any> => {
    const res = await apiClient.get('/account/confirm-email', {
      params: { userId, token },
    });
    return res.data;
  },

  getProfile: async (): Promise<any> => {
    const res = await apiClient.get('/account/profile');
    return res.data;
  },

  updateProfile: async (formData: FormData): Promise<any> => {
    const res = await apiClient.put('/account/profile', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return res.data;
  },

  changePassword: async (data: any): Promise<any> => {
    const res = await apiClient.post('/account/change-password', data);
    return res.data;
  },

  forgotPassword: async (email: string): Promise<any> => {
    const res = await apiClient.post('/account/forgot-password', { email });
    return res.data;
  },

  resetPassword: async (data: any): Promise<any> => {
    const res = await apiClient.post('/account/reset-password', data);
    return res.data;
  },

  getActivity: async (): Promise<any[]> => {
    const res = await apiClient.get<any[]>('/account/activity');
    return res.data || [];
  }
};

// ==================== PROPERTY ADAPTER ====================
export function adaptProperty(p: any): Property {
  if (!p) return p;

  let normalizedStatus = p.status || 'Available';
  if (normalizedStatus === 'Disponible' || normalizedStatus === 'Available') {
    normalizedStatus = 'Available';
  } else if (normalizedStatus === 'Reservada' || normalizedStatus === 'Reserved') {
    normalizedStatus = 'Reserved';
  } else if (normalizedStatus === 'Vendida' || normalizedStatus === 'Sold') {
    normalizedStatus = 'Sold';
  }

  const images = Array.isArray(p.images)
    ? p.images.map((img: any, index: number) => {
        if (typeof img === 'string') {
          return { id: index + 1, propertyId: p.id, imageUrl: img, isMain: index === 0 };
        }
        if (img && typeof img === 'object') {
          return {
            id: img.id ?? index + 1,
            propertyId: img.propertyId ?? p.id,
            imageUrl: img.imageUrl || img.url || '',
            isMain: img.isMain ?? index === 0,
          };
        }
        return img;
      })
    : [];

  const improvements = Array.isArray(p.improvements)
    ? p.improvements.map((imp: any, index: number) => {
        if (typeof imp === 'string') {
          return { id: index + 1, name: imp };
        }
        return imp;
      })
    : [];

  return {
    ...p,
    status: normalizedStatus,
    bedrooms: p.bedrooms ?? p.rooms ?? 0,
    rooms: p.rooms ?? p.bedrooms ?? 0,
    landSizeMeters: p.landSizeMeters ?? p.sizeInMeters ?? 0,
    sizeInMeters: p.sizeInMeters ?? p.landSizeMeters ?? 0,
    videoTourUrl: p.videoTourUrl ?? p.videoUrl ?? '',
    videoUrl: p.videoUrl ?? p.videoTourUrl ?? '',
    virtualTour360Url: p.virtualTour360Url ?? p.tour360Url ?? '',
    tour360Url: p.tour360Url ?? p.virtualTour360Url ?? '',
    images,
    improvements,
  };
}

export function adaptProperties(props: any[]): Property[] {
  if (!Array.isArray(props)) return [];
  return props.map(adaptProperty);
}

export function adaptVerification(v: any): any {
  if (!v) return v;
  let status = v.status;
  if (status === 'Pendiente' || status === 'Pending') status = 'Pending';
  else if (status === 'Aprobado' || status === 'Approved') status = 'Approved';
  else if (status === 'Rechazado' || status === 'Rejected') status = 'Rejected';
  return { ...v, status };
}

// ==================== PROPERTIES SERVICE ====================
export const propertiesService = {
  getAll: async (params?: any): Promise<Property[]> => {
    const res = await apiClient.get<any[]>('/properties', { params });
    return adaptProperties(res.data || []);
  },

  getById: async (id: number): Promise<Property> => {
    const res = await apiClient.get<any>(`/properties/${id}`);
    return adaptProperty(res.data);
  },

  getByCode: async (code: string): Promise<Property> => {
    const res = await apiClient.get<any>(`/properties/code/${code}`);
    return adaptProperty(res.data);
  },

  getMyProperties: async (): Promise<Property[]> => {
    const res = await apiClient.get<any[]>('/properties/my-properties');
    return adaptProperties(res.data || []);
  },

  create: async (formData: FormData): Promise<any> => {
    const res = await apiClient.post('/properties', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return res.data;
  },

  update: async (id: number, formData: FormData): Promise<any> => {
    const res = await apiClient.put(`/properties/${id}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return res.data;
  },

  delete: async (id: number): Promise<any> => {
    const res = await apiClient.delete(`/properties/${id}`);
    return res.data;
  },

  toggleFeatured: async (id: number, durationDays: number = 30): Promise<any> => {
    const res = await apiClient.post(`/properties/${id}/toggle-featured?durationDays=${durationDays}`);
    return res.data;
  },

  reassign: async (id: number, newAgentId: string): Promise<any> => {
    const res = await apiClient.post(`/properties/${id}/reassign`, { newAgentId });
    return res.data;
  },

  getPriceHistory: async (propertyId: number): Promise<PriceHistory[]> => {
    const res = await apiClient.get<PriceHistory[]>(`/properties/${propertyId}/price-history`);
    return res.data || [];
  }
};

// ==================== OFFERS SERVICE ====================
export const offersService = {
  getMyOffers: async (): Promise<Offer[]> => {
    const res = await apiClient.get<Offer[]>('/offers/my-offers');
    return res.data || [];
  },

  getReceivedOffers: async (): Promise<Offer[]> => {
    const res = await apiClient.get<Offer[]>('/offers/received');
    return res.data || [];
  },

  makeOffer: async (data: { propertyId: number; amount: number; notes?: string }): Promise<any> => {
    const res = await apiClient.post('/offers', {
      propertyId: data.propertyId,
      montoOfertado: data.amount,
      notes: data.notes,
    });
    return res.data;
  },

  acceptOffer: async (offerId: number): Promise<any> => {
    const res = await apiClient.post(`/offers/${offerId}/accept`);
    return res.data;
  },

  rejectOffer: async (offerId: number): Promise<any> => {
    const res = await apiClient.post(`/offers/${offerId}/reject`);
    return res.data;
  },

  counterOffer: async (offerId: number, data: { amount: number; message?: string }): Promise<any> => {
    const res = await apiClient.post(`/offers/${offerId}/counter-offer`, {
      counterAmount: data.amount,
      counterMessage: data.message,
    });
    return res.data;
  },

  acceptCounterOffer: async (offerId: number): Promise<any> => {
    const res = await apiClient.post(`/offers/${offerId}/accept-counter`);
    return res.data;
  },

  rejectCounterOffer: async (offerId: number): Promise<any> => {
    const res = await apiClient.post(`/offers/${offerId}/reject-counter`);
    return res.data;
  }
};

// ==================== APPOINTMENTS SERVICE ====================
export const appointmentsService = {
  getMyAppointments: async (): Promise<Appointment[]> => {
    const res = await apiClient.get<Appointment[]>('/appointments/my-appointments');
    return res.data || [];
  },

  requestAppointment: async (data: { propertyId: number; date: string; timeSlot: string; clientNotes?: string }): Promise<any> => {
    const appointmentDate = combineDateAndTime(data.date, data.timeSlot);
    const res = await apiClient.post('/appointments', {
      propertyId: data.propertyId,
      appointmentDate,
      comments: data.clientNotes ?? '',
    });
    return res.data;
  },

  confirmAppointment: async (id: number, notes?: string): Promise<any> => {
    const res = await apiClient.patch(`/appointments/${id}/confirm`, notes || null, {
      headers: { 'Content-Type': 'application/json' }
    });
    return res.data;
  },

  cancelAppointment: async (id: number, reason?: string): Promise<any> => {
    const res = await apiClient.patch(`/appointments/${id}/cancel`, reason || null, {
      headers: { 'Content-Type': 'application/json' }
    });
    return res.data;
  }
};

// ==================== FAVORITES SERVICE ====================
export const favoritesService = {
  getAll: async (): Promise<any[]> => {
    const res = await apiClient.get<any[]>('/favorites');
    return res.data || [];
  },

  checkIsFavorite: async (propertyId: number): Promise<boolean> => {
    const res = await apiClient.get<boolean>(`/favorites/check/${propertyId}`);
    return res.data;
  },

  add: async (propertyId: number): Promise<any> => {
    const res = await apiClient.post(`/favorites/${propertyId}`);
    return res.data;
  },

  remove: async (propertyId: number): Promise<any> => {
    const res = await apiClient.delete(`/favorites/${propertyId}`);
    return res.data;
  }
};

// ==================== SAVED SEARCHES SERVICE ====================
export const savedSearchesService = {
  getAll: async (): Promise<SavedSearch[]> => {
    const res = await apiClient.get<SavedSearch[]>('/savedsearches');
    return res.data || [];
  },

  save: async (data: any): Promise<any> => {
    const res = await apiClient.post('/savedsearches', data);
    return res.data;
  },

  toggleAlerts: async (id: number): Promise<any> => {
    const res = await apiClient.patch(`/savedsearches/${id}/toggle-alerts`);
    return res.data;
  },

  delete: async (id: number): Promise<any> => {
    const res = await apiClient.delete(`/savedsearches/${id}`);
    return res.data;
  }
};

// ==================== CHATS SERVICE ====================
export const chatsService = {
  getConversations: async (): Promise<ChatMessage[]> => {
    const res = await apiClient.get<ChatMessage[]>('/chats/conversations');
    return res.data || [];
  },

  getThread: async (clientId: string, agentId: string, propertyId?: number): Promise<ChatMessage[]> => {
    const res = await apiClient.get<ChatMessage[]>('/chats/thread', {
      params: { clientId, agentId, propertyId }
    });
    return res.data || [];
  },

  sendMessage: async (data: { propertyId?: number; recipientId: string; messageContent: string }): Promise<any> => {
    const res = await apiClient.post('/chats/send', data);
    return res.data;
  }
};

// ==================== VERIFICATIONS (KYC) SERVICE ====================
export const verificationsService = {
  getAll: async (): Promise<any[]> => {
    const res = await apiClient.get<any[]>('/verifications');
    return (res.data || []).map(adaptVerification);
  },

  getMyStatus: async (): Promise<any> => {
    const res = await apiClient.get<any>('/verifications/my-status');
    return adaptVerification(res.data);
  },

  submit: async (formData: FormData): Promise<any> => {
    const res = await apiClient.post('/verifications/submit', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return res.data;
  },

  approve: async (id: number): Promise<any> => {
    const res = await apiClient.post(`/verifications/${id}/approve`);
    return res.data;
  },

  reject: async (id: number, reason: string): Promise<any> => {
    const res = await apiClient.post(`/verifications/${id}/reject`, { reason });
    return res.data;
  }
};

// ==================== SUBSCRIPTIONS SERVICE ====================
export const subscriptionsService = {
  getPlans: async (): Promise<SubscriptionPlan[]> => {
    const res = await apiClient.get<SubscriptionPlan[]>('/subscriptions/plans');
    return res.data || [];
  },

  getMySubscription: async (): Promise<any> => {
    const res = await apiClient.get<any>('/subscriptions/my-subscription');
    return res.data;
  },

  upgrade: async (planId: number, paymentMethodId?: string): Promise<any> => {
    const res = await apiClient.post('/subscriptions/upgrade', { planId, paymentMethodId });
    return res.data;
  },

  updatePlanCommission: async (planId: number, percentage: number): Promise<any> => {
    const res = await apiClient.patch(`/subscriptions/plans/${planId}/commission`, { percentage });
    return res.data;
  },

  // ==================== Administración de planes (CRUD) ====================
  getAdminPlans: async (): Promise<SubscriptionPlan[]> => {
    const res = await apiClient.get<SubscriptionPlan[]>('/subscriptions/admin/plans');
    return res.data || [];
  },

  getAdminPlan: async (planId: number): Promise<SubscriptionPlan> => {
    const res = await apiClient.get<SubscriptionPlan>(`/subscriptions/admin/plans/${planId}`);
    return res.data;
  },

  createPlan: async (model: SubscriptionPlanInput): Promise<any> => {
    const res = await apiClient.post('/subscriptions/admin/plans', model);
    return res.data;
  },

  updatePlan: async (planId: number, model: SubscriptionPlanInput): Promise<any> => {
    const res = await apiClient.put(`/subscriptions/admin/plans/${planId}`, model);
    return res.data;
  },

  setPlanActive: async (planId: number, isActive: boolean): Promise<any> => {
    const res = await apiClient.patch(`/subscriptions/admin/plans/${planId}/active`, { isActive });
    return res.data;
  },

  deletePlan: async (planId: number): Promise<any> => {
    const res = await apiClient.delete(`/subscriptions/admin/plans/${planId}`);
    return res.data;
  }
};

// ==================== OWNERS SERVICE ====================
export const ownersService = {
  getMyProperties: async (): Promise<any> => {
    const res = await apiClient.get('/owners/my-properties');
    if (res.data && Array.isArray(res.data.properties)) {
      return {
        ...res.data,
        properties: adaptProperties(res.data.properties),
      };
    }
    return res.data;
  },

  createProperty: async (formData: FormData): Promise<any> => {
    const res = await apiClient.post('/owners/properties', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return res.data;
  },

  getProperty: async (id: number): Promise<any> => {
    const res = await apiClient.get(`/owners/properties/${id}`);
    return adaptProperty(res.data);
  },

  updateProperty: async (id: number, formData: FormData): Promise<any> => {
    const res = await apiClient.put(`/owners/properties/${id}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return res.data;
  },

  deleteProperty: async (id: number): Promise<any> => {
    const res = await apiClient.delete(`/owners/properties/${id}`);
    return res.data;
  }
};

// ==================== PROVINCES SERVICE ====================
export const provincesService = {
  getAll: async (): Promise<any[]> => {
    const res = await apiClient.get<any[]>('/provinces');
    return res.data || [];
  },

  getMunicipalities: async (provinceId: number): Promise<any[]> => {
    const res = await apiClient.get<any[]>(`/provinces/${provinceId}/municipalities`);
    return res.data || [];
  }
};

// ==================== CURRENCY SERVICE ====================
export const currencyService = {
  getRates: async (): Promise<any> => {
    const res = await apiClient.get('/currency/rates');
    return res.data;
  }
};

// ==================== AGENTS SERVICE (public directory) ====================
export const agentsService = {
  getPublicAgents: async (): Promise<any[]> => {
    const res = await apiClient.get<any[]>('/agents');
    return res.data || [];
  },

  getPublicAgentById: async (id: string): Promise<any> => {
    const res = await apiClient.get<any>(`/agents/${id}`);
    return res.data;
  },
};

// ==================== ADMIN SERVICE ====================
export const adminService = {
  getDashboardKPIs: async (): Promise<DashboardKPIs> => {
    const res = await apiClient.get<DashboardKPIs>('/admin/dashboard-kpis');
    return res.data;
  },

  getAgents: async (): Promise<any[]> => {
    const res = await apiClient.get<any[]>('/admin/agents');
    return res.data || [];
  },

  toggleAgentStatus: async (agentId: string, isActive: boolean): Promise<any> => {
    const res = await apiClient.patch(`/admin/agents/${agentId}/toggle-status`, isActive, {
      headers: { 'Content-Type': 'application/json' }
    });
    return res.data;
  },

  reassignProperties: async (sourceAgentId: string, targetAgentId: string): Promise<any> => {
    const res = await apiClient.post('/admin/agents/reassign-properties', null, {
      params: { sourceAgentId, targetAgentId }
    });
    return res.data;
  },

  deleteAgent: async (agentId: string): Promise<any> => {
    const res = await apiClient.delete(`/admin/agents/${agentId}`);
    return res.data;
  },

  getAdmins: async (): Promise<any[]> => {
    const res = await apiClient.get<any[]>('/admin/admins');
    return res.data || [];
  },

  toggleAdminStatus: async (userId: string, isActive: boolean): Promise<any> => {
    const res = await apiClient.patch(`/admin/admins/${userId}/toggle-status`, isActive, {
      headers: { 'Content-Type': 'application/json' }
    });
    return res.data;
  },

  getDevelopers: async (): Promise<any[]> => {
    const res = await apiClient.get<any[]>('/admin/developers');
    return res.data || [];
  },

  toggleDeveloperStatus: async (userId: string, isActive: boolean): Promise<any> => {
    const res = await apiClient.patch(`/admin/developers/${userId}/toggle-status`, isActive, {
      headers: { 'Content-Type': 'application/json' }
    });
    return res.data;
  },

  createDeveloper: async (data: any): Promise<any> => {
    const res = await apiClient.post('/admin/developers', data);
    return res.data;
  },

  updateDeveloper: async (userId: string, data: any): Promise<any> => {
    const res = await apiClient.put(`/admin/developers/${userId}`, data);
    return res.data;
  },

  createAdmin: async (data: any): Promise<any> => {
    const res = await apiClient.post('/admin/admins', data);
    return res.data;
  }
};

// ==================== CATALOGS SERVICE ====================
export const catalogsService = {
  getPropertyTypes: async (): Promise<PropertyType[]> => {
    const res = await apiClient.get<PropertyType[]>('/propertytypes');
    return res.data || [];
  },

  createPropertyType: async (data: { name: string; description: string }): Promise<any> => {
    const res = await apiClient.post('/propertytypes', data);
    return res.data;
  },

  updatePropertyType: async (id: number, data: { name: string; description: string }): Promise<any> => {
    const res = await apiClient.put(`/propertytypes/${id}`, data);
    return res.data;
  },

  deletePropertyType: async (id: number): Promise<any> => {
    const res = await apiClient.delete(`/propertytypes/${id}`);
    return res.data;
  },

  getSaleTypes: async (): Promise<SaleType[]> => {
    const res = await apiClient.get<SaleType[]>('/saletypes');
    return res.data || [];
  },

  createSaleType: async (data: { name: string; description: string }): Promise<any> => {
    const res = await apiClient.post('/saletypes', data);
    return res.data;
  },

  updateSaleType: async (id: number, data: { name: string; description: string }): Promise<any> => {
    const res = await apiClient.put(`/saletypes/${id}`, data);
    return res.data;
  },

  deleteSaleType: async (id: number): Promise<any> => {
    const res = await apiClient.delete(`/saletypes/${id}`);
    return res.data;
  },

  getImprovements: async (): Promise<Improvement[]> => {
    const res = await apiClient.get<Improvement[]>('/improvements');
    return res.data || [];
  },

  createImprovement: async (data: { name: string; description: string }): Promise<any> => {
    const res = await apiClient.post('/improvements', data);
    return res.data;
  },

  updateImprovement: async (id: number, data: { name: string; description: string }): Promise<any> => {
    const res = await apiClient.put(`/improvements/${id}`, data);
    return res.data;
  },

  deleteImprovement: async (id: number): Promise<any> => {
    const res = await apiClient.delete(`/improvements/${id}`);
    return res.data;
  }
};

// ==================== SIMULATOR SERVICE ====================
export const simulatorService = {
  calculate: async (params: { propertyPrice: number; downPayment: number; annualRate?: number; termInYears?: number }): Promise<MortgageSimulationResult> => {
    const res = await apiClient.post<MortgageSimulationResult>('/simulator/calculate', {
      propertyPrice: params.propertyPrice,
      downPayment: params.downPayment,
      annualRate: params.annualRate ?? 11.5,
      termInYears: params.termInYears ?? 20,
    });
    return res.data;
  }
};

// ==================== AVM SERVICE (Valuación Automatizada - F-01) ====================
export const avmService = {
  calculate: async (propertyId: number, searchRadiusKm?: number): Promise<ValuationResult> => {
    const res = await apiClient.get<ValuationResult>(`/valuation/properties/${propertyId}/valuation`, {
      params: { searchRadiusKm: searchRadiusKm ?? 5 },
    });
    return res.data;
  },
  getLast: async (propertyId: number): Promise<ValuationResult | null> => {
    const res = await apiClient.get<ValuationResult>(`/valuation/properties/${propertyId}/valuation/last`);
    return res.data || null;
  }
};

// ==================== LEAD PIPELINE SERVICE (Kanban - F-04) ====================
export const leadPipelineService = {
  getAll: async (): Promise<Lead[]> => {
    const res = await apiClient.get<Lead[]>('/leads');
    return res.data || [];
  },
  getByStage: async (stage: string): Promise<Lead[]> => {
    const res = await apiClient.get<Lead[]>(`/leads/stage/${encodeURIComponent(stage)}`);
    return res.data || [];
  },
  getStats: async (): Promise<LeadPipelineStats> => {
    const res = await apiClient.get<LeadPipelineStats>('/leads/stats');
    return res.data;
  },
  getById: async (id: number): Promise<Lead> => {
    const res = await apiClient.get<Lead>(`/leads/${id}`);
    return res.data;
  },
  create: async (payload: CreateLeadPayload): Promise<Lead> => {
    const res = await apiClient.post<Lead>('/leads', payload);
    return res.data;
  },
  update: async (id: number, payload: UpdateLeadPayload): Promise<Lead> => {
    const res = await apiClient.put<Lead>(`/leads/${id}`, payload);
    return res.data;
  },
  moveToStage: async (id: number, stage: string): Promise<Lead> => {
    const res = await apiClient.patch<Lead>(`/leads/${id}/stage`, { stage });
    return res.data;
  },
  updateSortOrder: async (id: number, sortOrder: number): Promise<Lead> => {
    const res = await apiClient.patch<Lead>(`/leads/${id}/sort-order`, { sortOrder });
    return res.data;
  },
  remove: async (id: number): Promise<void> => {
    await apiClient.delete(`/leads/${id}`);
  }
};

// ==================== BUY ABILITY SERVICE (Capacidad de Compra - F-09) ====================
export const buyAbilityService = {
  evaluate: async (payload: BuyAbilityRequest): Promise<BuyAbilityResult> => {
    const res = await apiClient.post<BuyAbilityResult>('/buyability/evaluate', payload);
    return res.data;
  },
  getLast: async (): Promise<BuyAbilityResult | null> => {
    const res = await apiClient.get<BuyAbilityResult>('/buyability/last');
    return res.data || null;
  }
};

// ==================== REVIEWS SERVICE (Reseñas a Agentes - Ítem 2.9) ====================
export const reviewsService = {
  getByAgent: async (agentId: string): Promise<Review[]> => {
    const res = await apiClient.get<Review[]>(`/reviews/agent/${agentId}`);
    return res.data || [];
  },

  getSummary: async (agentId: string): Promise<ReviewSummary> => {
    const res = await apiClient.get<ReviewSummary>(`/reviews/agent/${agentId}/summary`);
    return res.data;
  },

  canReview: async (agentId: string, propertyId: number): Promise<boolean> => {
    const res = await apiClient.get<boolean>('/reviews/can-review', {
      params: { agentId, propertyId }
    });
    return res.data;
  },

  hasReviewed: async (agentId: string, propertyId: number): Promise<boolean> => {
    const res = await apiClient.get<boolean>('/reviews/has-reviewed', {
      params: { agentId, propertyId }
    });
    return res.data;
  },

  create: async (data: { agentId: string; propertyId: number; rating: number; comment?: string }): Promise<any> => {
    const res = await apiClient.post('/reviews', data);
    return res.data;
  },

  getAll: async (): Promise<Review[]> => {
    const res = await apiClient.get<Review[]>('/reviews');
    return res.data || [];
  }
};

// ==================== COMMISSIONS SERVICE (Comisiones - Ítem 2.3) ====================
export const commissionsService = {
  getMyCommissions: async (): Promise<Commission[]> => {
    const res = await apiClient.get<Commission[]>('/commissions/my-commissions');
    return res.data || [];
  },

  getMySummary: async (): Promise<CommissionSummary> => {
    const res = await apiClient.get<CommissionSummary>('/commissions/my-commissions/summary');
    return res.data;
  },

  getAll: async (): Promise<Commission[]> => {
    const res = await apiClient.get<Commission[]>('/commissions');
    return res.data || [];
  },

  markAsPaid: async (commissionId: number): Promise<any> => {
    const res = await apiClient.patch(`/commissions/${commissionId}/pay`);
    return res.data;
  }
};

// ==================== PROPERTY DOCUMENTS SERVICE (Documentos - Ítem 2.6) ====================
export const documentsService = {
  upload: async (propertyId: number, documentType: string, file: File): Promise<any> => {
    const formData = new FormData();
    formData.append('propertyId', String(propertyId));
    formData.append('documentType', documentType);
    formData.append('file', file);
    const res = await apiClient.post('/documents', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return res.data;
  },

  getByProperty: async (propertyId: number): Promise<PropertyDocument[]> => {
    const res = await apiClient.get<PropertyDocument[]>(`/documents/property/${propertyId}`);
    return res.data || [];
  },

  getAll: async (): Promise<PropertyDocument[]> => {
    const res = await apiClient.get<PropertyDocument[]>('/documents');
    return res.data || [];
  },

  remove: async (documentId: number): Promise<any> => {
    const res = await apiClient.delete(`/documents/${documentId}`);
    return res.data;
  }
};

// ==================== AI CONVERSATIONAL SEARCH SERVICE (F-17) ====================
export const aiSearchService = {
  searchByAi: async (query: string): Promise<AiSearchInterpretation> => {
    const res = await apiClient.post<AiSearchInterpretation>('/aisearch/query', { query });
    if (res.data && Array.isArray(res.data.properties)) {
      res.data.properties = adaptProperties(res.data.properties);
    }
    return res.data;
  },

  interpret: async (query: string): Promise<AiSearchInterpretation> => {
    const res = await apiClient.post<AiSearchInterpretation>('/aisearch/interpret', { query });
    return res.data;
  },

  getSuggestions: async (): Promise<string[]> => {
    const res = await apiClient.get<AiSearchSuggestions>('/aisearch/suggestions');
    return res.data?.suggestions || [];
  }
};

