# WirelessZone Plugin Consolidation Blueprint

## Executive Summary

**Mission Objective:** A single consolidated plugin `WZ.Plugin.Misc.WirelessDivision` with modular internal architecture that provides all functionality with improved maintainability, performance, and architectural integrity, including seamless EntraID authentication integration.

--- 

## Resources

You have attached MCP servers for use:
- Context7 : use to retrieve the most up-to-date information on a subjet
- exa : use for web searches and research on specific topics 
- microsoft.docs.mcp : use for microsoft learn documentation

## Consolidated Plugin Architecture

### Unified Plugin Structure: `WZ.Plugin.Misc.WirelessDivision`

General Layout of the Plugin should be modular and organized as follows. This will allow for clear separation of concerns, easier maintenance, and scalability.

```
WZ.Plugin.Misc.WirelessDivision/
├── Core/                    # Shared infrastructure (formerly StoreDrop)
│   ├── Configuration/       # All shared configuration classes
│   ├── Services/           # Common services and utilities
│   ├── Logging/            # Unified logging infrastructure
│   ├── Api/                # External API access patterns
│   ├── Authentication/     # EntraID authentication infrastructure
│   └── Infrastructure/     # DI, startup, and framework integration
├── Modules/                # Feature modules
│   ├── Authentication/     # EntraID authentication and session management
│   ├── BuildInfo/          # Build information display
│   ├── SavedCart/          # Cart persistence functionality
│   ├── SKUAllocation/      # Inventory management
│   ├── OrderSubmit/        # Order processing enhancements
│   ├── OnePageCheckout/    # Checkout experience
│   ├── StoreRma/           # Return management
│   └── MassOrder/          # Bulk order processing
├── Shared/                 # Cross-module shared components
│   ├── Models/             # Common data transfer objects (includes ADUserModel)
│   ├── Services/           # Cross-cutting services
│   ├── Extensions/         # Shared extension methods
│   └── Session/            # Session management and user context services
├── Database/               # Unified data access layer
│   ├── Entities/           # All domain entities
│   ├── Mapping/            # Entity mappings
│   └── Migrations/         # Database schema changes
└── Views/                  # Consolidated view hierarchy
    ├── Admin/              # Administrative interfaces
    ├── Public/             # Customer-facing interfaces
    └── Shared/             # Reusable view components
```

**Role:** Acts as shared infrastructure and configuration provider for all WZ plugins

- **Configuration Management:** Provides maintenance of necessary configuration settings and stored in the database settings table
- **External API Configuration:** Azure Storage, ESB Endpoints, RMA Address, Mass Order configurations, FedEx Label API settings
- **Shared Services:** Debug logging, API access patterns, session management, and user context services
- **Critical Infrastructure:** All other wireless zone plugins depend on this for basic operations and will be in this modular structure

---

## Module Architecture Principles

1. **Authentication-First Design:** EntraID authentication establishes user context and permissions for all modules
2. **Core Infrastructure:** Single source of truth for configuration, logging, and external API access
3. **Module Isolation:** Each module encapsulates its business logic while leveraging shared core and authentication
4. **Unified Data Layer:** Single entity framework context with all domains
5. **Session-Aware Services:** All services access user context through unified session management
6. **Consistent UI Patterns:** Unified view hierarchy with consistent user experience and permissions
7. **Secrets Management:** Secure handling of secrets from Azure Key Vault, sensitive information through encrypted storage and retrieval mechanisms

---

## Implementation Blueprint

### Phase 1: Core Infrastructure and Authentication Consolidation 

**Objective:** Establish unified core infrastructure and EntraID authentication foundation

**Tasks:**

1. **Create Core Authentication Infrastructure**

   - Create EntraID authentication following the nopCommerce IExternalAuthentication interface in Core/Authentication
   - Implement with most up-to-date Microsoft.Identity.Web libraries and standards
   - Create UserProfile and its management in Core/Models
   - Create unified authentication startup and middleware configuration
   - Establish session management services for user context
   - Ensure authentication has refresh token handling and secure cookie management

2. **Create Core Configuration System**

   - Create all configuration classes in Core/Configuration
   - Include Azure AD configuration from authentication plugin
   - Implement unified configuration provider service
   - Include Azure Key Vault integration for secrets management
   - Create external API configurations for URLs and keys (ESB, Azure Storage, FedEx, etc.)

3. **Establish Unified Logging Infrastructure**

   - Create IDebugLogger to override and extend the current ILogger in Core/Logging
   - Create a unified logging service that all modules will use and include "Debug" logging capabilities
   - Implement structured logging with configuration-driven levels
   - Create logging extensions for consistent usage patterns

4. **Consolidate External API Services**

   - Create all API access classes to Core/Api
   - Implement unified HTTP client configuration and error handling
   - Standardize authentication and request/response patterns
   - Create base classes and interfaces for API services

5. **Create Plugin Framework**

   - Implement the base plugin class with unified startup and DI configuration
   - Create module registration system for feature modules
   - Establish shared routing and middleware patterns
   - **Critical:** Ensure authentication runs before all other module initialization

**Deliverables:**

- Core authentication infrastructure fully functional and tested
- All configuration classes consolidated and accessible
- Unified debug logging system operational
- External API patterns created
- Session / Profile management services established

---