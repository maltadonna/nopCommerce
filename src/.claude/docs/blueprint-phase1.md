# MISSION BLUEPRINT: WirelessDivision Modular nopCommerce Plugin Architecture

## Executive Summary

**Objective**: Design and implement a unified file structure for `WZ.Plugin.Misc.WirelessDivision` that maintains 100% nopCommerce plugin compliance while supporting a scalable modular architecture for multi-feature plugin development.

**Business Context**: WirelessDivision requires a sophisticated nopCommerce plugin that houses multiple independent feature modules (Authentication, SavedCart, SKUAllocation, OrderSubmit, etc.) while maintaining clean separation of concerns, testability, and compliance with nopCommerce extension standards.

**Technical Context**:
- nopCommerce Version: 4.90 (beta)
- .NET Version: 9.0
- Architecture Pattern: Modular Monolith with feature-based organization
- Authentication: OAuth2 Azure EntraId integration (authentication-first design)

**Investigation Summary**: Analysis of nopCommerce plugin structure requirements reveals mandatory components (plugin.json, IPlugin implementation, DependencyRegistrar) that must exist at plugin root. The WirelessDivision modular architecture requires feature isolation with shared infrastructure. The challenge is harmonizing nopCommerce's flat plugin structure expectations with a deep modular hierarchy while maintaining discoverability and compliance.

---

## Technical Requirements & Standards

### Environment

- **nopCommerce Version**: 4.90
- **.NET Version**: .NET 9.0
- **Target Framework**: net9.0
- **Required NuGet Packages**:
  - Nop.Core (4.90.0)
  - Nop.Services (4.90.0)
  - Nop.Web.Framework (4.90.0)
  - Autofac (10.0.0)
  - Microsoft.Identity.Web (3.4.0) - for Azure EntraId
  - Microsoft.AspNetCore.Authentication.OpenIdConnect (9.0.0)
- **Database**: SQL Server / MySQL / PostgreSQL (EF Core abstraction)

### nopCommerce Compliance Requirements (NON-NEGOTIABLE)

- [x] Plugin naming: `WZ.Plugin.Misc.WirelessDivision`
- [x] Valid plugin.json at plugin root with correct metadata
- [x] `WirelessDivisionPlugin.cs` implementing IPlugin at plugin root
- [x] `DependencyRegistrar.cs` at plugin root for service registration
- [x] Proper namespace structure: `WZ.Plugin.Misc.WirelessDivision.*`
- [x] No modifications to core nopCommerce files
- [x] Localization resources in `Localization/` folder
- [x] Views organized under `Views/` with Admin/Public separation

### Coding Standards (MANDATORY)

- [x] XML documentation comments on all public members
- [x] Language keywords over type names (`string` not `String`)
- [x] Async/await properly implemented for all I/O operations
- [x] Zero compiler warnings
- [x] Proper error handling and logging (ILogger)
- [x] Input validation on all user inputs
- [x] Consistent namespace hierarchy matching folder structure

### Security Requirements

- [x] OAuth2 Azure EntraId authentication implementation
- [x] SQL injection prevention (EF Core proper usage)
- [x] XSS protection in all views
- [x] Secure credential storage (Azure Key Vault integration or ISettingService)
- [x] Authorization checks per module
- [x] Data validation at all entry points

### Performance Requirements

- [x] Caching strategy using IStaticCacheManager
- [x] Efficient database queries (no N+1 problems)
- [x] Async operations for I/O throughout
- [x] Module lazy-loading where appropriate

---

## Mission Analysis

### Key Challenges

1. **nopCommerce Root File Requirements vs Modular Depth**
   - **Impact**: nopCommerce expects certain files at plugin root; modular architecture needs deep hierarchy
   - **Approach**: Place mandatory nopCommerce files at root; use namespace mapping and partial classes to delegate to modular components

2. **Service Registration Across Multiple Modules**
   - **Impact**: DependencyRegistrar must register services from 8+ independent modules plus Core/Shared
   - **Approach**: Create module-specific registrar interfaces; master DependencyRegistrar orchestrates all module registrations

3. **Unified Database Context for Multi-Module Data Access**
   - **Impact**: Multiple modules need entity management without conflicting migrations
   - **Approach**: Single `WirelessDivisionObjectContext` with organized entity configurations; modules access via repositories

4. **Authentication-First Design with Module Isolation**
   - **Impact**: Core authentication must initialize before any module can execute
   - **Approach**: Middleware pipeline ordering in PluginStartup; Core.Authentication loaded first with dependency injection

### Architectural Decisions

**Service Strategy**:
- **Core Services**: `IWorkContext`, `IStoreContext`, `ISettingService`, `ILogger`, `IStaticCacheManager`
- **Authentication Services**: Custom `IWirelessDivisionAuthenticationService`, `ITokenService`, `IAzureEntraIdProvider`
- **Module Services**: Each module exposes its own service interface (e.g., `ISavedCartService`, `ISKUAllocationService`)
- **Shared Services**: Cross-module utilities via `Shared.Services` namespace

**Data Access**:
- Single `WirelessDivisionObjectContext` inheriting from `DbContext`
- Entity configurations using `IEntityTypeConfiguration<TEntity>` in `Database/Mapping/`
- Repository pattern via `IRepository<TEntity>` from Nop.Data
- Migrations in `Database/Migrations/` namespace

**Caching**:
- Static cache via `IStaticCacheManager`
- Cache key preparation: `WirelessDivisionDefaults.{Module}CacheKey`
- Per-module cache invalidation
- Distributed cache support for multi-server deployments

**Integration Points**:
- **Events**: Subscribe to nopCommerce domain events (OrderPlacedEvent, CustomerLoggedInEvent)
- **Widgets**: Custom widget zones for UI injection
- **Custom Routes**: Module-specific routes via `RouteProvider`
- **Admin Menu**: Extensions to admin navigation via `IAdminMenuPlugin`

**Security Model**:
- OAuth2 OpenIdConnect with Azure EntraId
- JWT token validation and refresh
- Permission-based authorization per module (`WirelessDivision.{Module}.Access`)
- Claims-based identity mapping to nopCommerce customers

### Success Criteria (Definition of Done)

- [x] Plugin installs successfully via nopCommerce admin panel
- [x] Azure EntraId authentication flow completes end-to-end
- [x] Zero compiler warnings or errors
- [x] Database migrations execute cleanly
- [x] Admin configuration UI accessible and functional
- [x] Code quality standards met (XML docs, async/await, naming conventions)
- [x] Technical documentation complete

### Risks & Mitigation

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| DependencyRegistrar service collision across modules | Medium | High | Namespace isolation + registration order control + testing |
| nopCommerce plugin detection failure due to non-standard structure | Low | Critical | Strict adherence to root-level required files (plugin.json, IPlugin) |
| EF Core migration conflicts between modules | Medium | High | Single unified ObjectContext + entity configuration isolation |
| Authentication initialization race condition | Medium | Critical | Middleware ordering enforcement + startup dependency validation |
| Performance degradation from deep folder nesting | Low | Medium | Build-time namespace flattening + caching strategy |

