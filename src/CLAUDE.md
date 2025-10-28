# nopCommerce 4.90 - Comprehensive Development Guide

## Project Overview

**Project Type**: nopCommerce 4.90 E-Commerce Platform
**Technology Stack**: ASP.NET Core 9.0 (MVC), Entity Framework Core 9.0, C#
**Architecture**: Plugin-based modular architecture with layered design
**Database Support**: SQL Server, MySQL, PostgreSQL (via EF Core)
**Frontend**: Razor Views, jQuery, Bootstrap 4.6, Admin-LTE 3.2

## Critical: Mission Command Framework

This project uses a **Mission Command Framework** for AI-assisted development. The framework is located at `.claude/CLAUDE.md` and contains PRIMARY DIRECTIVES that MUST be followed:

### Request Classification (MANDATORY)

Every request must be classified before action:

| Classification | Criteria | Action |
|---------------|----------|--------|
| **Simple Task** | Single step, ≤2 files, no architectural decisions | Execute directly or delegate to single agent |
| **Complex Mission** | Multiple steps, >2 files, architectural impact, or ambiguous | Think about the user's prompt, dicern the objectives, and delegate for blueprinting |

**The `.claude/CLAUDE.md` file takes precedence over this file for all mission protocol decisions.**

## Solution Structure

```
nop4.90.0beta/src/
├── Libraries/                          # Core business logic and data access
│   ├── Nop.Core/                      # Core classes, entities, caching, events
│   ├── Nop.Data/                      # EF Core data access, migrations
│   └── Nop.Services/                  # Business services layer
├── Presentation/                       # Web application layer
│   ├── Nop.Web/                       # Main MVC application (public + admin)
│   └── Nop.Web.Framework/             # Web framework extensions
├── Plugins/                            # Modular plugin system (30+ plugins)
│   ├── Nop.Plugin.ExternalAuth.*/     # Authentication plugins
│   ├── Nop.Plugin.Payments.*/         # Payment gateway plugins
│   ├── Nop.Plugin.Shipping.*/         # Shipping provider plugins
│   ├── Nop.Plugin.Tax.*/              # Tax calculation plugins
│   ├── Nop.Plugin.Widgets.*/          # Widget/UI extension plugins
│   └── Nop.Plugin.Misc.*/             # Miscellaneous plugins
├── Tests/                              # Test projects
│   └── Nop.Tests/                     # NUnit tests with FluentAssertions
└── Build/                              # Build utilities
    └── src/ClearPluginAssemblies/     # Plugin assembly cleanup
```

## Technology Stack Details

### Backend (.NET/C#)
- **.NET Version**: 9.0 (ASP.NET Core 9.0.9)
- **Language**: C# (latest features)
- **ORM**: Entity Framework Core 9.0.9
- **DI Container**: Autofac 10.0.0
- **Object Mapping**: AutoMapper 14.0.0
- **Caching**:
  - In-Memory (IStaticCacheManager)
  - Redis (StackExchangeRedis 9.0.9)
  - SQL Server (9.0.9)
- **JSON**: Newtonsoft.Json
- **Identity**: Azure.Identity 1.16.0

### Frontend
- **CSS Framework**: Bootstrap 4.6.0 with RTL support
- **Admin Theme**: Admin-LTE 3.2.0
- **JavaScript Libraries**:
  - jQuery 3.7.1 with jQuery UI 1.13.2
  - jQuery Validation 1.19.5 (with Unobtrusive 4.0.0)
  - DataTables 2.3.4 with Bootstrap 4 integration
  - Chart.js 4.5.0
  - Swiper 11.1.14 (carousel/slider)
  - Summernote 0.9.1 (WYSIWYG editor)
  - FilePond 4.32.9 (file uploads)
  - Marked 16.3.0 (Markdown renderer)
- **Icons**: FontAwesome 7.0.1, Ionicons 2.0.1
- **Localization**: Globalize.js with CLDR data
- **Build System**: Gulp 4.0.2 with custom tasks

### Testing
- **Framework**: NUnit 4.4.0 with NUnit3TestAdapter 5.1.0
- **Assertions**: FluentAssertions 7.2.0
- **Mocking**: Moq 4.20.72
- **Database**: Microsoft.Data.Sqlite 9.0.9 (in-memory testing)
- **Test Runner**: Microsoft.NET.Test.Sdk 17.14.1

## Available Specialist Agents

