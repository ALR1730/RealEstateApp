import React from 'react';
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

// Public Pages
import { HomePage } from './pages/public/HomePage';
import { PropertiesCatalogPage } from './pages/public/PropertiesCatalogPage';
import { PropertyDetailPage } from './pages/public/PropertyDetailPage';
import { AgentsPage } from './pages/public/AgentsPage';
import { AgentDetailPage } from './pages/public/AgentDetailPage';
import { MortgagePage } from './pages/public/MortgagePage';
import { MapPage } from './pages/public/MapPage';
import { NotFoundPage } from './pages/public/NotFoundPage';
import { ComparePage } from './pages/public/ComparePage';

// Auth Pages
import { LoginPage } from './pages/auth/LoginPage';
import { RegisterClientPage } from './pages/auth/RegisterClientPage';
import { RegisterAgentPage } from './pages/auth/RegisterAgentPage';
import { ForgotPasswordPage } from './pages/auth/ForgotPasswordPage';
import { ResetPasswordPage } from './pages/auth/ResetPasswordPage';
import { PendingActivationPage } from './pages/auth/PendingActivationPage';
import { ConfirmEmailPage } from './pages/auth/ConfirmEmailPage';

// Client Pages
import { ClientDashboard } from './pages/client/ClientDashboard';
import { MyFavoritesPage } from './pages/client/MyFavoritesPage';
import { MyOffersPage } from './pages/client/MyOffersPage';
import { MyAppointmentsPage } from './pages/client/MyAppointmentsPage';
import { SavedSearchesPage } from './pages/client/SavedSearchesPage';
import { ClientChatPage } from './pages/client/ClientChatPage';
import { ClientProfilePage } from './pages/client/ClientProfilePage';
import { ActivityPage } from './pages/client/ActivityPage';

// Agent Pages
import { AgentDashboard } from './pages/agent/AgentDashboard';
import { MyPropertiesPage } from './pages/agent/MyPropertiesPage';
import { CreateEditPropertyPage } from './pages/agent/CreateEditPropertyPage';
import { ReceivedOffersPage } from './pages/agent/ReceivedOffersPage';
import { AgentAppointmentsPage } from './pages/agent/AgentAppointmentsPage';
import { AgentChatPage } from './pages/agent/AgentChatPage';
import { AgentProfilePage } from './pages/agent/AgentProfilePage';
import { AgentVerificationPage } from './pages/agent/AgentVerificationPage';
import { AgentSubscriptionPage } from './pages/agent/AgentSubscriptionPage';

// Owner Pages
import { OwnerDashboard } from './pages/owner/OwnerDashboard';
import { CreateOwnerPropertyPage } from './pages/owner/CreateOwnerPropertyPage';

// Admin Pages
import { AdminDashboard } from './pages/admin/AdminDashboard';
import { ManageAgentsPage } from './pages/admin/ManageAgentsPage';
import { ManageDevelopersPage } from './pages/admin/ManageDevelopersPage';
import { CreateEditDeveloperPage } from './pages/admin/CreateEditDeveloperPage';
import { ManageAdminsPage } from './pages/admin/ManageAdminsPage';
import { CreateAdminPage } from './pages/admin/CreateAdminPage';
import { ManageUsersPage } from './pages/admin/ManageUsersPage';
import { ManagePropertyTypesPage } from './pages/admin/ManagePropertyTypesPage';
import { ManageSaleTypesPage } from './pages/admin/ManageSaleTypesPage';
import { ManageImprovementsPage } from './pages/admin/ManageImprovementsPage';
import { ManageVerificationsPage } from './pages/admin/ManageVerificationsPage';
import { ManageSubscriptionsPage } from './pages/admin/ManageSubscriptionsPage';
import { ManageAllPropertiesPage } from './pages/admin/ManageAllPropertiesPage';

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
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ requiredRole }) => {
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
          <Routes>
            {/* Public Routes */}
            <Route element={<PublicLayout />}>
              <Route path="/" element={<HomePage />} />
              <Route path="/catalog" element={<PropertiesCatalogPage />} />
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
              </Route>
            </Route>

            {/* Owner Portal Routes */}
            <Route element={<ProtectedRoute requiredRole="Owner" />}>
              <Route element={<DashboardLayout />}>
                <Route path="/owner" element={<OwnerDashboard />} />
                <Route path="/owner/properties/create" element={<CreateOwnerPropertyPage />} />
                <Route path="/owner/profile" element={<ClientProfilePage />} />
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
        </BrowserRouter>
            </CompareProvider>
          </NotificationProvider>
        </AuthProvider>
      </CurrencyProvider>
    </ThemeProvider>
  );
};
