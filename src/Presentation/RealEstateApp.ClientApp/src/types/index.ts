export type UserRole = 'Admin' | 'Agent' | 'Client' | 'Developer' | 'Owner';

export interface FilterState {
  code?: string;
  propertyTypeId?: number;
  saleTypeId?: number;
  minPrice?: number;
  maxPrice?: number;
  minRooms?: number;
  minBathrooms?: number;
  minSizeInMeters?: number;
  maxSizeInMeters?: number;
  provinceId?: number;
  municipalityId?: number;
  sector?: string;
  agentId?: string;
  onlyFeatured?: boolean;
  onlyVerifiedAgents?: boolean;
  onlyFinanciable?: boolean;
  onlyWithVirtualTour?: boolean;
  improvementIds?: number[];
  userLat?: number;
  userLng?: number;
  maxDistanceKm?: number;
}

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
  propertiesCount?: number;
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
  name?: string;
  fullAddress?: string;
  montoSeparacion?: number;
  porcentajeInicialRequerido?: number;
  propertyTypeId: number;
  propertyTypeName?: string;
  saleTypeId: number;
  saleTypeName?: string;
  price: number;
  landSizeMeters: number;
  sizeInMeters?: number;
  bedrooms: number;
  rooms?: number;
  bathrooms: number;
  description: string;
  agentId: string;
  agentName?: string;
  agentPhone?: string;
  agentEmail?: string;
  agentPhotoUrl?: string;
  status: string; // 'Available' | 'Reserved' | 'Sold' | 'Disponible' | 'Reservada' | 'Vendida'
  isFeatured?: boolean;
  featuredUntil?: string;
  provinceName?: string;
  municipalityName?: string;
  sector?: string;
  latitude?: number;
  longitude?: number;
  videoTourUrl?: string;
  videoUrl?: string;
  virtualTour360Url?: string;
  tour360Url?: string;
  images: PropertyImage[];
  improvements: Improvement[];
  created?: string;
}

export interface Favorite {
  id?: number;
  clientId?: string;
  propertyId: number;
  created?: string;
  property?: Property;
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
  agentId?: string;
  amount: number;
  status: 'Pending' | 'Accepted' | 'Rejected' | 'CounterOffered' | 'Aceptada' | 'Pendiente' | 'Rechazada' | 'Contraofertada';
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
  status: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled' | 'Pendiente' | 'Confirmada' | 'Completada' | 'Cancelada';
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
  minRooms?: number;
  maxRooms?: number;
  minBathrooms?: number;
  maxBathrooms?: number;
  minSizeInMeters?: number;
  maxSizeInMeters?: number;
  provinceId?: number;
  municipalityId?: number;
  sector?: string;
  onlyFinanciable?: boolean;
  onlyWithVirtualTour?: boolean;
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
  status: 'Pending' | 'Approved' | 'Rejected' | 'Pendiente' | 'Aprobado' | 'Rechazado';
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
  maxActiveProperties?: number;
  maxFeaturedProperties: number;
  allows3DTours?: boolean;
  allowsVideo?: boolean;
  commissionPercentage?: number;
  isActive?: boolean;
}

