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
  PriceHistory
} from '../types';

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

// ==================== PROPERTIES SERVICE ====================
export const propertiesService = {
  getAll: async (params?: any): Promise<Property[]> => {
    const res = await apiClient.get<Property[]>('/properties', { params });
    return res.data || [];
  },

  getById: async (id: number): Promise<Property> => {
    const res = await apiClient.get<Property>(`/properties/${id}`);
    return res.data;
  },

  getByCode: async (code: string): Promise<Property> => {
    const res = await apiClient.get<Property>(`/properties/code/${code}`);
    return res.data;
  },

  getMyProperties: async (): Promise<Property[]> => {
    const res = await apiClient.get<Property[]>('/properties/my-properties');
    return res.data || [];
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
    const res = await apiClient.post('/offers', data);
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
    const res = await apiClient.post(`/offers/${offerId}/counter-offer`, data);
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
    const res = await apiClient.post('/appointments', data);
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
    return res.data || [];
  },

  getMyStatus: async (): Promise<any> => {
    const res = await apiClient.get<any>('/verifications/my-status');
    return res.data;
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
  getPlans: async (): Promise<any[]> => {
    const res = await apiClient.get<any[]>('/subscriptions/plans');
    return res.data || [];
  },

  getMySubscription: async (): Promise<any> => {
    const res = await apiClient.get<any>('/subscriptions/my-subscription');
    return res.data;
  },

  upgrade: async (planId: number, paymentMethodId?: string): Promise<any> => {
    const res = await apiClient.post('/subscriptions/upgrade', { planId, paymentMethodId });
    return res.data;
  }
};

// ==================== OWNERS SERVICE ====================
export const ownersService = {
  getMyProperties: async (): Promise<any> => {
    const res = await apiClient.get('/owners/my-properties');
    return res.data;
  },

  createProperty: async (formData: FormData): Promise<any> => {
    const res = await apiClient.post('/owners/properties', formData, {
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