---

## Unified Directory Structure

```
WZ.Plugin.Misc.WirelessDivision/
│
├── plugin.json                                    # REQUIRED: Plugin descriptor
├── WZ.Plugin.Misc.WirelessDivision.csproj        # REQUIRED: Project file
├── WirelessDivisionPlugin.cs                      # REQUIRED: IPlugin implementation (orchestrates modules)
├── DependencyRegistrar.cs                         # REQUIRED: Master service registrar (calls module registrars)
│
├── Infrastructure/                                # nopCommerce infrastructure extensions
│   ├── RouteProvider.cs                          # Custom routes for all modules
│   ├── PluginStartup.cs                          # Startup configuration (auth middleware, etc.)
│   ├── DependencyConfig.cs                       # Autofac container configuration
│   └── ViewLocationExpander.cs                   # Custom view path resolution for modules
│
├── Core/                                          # Shared plugin infrastructure (authentication-first)
│   ├── Configuration/
│   │   ├── WirelessDivisionSettings.cs           # Plugin-wide settings model
│   │   ├── ModuleSettings.cs                     # Base class for module settings
│   │   └── Constants/
│   │       ├── WirelessDivisionDefaults.cs       # Cache keys, constants
│   │       └── PermissionProvider.cs             # Permission definitions
│   │
│   ├── Authentication/                           # CORE: Authentication system (loads first)
│   │   ├── Services/
│   │   │   ├── IWirelessDivisionAuthenticationService.cs
│   │   │   ├── WirelessDivisionAuthenticationService.cs
│   │   │   ├── IAzureEntraIdProvider.cs
│   │   │   ├── AzureEntraIdProvider.cs
│   │   │   ├── ITokenService.cs
│   │   │   └── TokenService.cs
│   │   ├── Models/
│   │   │   ├── AuthenticationResult.cs
│   │   │   ├── TokenResponse.cs
│   │   │   └── UserClaims.cs
│   │   ├── Handlers/
│   │   │   ├── WirelessDivisionAuthenticationHandler.cs
│   │   │   └── TokenRefreshHandler.cs
│   │   └── DependencyRegistrar.cs                # Auth-specific service registration
│   │
│   ├── Api/                                       # API client infrastructure
│   │   ├── IWirelessDivisionApiClient.cs
│   │   ├── WirelessDivisionApiClient.cs
│   │   ├── ApiConfiguration.cs
│   │   └── ApiResponseModels/
│   │
│   ├── Logging/                                   # Enhanced logging
│   │   ├── IWirelessDivisionLogger.cs
│   │   ├── WirelessDivisionLogger.cs
│   │   └── LoggingExtensions.cs
│   │
│   └── Services/                                  # Core cross-cutting services
│       ├── IModuleManager.cs                     # Module lifecycle management
│       ├── ModuleManager.cs
│       ├── ICacheKeyProvider.cs
│       └── CacheKeyProvider.cs
│
├── Modules/                                       # Feature modules (independent units)
│   ├── Authentication/                           # Module: External auth UI/controllers
│   │   ├── Controllers/
│   │   │   ├── WirelessDivisionAuthenticationController.cs  # Public controller
│   │   │   └── Admin/
│   │   │       └── AuthenticationAdminController.cs
│   │   ├── Models/
│   │   │   ├── LoginModel.cs
│   │   │   └── ConfigurationModel.cs
│   │   ├── Validators/
│   │   │   └── ConfigurationModelValidator.cs
│   │   └── DependencyRegistrar.cs                # Module-specific registration
│   │
│   ├── BuildInfo/                                # Module: Build information display
│   │   ├── Controllers/
│   │   │   └── BuildInfoController.cs
│   │   ├── Models/
│   │   │   └── BuildInfoModel.cs
│   │   ├── Services/
│   │   │   ├── IBuildInfoService.cs
│   │   │   └── BuildInfoService.cs
│   │   └── DependencyRegistrar.cs
│   │
│   ├── SavedCart/                                # Module: Saved cart functionality
│   │   ├── Controllers/
│   │   │   ├── SavedCartController.cs
│   │   │   └── Admin/
│   │   │       └── SavedCartAdminController.cs
│   │   ├── Models/
│   │   │   ├── SavedCartModel.cs
│   │   │   └── SavedCartItemModel.cs
│   │   ├── Services/
│   │   │   ├── ISavedCartService.cs
│   │   │   └── SavedCartService.cs
│   │   ├── Validators/
│   │   │   └── SavedCartModelValidator.cs
│   │   └── DependencyRegistrar.cs
│   │
│   ├── SKUAllocation/                            # Module: SKU allocation logic
│   │   ├── Controllers/
│   │   │   └── SKUAllocationController.cs
│   │   ├── Models/
│   │   │   └── SKUAllocationModel.cs
│   │   ├── Services/
│   │   │   ├── ISKUAllocationService.cs
│   │   │   └── SKUAllocationService.cs
│   │   └── DependencyRegistrar.cs
│   │
│   ├── OrderSubmit/                              # Module: Order submission workflow
│   │   ├── Controllers/
│   │   │   └── OrderSubmitController.cs
│   │   ├── Models/
│   │   │   └── OrderSubmitModel.cs
│   │   ├── Services/
│   │   │   ├── IOrderSubmitService.cs
│   │   │   └── OrderSubmitService.cs
│   │   └── DependencyRegistrar.cs
│   │
│   ├── OnePageCheckout/                          # Module: Custom checkout flow
│   │   ├── Controllers/
│   │   │   └── OnePageCheckoutController.cs
│   │   ├── Models/
│   │   │   └── CheckoutModel.cs
│   │   ├── Services/
│   │   │   ├── IOnePageCheckoutService.cs
│   │   │   └── OnePageCheckoutService.cs
│   │   └── DependencyRegistrar.cs
│   │
│   ├── StoreRma/                                 # Module: RMA processing
│   │   ├── Controllers/
│   │   │   ├── StoreRmaController.cs
│   │   │   └── Admin/
│   │   │       └── StoreRmaAdminController.cs
│   │   ├── Models/
│   │   │   └── RmaModel.cs
│   │   ├── Services/
│   │   │   ├── IStoreRmaService.cs
│   │   │   └── StoreRmaService.cs
│   │   └── DependencyRegistrar.cs
│   │
│   └── MassOrder/                                # Module: Bulk order operations
│       ├── Controllers/
│       │   └── MassOrderController.cs
│       ├── Models/
│       │   └── MassOrderModel.cs
│       ├── Services/
│       │   ├── IMassOrderService.cs
│       │   └── MassOrderService.cs
│       └── DependencyRegistrar.cs
│
├── Shared/                                        # Cross-module shared components
│   ├── Models/                                   # Shared view models and DTOs
│   │   ├── BaseModuleModel.cs
│   │   ├── ApiResultModel.cs
│   │   └── PaginationModel.cs
│   │
│   ├── Services/                                 # Shared utility services
│   │   ├── ISessionService.cs
│   │   ├── SessionService.cs
│   │   ├── INotificationService.cs
│   │   └── NotificationService.cs
│   │
│   ├── Extensions/                               # Extension methods
│   │   ├── StringExtensions.cs
│   │   ├── ModelExtensions.cs
│   │   └── ServiceCollectionExtensions.cs
│   │
│   ├── Attributes/                               # Custom attributes
│   │   ├── WirelessDivisionAuthorizeAttribute.cs
│   │   └── ModuleEnabledAttribute.cs
│   │
│   └── Helpers/                                  # Utility classes
│       ├── CryptoHelper.cs
│       └── JsonHelper.cs
│
├── Database/                                      # Unified data access layer
│   ├── WirelessDivisionObjectContext.cs          # Main EF Core DbContext
│   │
│   ├── Entities/                                 # Domain entities (organized by module)
│   │   ├── SavedCart/
│   │   │   ├── SavedCartEntity.cs
│   │   │   └── SavedCartItemEntity.cs
│   │   ├── SKUAllocation/
│   │   │   └── SKUAllocationEntity.cs
│   │   ├── StoreRma/
│   │   │   └── RmaRequestEntity.cs
│   │   ├── Authentication/
│   │   │   ├── WirelessDivisionUserEntity.cs
│   │   │   └── RefreshTokenEntity.cs
│   │   └── Audit/
│   │       └── ModuleActivityLogEntity.cs
│   │
│   ├── Mapping/                                  # EF Core entity configurations
│   │   ├── SavedCart/
│   │   │   ├── SavedCartEntityMap.cs
│   │   │   └── SavedCartItemEntityMap.cs
│   │   ├── SKUAllocation/
│   │   │   └── SKUAllocationEntityMap.cs
│   │   ├── StoreRma/
│   │   │   └── RmaRequestEntityMap.cs
│   │   ├── Authentication/
│   │   │   ├── WirelessDivisionUserEntityMap.cs
│   │   │   └── RefreshTokenEntityMap.cs
│   │   └── Audit/
│   │       └── ModuleActivityLogEntityMap.cs
│   │
│   └── Migrations/                               # EF Core migrations
│       └── (Generated migration files)
│
├── Views/                                         # Razor views (nopCommerce standard)
│   ├── Admin/                                    # Admin panel views
│   │   ├── Configure.cshtml                      # Main plugin configuration
│   │   ├── Authentication/
│   │   │   └── Configure.cshtml
│   │   ├── SavedCart/
│   │   │   ├── List.cshtml
│   │   │   └── Edit.cshtml
│   │   └── StoreRma/
│   │       ├── List.cshtml
│   │       └── Details.cshtml
│   │
│   ├── Public/                                   # Public-facing views
│   │   ├── Login.cshtml
│   │   ├── SavedCart/
│   │   │   └── Index.cshtml
│   │   ├── OnePageCheckout/
│   │   │   └── Index.cshtml
│   │   └── MassOrder/
│   │       └── Index.cshtml
│   │
│   └── Shared/                                   # Shared partial views
│       ├── _WirelessDivisionLayout.cshtml
│       ├── _ModuleNavigation.cshtml
│       └── Components/
│           └── WirelessDivisionWidget/
│               └── Default.cshtml
│
├── Content/                                       # Static assets
│   ├── css/
│   │   ├── wirelessdivision.admin.css
│   │   └── wirelessdivision.public.css
│   ├── js/
│   │   ├── wirelessdivision.common.js
│   │   ├── modules/
│   │   │   ├── savedcart.js
│   │   │   ├── massorder.js
│   │   │   └── onepagecheckout.js
│   │   └── admin/
│   │       └── wirelessdivision.admin.js
│   └── images/
│       └── logo.png
│
├── Localization/                                  # Resource files
│   ├── en-US.xml                                 # English resources
│   └── es-ES.xml                                 # Spanish resources (example)
│
└── README.md                                      # Plugin documentation
```

