---
name: gmms-codebase-knowledge
description: GMMS (Gym Membership Management System) codebase knowledge — solution structure, business logic, domain rules, entities, enums, API endpoints, UI architecture, and known gotchas. Use when working on ANY GMMS task: adding features, fixing bugs, extending the API, Blazor UI, MAUI mobile app, EF Core migrations, or answering questions about how the system works.
---

# GMMS Codebase Knowledge

Gym Membership Management System — .NET 8 Clean Architecture solution for managing gym members, membership plans, memberships, and payments.

## Solution Structure

```
GymMembershipManagementSystem.sln
├── GMMS.Api         ASP.NET Core 8 Web API — controllers, JWT auth, Serilog, health checks
├── GMMS.App         Blazor Server (Interactive Server) UI — MudBlazor 9.7.0, pure HTTP client of GMMS.Api
├── GMMS.Domain      ALL business logic — services, DTOs, FluentValidation validators, Result<T>
├── GMMS.Database    EF Core 9.0.17 — entities (Tbl_*), AppDbContext, 1 migration, seed users
└── GMMS.Mobile      .NET MAUI app (net8.0-android + net8.0-windows) — MVVM, thin REST client
```

**Dependency chain:** `GMMS.Domain → GMMS.Database`; `GMMS.Api → Domain + Database`; `GMMS.App → Domain` (DTOs only); `GMMS.Mobile → nothing` (duplicated DTOs, pure HTTP).

**Build/verify command:** `dotnet build GymMembershipManagementSystem.sln -c Debug --nologo -v minimal`
**EF migrations:** `dotnet dotnet-ef migrations add <Name> --project GMMS.Database --startup-project GMMS.Api` / `dotnet dotnet-ef database update --project GMMS.Database --startup-project GMMS.Api` (dotnet-ef 9.0.17 is a **local tool** — manifest at `.config/dotnet-tools.json`, run via `dotnet dotnet-ef`, not `dotnet ef`)

## Core Patterns

- **Result\<T>** (`GMMS.Domain/Result.cs`): `{ IsSuccess, IsError, Message, Data?, StatusCode (default 400) }`. Every service returns this (except DashBoardService). Services set 404 (not found) / 409 (conflict) explicitly; success paths often leave 400 default — do NOT rely on StatusCode of success results.
- **Soft delete**: `IsDeleted` flag on all BaseEntity tables. All reads filter `!IsDeleted` (exceptions: PaymentService queries, dashboard aggregates).
- **Audit fields**: `CreatedBy` (int, DB default 1), `CreatedAt` (DB default getdate), `UpdatedBy` (int?), `UpdatedAt` (DateTime?). Projections decorate `CreatedByUser`/`UpdatedByUser` as `"{userId} - {userName}"`.
- **Validators**: FluentValidation, constructor-injected, invoked MANUALLY inside each service (`ValidateAsync`). NOT auto-wired into MVC.
- **Services**: plain concrete classes registered `AddScoped` in GMMS.Api/Program.cs. No interfaces, no MediatR, no unit tests.

## Database Schema (GMMS.Database, namespace `GMMS.Database.AppDbContextModels`)

Tables (all `Tbl_` prefix): `Tbl_Member`, `Tbl_Membership`, `Tbl_MembershipPlan`, `Tbl_Payment`, `Tbl_PaymentMethod`, `Tbl_User`, `Tbl_UserSession`, `Tbl_AuditLog`.

| Entity | Key fields |
|---|---|
| `TblMember` | MemberId (PK), MemberCode (nvarchar50, UNIQUE), Name (nvarchar100) |
| `TblMembershipPlan` | MembershipPlanId, PlanCode, PlanName, Price (decimal18,2), DurationDays, Description?, IsActive (default true) |
| `TblMembership` | MembershipId, MemberId (FK), MembershipPlanId (FK), StartDate/EndDate (DateOnly→sql date), Status (nvarchar20) |
| `TblPayment` | PaymentId, MembershipId (FK), PaymentMethodId (FK), Amount (decimal18,2), Sspath? (**DB column is `SSPath`**), Status (nvarchar20) |
| `TblPaymentMethod` | PaymentMethodId, PaymentMethodCode, Name, IsActive |
| `TblUser` | UserId, UserName (nvarchar100, UNIQUE), PasswordHash (bcrypt, nvarchar256), Role ("Owner"/"Admin"), IsActive, MustChangePassword (default true) |
| `TblUserSession` | UserSessionId, SessionId (Guid), UserId (FK, **ON DELETE CASCADE**), RefreshTokenHash (nvarchar max), LoginTime, AccessTokenExpiresAt, RefreshTokenExpiresAt, RevokedAt?, IsExpired |
| `TblAuditLog` | AuditId (bigint), TableName, RecordId, Action, UserId (no FK), OldValue?, NewValue?, CreatedAt |