This project includes a comprehensive agent framework located in `.claude/agents/`:

### Mission Command & Planning
- **mission-commander**: Strategic architect for complex missions (PRIMARY AGENT for complex work)
- **coa-team**: Creates detailed course of action with task prioritization
- **analysis-team**: Analyzes complex/legacy codebases to uncover structures
- **red-team**: Identifies vulnerabilities and omissions in planning

### Application Development
- **nopcommerce-plugin-developer**: PRIMARY agent for ALL plugin work (create/modify/troubleshoot)
- **csharp-expert**: Clean, idiomatic C# code with .NET best practices
- **domain-expert**: Business logic and domain models (DDD principles)
- **efcore-expert**: Entity Framework Core data access and performance
- **mvc-expert**: ASP.NET Core MVC server-side rendering
- **api-expert**: RESTful Web API development
- **debug-expert**: Bug diagnosis, root cause analysis, and fixing

### Documentation & Quality
- **technical-documentation-expert**: Developer-focused technical documentation
- **user-documentation-expert**: End-user guides in business terms
- **debriefing-expert**: Post-execution analysis and improvement strategies

## MCP (Model Context Protocol) Servers

Enabled MCP servers (configured in `.claude/settings.local.json`):
- **context7**: Context management
- **sequential_thinking**: Structured reasoning
- **microsoft.docs.mcp**: Microsoft documentation access

## Build & Development Commands

### Solution Build
```powershell
# Build entire solution
dotnet build NopCommerce.sln

# Build specific configuration
dotnet build NopCommerce.sln --configuration Release

# Restore NuGet packages
dotnet restore NopCommerce.sln

# Clean solution
dotnet clean NopCommerce.sln
```

### Running the Application
```powershell
# Run from Nop.Web project directory
cd Presentation\Nop.Web
dotnet run

# Run with specific launch profile
dotnet run --launch-profile "Nop.Web"

# Watch mode for development
dotnet watch run
```

### Frontend Build (Gulp)
```powershell
# Navigate to Nop.Web directory
cd Presentation\Nop.Web

# Install npm dependencies (first time only)
npm install

# Run default Gulp task (clean, copy dependencies, prepare CLDR)
npx gulp

# Run specific Gulp tasks
npx gulp clean
npx gulp copyDependencies
npx gulp prepareCldr
```

### Testing
```powershell
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests in specific project
dotnet test Tests\Nop.Tests\Nop.Tests.csproj

# Run tests with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```

### Plugin Development
```powershell
# Build specific plugin
dotnet build Plugins\Nop.Plugin.[Group].[Name]\Nop.Plugin.[Group].[Name].csproj

# Clean plugin assemblies (runs automatically after build via MSBuild target)
# This removes unnecessary libraries from plugin output directories
```

## nopCommerce Plugin Architecture

### Plugin Naming Convention
**MANDATORY**: All plugins must follow `Nop.Plugin.{Group}.{Name}` convention

**Plugin Groups**:
- `ExternalAuth`: Authentication providers (Facebook, Google, Microsoft, etc.)
- `Payments`: Payment gateways (PayPal, Stripe, Manual, etc.)
- `Shipping`: Shipping rate providers (UPS, FedEx, FixedByWeightByTotal)
- `Tax`: Tax calculation providers (Avalara, FixedOrByCountryStateZip)
- `Widgets`: UI widgets and integrations (GoogleAnalytics, FacebookPixel, Swiper)
- `Misc`: Miscellaneous plugins (Azure Blob, WebAPI, PowerBI, RFQ, etc.)
- `Pickup`: Pickup point providers (PickupInStore)
- `DiscountRules`: Discount rule providers (CustomerRoles)
- `Search`: Search providers (Lucene)
- `MultiFactorAuth`: MFA providers (GoogleAuthenticator)

### Plugin Structure (Required)
```
Plugins/Nop.Plugin.{Group}.{Name}/
├── plugin.json                           # Plugin descriptor (REQUIRED)
├── Nop.Plugin.{Group}.{Name}.csproj     # Project file
├── {Name}Plugin.cs                       # IPlugin implementation (REQUIRED)
├── DependencyRegistrar.cs                # Service registration (REQUIRED)
├── Infrastructure/
│   ├── RouteProvider.cs                  # Custom routes (optional)
│   └── PluginStartup.cs                  # Startup config (optional)
├── Services/                             # Business logic services
├── Domain/                               # Domain entities
├── Data/                                 # EF Core mappings & migrations
├── Controllers/                          # Admin/public controllers
├── Models/                               # View models
├── Views/                                # Razor views
├── Content/                              # CSS/JS/images
└── Localization/                         # Resource files
```

