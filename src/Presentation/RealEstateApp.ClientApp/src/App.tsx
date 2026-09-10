import React, { Suspense, lazy } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { NotificationProvider } from './context/NotificationContext';
import { CurrencyProvider } from './context/CurrencyContext';
import { ThemeProvider } from './context/ThemeContext';
import { CompareProvider } from './context/CompareContext';
import { Navbar } from './components/common/Navbar';
import { Footer } from './components/common/Footer';
import { Sidebar } from './components/common/Sidebar';
import { Loader } from './components/common/Loader';

// Public Pages (lazy)
const HomePage = lazy(() => import('./pages/public/HomePage').then((m) => ({ default: m.HomePage })));
const PropertiesCatalogPage = lazy(() => import('./pages/public/PropertiesCatalogPage').then((m) => ({ default: m.PropertiesCatalogPage })));
const PropertyDetailPage = lazy(() => import('./pages/public/PropertyDetailPage').then((m) => ({ default: m.PropertyDetailPage })));
const AgentsPage = lazy(() => import('./pages/public/AgentsPage').then((m) => ({ default: m.AgentsPage })));
const AgentDetailPage = lazy(() => import('./pages/public/AgentDetailPage').then((m) => ({ default: m.AgentDetailPage })));
const MortgagePage = lazy(() => import('./pages/public/MortgagePage').then((m) => ({ default: m.MortgagePage })));
const MapPage = lazy(() => import('./pages/public/MapPage').then((m) => ({ default: m.MapPage })));
const NotFoundPage = lazy(() => import('./pages/public/NotFoundPage').then((m) => ({ default: m.NotFoundPage })));
const ComparePage = lazy(() => import('./pages/public/ComparePage').then((m) => ({ default: m.ComparePage })));

// Auth Pages (lazy)
const LoginPage = lazy(() => import('./pages/auth/LoginPage').then((m) => ({ default: m.LoginPage })));
const RegisterClientPage = lazy(() => import('./pages/auth/RegisterClientPage').then((m) => ({ default: m.RegisterClientPage })));
const RegisterAgentPage = lazy(() => import('./pages/auth/RegisterAgentPage').then((m) => ({ default: m.RegisterAgentPage })));
const ForgotPasswordPage = lazy(() => import('./pages/auth/ForgotPasswordPage').then((m) => ({ default: m.ForgotPasswordPage })));
const ResetPasswordPage = lazy(() => import('./pages/auth/ResetPasswordPage').then((m) => ({ default: m.ResetPasswordPage })));
const PendingActivationPage = lazy(() => import('./pages/auth/PendingActivationPage').then((m) => ({ default: m.PendingActivationPage })));
const ConfirmEmailPage = lazy(() => import('./pages/auth/ConfirmEmailPage').then((m) => ({ default: m.ConfirmEmailPage })));

// Client Pages (lazy)
const ClientDashboard = lazy(() => import('./pages/client/ClientDashboard').then((m) => ({ default: m.ClientDashboard })));
const MyFavoritesPage = lazy(() => import('./pages/client/MyFavoritesPage').then((m) => ({ default: m.MyFavoritesPage })));
const MyOffersPage = lazy(() => import('./pages/client/MyOffersPage').then((m) => ({ default: m.MyOffersPage })));
const MyAppointmentsPage = lazy(() => import('./pages/client/MyAppointmentsPage').then((m) => ({ default: m.MyAppointmentsPage })));
const SavedSearchesPage = lazy(() => import('./pages/client/SavedSearchesPage').then((m) => ({ default: m.SavedSearchesPage })));
const ClientChatPage = lazy(() => import('./pages/client/ClientChatPage').then((m) => ({ default: m.ClientChatPage })));
const ClientProfilePage = lazy(() => import('./pages/client/ClientProfilePage').then((m) => ({ default: m.ClientProfilePage })));
const ActivityPage = lazy(() => import('./pages/client/ActivityPage').then((m) => ({ default: m.ActivityPage })));
const BuyAbilityPage = lazy(() => import('./pages/client/BuyAbilityPage').then((m) => ({ default: m.BuyAbilityPage })));