---

## File Placement & Integration Guide

### Root-Level Required Files

| File | Purpose | Key Content |
|------|---------|-------------|
| `plugin.json` | Plugin descriptor for nopCommerce | `{ "Group": "Misc", "SystemName": "WZ.Plugin.Misc.WirelessDivision", "Version": "1.0.0", "SupportedVersions": ["4.90"], "FileName": "WZ.Plugin.Misc.WirelessDivision.dll" }` |
| `WirelessDivisionPlugin.cs` | IPlugin implementation | Implements `Install()`, `Uninstall()`, `GetConfigurationPageUrl()`, orchestrates module initialization |
| `DependencyRegistrar.cs` | Master service registration | Calls all module-specific registrars in correct order (Core.Authentication first, then modules) |
| `WZ.Plugin.Misc.WirelessDivision.csproj` | Project file | References Nop.Core, Nop.Services, Nop.Web.Framework, Microsoft.Identity.Web |

### Infrastructure Integration Points

| Component | Location | Integrates With |
|-----------|----------|-----------------|
| `RouteProvider.cs` | `Infrastructure/RouteProvider.cs` | Registers module-specific routes (e.g., `/WirelessDivision/SavedCart`) |
| `PluginStartup.cs` | `Infrastructure/PluginStartup.cs` | Configures authentication middleware, adds OAuth2, sets up authorization policies |
| `ViewLocationExpander.cs` | `Infrastructure/ViewLocationExpander.cs` | Enables module-based view resolution (`Views/Public/{Module}/`) |
| `DependencyConfig.cs` | `Infrastructure/DependencyConfig.cs` | Autofac container builder configuration (if advanced DI needed) |

### Core Architecture Mapping

| Modular Component | Maps to nopCommerce Standard | Notes |
|-------------------|------------------------------|-------|
| `Core/Configuration/` | `Infrastructure/` or root-level settings | Plugin-wide settings accessible via ISettingService |
| `Core/Authentication/` | Custom (not standard nopCommerce) | Core infrastructure loaded first via DependencyRegistrar ordering |
| `Core/Api/` | `Services/` | HTTP client for external WirelessDivision API |
| `Core/Logging/` | Extends nopCommerce `ILogger` | Wrapper for structured logging |

### Module to nopCommerce Mapping

| Module Component | nopCommerce Equivalent | Implementation |
|------------------|------------------------|----------------|
| `Modules/{Module}/Controllers/` | `Controllers/` | Standard MVC controllers with `[Area(AreaNames.Admin)]` for admin |
| `Modules/{Module}/Models/` | `Models/` | View models and DTOs |
| `Modules/{Module}/Services/` | `Services/` | Business logic services registered in module's DependencyRegistrar |
| `Modules/{Module}/Validators/` | `Validators/` | FluentValidation validators |

### Database Layer Integration