### plugin.json Structure
```json
{
  "Group": "Misc",
  "FriendlyName": "Plugin Display Name",
  "SystemName": "Namespace.Of.Plugin",
  "Version": "1.0.0",
  "SupportedVersions": [ "4.90" ],
  "Author": "Your Name/Company",
  "DisplayOrder": 1,
  "FileName": "Nop.Plugin.Group.Name.dll",
  "Description": "Brief description of plugin functionality"
}
```

### Core Plugin Interfaces
- **IPlugin**: Base plugin interface (install/uninstall/configuration)
- **IMiscPlugin**: Miscellaneous plugins
- **IExternalAuthenticationMethod**: External auth providers
- **IPaymentMethod**: Payment gateways
- **IShippingRateComputationMethod**: Shipping providers
- **ITaxProvider**: Tax calculation providers
- **IWidgetPlugin**: Widget/UI plugins
- **IExchangeRateProvider**: Currency exchange rate providers

## nopCommerce Coding Standards

### XML Documentation (MANDATORY)
ALL public members must have XML documentation:
```csharp
/// <summary>
/// Brief description of what this does
/// </summary>
/// <param name="customerId">The customer identifier</param>
/// <returns>A task that represents the asynchronous operation</returns>
public async Task<Customer> GetCustomerByIdAsync(int customerId)
{
    // Implementation
}
```

### Async/Await Pattern (REQUIRED)
- All I/O operations must be async
- Method names must end with `Async`
- Use `async/await` throughout (never `.Result` or `.Wait()`)

```csharp
// CORRECT
public async Task<Order> GetOrderAsync(int orderId)
{
    return await _orderRepository.GetByIdAsync(orderId);
}

// WRONG - blocking async call
public Order GetOrder(int orderId)
{
    return _orderRepository.GetByIdAsync(orderId).Result; // NEVER DO THIS
}
```

### Language Keywords Over Type Names
```csharp
// CORRECT
string name = "John";
int count = 10;
bool isActive = true;

// WRONG
String name = "John";
Int32 count = 10;
Boolean isActive = true;
```

### Dependency Injection Pattern
```csharp
/// <summary>
/// Dependency registrar for plugin services
/// </summary>
public class DependencyRegistrar : IDependencyRegistrar
{
    /// <summary>
    /// Register services and interfaces
    /// </summary>
    public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
    {
        services.AddScoped<IMyService, MyService>();
    }

    /// <summary>
    /// Order of this dependency registrar implementation
    /// </summary>
    public int Order => 1;
}
```

### Caching Pattern (IStaticCacheManager)
```csharp
private readonly IStaticCacheManager _cacheManager;

public async Task<Customer> GetCustomerByIdAsync(int customerId)
{
    var cacheKey = _cacheManager.PrepareKeyForDefaultCache(
        NopCustomerDefaults.CustomerByIdCacheKey,
        customerId);

    return await _cacheManager.GetAsync(cacheKey, async () =>
    {
        return await _customerRepository.GetByIdAsync(customerId);
    });
}
```

### Error Handling & Logging
```csharp
private readonly ILogger _logger;

public async Task ProcessOrderAsync(Order order)
{
    try
    {
        await _orderService.ProcessOrderAsync(order);
    }
    catch (Exception ex)
    {
        await _logger.ErrorAsync($"Error processing order {order.Id}", ex);
        throw; // Re-throw or handle appropriately
    }
}
```

## Core nopCommerce Services

### Customer Management
- `ICustomerService`: Customer CRUD operations
- `ICustomerRegistrationService`: Registration/login logic
- `ICustomerAttributeService`: Custom customer attributes
- `ICustomerActivityService`: Activity logging

### Catalog Management
- `IProductService`: Product operations
- `ICategoryService`: Category management
- `IManufacturerService`: Manufacturer management
- `ISpecificationAttributeService`: Product specifications

### Order Management
- `IOrderService`: Order CRUD and processing
- `IOrderProcessingService`: Order workflow
- `IShoppingCartService`: Shopping cart operations
- `ICheckoutAttributeService`: Checkout attributes