// Agent Pages (lazy)
const AgentDashboard = lazy(() => import('./pages/agent/AgentDashboard').then((m) => ({ default: m.AgentDashboard })));
const MyPropertiesPage = lazy(() => import('./pages/agent/MyPropertiesPage').then((m) => ({ default: m.MyPropertiesPage })));
const CreateEditPropertyPage = lazy(() => import('./pages/agent/CreateEditPropertyPage').then((m) => ({ default: m.CreateEditPropertyPage })));
const ReceivedOffersPage = lazy(() => import('./pages/agent/ReceivedOffersPage').then((m) => ({ default: m.ReceivedOffersPage })));
const AgentAppointmentsPage = lazy(() => import('./pages/agent/AgentAppointmentsPage').then((m) => ({ default: m.AgentAppointmentsPage })));
const AgentChatPage = lazy(() => import('./pages/agent/AgentChatPage').then((m) => ({ default: m.AgentChatPage })));
const AgentProfilePage = lazy(() => import('./pages/agent/AgentProfilePage').then((m) => ({ default: m.AgentProfilePage })));
const AgentVerificationPage = lazy(() => import('./pages/agent/AgentVerificationPage').then((m) => ({ default: m.AgentVerificationPage })));
const AgentSubscriptionPage = lazy(() => import('./pages/agent/AgentSubscriptionPage').then((m) => ({ default: m.AgentSubscriptionPage })));
const AvmValuationPage = lazy(() => import('./pages/agent/AvmValuationPage').then((m) => ({ default: m.AvmValuationPage })));
const LeadPipelinePage = lazy(() => import('./pages/agent/LeadPipelinePage').then((m) => ({ default: m.LeadPipelinePage })));
const AgentCommissionsPage = lazy(() => import('./pages/agent/AgentCommissionsPage').then((m) => ({ default: m.AgentCommissionsPage })));
const AgentDocumentsPage = lazy(() => import('./pages/agent/AgentDocumentsPage').then((m) => ({ default: m.AgentDocumentsPage })));

// Owner Pages (lazy)
const OwnerDashboard = lazy(() => import('./pages/owner/OwnerDashboard').then((m) => ({ default: m.OwnerDashboard })));
const CreateOwnerPropertyPage = lazy(() => import('./pages/owner/CreateOwnerPropertyPage').then((m) => ({ default: m.CreateOwnerPropertyPage })));

// Admin Pages (lazy)
const AdminDashboard = lazy(() => import('./pages/admin/AdminDashboard').then((m) => ({ default: m.AdminDashboard })));
const ManageAgentsPage = lazy(() => import('./pages/admin/ManageAgentsPage').then((m) => ({ default: m.ManageAgentsPage })));
const ManageDevelopersPage = lazy(() => import('./pages/admin/ManageDevelopersPage').then((m) => ({ default: m.ManageDevelopersPage })));
const CreateEditDeveloperPage = lazy(() => import('./pages/admin/CreateEditDeveloperPage').then((m) => ({ default: m.CreateEditDeveloperPage })));
const ManageAdminsPage = lazy(() => import('./pages/admin/ManageAdminsPage').then((m) => ({ default: m.ManageAdminsPage })));
const CreateAdminPage = lazy(() => import('./pages/admin/CreateAdminPage').then((m) => ({ default: m.CreateAdminPage })));
const ManageUsersPage = lazy(() => import('./pages/admin/ManageUsersPage').then((m) => ({ default: m.ManageUsersPage })));
const ManagePropertyTypesPage = lazy(() => import('./pages/admin/ManagePropertyTypesPage').then((m) => ({ default: m.ManagePropertyTypesPage })));
const ManageSaleTypesPage = lazy(() => import('./pages/admin/ManageSaleTypesPage').then((m) => ({ default: m.ManageSaleTypesPage })));
const ManageImprovementsPage = lazy(() => import('./pages/admin/ManageImprovementsPage').then((m) => ({ default: m.ManageImprovementsPage })));
const ManageVerificationsPage = lazy(() => import('./pages/admin/ManageVerificationsPage').then((m) => ({ default: m.ManageVerificationsPage })));
const ManageSubscriptionsPage = lazy(() => import('./pages/admin/ManageSubscriptionsPage').then((m) => ({ default: m.ManageSubscriptionsPage })));
const ManageAllPropertiesPage = lazy(() => import('./pages/admin/ManageAllPropertiesPage').then((m) => ({ default: m.ManageAllPropertiesPage })));
const AdminReviewsPage = lazy(() => import('./pages/admin/AdminReviewsPage').then((m) => ({ default: m.AdminReviewsPage })));
const AdminCommissionsPage = lazy(() => import('./pages/admin/AdminCommissionsPage').then((m) => ({ default: m.AdminCommissionsPage })));
const AdminDocumentsPage = lazy(() => import('./pages/admin/AdminDocumentsPage').then((m) => ({ default: m.AdminDocumentsPage })));