| Database Component | nopCommerce Pattern | Implementation |
|--------------------|---------------------|----------------|
| `Database/WirelessDivisionObjectContext.cs` | Custom DbContext | Inherits from `DbContext`, registered in DependencyRegistrar, uses connection string from nopCommerce settings |
| `Database/Entities/` | `Domain/` | Entity classes inheriting from `BaseEntity` |
| `Database/Mapping/` | `Data/Mapping/` | `IEntityTypeConfiguration<T>` implementations applied in `OnModelCreating` |
| `Database/Migrations/` | `Data/Migrations/` | EF Core code-first migrations |

### Shared Components Usage Pattern

Shared components are consumed by modules via dependency injection:

```csharp
// In Modules/SavedCart/Services/SavedCartService.cs
public class SavedCartService : ISavedCartService
{
    private readonly ISessionService _sessionService;          // From Shared/Services
    private readonly INotificationService _notificationService; // From Shared/Services
    private readonly IRepository<SavedCartEntity> _repository; // From Database

    public SavedCartService(
        ISessionService sessionService,
        INotificationService notificationService,
        IRepository<SavedCartEntity> repository)
    {
        _sessionService = sessionService;
        _notificationService = notificationService;
        _repository = repository;
    }
}
```

---

## Execution Blueprint with Quality Gates