### Core Services
- `ISettingService`: Configuration settings (per-store support)
- `ILocalizationService`: Multi-language resources
- `IWorkContext`: Current customer/language/currency context
- `IStoreContext`: Multi-store context
- `IStaticCacheManager`: Caching abstraction
- `IEventPublisher`: Domain event publishing
- `IScheduleTaskService`: Background task scheduling

### Payment & Shipping
- `IPaymentService`: Payment processing abstraction
- `IShippingService`: Shipping rate calculation
- `ITaxService`: Tax calculation

### Media & Files
- `IPictureService`: Image management
- `IDownloadService`: File downloads

## Database Architecture

### Entity Framework Core Usage
- **Code-First Migrations**: Use EF Core migrations for schema changes
- **Repository Pattern**: Access via `IRepository<TEntity>` from `Nop.Data`
- **Entity Configuration**: Use `IEntityTypeConfiguration<TEntity>` for Fluent API
- **No Tracking**: Use `.AsNoTracking()` for read-only queries

### Common Entity Base Classes
- `BaseEntity`: Base class with `Id` property
- `BaseEntityWithDate`: Adds `CreatedOnUtc` property
- `SoftDeleteEntity`: Adds `Deleted` flag for soft deletes

### Multi-Store & Multi-Tenant Support
Many entities support multi-store via mapping tables (e.g., `StoreMapping`)

## Security Considerations

### Input Validation
- Validate all user inputs
- Use Data Annotations and FluentValidation
- Sanitize HTML inputs (XSS prevention)

### SQL Injection Prevention
- ALWAYS use EF Core LINQ queries (parameterized)
- NEVER concatenate SQL strings with user input

### Authentication & Authorization
- Use `[AuthorizeAdmin]` attribute for admin controllers
- Use `[ValidateIpAddress]` for IP restrictions
- Use `IPermissionService` for permission checks

### Secure Configuration
- Store secrets in `appsettings.json` (not in code)
- Use `ISettingService` for plugin configuration (encrypted storage)
- Use Azure Key Vault or similar for production secrets

## Performance Best Practices

### Caching Strategy
- Use `IStaticCacheManager` for frequently accessed data
- Cache keys should be specific and include relevant IDs
- Clear cache on entity updates (use `ClearCache()`)

### Database Query Optimization
- Avoid N+1 queries (use `.Include()` for eager loading)
- Use `.AsNoTracking()` for read-only queries
- Use pagination for large result sets
- Index frequently queried columns

### Async Operations
- All I/O operations must be async
- Use `async/await` throughout the call stack
- Avoid blocking calls (`.Result`, `.Wait()`)

## Git Workflow

**Current Branch**: `develop` (main development branch)
**Main Branch**: `develop` (use for PRs)

### Recent Commits
```
31d07b61f6 - Updated the search engine list synchronization date
00a628910c - #405 Added a couple of validator tests
c1ef0ced7c - Merge branch 'issue-7889-Summernote-Code' into develop
b5dac03843 - #7889 Summernote "Source" editor will not save changes when active
8ff20e975e - Merge branch 'issue-7876-MFA-Guest' into develop
```

### Git Commands (Allowed via settings.local.json)
```powershell
# View status
git status

# View differences
git diff

# View commit history
git log --oneline -10
```

## Configuration Files

### Permission Configuration (.claude/settings.local.json)
Pre-approved bash commands:
- `Select-Object FullName` (PowerShell object selection)
- `dir:*` (directory listing)
- `findstr:*` (string search in files)

### IDE Configuration
- No `.editorconfig` found (use Visual Studio defaults)
- No `.vscode` or `.github` directories
- Solution configured for Visual Studio 2022+

## Development Workflow

### 1. Starting a New Feature/Plugin
```powershell
# Create feature branch
git checkout -b feature/my-feature develop

# If creating a plugin, use nopcommerce-plugin-developer agent
# If complex, delegate / coordinate with agents to create the mission blueprint
```

### 2. Development Process
1. Follow mission blueprint (if complex task)
2. Implement following nopCommerce coding standards
3. Add XML documentation to all public members
4. Write unit tests (in Nop.Tests project)
5. Test in admin panel and public store

### 3. Building & Testing
```powershell
# Build solution
dotnet build

# Run frontend build
cd Presentation\Nop.Web
npm install
npx gulp

# Run tests
dotnet test

# Run application
dotnet run --project Presentation\Nop.Web\Nop.Web.csproj
```