// Developer Pages (lazy)
const DeveloperDashboard = lazy(() => import('./pages/developer/DeveloperDashboard').then((m) => ({ default: m.DeveloperDashboard })));

// Public Layout
const PublicLayout: React.FC = () => {
  return (
    <div className="min-h-screen flex flex-col justify-between bg-slate-50 text-slate-900">
      <Navbar />
      <main className="flex-1">
        <Outlet />
      </main>
      <Footer />
    </div>
  );
};

// Protected Dashboard Layout with Sidebar
const DashboardLayout: React.FC = () => {
  return (
    <div className="min-h-screen flex flex-col justify-between bg-slate-50 text-slate-900">
      <Navbar />
      <div className="flex-1 flex max-w-7xl w-full mx-auto">
        <Sidebar />
        <main className="flex-1 p-6 sm:p-8 overflow-y-auto">
          <Outlet />
        </main>
      </div>
      <Footer />
    </div>
  );
};

// Protected Route Guard
interface ProtectedRouteProps {
  requiredRole?: 'Admin' | 'Agent' | 'Client' | 'Developer' | 'Owner';
  allowedRoles?: Array<'Admin' | 'Agent' | 'Client' | 'Developer' | 'Owner'>;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ requiredRole, allowedRoles }) => {
  const { isAuthenticated, isLoading, hasRole } = useAuth();

  if (isLoading) {
    return <Loader text="Comprobando autenticación..." />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole && !hasRole(requiredRole)) {
    return <Navigate to="/" replace />;
  }

  if (allowedRoles && !allowedRoles.some((r) => hasRole(r))) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
};