| Step | Task Description | Agent | Quality Standards | Required Inputs | Expected Outputs | Acceptance Criteria | Depends On |
|:----:|:----------------|:------|:------------------|:----------------|:-----------------|:--------------------|:-----------|
| **PHASE 1: Core nopCommerce Compliance** |
| 1 | Create plugin.json with correct metadata | nopcommerce-plugin-developer | Valid JSON, all required fields, SupportedVersions = ["4.90"] | Plugin naming convention, version info | plugin.json file | JSON validates, fields match spec | - |
| 2 | Create .csproj file with package references | nopcommerce-plugin-developer | .NET 9.0 target, correct NuGet packages, OutputPath configuration | .NET 9.0 SDK, package versions from requirements | WZ.Plugin.Misc.WirelessDivision.csproj | Project loads in solution, builds without errors | - |
| 3 | Create WirelessDivisionPlugin.cs (IPlugin) | nopcommerce-plugin-developer | XML docs, async/await, implements Install/Uninstall/GetConfigurationPageUrl | IPlugin interface spec, nopCommerce patterns | WirelessDivisionPlugin.cs | Implements IPlugin, compiles, XML docs present | Step 2 |
| 4 | Create master DependencyRegistrar.cs shell | nopcommerce-plugin-developer | IDependencyRegistrar implementation, Order property, calls module registrars | Autofac patterns, module list | DependencyRegistrar.cs | Compiles, implements interface, Order = 1 | Step 3 |
| 5 | Create Infrastructure/PluginStartup.cs | nopcommerce-plugin-developer | IStartup implementation, ConfigureServices/Configure methods | nopCommerce startup patterns | Infrastructure/PluginStartup.cs | Compiles, middleware placeholder exists | Step 4 |
| 6 | Create Infrastructure/RouteProvider.cs | nopcommerce-plugin-developer | IRouteProvider implementation, GetRoutes method | nopCommerce routing patterns | Infrastructure/RouteProvider.cs | Compiles, routes registered correctly | Step 5 |
| 7 | **BUILD VERIFICATION GATE** | debug-expert | Zero compiler warnings/errors, solution builds | Code from Steps 1-6 | Build report | `dotnet build` succeeds | Steps 1-6 |
| **PHASE 2: Core Infrastructure (Authentication-First)** |
| 8 | Create Core/Configuration structure | csharp-expert | Settings models with validation, cache key constants | Plugin requirements, module list | WirelessDivisionSettings.cs, WirelessDivisionDefaults.cs, PermissionProvider.cs | Compiles, XML docs, follows naming conventions | Step 7 |
| 9 | Create Core/Authentication service interfaces | domain-expert | Interface design, XML docs, async methods | OAuth2 flow requirements, Azure EntraId specs | IWirelessDivisionAuthenticationService.cs, IAzureEntraIdProvider.cs, ITokenService.cs | Interfaces define complete contract, XML documented | Step 8 |
| 10 | Implement Core/Authentication services | csharp-expert | Async/await, error handling, logging, token management | Interfaces from Step 9, Microsoft.Identity.Web SDK | Service implementations (3 classes) | Compiles, all methods implemented, error handling present | Step 9 |
| 11 | Create Core/Authentication handlers | csharp-expert | AuthenticationHandler<T> implementation, token refresh logic | ASP.NET Core authentication patterns | WirelessDivisionAuthenticationHandler.cs, TokenRefreshHandler.cs | Compiles, inherits correctly, async throughout | Step 10 |
| 12 | Create Core/Authentication DependencyRegistrar | nopcommerce-plugin-developer | Register auth services, handlers, schemes | Service classes from Steps 10-11 | Core/Authentication/DependencyRegistrar.cs | Services registered correctly, Order = 0 (loads first) | Step 11 |
| 13 | Update master DependencyRegistrar to call Auth registrar | nopcommerce-plugin-developer | Call Core.Authentication registrar first | Master DependencyRegistrar, Auth registrar | Updated DependencyRegistrar.cs | Auth services registered before modules | Step 12 |
| 14 | Update PluginStartup with OAuth2 middleware | csharp-expert | AddAuthentication, AddOpenIdConnect, UseAuthentication ordering | Azure EntraId config, middleware patterns | Updated Infrastructure/PluginStartup.cs | Middleware configured, auth scheme registered | Step 13 |
| 15 | Create Core/Api HTTP client infrastructure | api-expert | IHttpClientFactory usage, retry policies, error handling | WirelessDivision API spec | IWirelessDivisionApiClient.cs, implementation, config | Compiles, HTTP client configured, typed client pattern | Step 8 |
| 16 | Create Core/Logging wrapper | csharp-expert | ILogger wrapper with structured logging | nopCommerce ILogger, module context | IWirelessDivisionLogger.cs, implementation | Compiles, extends ILogger, module-aware logging | Step 8 |
| 17 | Create Core/Services (ModuleManager, CacheKeyProvider) | domain-expert | Module lifecycle, cache key generation | Module list, caching patterns | IModuleManager.cs, ModuleManager.cs, ICacheKeyProvider.cs, CacheKeyProvider.cs | Compiles, module discovery/init logic present | Step 8 |
| 18 | **CORE INFRASTRUCTURE GATE** | debug-expert | All Core services registered, middleware configured, builds | Code from Steps 8-17 | Core validation report | Build succeeds, auth services resolve via DI | Steps 8-17 |
| **PHASE 3: Database Layer** |
| 19 | Create Database/Entities for all modules | efcore-expert | BaseEntity inheritance, navigation properties, XML docs | Module requirements, entity relationships | 7 entity files (SavedCart, SKUAllocation, RMA, User, RefreshToken, Activity, CartItem) | Entities model domain correctly, inherit BaseEntity | Step 18 |
| 20 | Create Database/Mapping configurations | efcore-expert | IEntityTypeConfiguration<T>, FluentAPI, indexes, constraints | Entities from Step 19 | 7 mapping files matching entities | Configurations apply in OnModelCreating, indexes defined | Step 19 |
| 21 | Create WirelessDivisionObjectContext | efcore-expert | DbContext inheritance, OnModelCreating, DbSet properties | Entities and mappings from Steps 19-20 | Database/WirelessDivisionObjectContext.cs | Compiles, applies all configurations, connection string from settings | Step 20 |
| 22 | Register ObjectContext in DependencyRegistrar | efcore-expert | Register DbContext with nopCommerce patterns | ObjectContext from Step 21 | Updated DependencyRegistrar.cs | DbContext registered, connection string resolved | Step 21 |
| 23 | Create initial EF Core migration | efcore-expert | Migration creates all tables, indexes, foreign keys | ObjectContext, entities, mappings | Database/Migrations/InitialCreate.cs | Migration generates valid SQL, up/down methods present | Step 22 |
| 24 | **DATABASE LAYER GATE** | debug-expert | Migration executes cleanly, tables created | Migration from Step 23, test database | Database validation report | Migration runs without errors, schema matches entities | Step 23 |
| **PHASE 4: Shared Components** |
| 25 | Create Shared/Models base classes | csharp-expert | Base view models, DTOs, pagination | Common model patterns | BaseModuleModel.cs, ApiResultModel.cs, PaginationModel.cs | Compiles, reusable across modules | Step 24 |
| 26 | Create Shared/Services utilities | csharp-expert | Session service, notification service | nopCommerce session, notification patterns | ISessionService.cs, SessionService.cs, INotificationService.cs, NotificationService.cs | Compiles, XML docs, async methods | Step 25 |
| 27 | Create Shared/Extensions | csharp-expert | Extension methods for strings, models, DI | Common transformation needs | 3 extension class files | Compiles, static classes with XML docs | Step 26 |
| 28 | Create Shared/Attributes custom attributes | csharp-expert | WirelessDivisionAuthorizeAttribute, ModuleEnabledAttribute | ASP.NET Core attribute patterns | 2 attribute classes | Compiles, inherits from Attribute, usable on controllers | Step 27 |
| 29 | Create Shared/Helpers utilities | csharp-expert | Crypto helper, JSON helper | Security best practices | CryptoHelper.cs, JsonHelper.cs | Compiles, no hardcoded secrets, uses standard libraries | Step 28 |
| 30 | Register Shared services in DependencyRegistrar | nopcommerce-plugin-developer | Register session, notification services | Services from Step 26 | Updated DependencyRegistrar.cs | Shared services registered, scoped correctly | Step 29 |
| 31 | **SHARED COMPONENTS GATE** | debug-expert | All shared services resolve, attributes usable | Code from Steps 25-30 | Shared validation report | Build succeeds, DI resolves shared services | Steps 25-30 |
| **PHASE 5: Module Implementation (Parallel After Step 31)** |
| 32 | Implement Modules/Authentication (UI/Controllers) | mvc-expert | Admin/public controllers, login view, config UI | Core.Authentication services, OAuth2 flow | Controllers (2), Models (2), Views (2), Validators (1), DependencyRegistrar | Compiles, controllers route correctly, views render | Step 31 |
| 33 | Implement Modules/BuildInfo | mvc-expert | BuildInfo controller, model, service, view | Build metadata access patterns | Controller, Model, Service interface/impl, View, DependencyRegistrar | Compiles, displays build info correctly | Step 31 |
| 34 | Implement Modules/SavedCart | domain-expert | CRUD controllers, service layer, admin views | SavedCartEntity, IRepository, business logic | Controllers (2), Models (2), Service interface/impl, Views (3), DependencyRegistrar | Compiles, CRUD operations work, admin list/edit functional | Step 31 |
| 35 | Implement Modules/SKUAllocation | domain-expert | SKU allocation logic, controller, service | SKUAllocationEntity, business rules | Controller, Model, Service interface/impl, DependencyRegistrar | Compiles, allocation logic implemented | Step 31 |
| 36 | Implement Modules/OrderSubmit | domain-expert | Order submission workflow, integration with nopCommerce orders | IOrderService, IShoppingCartService | Controller, Model, Service interface/impl, DependencyRegistrar | Compiles, integrates with nopCommerce order flow | Step 31 |
| 37 | Implement Modules/OnePageCheckout | mvc-expert | Custom checkout UI, checkout service, address/payment handling | nopCommerce checkout services | Controller, Model, Service interface/impl, View, DependencyRegistrar | Compiles, checkout page renders, integrates with nopCommerce | Step 31 |
| 38 | Implement Modules/StoreRma | domain-expert | RMA CRUD, admin management, customer RMA view | RmaRequestEntity, IRepository | Controllers (2), Models, Service interface/impl, Views (3), DependencyRegistrar | Compiles, RMA creation/management works | Step 31 |
| 39 | Implement Modules/MassOrder | domain-expert | Bulk order processing, CSV import, batch operations | IOrderService, file upload handling | Controller, Model, Service interface/impl, View, DependencyRegistrar | Compiles, handles bulk orders, error handling present | Step 31 |
| 40 | Update master DependencyRegistrar with all module registrars | nopcommerce-plugin-developer | Call all 8 module registrars in correct order | Module registrars from Steps 32-39 | Updated DependencyRegistrar.cs | All modules registered, no circular dependencies | Steps 32-39 |
| 41 | Update RouteProvider with all module routes | nopcommerce-plugin-developer | Register routes for all modules | Module controllers from Steps 32-39 | Updated Infrastructure/RouteProvider.cs | All module routes registered, no conflicts | Steps 32-39 |
| 42 | **MODULE IMPLEMENTATION GATE** | debug-expert | All modules build, services resolve, routes work | Code from Steps 32-41 | Module validation report | Build succeeds, DI resolves all module services, routes accessible | Steps 32-41 |
| **PHASE 6: Views & Frontend** |
| 43 | Create Views/Admin/Configure.cshtml (main plugin config) | mvc-expert | Admin configuration form, settings binding | WirelessDivisionSettings, admin patterns | Views/Admin/Configure.cshtml | Renders correctly, saves settings via ISettingService | Step 42 |
| 44 | Create all Admin module views | mvc-expert | Admin views for modules (SavedCart, StoreRma, Authentication) | Module models, DataTables, admin-LTE | 8+ admin view files | Views render, DataTables work, CRUD operations functional | Step 43 |
| 45 | Create all Public module views | mvc-expert | Public-facing views (Login, SavedCart, Checkout, MassOrder) | Module models, Bootstrap 4 | 6+ public view files | Views render, responsive, accessible | Step 44 |
| 46 | Create Shared views and layouts | mvc-expert | Shared layout, navigation, widget components | nopCommerce view patterns | _WirelessDivisionLayout.cshtml, _ModuleNavigation.cshtml, widget views | Shared views render, navigation works | Step 45 |
| 47 | Create Content/css stylesheets | mvc-expert | Admin and public CSS, responsive design | Bootstrap 4, Admin-LTE overrides | wirelessdivision.admin.css, wirelessdivision.public.css | Styles apply correctly, no conflicts with nopCommerce CSS | Step 46 |
| 48 | Create Content/js JavaScript files | mvc-expert | Common JS, module-specific JS, admin JS | jQuery, validation, module interactions | wirelessdivision.common.js, module JS files, admin JS | JS functions work, no console errors, validation works | Step 47 |
| 49 | Create Infrastructure/ViewLocationExpander | mvc-expert | Custom view path resolution for module structure | nopCommerce view engine patterns | Infrastructure/ViewLocationExpander.cs | Views resolve from module folders correctly | Step 48 |
| 50 | Register ViewLocationExpander in PluginStartup | mvc-expert | Add ViewLocationExpander to MVC options | ViewLocationExpander from Step 49 | Updated Infrastructure/PluginStartup.cs | View resolution works for module paths | Step 49 |
| 51 | **FRONTEND GATE** | debug-expert | All views render, CSS/JS load, no console errors | Code from Steps 43-50, running application | Frontend validation report | Views accessible, styles applied, JS functional | Steps 43-50 |
| **PHASE 7: Localization & Documentation** |
| 52 | Create Localization/en-US.xml resource file | nopcommerce-plugin-developer | All resource strings for plugin and modules | View content, validation messages, labels | Localization/en-US.xml | All strings defined, XML validates, no duplicates | Step 51 |
| 53 | Update views to use localization | mvc-expert | Replace hardcoded strings with @T("Resource.Key") | Resource file from Step 52 | Updated view files | Views display localized strings correctly | Step 52 |
| 54 | Create README.md plugin documentation | technical-documentation-expert | Plugin purpose, installation, configuration, module descriptions | Complete plugin codebase | README.md | Documentation complete, accurate, well-structured | Step 53 |
| 55 | **LOCALIZATION GATE** | debug-expert | All strings localized, no hardcoded text in views | Views, resource file | Localization validation report | No hardcoded strings, all resources display | Steps 52-54 |
| **PHASE 8: Testing & Validation** |
| 56 | Perform plugin installation test | debug-expert | Install via admin panel, verify tables created, settings initialized | Complete plugin, test nopCommerce instance | Installation test report | Plugin installs without errors, appears in admin | Step 55 |
| 57 | Test authentication flow end-to-end | debug-expert | OAuth2 login, token acquisition, user mapping, logout | Azure EntraId test tenant, configured plugin | Authentication test report | Users can authenticate, claims mapped correctly | Step 56 |
| 58 | Test all module functionality | debug-expert | Test each module's core features, CRUD operations, integrations | Configured plugin, test data | Module functionality report | All modules work as specified, no errors | Step 57 |
| 59 | Perform code quality review | csharp-expert | Review for coding standards compliance (XML docs, async, naming) | Complete codebase | Code quality report | Zero warnings, standards met, XML docs complete | Step 58 |
| 60 | Perform security review | debug-expert | Check input validation, SQL injection prevention, XSS protection, auth checks | Complete codebase | Security review report | No vulnerabilities found, validation present | Step 59 |
| 61 | Perform performance review | efcore-expert | Check for N+1 queries, caching usage, async operations | Complete codebase, profiling tools | Performance review report | No N+1 problems, caching implemented, async throughout | Step 60 |
| 62 | **FINAL QUALITY GATE** | debug-expert | All quality checks passed, plugin fully functional | All test reports | Final validation report | All gates passed, plugin production-ready | Steps 56-61 |