- FKs: Membership→Member/MembershipPlan, Payment→Membership/PaymentMethod all `ClientSetNull` (DB NO ACTION). Only UserSession→User cascades.
- One migration only: `20260728093937_InitialCreate`. Seeds `owner` + `admin` (bcrypt `$2a$11$...`, MustChangePassword=true, CreatedAt 2026-01-01).
- Connection string (GMMS.Api/appsettings.json): `Server=.;Database=GMMSDb;User Id=sa;Password=12345;TrustServerCertificate=True;`

## Enums (`GMMS.Domain/Enums/Enums.cs`)

```csharp
MembershipPlanStatus { none = 0, Pending = 1, Active = 2, Expired = 3 }   // NO "Cancelled"!
PaymentStatus       { none = 0, Pending = 1, Completed = 2, Failed = 3 }  // NO "Paid"!
```

**⚠ Enum/status mismatches (known bugs):**
- Membership created with status string `"Active"`; plans are never `Pending`/`Cancelled`.
- **FIXED (2026-08):** payments are now created with `Status = "Completed"` (both `PaymentService.Create` and the membership auto-payment), and the dashboard income queries filter `Status == "Completed"`. No `"Paid"` anywhere. Migration `20260809020218_FixPendingPaymentsToCompleted` updates legacy `"Pending"` rows (also drops stale `((1))` CreatedBy DB defaults).
- `AllMemberShipListRequestValidator` error message lists "Cancelled" but enum has none.

## Business Rules (GMMS.Domain services)