export const App: React.FC = () => {
  return (
    <ThemeProvider>
      <CurrencyProvider>
        <AuthProvider>
          <NotificationProvider>
            <CompareProvider>
              <BrowserRouter>
          <Suspense fallback={<Loader text="Cargando aplicación..." />}>
          <Routes>
            {/* Public Routes */}
            <Route element={<PublicLayout />}>
              <Route path="/" element={<HomePage />} />
              <Route path="/catalog" element={<PropertiesCatalogPage />} />
              <Route path="/properties" element={<Navigate to="/catalog" replace />} />
              <Route path="/property/:id" element={<PropertyDetailPage />} />
              <Route path="/agents" element={<AgentsPage />} />
              <Route path="/agent-details/:id" element={<AgentDetailPage />} />
              <Route path="/simulator" element={<MortgagePage />} />
              <Route path="/map" element={<MapPage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterClientPage />} />
              <Route path="/register-agent" element={<RegisterAgentPage />} />
              <Route path="/forgot-password" element={<ForgotPasswordPage />} />
              <Route path="/reset-password" element={<ResetPasswordPage />} />
              <Route path="/compare" element={<ComparePage />} />
              <Route path="/pending-activation" element={<PendingActivationPage />} />
              <Route path="/confirm-email" element={<ConfirmEmailPage />} />
            </Route>

            {/* Client Portal Routes */}
            <Route element={<ProtectedRoute requiredRole="Client" />}>
              <Route element={<DashboardLayout />}>
                <Route path="/client" element={<ClientDashboard />} />
                <Route path="/client/favorites" element={<MyFavoritesPage />} />
                <Route path="/client/offers" element={<MyOffersPage />} />
                <Route path="/client/appointments" element={<MyAppointmentsPage />} />
                <Route path="/client/saved-searches" element={<SavedSearchesPage />} />
                <Route path="/client/chats" element={<ClientChatPage />} />
                <Route path="/client/profile" element={<ClientProfilePage />} />
                <Route path="/client/activity" element={<ActivityPage />} />
                <Route path="/client/buy-ability" element={<BuyAbilityPage />} />
              </Route>
            </Route>

            {/* Agent Portal Routes */}
            <Route element={<ProtectedRoute requiredRole="Agent" />}>
              <Route element={<DashboardLayout />}>
                <Route path="/agent" element={<AgentDashboard />} />
                <Route path="/agent/properties" element={<MyPropertiesPage />} />
                <Route path="/agent/properties/create" element={<CreateEditPropertyPage />} />
                <Route path="/agent/properties/edit/:id" element={<CreateEditPropertyPage />} />
                <Route path="/agent/offers" element={<ReceivedOffersPage />} />
                <Route path="/agent/appointments" element={<AgentAppointmentsPage />} />
                <Route path="/agent/chats" element={<AgentChatPage />} />
                <Route path="/agent/profile" element={<AgentProfilePage />} />
                <Route path="/agent/verification" element={<AgentVerificationPage />} />
                <Route path="/agent/subscription" element={<AgentSubscriptionPage />} />
                <Route path="/agent/avm" element={<AvmValuationPage />} />
                <Route path="/agent/leads" element={<LeadPipelinePage />} />
                <Route path="/agent/commissions" element={<AgentCommissionsPage />} />
                <Route path="/agent/documents/:propertyId" element={<AgentDocumentsPage />} />
              </Route>
            </Route>

            {/* Owner Portal Routes */}
            <Route element={<ProtectedRoute requiredRole="Owner" />}>
              <Route element={<DashboardLayout />}>
                <Route path="/owner" element={<OwnerDashboard />} />
                <Route path="/owner/properties/create" element={<CreateOwnerPropertyPage />} />
                <Route path="/owner/properties/edit/:id" element={<CreateOwnerPropertyPage />} />
                <Route path="/owner/offers" element={<ReceivedOffersPage />} />
                <Route path="/owner/profile" element={<ClientProfilePage />} />
              </Route>
            </Route>

            {/* Developer Portal Routes */}
            <Route element={<ProtectedRoute requiredRole="Developer" />}>
              <Route element={<DashboardLayout />}>
                <Route path="/developer" element={<DeveloperDashboard />} />
                <Route path="/developer/property-types" element={<ManagePropertyTypesPage />} />
                <Route path="/developer/sale-types" element={<ManageSaleTypesPage />} />
                <Route path="/developer/improvements" element={<ManageImprovementsPage />} />
                <Route path="/developer/profile" element={<ClientProfilePage />} />
              </Route>
            </Route>

            {/* Admin Portal Routes */}
            <Route element={<ProtectedRoute requiredRole="Admin" />}>
              <Route element={<DashboardLayout />}>
                <Route path="/admin" element={<AdminDashboard />} />
                <Route path="/admin/agents" element={<ManageAgentsPage />} />
                <Route path="/admin/developers" element={<ManageDevelopersPage />} />
                <Route path="/admin/developers/create" element={<CreateEditDeveloperPage />} />
                <Route path="/admin/developers/edit/:id" element={<CreateEditDeveloperPage />} />
                <Route path="/admin/admins" element={<ManageAdminsPage />} />
                <Route path="/admin/admins/create" element={<CreateAdminPage />} />
                <Route path="/admin/all-properties" element={<ManageAllPropertiesPage />} />
                <Route path="/admin/verifications" element={<ManageVerificationsPage />} />
                <Route path="/admin/subscriptions" element={<ManageSubscriptionsPage />} />
                <Route path="/admin/reviews" element={<AdminReviewsPage />} />
                <Route path="/admin/commissions" element={<AdminCommissionsPage />} />
                <Route path="/admin/documents" element={<AdminDocumentsPage />} />
                <Route path="/admin/users" element={<ManageUsersPage />} />
                <Route path="/admin/property-types" element={<ManagePropertyTypesPage />} />
                <Route path="/admin/sale-types" element={<ManageSaleTypesPage />} />
                <Route path="/admin/improvements" element={<ManageImprovementsPage />} />
                <Route path="/admin/profile" element={<ClientProfilePage />} />
              </Route>
            </Route>

            {/* 404 Fallback */}
            <Route element={<PublicLayout />}>
              <Route path="*" element={<NotFoundPage />} />
            </Route>
          </Routes>
          </Suspense>
        </BrowserRouter>
            </CompareProvider>
          </NotificationProvider>
        </AuthProvider>
      </CurrencyProvider>
    </ThemeProvider>
  );
};