---

## Execution Flow

### Critical Path (Sequential)

```
Phase 1 (Steps 1-7) → Phase 2 (Steps 8-18) → Phase 3 (Steps 19-24) →
Phase 4 (Steps 25-31) → Phase 5 (Steps 32-42) → Phase 6 (Steps 43-51) →
Phase 7 (Steps 52-55) → Phase 8 (Steps 56-62)
```

### Parallel Opportunities

**After Step 31 completes (Shared Components ready):**
- Run Steps 32-39 (all 8 module implementations) **IN PARALLEL**
- Each module is independent and can be built simultaneously
- Blocking point: Step 40 requires all module registrars (32-39 complete)

**After Step 42 completes (Modules implemented):**
- Steps 43-48 (view creation) can run in parallel streams:
  - Stream A: Admin views (Steps 43-44)
  - Stream B: Public views (Step 45)
  - Stream C: Shared views (Step 46)
  - Stream D: CSS/JS (Steps 47-48)
- Converge at Step 49 (ViewLocationExpander)

**After Step 55 completes (Localization done):**
- Steps 56-61 (testing phases) can partially overlap:
  - Step 56 must complete first (installation prerequisite)
  - Steps 57-58 can run in parallel (auth testing + module testing)
  - Steps 59-61 (reviews) can run in parallel

---

## Agent Roster & Delegation Instructions

| Agent | Primary Role | Task Numbers | Critical Context to Provide |
|-------|-------------|--------------|---------------------------|
| **nopcommerce-plugin-developer** | Plugin compliance & structure | 1, 2, 3, 4, 5, 6, 12, 13, 22, 30, 40, 41, 52 | Plugin naming: `WZ.Plugin.Misc.WirelessDivision`, nopCommerce 4.90 patterns, existing plugin references (ExternalAuth.*) |
| **csharp-expert** | C# implementation & standards | 8, 10, 11, 14, 16, 25, 26, 27, 28, 29, 59 | .NET 9.0 features, async/await patterns, XML documentation requirements, language keywords mandate |
| **domain-expert** | Business logic & service design | 9, 17, 34, 35, 36, 38, 39 | WirelessDivision business rules, nopCommerce domain model (Order, Customer, ShoppingCart) |
| **efcore-expert** | Database & EF Core | 19, 20, 21, 22, 23, 61 | nopCommerce data patterns, BaseEntity, repository pattern, connection string from ISettingService |
| **mvc-expert** | Controllers, views, frontend | 32, 33, 37, 43, 44, 45, 46, 47, 48, 49, 50, 53 | nopCommerce admin patterns, Bootstrap 4, Admin-LTE, DataTables, view engine customization |
| **api-expert** | HTTP client infrastructure | 15 | WirelessDivision external API spec, IHttpClientFactory, retry policies, typed clients |
| **debug-expert** | Testing, validation, quality gates | 7, 18, 24, 31, 42, 51, 55, 56, 57, 58, 60, 62 | Quality standards, test scenarios, nopCommerce admin panel access, Azure EntraId test tenant |
| **technical-documentation-expert** | Documentation | 54 | Complete plugin functionality, module descriptions, installation steps, configuration guide |