### Membership lifecycle (`Features/MemberShip/MemberShipService.cs`)
- **Create**: today = UtcNow date. StartDate = today, EndDate = today.AddDays(plan.DurationDays), Status = "Active". Creates membership + Pending payment (Amount = **plan.Price**, request's Amount is validated but IGNORED) atomically in a transaction.
- **Update**: only plan change (`MembershipId` + `MembershipPlanId`). EndDate recomputed = **original StartDate + new plan.DurationDays**. Status column NOT touched.
- **Status override at read time**: `Status = EndDate < today ? "Expired" : storedStatus`. "Expired" is computed, never persisted. No background job.
- **One active membership per member**: active = `!IsDeleted && EndDate >= today`. Create and Update both enforce it (409 "Member already has an active membership.").
- **Delete**: soft delete. Member delete blocked (409) if member has ANY non-deleted membership. Membership delete does NOT touch payments.

### Member (`Features/Member/MemberService.cs`)
- MemberCode normalized `Trim().ToUpperInvariant()`, regex `^[A-Z0-9-]+$`, unique (409 "Member already exists.", incl. DbUpdateException race handling).

### Plan (`Features/MemberShipPlan/MemberShipPlanService.cs`)
- PlanCode normalized uppercase, unique. Price > 0 ≤ 1,000,000; DurationDays > 0 ≤ 3650. ⚠ Update writes `request.PlanCode` UN-normalized (bug). Delete has no memberships-in-use guard.

### Payment (`Features/Payment/PaymentService.cs`)
- Create: payment recorded with `Status = "Completed"` immediately (payment collected at signup; enum-based). Membership must exist & not deleted, payment method must be active. **No duplicate-payment guard** (removed 2026-08) — installments per membership are allowed. Returns `Result<PaymentModel>` with `Data` populated. No approval workflow (payments are Completed at creation).

### PaymentMethod
- Code uppercase-unique (case-insensitive compare). Active methods only usable in membership/payment creation.

### User (`Features/User/UserService.cs`)
- Owner-only endpoints. UserName lowercased unique. New users always `MustChangePassword = true`. Roles restricted to "Owner"/"Admin" (string, no enum). ResetPassword sets MustChangePassword=true but does NOT revoke sessions.

### Auth (`Features/Auth/AuthService.cs` + `TokenService.cs`)
- Login: bcrypt verify, requires IsActive && !IsDeleted. Creates session Guid, JWT access token + 64-byte Base64 refresh token (stored as BCrypt hash).
- JWT claims: NameIdentifier (userId), UniqueName (username), Role, "SessionId" (Guid), "MustChangePassword", Jti, Iat. HS256, expiry 15 min (config), refresh 7 days.
- Refresh: rotation — old session revoked, new session created. Match by BCrypt hash equality.
- ChangePassword: verifies current, new must differ + complex regex `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$`, clears MustChangePassword, **revokes ALL user sessions**.
- Logout: revokes session by SessionId claim.

### Dashboard (`Features/DashBoard/DashBoardService.cs`)
- Returns `Result<DashboardResponseModel>` (wrapped, like all other services; controller uses `BaseController.Execute`). TotalMembers (excludes soft-deleted), TodayIncome + MonthlyRevenue (filter `Status == "Completed"`), MemberGrowths/RevenueTrends (continuous last-6-month series, zero-padded `"yyyy-MM"` labels, missing months filled with 0), MostUsedPlans (top 8, excludes deleted memberships/plans). Dashboard endpoint now requires `[Authorize]` (was public). **Unused by any UI** (no dashboard page exists yet).

## API Surface (GMMS.Api)

Base URL dev: `https://localhost:7069` (http profile `localhost:5161` — used by mobile). All responses are `Result<T>` envelope via `BaseController.Execute(result)`. Auth: JWT Bearer header OR `accessToken` cookie (HttpOnly, SameSite=Lax, ⚠ Secure=false).

| Controller | Route | Endpoints (auth) |
|---|---|---|
| AuthController | `api/Auth` | POST login (anon), POST refresh (anon), POST change-password [Authorize], POST logout [Authorize] |
| DashBoardController | `api/DashBoard` | GET (⚠ NO [Authorize] — public revenue data!) |
| MemberController | `api/Member` | GET list?pageNumber&pageSize&searchTerm, GET {id} [Authorize]; POST, PUT {id}, DELETE {id} [Authorize(Roles="Admin")] |
| MemberShipController | `api/MemberShip` | GET list?memberId, GET all (search/status/date filters), GET {id} [Authorize]; POST, PUT {id}, DELETE {id} [Admin] |
| MemberShipPlanConroller (misspelled file) | `api/MemberShipPlan` | GETs [Authorize]; POST/PUT/DELETE [Authorize(Roles="Owner")] |
| PaymentController | `api/Payment` | GET list?pageNumber&pageSize&searchTerm&fromDate&toDate, GET {id} [Authorize]; POST [Admin]. No PUT/DELETE. |
| PaymentMethodController | `api/PaymentMethod` | GETs [Authorize]; POST/PUT/DELETE [Owner] |
| UserController | `api/User` | ALL endpoints [Authorize(Roles="Owner")] — includes POST (create user, de-facto registration), POST reset-password |

Middleware pipeline: ExceptionMiddleware (SqlException→500, Timeout→408, other→500, PascalCase {IsSuccess,Message} body) → Serilog request logging → (Dev: Swagger) → (non-Dev: HTTPS redirect) → CORS "AllowAll" → AuthN → AuthZ → `/health` (custom JSON, DB CanConnectAsync check) → controllers.

Config (appsettings.json): JwtSettings {Key, Issuer=GMMS.Api, Audience=GMMS.App, ExpiryMinutes=15, RefreshTokenDays=7}, ConnectionStrings:DbConnection, Serilog (console + Logs/gmms-api-.log, 30 files retained).

**API gotchas:** no public registration (POST api/User is Owner-only); FluentValidation not auto-wired; plain-string 400s on some endpoints (e.g. MemberShip missing MemberId) vs envelope elsewhere; MemberShipPlan PUT error message wrongly says "Member ID"; ClockSkew=0.

## Blazor UI (GMMS.App)

- Routes: `/` Dashboard (home), `/login`, `/change-password` (AuthLayout); `/member-list`, `/membership-list?memberId=`, `/membership-list-all`, `/payment-list`, `/user-list`, `/settings` (MainLayout). Counter/Weather are template stubs (Home.razor deleted).
- **Auth is UI-only**: NO [Authorize], NO AuthenticationStateProvider, NO AuthorizeView, NO localStorage/cookies in the app. `AuthTokenStore` (scoped, in-memory) holds AccessToken + CurrentUser. `MainLayout.OnAfterRender` redirects to `/login` if not logged in (token lost on every hard reload).
- Login lands on `/` (dashboard). MustChangePassword=true → `/change-password`, then back to login.
- `Services/ApiService.cs` = typed facade; `HttpClientService` = low-level wrapper (Bearer header from AuthTokenStore, BaseAddress from `BackendApiUrl` = https://localhost:7069, 10s timeout). `ApiEndpoints.cs` = endpoint constants. `RefreshAsync` defined but never called; no 401 auto-handling.
- `Services/CalendarService.cs` + `Components/Layout/CalendarDropdown.razor`: app-bar calendar with month events (Payments green dots, membership Starts blue, Ends orange) → opens PaymentDetail/MembershipDetail dialogs.
- Roles (inverted model — be careful): **Owner** = read-only on Members/Memberships but full control of Plans, PaymentMethods, Users, sees audit fields; **Admin** = CRUD on Members/Memberships and **only Admin can create Payments** (Owner cannot!).
- Dialogs per feature: Create/Edit/Delete/Detail as `MudDialog` components, dialog closes with `DialogResult.Ok(true)` triggering list reload. All pickers are `MudSelect` (no raw HTML selects left; `PaymentCreate` was converted from `<select class="form-select">` during the Payment UI redesign).
- UI pattern: skeleton loading → error card w/ "Try Again" → table → empty state; debounced search (300 ms CancellationTokenSource); URL query sync (page/search/memberId via SupplyParameterFromQuery).
- Theme: `Theme/GMMSTheme.cs` — dark mode primary `#7C3AED` purple, secondary `#6366F1`; accent greens `#10B981`, blue `#60A5FA`, orange `#F59E0B`, red `#EF4444`, purple `#A78BFA`.
- **Dashboard UI implemented** (`Feature/DashBoard/Dashboard.razor` + `.razor.cs`, route `/`): 2 stat cards (Total Members violet `#6D28D9`, Today's Income amber `#F59E0B`, `"C2"` currency) + 3 ApexCharts (Member Growth line, Monthly Revenue line w/ `$` Y-axis formatter, Most Used Plans horizontal bar), zero-padded `yyyy-MM` backend labels re-formatted to `MMM yyyy` in code-behind; skeleton/error/empty pattern like MemberList; uses `ApiService.GetDashboardAsync<Result<DashboardResponseModel>>()` + `ApiEndpoints.Dashboard` (already existed). Charts use `<ApexPointSeries TItem="ChartPoint" XValue="e => e.X" YValue="e => e.Y">` with private `record ChartPoint(string X, decimal Y)`.
- **Blazor-ApexCharts 6.1.0 gotchas**: JS is auto-loaded (3.5+; do NOT add script tags); `@using ApexCharts` must be scoped to `Feature/DashBoard/_Imports.razor` — a global import collides with MudBlazor (`Color`/`Size` ambiguity → use `MudBlazor.Size.Large`-style qualification); `Stroke.Curve` uses enum `ApexCharts.Curve` (Smooth/Stepline/Straight/MonotoneCubic/Linestep); `Stroke.Width` is `ApexCharts.Size` (ValueOrList<double>, implicit from number); `ApexChartOptions.Yaxis` is `List<YAxis>`; options must not be shared across chart instances.
- **Settings page** (`Feature/Setting/Settings.razor`, route `/settings`, Owner-only, entry via account popover in NavMenu): profile card (avatar initial, UserName, role label, Change Password shortcut) + `MudTabs` with two tabs hosting `MembershipPlanManager` and `PaymentMethodManager`. The old standalone pages `/membershipplan-list` and `/paymentmethod-list` were REMOVED; their list logic lives in routeless components `Feature/MembershipPlan/MembershipPlanManager.razor` + `Feature/PaymentMethod/PaymentMethodManager.razor` (search/table/pagination + Create/Edit/Delete/Detail dialogs, Owner-gated actions) — dialogs (`MembershipPlanCreate/Edit/Delete/Detail`, `PaymentMethodCreate/Edit/Delete`) reused unchanged.
- **Payment UI** (`Feature/Payment/`): fully redesigned to the design system — page header + subtitle, amber stats card (Total Payments), search + two compact `MudDatePicker`s (From/To → `_fromDate`/`_toDate` in code-behind, wired to `GetPaymentListAsync` fromDate/toDate params), skeleton rows, error + Try Again, outlined table (no duplicate # / Payment ID columns), `gmm-pay__*` scoped CSS. `PaymentCreate`: `MudSelect` pickers + "Selected Membership" context card (member code chip, plan, dates, status chip) + skeleton/loadFailed states. `PaymentDetail`: Payment # id chip + status chip, receipt card (big green amount + created date), info card with icon label/value rows (Member, Membership, Payment Method, Created At, Created By Owner-only) + skeleton/loadFailed states.
- **Not implemented**: "Remember me" checkbox cosmetic.

## MAUI Mobile (GMMS.Mobile)

- MVVM + CommunityToolkit.Mvvm 8.3.2. Targets: net8.0-android (min API 21), net8.0-windows (unpackaged WinUI). No project refs — pure REST client.
- Pages: `login` (LoginPage), `member-list` (MemberListPage — post-login landing), `member-detail?Id=` (MemberDetailPage). No tab bar/flyout; programmatic `GoToAsync`.
- Services: `ApiServiceBase` (ApiEnvelope<T> {IsSuccess,Message,Data} + ApiException), `AuthApiService`, `MemberApiService`. Base URL: Android → `http://10.0.2.2:5161`, else `http://localhost:5161` (plain HTTP, cleartext allowed in Android manifest).
- `Handlers/AuthMessageHandler.cs`: attaches Bearer token, single-flight 401→refresh-token rotation → retry original request.
- `Storage/TokenStorage.cs`: SecureStorage keys `gmm_access_token`, `gmm_refresh_token`, `gmm_user`. `HasSessionAsync` exists but unused — no session restore, always starts at login.
- ⚠ **Known bug**: `LoginResponseData` expects `user`/`accessToken`/`refreshToken` flat at data top level, but API returns `{ user, tokens: { accessToken, refreshToken } }` — login stores EMPTY tokens → subsequent authed calls fail.
- One `MemberViewModel` serves both list + detail pages (separate transient instances).

## Conventions Checklist (follow when editing)

1. New business logic → GMMS.Domain `Features/<Feature>/<X>Service.cs` + `Models/` DTOs + validators; register scoped in GMMS.Api/Program.cs.
2. Entity changes → GMMS.Database entities + new migration (never edit the existing migration).
3. New API endpoint → controller in GMMS.Api, wrap in `BaseController.Execute(result)`.
4. Use `Result<T>` for all service returns; set StatusCode 404/409 explicitly; audit fields on writes; `!IsDeleted` filters on reads; UtcNow timestamps.
5. Codes (Member/Plan/PaymentMethod): uppercase + regex `^[A-Z0-9-]+$` + uniqueness checks.
6. UI → Blazor components follow skeleton/error/empty pattern, debounced search, dialog-per-CRUD with `DialogResult.Ok(true)`.
7. Names are quirky: `MemberShip` (service/namespace) vs `Membership` (entity), `Sspath`, `DashBoard`. Match existing naming in the file you touch.
8. After changes: `dotnet build GymMembershipManagementSystem.sln -c Debug --nologo -v minimal` — 0 errors.

## Known Gotchas / Fix Candidates

1. ~~Dashboard `"Paid"` vs enum `Completed` mismatch~~ **FIXED** — payments are Completed at creation; dashboard counts Completed.
2. Mobile `LoginResponseData` shape mismatch → broken mobile login.
3. ~~`DashBoardController` missing [Authorize]~~ **FIXED** — now `[Authorize]` + Result<T> envelope.
4. ~~No payment approval workflow → payments stuck "Pending"~~ **FIXED** — payments recorded as Completed at creation; pending-duplicate guard removed (installments allowed).
5. `MemberShipPlanService.Update` stores unnormalized PlanCode.
6. Cookie `Secure=false`; CORS AllowAnyOrigin + Lax cookies incompatible for cross-site cookie auth.
7. `README.md` has unmerged merge-conflict markers (`<<<<<<< HEAD` / `>>>>>>> 6b34cb3...`).
8. Soft-delete leaks: `PaymentService` queries don't filter IsDeleted (dashboard queries do).
9. AuthCookie `ClearAuthCoookies` typo; `"Referch token is missing"` typo; `MemberShipPlanConroller.cs` filename typo.
10. `TblAuditLog` never written by any service (dead table for now).
11. Migrations: run with `dotnet dotnet-ef` (local tool), not `dotnet ef`.
