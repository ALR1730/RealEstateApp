export type UserRole = 'Admin' | 'Agent' | 'Client' | 'Developer';

export interface User {
  id: string;
  userName: string;
  email: string;
  roles: string[];
  isVerified?: boolean;
  jwToken?: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  photoUrl?: string;
}

export interface AuthResponse {
  id: string;
  userName: string;
  email: string;
  roles: string[];
  isVerified: boolean;
  jwToken: string;
  hasError: boolean;
  error?: string;
}

export interface PropertyImage {
  id: number;
  propertyId: number;
  imageUrl: string;
  isMain?: boolean;
}

export interface Improvement {
  id: number;
  name: string;
  description?: string;
}

export interface PropertyType {
  id: number;
  name: string;
  description?: string;
  propertiesCount?: number;
}

export interface SaleType {
  id: number;
  name: string;
  description?: string;
  propertiesCount?: number;
}

export interface Property {
  id: number;
  code: string;
  propertyTypeId: number;
  propertyTypeName?: string;
  saleTypeId: number;
  saleTypeName?: string;
  price: number;
  landSizeMeters: number;
  bedrooms: number;
  bathrooms: number;
  description: string;
  agentId: string;
  agentName?: string;
  agentPhone?: string;
  agentEmail?: string;
  agentPhotoUrl?: string;
  status: string; // 'Available' | 'Reserved' | 'Sold'
  isFeatured?: boolean;
  featuredUntil?: string;
  provinceName?: string;
  municipalityName?: string;
  sector?: string;
  latitude?: number;
  longitude?: number;
  videoTourUrl?: string;
  virtualTour360Url?: string;
  images: PropertyImage[];
  improvements: Improvement[];
  created?: string;
}

export interface Offer {
  id: number;
  propertyId: number;
  propertyCode?: string;
  propertyDescription?: string;
  propertyPrice?: number;
  propertyMainImageUrl?: string;
  clientId: string;
  clientName?: string;
  clientEmail?: string;
  clientPhone?: string;
  amount: number;
  status: 'Pending' | 'Accepted' | 'Rejected' | 'CounterOffered';
  created?: string;
  notes?: string;
  counterOfferAmount?: number;
  counterOfferMessage?: string;
}

export interface Appointment {
  id: number;
  propertyId: number;
  propertyCode?: string;
  propertyDescription?: string;
  propertyMainImageUrl?: string;
  clientId: string;
  clientName?: string;
  clientEmail?: string;
  clientPhone?: string;
  agentId: string;
  agentName?: string;
  date: string;
  timeSlot: string;
  status: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled';
  clientNotes?: string;
  agentNotes?: string;
}

export interface ChatMessage {
  id: number;
  propertyId: number;
  propertyCode?: string;
  senderId: string;
  senderName?: string;
  recipientId: string;
  recipientName?: string;
  messageContent: string;
  sentAt: string;
  sentAtFormatted?: string;
  isRead?: boolean;
}

export interface SavedSearch {
  id: number;
  userId: string;
  name: string;
  propertyTypeId?: number;
  saleTypeId?: number;
  minPrice?: number;
  maxPrice?: number;
  bedrooms?: number;
  bathrooms?: number;
  emailAlertsEnabled: boolean;
  created: string;
}

export interface AmortizationScheduleItem {
  period: number;
  installment: number;
  interest: number;
  principal: number;
  remainingBalance: number;
}

export interface MortgageSimulationResult {
  propertyPrice: number;
  downPayment: number;
  downPaymentPercentage: number;
  loanAmount: number;
  annualRate: number;
  termInYears: number;
  totalMonths: number;
  monthlyInstallment: number;
  totalInterest: number;
  totalCost: number;
  schedule: AmortizationScheduleItem[];
}

export interface DashboardKPIs {
  totalAvailableProperties: number;
  totalReservedProperties: number;
  totalSoldProperties: number;
  totalActiveAgents: number;
  totalInactiveAgents: number;
  totalClients: number;
  totalDevelopers: number;
  totalProperties: number;
  propertiesByType: { typeName: string; count: number }[];
}

export interface PriceHistory {
  id: number;
  propertyId: number;
  oldPrice: number;
  newPrice: number;
  percentageChange: number;
  currency: string;
  changeDate: string;
  changeReason?: string;
  changedByUserId?: string;
}

export interface AgentVerification {
  id: number;
  agentId: string;
  agentName?: string;
  agentEmail?: string;
  agentPhone?: string;
  cedula: string;
  cedulaFrontImageUrl?: string;
  cedulaBackImageUrl?: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  rejectionReason?: string;
  reviewedByAdminId?: string;
  reviewedAt?: string;
  submittedAt?: string;
}

export interface SubscriptionPlan {
  id: number;
  name: string;
  description?: string;
  monthlyPrice: number;
  maxFeaturedProperties: number;
  hasPrioritySupport?: boolean;
  hasDirectChat?: boolean;
  hasAnalyticsAccess?: boolean;
}

export interface AgentSubscription {
  id: number;
  agentId: string;
  subscriptionPlanId: number;
  planName?: string;
  monthlyPrice?: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  maxFeaturedProperties?: number;
}

export interface Municipality {
  id: number;
  name: string;
  provinceId: number;
}

export interface Province {
  id: number;
  name: string;
  municipalities?: Municipality[];
}

export interface OwnerPropertySummary {
  maxAllowed: number;
  currentCount: number;
  canCreate: boolean;
  properties: Property[];
}