### Delegation Notes for Team Commander

**Phase 1 (Steps 1-7)**: Assign entirely to `nopcommerce-plugin-developer`. This establishes plugin foundation. Provide them with:
- Plugin naming convention: `WZ.Plugin.Misc.WirelessDivision`
- nopCommerce 4.90 as target version
- .NET 9.0 as framework
- List of 8 modules for context

**Phase 2 (Steps 8-18)**: Mix of `domain-expert`, `csharp-expert`, `api-expert`, `nopcommerce-plugin-developer`. Critical order:
- Steps 8-9: `domain-expert` and `csharp-expert` design interfaces first
- Steps 10-11: `csharp-expert` implements (needs Microsoft.Identity.Web SDK context)
- Steps 12-14: `nopcommerce-plugin-developer` and `csharp-expert` integrate with nopCommerce DI/middleware
- Step 15: `api-expert` works independently (can parallelize with Steps 16-17)
- Step 18: `debug-expert` validates entire phase

**Phase 3 (Steps 19-24)**: Assign entirely to `efcore-expert`. Provide them with:
- Module entity requirements (SavedCart, SKUAllocation, RMA, etc.)
- nopCommerce BaseEntity pattern
- Repository pattern expectations
- Connection string resolution pattern (`ISettingService`)

**Phase 4 (Steps 25-31)**: `csharp-expert` handles most, `nopcommerce-plugin-developer` handles DI registration. Can parallelize Steps 25-29.

**Phase 5 (Steps 32-42)**: **MAXIMUM PARALLELIZATION OPPORTUNITY**
- Assign each module (Steps 32-39) to appropriate specialist:
  - Step 32 (Authentication UI): `mvc-expert`
  - Step 33 (BuildInfo): `mvc-expert`
  - Steps 34, 35, 36, 38, 39 (business-heavy modules): `domain-expert`
  - Step 37 (OnePageCheckout): `mvc-expert`
- All 8 can work simultaneously (shared components ready from Phase 4)
- Steps 40-41: `nopcommerce-plugin-developer` integrates all modules
- Step 42: `debug-expert` validates

**Phase 6 (Steps 43-51)**: Assign to `mvc-expert`. Can parallelize view creation (43-46) and assets (47-48).

**Phase 7 (Steps 52-55)**: Sequential. Step 52: `nopcommerce-plugin-developer`, Step 53: `mvc-expert`, Step 54: `technical-documentation-expert`.

**Phase 8 (Steps 56-62)**: Primarily `debug-expert` with specialist reviews:
- Steps 56-58: `debug-expert` performs functional testing
- Step 59: `csharp-expert` reviews code quality
- Step 60: `debug-expert` security review
- Step 61: `efcore-expert` performance review
- Step 62: `debug-expert` final validation

**Context Sharing Strategy**:
- After Phase 1: Share `plugin.json` and `WirelessDivisionPlugin.cs` structure with all agents
- After Phase 2: Share Core.Authentication service interfaces with module developers (Steps 32-39)
- After Phase 3: Share entity models with all service/controller developers
- After Phase 4: Share Shared component interfaces with module developers

**Bottleneck Warnings**:
- Step 31 is a critical synchronization point (blocks all of Phase 5)
- Step 42 blocks all frontend work (Phase 6)
- Step 55 blocks testing (Phase 8)
- Monitor `debug-expert` workload in Phase 8 (6 sequential tasks)

---

## Quality Gates & Compliance Verification

**Team Commander must verify these before marking mission complete:**

### nopCommerce Compliance

- [ ] Plugin naming follows `WZ.Plugin.Misc.WirelessDivision` convention exactly
- [ ] plugin.json exists at root with all required fields (Group, SystemName, Version, SupportedVersions, FileName)
- [ ] `WirelessDivisionPlugin.cs` implements `IPlugin` interface correctly
- [ ] `DependencyRegistrar.cs` implements `IDependencyRegistrar` at root
- [ ] Plugin installs without errors via admin panel
- [ ] Plugin uninstalls cleanly (database cleanup in Uninstall method)
- [ ] Plugin appears in Admin > Configuration > Local Plugins
- [ ] Configuration page accessible via admin menu
- [ ] No core nopCommerce files were modified
- [ ] Multi-store compatibility tested (settings per store)

### Code Quality

- [ ] Zero compiler warnings in Release and Debug configurations
- [ ] All public members have XML documentation comments (`/// <summary>`)
- [ ] Async/await used for all I/O operations (no `.Result` or `.Wait()`)
- [ ] Language keywords used over type names (`string` not `String`, `int` not `Int32`)
- [ ] Proper namespace structure: `WZ.Plugin.Misc.WirelessDivision.{Folder}.{Subfolder}`
- [ ] Error handling present in all service methods (try/catch with logging)
- [ ] ILogger used for logging throughout
- [ ] No hardcoded strings (all in localization resources)
- [ ] Proper disposal of resources (DbContext, HttpClient via DI)

### Security

- [ ] All user inputs validated (FluentValidation or Data Annotations)
- [ ] No SQL injection vulnerabilities (EF Core LINQ only, no raw SQL with user input)
- [ ] XSS protection in all views (HTML encoding, @Html.Raw only when safe)
- [ ] Azure EntraId credentials stored securely (ISettingService encrypted storage)
- [ ] Authorization checks on all admin controllers (`[AuthorizeAdmin]`)
- [ ] Module-specific permissions defined and enforced
- [ ] CSRF protection enabled (AntiForgeryToken in forms)
- [ ] HTTPS enforced for authentication endpoints

### Performance

- [ ] IStaticCacheManager used for frequently accessed data
- [ ] Cache keys follow pattern: `WirelessDivisionDefaults.{Module}CacheKey`
- [ ] No N+1 query problems (checked with SQL profiler)
- [ ] `.AsNoTracking()` used for read-only queries
- [ ] Async operations used for all database and HTTP calls
- [ ] Pagination implemented for large data sets
- [ ] Database indexes defined on foreign keys and frequently queried columns

### Database

- [ ] All entities inherit from `BaseEntity`
- [ ] Entity configurations use `IEntityTypeConfiguration<T>`
- [ ] `WirelessDivisionObjectContext` registered in DI correctly
- [ ] Initial migration creates all tables without errors
- [ ] Foreign key relationships defined correctly
- [ ] No orphaned records on plugin uninstall (cleanup in Uninstall method)
- [ ] Connection string resolved from nopCommerce settings

### Module Architecture

- [ ] All 8 modules implemented (Authentication, BuildInfo, SavedCart, SKUAllocation, OrderSubmit, OnePageCheckout, StoreRma, MassOrder)
- [ ] Each module has own DependencyRegistrar
- [ ] Module services registered in correct scope (Scoped for DB access, Singleton for stateless)
- [ ] Module independence verified (no direct module-to-module dependencies)
- [ ] Shared components used correctly via DI
- [ ] Core.Authentication loads first (DependencyRegistrar Order = 0)