export interface SubscriptionPlanInput {
  name: string;
  description?: string;
  monthlyPrice: number;
  maxActiveProperties: number;
  maxFeaturedProperties: number;
  allows3DTours: boolean;
  allowsVideo: boolean;
  commissionPercentage?: number;
  isActive: boolean;
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
  maxActiveProperties?: number;
  maxFeaturedProperties?: number;
  commissionPercentage?: number;
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

// ============ AVM (Valuación Automatizada - F-01) ============
export interface ComparableProperty {
  propertyId: number;
  name: string;
  code: string;
  price: number;
  currency: string;
  sizeInMeters: number;
  pricePerSqm: number;
  sector?: string;
  municipalityName?: string;
  rooms: number;
  bathrooms: number;
  status: string;
  distanceKm: number;
}

export interface ValuationResult {
  propertyId: number;
  propertyName: string;
  propertyCode: string;
  currentPrice: number;
  currentCurrency: string;
  estimatedPricePerSqm: number;
  estimatedTotalPrice: number;
  propertySizeSqm: number;
  comparableCount: number;
  minPricePerSqm: number;
  maxPricePerSqm: number;
  averagePricePerSqm: number;
  standardDeviation: number;
  priceDifference: number;
  priceDifferencePercentage: number;
  valuationRating: string; // Sobrevalorada | Justa | Subvalorada
  confidenceScore: number;
  searchRadiusKm: number;
  comparables: ComparableProperty[];
  calculatedAt: string;
}

// ============ Lead Pipeline / Kanban (F-04) ============
export interface Lead {
  id: number;
  leadName: string;
  leadEmail?: string;
  leadPhone?: string;
  notes?: string;
  agentId: string;
  stage: string; // 'Nuevo Lead' | 'Contactado' | 'Visita' | 'Oferta' | 'Cierre' | 'Ganado' | 'Perdido'
  priority: string; // Baja | Normal | Alta | Urgente
  source?: string;
  estimatedBudget?: number;
  budgetCurrency?: string;
  propertyId?: number;
  propertyName?: string;
  lastContactDate?: string;
  nextFollowUpDate?: string;
  sortOrder: number;
  created: string;
}

export interface CreateLeadPayload {
  leadName: string;
  leadEmail?: string;
  leadPhone?: string;
  notes?: string;
  priority?: string;
  source?: string;
  estimatedBudget?: number;
  budgetCurrency?: string;
  propertyId?: number;
}

export interface UpdateLeadPayload {
  leadName?: string;
  leadEmail?: string;
  leadPhone?: string;
  notes?: string;
  priority?: string;
  source?: string;
  estimatedBudget?: number;
  budgetCurrency?: string;
  propertyId?: number;
  nextFollowUpDate?: string;
}

export interface LeadPipelineStats {
  totalLeads: number;
  newLeadCount: number;
  contactedCount: number;
  visitCount: number;
  offerCount: number;
  closingCount: number;
  wonCount: number;
  lostCount: number;
  conversionRate: number;
  totalPipelineValue: number;
  leadsNeedingFollowUp: Lead[];
}

// ============ Buy Ability (Capacidad de Compra - F-09) ============
export interface BuyAbilityRequest {
  monthlyGrossIncome: number;
  monthlyNetIncome: number;
  monthlyDebtPayments: number;
  availableDownPayment: number;
  currency?: string;
}

export interface BuyAbilityResult {
  evaluationId?: number;
  clientId: string;
  monthlyGrossIncome: number;
  monthlyNetIncome: number;
  monthlyDebtPayments: number;
  availableDownPayment: number;
  maxMonthlyPayment: number;
  maxMortgageAmount: number;
  maxPropertyPrice: number;
  debtToIncomeRatio: number;
  creditScoreRating: string;
  estimatedAnnualRate: number;
  recommendedTermYears: number;
  evaluationResult: string; // Aprobado | Pre-Aprobado | No Aprobado
  currency: string;
  observations?: string;
  evaluationDate: string;
}

// ============ Agent Reviews / Reseñas (Ítem 2.9) ============
export interface Review {
  id: number;
  agentId: string;
  clientId: string;
  clientName?: string;
  clientEmail?: string;
  propertyId: number;
  propertyCode?: string;
  propertyName?: string;
  rating: number; // 1-5
  comment?: string;
  created?: string;
}

export interface ReviewSummary {
  reviewCount: number;
  averageRating: number;
  fiveStars: number;
  fourStars: number;
  threeStars: number;
  twoStars: number;
  oneStar: number;
}

// ============ Commissions / Comisiones (Ítem 2.3) ============
export interface Commission {
  id: number;
  agentId: string;
  agentName?: string;
  propertyId: number;
  propertyCode?: string;
  propertyName?: string;
  offerId: number;
  salePrice: number;
  rate: number; // porcentaje, ej: 5.00
  amount: number;
  status: 'Pendiente' | 'Pagada';
  created?: string;
}

export interface CommissionSummary {
  totalCount: number;
  totalAmount: number;
  pendingCount: number;
  pendingAmount: number;
  paidCount: number;
  paidAmount: number;
  averageRate: number;
}

// ============ Property Documents / Documentos (Ítem 2.6) ============
export interface PropertyDocument {
  id: number;
  propertyId: number;
  propertyCode?: string;
  propertyName?: string;
  documentType: string;
  fileUrl: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedBy: string;
  uploadedByName?: string;
  uploadedAt?: string;
  isImage: boolean;
}

export const DOCUMENT_TYPES = [
  'Título de propiedad',
  'Contrato de compra-venta',
  'Contrato de alquiler',
  'Carta de pre-aprobación',
  'Certificación de registro',
  'Otro',
] as const;

// ============ AI Conversational Search (F-17) ============
export interface AiSearchQueryRequest {
  query: string;
}

export interface AiExtractedEntities {
  propertyType?: string;
  saleType?: string;
  province?: string;
  sector?: string;
  minRooms?: number;
  minBathrooms?: number;
  minPrice?: number;
  maxPrice?: number;
  improvements: string[];
  hasVirtualTour?: boolean;
  isFinanciable?: boolean;
  isFeatured?: boolean;
}

export interface AiSearchInterpretation {
  originalQuery: string;
  explanation: string;
  confidenceScore: number;
  extractedEntities: AiExtractedEntities;
  parsedFilter: FilterState;
  matchedPropertiesCount: number;
  properties: Property[];
}

export interface AiSearchSuggestions {
  suggestions: string[];
}