### 4. Quality Checklist
- [ ] Zero compiler warnings
- [ ] All public members have XML docs
- [ ] Async/await used for I/O operations
- [ ] Language keywords used (not type names)
- [ ] Input validation implemented
- [ ] Error handling and logging added
- [ ] Caching implemented where appropriate
- [ ] Tests written and passing
- [ ] Plugin installs/uninstalls cleanly (for plugins)

### 5. Committing Changes
```powershell
# Stage changes
git add .

# Commit with descriptive message
git commit -m "Feature: Description of changes"

# Push to remote
git push origin feature/my-feature
```

## Troubleshooting Common Issues

### Plugin Not Appearing in Admin
- Verify `plugin.json` is correct (especially `SupportedVersions`)
- Check plugin DLL is in `Plugins/` output directory
- Restart application
- Check `App_Data/plugins.json` for plugin registration
- Check admin logs for load errors

### Build Errors
- Clean solution: `dotnet clean`
- Delete `bin/` and `obj/` folders
- Restore packages: `dotnet restore`
- Check .NET 9.0 SDK is installed

### Frontend Asset Issues
- Run `npm install` in `Presentation/Nop.Web`
- Run `npx gulp` to rebuild assets
- Check `wwwroot/lib/` for copied dependencies

### Database Migration Issues
- Verify connection string in `appsettings.json`
- Run migrations manually: `dotnet ef database update`
- Check migration files in `Nop.Data/Migrations/`

## Documentation Resources

### Internal Documentation
- `.claude/CLAUDE.md`: Mission protocol (PRIMARY DIRECTIVES)
- `.claude/agents/`: Specialist agent specifications
- `.claude/docs/`: Analysis and planning documents
- `.claude/diagrams/`: Architecture diagrams (Mermaid format)

### External Resources
- nopCommerce Official Docs: https://docs.nopcommerce.com/
- nopCommerce GitHub: https://github.com/nopSolutions/nopCommerce
- nopCommerce Forums: https://www.nopcommerce.com/boards
- ASP.NET Core Docs: https://docs.microsoft.com/aspnet/core

## Key Architectural Patterns

### 1. Plugin-Based Extensibility
- Core platform remains untouched
- All customizations via plugins
- Plugins can override/extend core functionality
- Hot-reload support (with restart)

### 2. Service Layer Pattern
- Business logic in service classes
- Services injected via DI
- Services work with repositories
- Services publish domain events

### 3. Repository Pattern
- Data access abstracted via `IRepository<TEntity>`
- EF Core `DbContext` hidden from services
- Unit of Work pattern for transactions

### 4. Event-Driven Architecture
- Domain events published via `IEventPublisher`
- Event consumers in `Infrastructure/` folder
- Loosely coupled plugin communication

### 5. Multi-Tenancy (Multi-Store)
- Store-specific configuration via `ISettingService`
- Store context via `IStoreContext`
- Entity-store mappings for data isolation

### 6. Localization
- Resource strings in XML files
- `ILocalizationService` for retrieval
- Culture-specific formatting
- Admin UI for managing translations

### 7. Caching Strategy
- Multiple cache providers (Memory, Redis, SQL)
- `IStaticCacheManager` abstraction
- Cache key preparation with entity IDs
- Automatic cache invalidation on updates

## Project Maturity & Status

- **Version**: 4.90 (beta)
- **Stability**: Development/Beta
- **Plugin Count**: 30+ included plugins
- **Active Development**: Yes (recent commits show ongoing work)
- **Test Coverage**: NUnit tests in place (validator tests recently added)

## Success Metrics for AI-Assisted Development

### Code Quality
- Zero compiler warnings
- All public APIs documented
- Async/await used consistently
- Proper error handling and logging

### nopCommerce Compliance
- Plugin naming convention followed
- plugin.json structure correct
- IPlugin interface properly implemented
- No core file modifications

### Security
- All inputs validated
- No SQL injection vulnerabilities
- XSS protection in views
- Secrets stored securely

### Performance
- Caching implemented appropriately
- No N+1 query problems
- Async operations for I/O
- Database queries optimized

---

**Remember**: For complex missions requiring multiple agents or architectural decisions, ALWAYS delegate and coordinate with agents to create a comprehensive blueprint that ensures quality, compliance, and proper coordination.

**Primary Directive Source**: `.claude/CLAUDE.md` (takes precedence for mission protocols)