### Authentication

- [ ] OAuth2 flow completes successfully with Azure EntraId test tenant
- [ ] Tokens acquired and stored securely
- [ ] Token refresh logic works correctly
- [ ] User claims mapped to nopCommerce customer properties
- [ ] Customer record created/linked on first login
- [ ] Logout clears session and invalidates tokens
- [ ] Authentication required for protected modules/endpoints

### Views & Frontend

- [ ] All views render without errors
- [ ] Views use localized strings (no hardcoded text)
- [ ] Admin views follow Admin-LTE theme patterns
- [ ] Public views use Bootstrap 4 responsive design
- [ ] DataTables configured correctly in admin list views
- [ ] JavaScript executes without console errors
- [ ] CSS styles apply correctly (no conflicts with nopCommerce)
- [ ] ViewLocationExpander resolves module views correctly

### Testing

- [ ] Plugin installation tested (clean install, tables created)
- [ ] Plugin uninstallation tested (tables dropped, settings removed)
- [ ] All module functionality manually tested
- [ ] Authentication flow tested end-to-end
- [ ] Admin configuration saves and loads correctly
- [ ] Multi-store tested (settings isolated per store)
- [ ] Error scenarios tested (invalid input, network failures, etc.)

### Documentation

- [ ] README.md complete with installation instructions
- [ ] Module descriptions documented
- [ ] Configuration steps documented
- [ ] API endpoints documented (if applicable)
- [ ] Troubleshooting guide included
- [ ] Version history / changelog present

### Deployment

- [ ] `dotnet build` succeeds with zero warnings
- [ ] Plugin DLL outputs to `Plugins/WZ.Plugin.Misc.WirelessDivision/` directory
- [ ] All dependencies copied to plugin output folder
- [ ] plugin.json copied to output folder
- [ ] Content folder (CSS/JS/images) copied to output
- [ ] Views copied to output folder
- [ ] Localization resources copied to output

---

## Additional Architecture Details

### DependencyRegistrar Orchestration Pattern

**Master DependencyRegistrar.cs** at plugin root:

```csharp
/// <summary>
/// Dependency registrar for WirelessDivision plugin
/// </summary>
public class DependencyRegistrar : IDependencyRegistrar
{
    /// <summary>
    /// Register services and interfaces
    /// </summary>
    public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
    {
        // Register Core infrastructure first (authentication must load before modules)
        new Core.Authentication.DependencyRegistrar().Register(services, typeFinder, appSettings);

        // Register Shared services
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Register Database context
        services.AddDbContext<WirelessDivisionObjectContext>(options =>
        {
            var dataSettings = DataSettingsManager.LoadSettings();
            options.UseSqlServer(dataSettings.ConnectionString);
        });

        // Register all module-specific services (order doesn't matter after auth)
        new Modules.Authentication.DependencyRegistrar().Register(services, typeFinder, appSettings);
        new Modules.BuildInfo.DependencyRegistrar().Register(services, typeFinder, appSettings);
        new Modules.SavedCart.DependencyRegistrar().Register(services, typeFinder, appSettings);
        new Modules.SKUAllocation.DependencyRegistrar().Register(services, typeFinder, appSettings);
        new Modules.OrderSubmit.DependencyRegistrar().Register(services, typeFinder, appSettings);
        new Modules.OnePageCheckout.DependencyRegistrar().Register(services, typeFinder, appSettings);
        new Modules.StoreRma.DependencyRegistrar().Register(services, typeFinder, appSettings);
        new Modules.MassOrder.DependencyRegistrar().Register(services, typeFinder, appSettings);
    }

    /// <summary>
    /// Order of this dependency registrar implementation
    /// </summary>
    public int Order => 1;
}
```

### PluginStartup Middleware Configuration

**Infrastructure/PluginStartup.cs** authentication middleware setup:

```csharp
/// <summary>
/// Startup configuration for WirelessDivision plugin
/// </summary>
public class PluginStartup : INopStartup
{
    /// <summary>
    /// Configure services
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure authentication
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "WirelessDivision";
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddScheme<AuthenticationSchemeOptions, WirelessDivisionAuthenticationHandler>("WirelessDivision", null)
        .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"));

        // Configure authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("WirelessDivisionAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("extension_WirelessDivisionUser", "true"));
        });
    }

    /// <summary>
    /// Configure middleware pipeline
    /// </summary>
    public void Configure(IApplicationBuilder app)
    {
        // Authentication must run early in pipeline
        app.UseAuthentication();
        app.UseAuthorization();
    }

    /// <summary>
    /// Order of this startup configuration
    /// </summary>
    public int Order => 100; // After nopCommerce core auth
}
```

### ViewLocationExpander Pattern

**Infrastructure/ViewLocationExpander.cs** for module-based view resolution:

```csharp
/// <summary>
/// Expands view locations to support modular structure
/// </summary>
public class ViewLocationExpander : IViewLocationExpander
{
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        // Add module-specific view paths
        var modulePaths = new[]
        {
            "/Plugins/WZ.Plugin.Misc.WirelessDivision/Views/{1}/{0}.cshtml",
            "/Plugins/WZ.Plugin.Misc.WirelessDivision/Views/Shared/{0}.cshtml",
            "/Plugins/WZ.Plugin.Misc.WirelessDivision/Modules/{1}/Views/{0}.cshtml",
        };

        return modulePaths.Concat(viewLocations);
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        // No additional values needed
    }
}
```

### Module DependencyRegistrar Example

**Modules/SavedCart/DependencyRegistrar.cs** pattern:

```csharp
/// <summary>
/// Dependency registrar for SavedCart module
/// </summary>
public class DependencyRegistrar
{
    /// <summary>
    /// Register services for SavedCart module
    /// </summary>
    public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
    {
        services.AddScoped<ISavedCartService, SavedCartService>();

        // Register validators
        services.AddTransient<IValidator<SavedCartModel>, SavedCartModelValidator>();
    }
}
```

---

## Mission Success Definition

This mission is **COMPLETE** when:

1. **Plugin installs successfully** in nopCommerce 4.90 admin panel without errors
2. **All 8 modules are functional** and accessible via their respective routes
3. **Azure EntraId authentication** completes end-to-end (login, token acquisition, user mapping, logout)
4. **All quality gates passed** (build, code quality, security, performance, testing)
5. **Zero compiler warnings** in both Debug and Release configurations
6. **Documentation complete** (README with installation/configuration/module descriptions)
7. **Code meets all nopCommerce standards** (naming, XML docs, async/await, no core modifications)

**Mission Owner**: Team Commander
**Success Verification**: debug-expert performs final validation (Step 62)
**Delivery Artifact**: Fully functional WZ.Plugin.Misc.WirelessDivision plugin ready for deployment

---

**This blueprint is now ready for execution. Team Commander should begin with Phase 1, delegating to nopcommerce-plugin-developer for plugin foundation setup.**
