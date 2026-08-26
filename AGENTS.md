# AGENTS.md — RealEstateApp

## Architecture

Onion architecture with two independent presentation layers:

- **`src/Presentation/RealEstateApp.ClientApp`** — React 18 SPA (Vite + TypeScript + Tailwind). This is the active frontend.
- **`src/Presentation/RealEstateApp.Presentation.WebApi`** — .NET 10 ASP.NET Core REST API. The backend the SPA talks to.
- **`src/Presentation/RealEstateApp.Presentation.WebApp`** — Legacy ASP.NET MVC Razor app. Reference only, do not modify.
- **`src/Core/RealEstateApp.Core.Application`** — Business logic, DTOs, services.
- **`src/Core/RealEstateApp.Core.Domain`** — Domain entities.
- **`src/Infrastructure/RealEstateApp.Infrastructure.Persistence`** — EF Core + SQL Server.
- **`src/Infrastructure/RealEstateApp.Infrastructure.Shared`** — Shared infra services (culture, file storage).
- **`tests/RealEstateApp.UnitTests`** — xUnit + Moq + FluentAssertions.

**Important**: The solution file (`RealEstateApp.slnx`) does NOT include `ClientApp`. The React app is a standalone Node.js project.

## Commands

### Frontend (ClientApp)

All commands run from `src/Presentation/RealEstateApp.ClientApp/`:

```bash
npm run dev          # Vite dev server on port 5173
npm run build        # tsc + vite build
npm run test         # vitest run (single pass)
npm run test:watch   # vitest (watch mode)
npm run lint         # eslint src/
npm run lint:fix     # eslint --fix
npm run format       # prettier --write
```

Verification order: `npm run lint` → `npm run build` → `npm run test`

### Backend (.NET)

From repo root:

```bash
dotnet build RealEstateApp.slnx
dotnet test RealEstateApp.slnx
```

## Environment

### Frontend

- `.env` at `ClientApp/.env` sets `VITE_API_URL` and `VITE_HUB_URL`
- Default API base: `http://localhost:5196/api/v1`
- Vite proxy forwards `/api`, `/hubs` (WebSocket), `/images` to `localhost:5196`

### Backend

- User Secrets for connection strings and Google OAuth (see `.csproj` `UserSecretsId` fields)
- WebApp runs on HTTP :5080 / HTTPS :7103
- WebApi runs on port 5196 (configured in `launchSettings.json`)

## Frontend Conventions

- **State management**: React Context API only (AuthContext, NotificationContext, CurrencyContext, ThemeContext, CompareContext). No Redux/Zustand.
- **Styling**: Tailwind CSS 3.4 utility classes. No component library (all custom components). `darkMode: 'class'` enabled.
- **Icons**: `lucide-react` exclusively.
- **HTTP**: Axios with JWT Bearer interceptor. All API calls go through `src/api/services.ts`.
- **Real-time**: SignalR (`@microsoft/signalr`) via `NotificationContext`.
- **Routing**: `react-router-dom` v6 with role-based `ProtectedRoute` guards.
- **Roles**: `Admin`, `Agent`, `Client`, `Developer`, `Owner` — checked via `useAuth().hasRole()`.
- **Currency**: All prices stored as DOP. `useCurrency()` converts for display.
- **Language**: UI is entirely in Spanish (es-DO locale, RD$ currency).

### File Organization

```
src/
  api/           — axiosClient.ts + services.ts (all API calls)
  components/    — Reusable UI (common/, properties/, offers/, appointments/, chat/, simulator/)
  context/       — React Context providers
  pages/         — Route pages (auth/, public/, client/, agent/, owner/, admin/, developer/)
  tests/         — Vitest test files + setup.ts
  types/         — TypeScript interfaces (single index.ts)
  utils/         — formatters.ts
```

### Adding New Routes

1. Create the page in `src/pages/{role}/`
2. Import it in `src/App.tsx`
3. Add `<Route>` under the appropriate layout (PublicLayout or DashboardLayout inside ProtectedRoute)
4. Add to the Sidebar menu in `src/components/common/Sidebar.tsx` if it's a dashboard page

### Testing (Frontend)

- Framework: Vitest with jsdom environment
- Setup file: `src/tests/setup.ts` — includes `@testing-library/jest-dom` matchers and `window.matchMedia` mock
- Libraries: `@testing-library/react`, `@testing-library/dom`, `@testing-library/jest-dom`
- Tests live in `src/tests/*.test.ts(x)` or colocated with components
- Context tests use `renderHook` from `@testing-library/react`
- Run a single test file: `npx vitest run src/tests/CurrencyContext.test.tsx`

## Backend Conventions

- **Test naming**: `[Method]_[Expected]_[Condition]` (Hungarian-style, Spanish)
- **Test pattern**: AAA (Arrange-Act-Assert) with FluentAssertions
- **Test stack**: xUnit + Moq + FluentAssertions
- **Test location**: `tests/RealEstateApp.UnitTests/Services/`, `Controllers/`, `Domain/`
- **Nullability**: Enabled project-wide
- **Target**: .NET 10.0

## Key Business Rules

- Offer acceptance is atomic: accepting one offer cascade-rejects all others for that property
- Owners limited to 2 active properties max
- Property codes are 6-character unique identifiers
- Agent KYC verification requires admin approval
- Subscription plans limit featured property count

## Testing Standards

See `.agents/rules/testing_standards.md` — mandatory unit tests for every new feature/fix. `dotnet test` must pass 100% before any change is considered complete.
