# RoundRoom Legacy Plugin Functional Analysis

**Document Version**: 1.0
**Created**: 2025-10-15
**Target**: nopCommerce 4.90 Migration
**Source**: nopCommerce 4.8 Legacy Plugins

---

## Executive Summary

This document provides a comprehensive functional analysis of 9 legacy plugins from nopCommerce 4.8 that will be consolidated into the unified **Nop.Plugin.Misc.RoundRoom** plugin for nopCommerce 4.90. The analysis focuses on functional inputs/outputs, data flows, external integrations, and business logic—independent of implementation details.

### Legacy Plugins Analyzed

1. Nop.Plugin.ExternalAuth.Office
2. WZ.Plugin.SKUAllocationByOwner
3. WZ.Plugin.Widget.SavedCart
4. WZ.Plugin.Misc.OrderSubmit
5. WZ.Plugin.Widgets.MassOrder
6. WZ.Plugin.Widgets.OnePageCheckout
7. WZ.Plugin.Widgets.StoreDrop
8. WZ.Plugin.Widgets.StoreRma
9. WZ.Plugin.Widgets.BuildInfo

---

## 1. Nop.Plugin.ExternalAuth.Office

### Purpose
Provides Microsoft Office 365/Azure AD authentication for nopCommerce using OpenID Connect (OIDC).

### Functional Inputs

#### Configuration Inputs
- **Client ID**: Azure AD Application ID (GUID format)
- **Tenant ID**: Azure tenant identifier or "common"/"organizations"
- **Domain**: Tenant domain (e.g., `contoso.onmicrosoft.com`)
- **Instance**: Azure login endpoint (default: `https://login.microsoftonline.com/`)
- **Callback Path**: OIDC callback route (default: `/signin-oidc`)

#### User Authentication Flow Inputs
- **Azure AD Claims**: Received from Microsoft Identity Platform
  - `name` (email address)
  - `NameIdentifier` (unique user ID)
  - Various user profile claims
- **Access Token**: OAuth 2.0 access token from Azure AD

### Functional Outputs

#### User Session Data
- **AD_USER**: Username extracted from email (before @)
- **AD_EMAIL**: Full email address
- **USER_MODEL**: Complete user profile from external database
- **COMPANY_ID**: Company/tenant identifier (data_area_id)
- **CUST_ID**: ERP customer identifier
- **STORE_LIST**: Comma-separated list of authorized stores
- **SESSION_GUID**: Unique session identifier

#### User Profile Data (via External DB Call)
- **displayName**: User's display name
- **WZListForRole**: Array of authorized store identifiers
- **PrimaryWZNumber**: User's primary warehouse/store number
- **cust_id**: ERP customer identifier
- **isEarlyPayAccess**: Boolean flag for early payment access
- **canSubmitCart**: Boolean flag for cart submission permission
- **data_area_id**: Company identifier (1 = WZ, 2 = TCC, etc.)
- **isAdmin**: Boolean flag for admin privileges (auto-set if PrimaryWZNumber = "ATI")

### External Dependencies

#### Database: WZOlympus
- **Stored Procedure**: `spwzGetUserExtendedProfile`
  - **Input**: `@UserLoginName` (varchar)
  - **Output**: User profile record with permissions and store access

#### Database: WZIntegrations
- **Table**: `twzNOPSessions`
  - **Purpose**: Track active user sessions
  - **Operations**: INSERT on login, DELETE on logout
  - **Columns**: UserName, SessionGUID, additional session metadata

### Authentication Flow

1. **Login Initiation**: User clicks login → Challenge sent to Azure AD
2. **Azure AD Authentication**: User authenticates with Microsoft credentials
3. **Callback Processing**:
   - Validate Azure AD response
   - Extract user email from claims
   - Query `spwzGetUserExtendedProfile` for user details
   - Set session data
   - Create/update nopCommerce customer record
4. **Session Management**:
   - Create session record in `twzNOPSessions`
   - Store user model in HTTP session
5. **Logout**:
   - Remove session from `twzNOPSessions`
   - Clear HTTP session
   - Redirect to Azure AD logout endpoint

### Security Considerations
- SQL injection risk in `RemoveSessionFromTable` (uses string concatenation)
- Session data stored in HTTP session (not encrypted)
- Hard-coded tenant ID in logout URL
- Admin privileges auto-granted for "ATI" warehouse

---

## 2. WZ.Plugin.SKUAllocationByOwner

### Purpose
Manages product SKU allocations by store owner, restricting quantity purchases per store/time period.

### Functional Inputs

#### Entity: SkuAllocation
- **WZNumber**: Store identifier (warehouse number)
- **Sku**: Product SKU
- **AllocationQuantity**: Total allocated quantity
- **UsedAllocationQuantity**: Quantity already consumed
- **FromDateTime**: Allocation start date/time
- **ToDateTime**: Allocation end date/time

#### Search/Filter Inputs
- **searchWZNumber**: Filter by store identifier
- **searchSKU**: Filter by product SKU
- **searchAllocationQuantity**: Filter by allocation quantity
- **searchUsedAllocationQuantity**: Filter by used quantity
- **searchFromDateTime**: Filter by start date
- **searchToDateTime**: Filter by end date
- **pageIndex**, **pageSize**: Pagination parameters

#### Business Operations
- **Create Allocation**: Add new SKU allocation for a store
- **Update Allocation**: Modify existing allocation
- **Delete Allocation**: Remove allocation
- **Bulk Import**: Import allocations from Excel/CSV
- **Bulk Export**: Export allocations to Excel
- **Revert Allocation**: Reset used quantity to previous state

### Functional Outputs

#### AvailableSkuAllocationModel
- **sku**: Product SKU
- **owner**: Store identifier (WZNumber)
- **availableQuantity**: Remaining quantity (AllocationQuantity - UsedAllocationQuantity)
- **allocationStart**: FromDateTime
- **allocationEnd**: ToDateTime

#### Business Rules
- **Overlapping Allocations**: Prevents duplicate allocations for same SKU/Store/TimeRange
- **Allocation Consumption**: Decrements available quantity when items added to cart
- **Time-Based Validation**: Only active allocations (current time between From/To dates)
- **Zero-Quantity Handling**: Blocks checkout if allocation exhausted

### External Dependencies

#### Database Tables
- **SkuAllocation** (custom table)
  - Stores allocation records
  - EF Core migrations: `SchemaMigration_00.cs`, `SchemaMigration_01.cs`, `SchemaMigration_02.cs`

#### Integration Points
- **Shopping Cart Service**: Validates allocation before adding to cart
- **Product Details Page**: Displays available allocation quantity
- **Admin Panel**: Custom menu item under "Catalog" → "SKU Allocation"

### Use Cases

1. **Admin Creates Allocation**:
   - Store manager allocates 50 units of SKU "PHONE123" to WZ store "0001" for Q1

2. **Customer Views Product**:
   - Product page shows "Available: 45 units (Allocation-based)"

3. **Customer Adds to Cart**:
   - System checks allocation
   - If sufficient: Increment UsedAllocationQuantity
   - If insufficient: Display error message

4. **Customer Removes from Cart**:
   - Decrement UsedAllocationQuantity (restore allocation)

5. **Allocation Expires**:
   - After ToDateTime, allocation no longer applies

### Admin UI Features
- **List View**: DataTable with search/filter/pagination
- **Create/Edit Form**: Input form with validation
- **Bulk Import**: Upload Excel file with allocations
- **Bulk Export**: Download current allocations as Excel
- **Admin Widget Zone**: `admin_sku_allocation`

---

## 3. WZ.Plugin.Widget.SavedCart

### Purpose
Enables customers to save shopping carts for later retrieval and integrates with VSOD (Visual Store Order Drop) system for importing external saved lists.

### Functional Inputs

#### Entity: SavedCart
- **StoreId**: nopCommerce store identifier
- **CustomerId**: nopCommerce customer identifier
- **CartName**: User-provided cart name
- **CreatedOnUtc**: Timestamp of cart creation
- **UpdatedOnUtc**: Timestamp of last modification

#### Entity: SavedCartItem
- **SavedCartId**: Foreign key to SavedCart
- **ProductId**: nopCommerce product identifier
- **Quantity**: Item quantity
- **CreatedOnUtc**: Timestamp of item creation
- **UpdatedOnUtc**: Timestamp of last modification

#### Entity: VSOTransaction
- **Purpose**: Tracks VSOD import transactions
- **Fields**: Transaction metadata, status, error messages

#### Operations
- **Create Saved Cart**: Customer saves current shopping cart
- **Load Saved Cart**: Customer loads previously saved cart
- **Delete Saved Cart**: Customer deletes saved cart
- **List Saved Carts**: Display all saved carts for customer
- **VSOD Import**: Import saved lists from external VSOD system

### Functional Outputs

#### Saved Cart Operations
- **SavedCartListModel**: List of saved carts for customer
  - Cart name
  - Item count
  - Total value
  - Created/updated dates

- **SavedCartDetailsModel**: Full cart details
  - Cart metadata
  - Line items with product details
  - Quantities
  - Pricing

#### VSOD Integration
- **Import Results**:
  - Number of carts imported
  - Success/failure status
  - Error messages for failed imports

### External Dependencies

#### Database: NopDB (SQL Server)
- **SQL Job**: `WZ VSOD Create Saved Lists`
  - **Purpose**: Automated import of saved lists from VSOD system
  - **Trigger**: Manual via admin or scheduled
  - **Connection**: Uses `StoreDropConfig.NopDB` connection string

#### Database Tables
- **SavedCart** (custom table)
- **SavedCartItem** (custom table)
- **VSOTransaction** (custom table)

#### Configuration
- **StoreDropConfig**: External configuration for database connections
  - `NopDB`: Connection string for nopCommerce database
  - `WZIntegrations`: Connection string for integration database
  - `WZOlympus`: Connection string for user profile database

### Integration Points
- **Customer Account Page**: Custom navigation item "Saved Carts"
- **Shopping Cart Page**: "Save Cart" button
- **Admin Panel**: Import saved carts from VSOD
- **Mass Order Plugin**: Uses saved carts as input

### Use Cases

1. **Customer Saves Cart**:
   - Input: Current shopping cart + cart name
   - Output: SavedCart record + SavedCartItem records
   - Result: Cart saved for later retrieval

2. **Customer Loads Cart**:
   - Input: SavedCartId
   - Output: Shopping cart populated with saved items
   - Result: Customer can continue shopping or checkout

3. **VSOD Import**:
   - Input: External VSOD data (via SQL Job)
   - Process: SQL Job `WZ VSOD Create Saved Lists` runs
   - Output: Multiple SavedCart records created
   - Result: Customer can access imported lists

4. **Admin Triggers Import**:
   - Input: Admin clicks "Import VSOD"
   - Process: Calls `sp_start_job` stored procedure
   - Output: Job execution status
   - Result: Background import initiated

### Business Rules
- **Cart Ownership**: Saved carts tied to CustomerId (not transferable)
- **Store Scope**: Saved carts scoped to StoreId
- **Duplicate Names**: Allow duplicate cart names (differentiated by ID)
- **Product Availability**: No validation on save; validation on load
- **Quantity Limits**: No quantity limits on save; apply on checkout

---

## 4. WZ.Plugin.Misc.OrderSubmit

### Purpose
Submits completed nopCommerce orders to external ERP system via REST API.

### Functional Inputs

#### Configuration
- **ApiUrl**: ERP API endpoint (e.g., `https://devaeolusapi.wirelesszone.com/`)

#### Order Data (SalesOrder Model)
- **companyId**: Company identifier (data_area_id: 1=WZ, 2=TCC)
- **externalSalesOrderKey**: nopCommerce order number (truncated to 15 chars)
- **custPONumber**: Customer-provided PO number
- **storeId**: Store identifier (WZNumber)
- **comment**: Order comments/notes
- **paymentTermsId**: Payment terms (e.g., "Default", "Net5")
- **cashDiscountCode**: Cash discount code (optional, for "Net5" terms)
- **shipMethod**: Shipping method identifier
- **salesOrderDate**: Order submission date (ISO format)
- **lines**: Array of SalesOrderLine objects
- **createdBy**: Username of order submitter (truncated to 15 chars)

#### Order Line Data (SalesOrderLine Model)
- **lineNumber**: Sequential line number
- **itemId**: Product SKU
- **quantity**: Order quantity
- **unitPrice**: Unit price
- **discountAmount**: Line discount (if applicable)

### Functional Outputs

#### API Response
- **Success Response**:
  - HTTP 200/201
  - Sales order number from ERP
  - Confirmation message

- **Error Response**:
  - HTTP 4xx/5xx
  - Error message
  - Validation errors

#### Order Status Update
- **nopCommerce Order Status**: Updated based on API response
  - Success: Mark as "Processing" or "Complete"
  - Failure: Log error, leave in "Pending" state

### External Dependencies

#### ERP API Endpoint
- **Base URL**: Configurable via plugin settings
- **Authentication**: Not specified in code (likely API key or OAuth)
- **Content-Type**: `application/json`
- **Method**: POST

### Integration Points
- **Order Processing Workflow**: Triggered after order placement
- **Event Consumer**: Likely subscribes to `OrderPlacedEvent`
- **Admin Configuration**: Settings page for API URL

### Use Cases

1. **Order Placed**:
   - Input: nopCommerce Order entity
   - Process: Transform to SalesOrder model → POST to ERP API
   - Output: ERP sales order number
   - Result: Order synced to ERP

2. **Order Submit Failure**:
   - Input: API error response
   - Process: Log error message
   - Output: Notification to admin
   - Result: Manual intervention required

3. **Configuration**:
   - Input: Admin enters API URL
   - Process: Save to OrderSubmitSettings
   - Output: Settings persisted
   - Result: Plugin ready to submit orders

### Business Rules
- **Order ID Truncation**: External key truncated to 15 characters
- **Username Truncation**: CreatedBy truncated to 15 characters
- **Payment Terms Mapping**: Map nopCommerce payment methods to ERP payment terms
- **Shipping Method Mapping**: Map nopCommerce shipping options to ERP ship methods
- **Company Identification**: Determine company ID from user profile or store

---

## 5. WZ.Plugin.Widgets.MassOrder

### Purpose
Enables bulk order entry by selecting multiple saved carts across multiple stores and submitting them as individual orders.

### Functional Inputs

#### Entity: MassOrder
- **UserName**: User who created the mass order
- **WZNumber**: Store identifier
- **CartName**: Saved cart name
- **SKU**: Product SKU
- **Quantity**: Order quantity
- **ShippingMethod**: Shipping method identifier (nullable)
- **PaymentMethod**: Payment method identifier (nullable)
- **CustPONumber**: Customer PO number
- **Status**: Processing status (0=Pending, 1=Validated, 2=Submitted, 3=Failed)
- **Message**: Status/error message
- **Priority**: Order priority (nullable)
- **SONumber**: Generated sales order number
- **CreatedOnUtc**: Timestamp
- **UpdatedOnUtc**: Timestamp

#### Validation Request (ValidateMassOrderRequest)
- **massOrderIds**: Array of MassOrder IDs to validate
- **Purpose**: Validate orders before submission (inventory check, pricing, etc.)

#### Submission Request (SubmitMassOrderRequest)
- **massOrderIds**: Array of MassOrder IDs to submit
- **Purpose**: Submit validated orders to ERP

### Functional Outputs

#### MassOrderValidationResultModel
- **massOrderId**: Unique identifier
- **isValid**: Boolean validation status
- **errors**: Array of error messages
- **warnings**: Array of warning messages
- **lineValidations**: Per-SKU validation results

#### MassOrderTotalsModel
- **totalOrders**: Count of orders
- **totalItems**: Total quantity across all orders
- **totalValue**: Total monetary value
- **storeBreakdown**: Per-store summary

### External Dependencies

#### Database Table
- **MassOrder** (custom table)

#### Integration Points
- **Saved Cart Plugin**: Loads saved carts as mass order source
- **SKU Allocation Plugin**: Validates allocations for mass orders
- **Order Submit Plugin**: Submits individual orders to ERP
- **External Validation API**: Validates orders before submission
  - Endpoint: Configurable
  - Purpose: Inventory check, pricing validation, customer credit check

### Workflow

1. **Mass Order Creation**:
   - Input: User selects multiple saved carts
   - Process: Create MassOrder records for each cart line
   - Output: MassOrder records with Status=Pending

2. **Mass Order Validation**:
   - Input: MassOrder IDs
   - Process: Call external validation API
   - Output: ValidationResultModel per order
   - Status Update: Pending → Validated or Failed

3. **Mass Order Submission**:
   - Input: Validated MassOrder IDs
   - Process:
     - Convert MassOrder to nopCommerce Order
     - Trigger OrderSubmit plugin
     - Update MassOrder.Status and SONumber
   - Output: Sales order numbers from ERP
   - Status Update: Validated → Submitted or Failed

### Use Cases

1. **Store Manager Creates Mass Order**:
   - Scenario: Manager wants to order same items for 10 stores
   - Input: Select 10 saved carts (one per store)
   - Output: 10 MassOrder records
   - Result: Batch order entry initiated

2. **Validate Before Submission**:
   - Scenario: Ensure inventory available before submitting
   - Input: MassOrder IDs
   - Process: API checks inventory, pricing, credit limit
   - Output: Validation results
   - Result: Only valid orders proceed

3. **Submit Mass Orders**:
   - Scenario: Submit validated orders to ERP
   - Input: Validated MassOrder IDs
   - Process: Create nopCommerce orders → Submit to ERP
   - Output: Sales order numbers
   - Result: Orders placed in ERP

4. **Handle Failures**:
   - Scenario: Some orders fail submission
   - Input: Failed MassOrder IDs
   - Process: Update Status=Failed, set error Message
   - Output: Error report for admin
   - Result: Admin can retry or fix issues

### Business Rules
- **Store-Specific Orders**: Each store's items become separate order
- **SKU Consolidation**: Multiple saved carts can contain same SKU (quantities summed)
- **Allocation Checking**: Validate against SKU allocations before submission
- **Payment Terms**: Inherit from store configuration or user profile
- **Shipping Methods**: Allow per-store shipping method selection
- **Order Prioritization**: Optional priority field for order processing sequence

---

## 6. WZ.Plugin.Widgets.OnePageCheckout

### Purpose
Custom one-page checkout flow with payment terms selection (Standard vs Early Pay), shipping calculation, and order submission.

### Functional Inputs

#### User Context
- **USER_MODEL**: User profile from session (from ExternalAuth.Office plugin)
  - `isEarlyPayAccess`: Eligibility for early payment terms
  - `canSubmitCart`: Permission to submit orders
  - `data_area_id`: Company identifier

- **CURRENT_STORE**: Active store from session
  - `PaymentTerms`: Standard payment terms for store
  - `WZNumber`: Store identifier

#### Cart Data
- **ShoppingCartItems**: Current cart items
- **ShippingAddress**: Customer shipping address
- **BillingAddress**: Customer billing address

#### Checkout Options
- **Payment Terms Selection**:
  - Standard Payment Terms (default)
  - Early Pay (Net5 with cash discount) - if eligible
- **Shipping Method**: Selected shipping option
- **Customer PO Number**: Optional customer reference

### Functional Outputs

#### OnePageCheckoutPluginModel
- **billingAddressModel**: Billing address form/data
- **shippingAddressModel**: Shipping address form/data
- **shippingMethodModel**: Available shipping methods with fees
- **paymentInfoModel**: Payment method details
- **confirmModel**: Order confirmation summary
- **isEarlyPay**: User eligibility for early pay
- **earlyPayTermsEnabled**: Whether cart contains early pay eligible items
- **earlyPayTermsAccessibilityNotes**: Error/info messages
- **canSubmitCart**: User permission to submit
- **stdPmtTerms**: Standard payment terms for store
- **inDeferralPeriod**: Whether early pay is in deferral period

#### OnePageCheckoutCompletedModel
- **OrderId**: nopCommerce order ID
- **CustomOrderNumber**: Formatted order number
- **OnePageCheckoutEnabled**: Boolean flag

### External Dependencies

#### Early Pay Service
- **IEarlyPayService**:
  - `GetEarlyPayVendors()`: Returns list of vendor IDs eligible for early pay
  - Purpose: Determine if cart contains early pay items

#### Configuration
- **Setting**: `PaymentTerms.InDeferralPeriod`
  - Purpose: Global flag to disable early pay during deferral periods
  - Scope: Per-store

#### Session Data
- **USER_MODEL**: User profile from authentication
- **CURRENT_STORE**: Active store context
- **COMPANY_ID**: Company identifier for multi-tenant support

### Checkout Flow

1. **Checkout Page Load**:
   - Input: Shopping cart
   - Process:
     - Load user profile from session
     - Check early pay eligibility
     - Load shipping methods
     - Pre-select payment method (Purchase Order)
   - Output: OnePageCheckoutPluginModel

2. **Billing Address**:
   - Input: Existing addresses or new address form
   - Validation: Address completeness, country/state
   - Output: Selected billing address

3. **Shipping Address**:
   - Input: Same as billing or different address
   - Validation: Address completeness, shipping availability
   - Output: Selected shipping address

4. **Shipping Method**:
   - Input: Shipping address
   - Process: Calculate shipping rates via IShippingService
   - Output: Shipping methods with fees

5. **Payment Terms**:
   - Input: User selection (Standard or Early Pay)
   - Validation:
     - User has early pay access
     - Cart contains early pay items
     - Not in deferral period
   - Output: Selected payment terms

6. **Order Confirmation**:
   - Input: All checkout data
   - Validation: Min order total, terms of service
   - Output: Order summary

7. **Order Placement**:
   - Input: Confirmed checkout data
   - Process:
     - Create nopCommerce order
     - Apply payment terms
     - Trigger order submit (to ERP)
   - Output: Order confirmation

### Early Pay Logic

**Eligibility Criteria** (ALL must be true):
- `userModel.isEarlyPayAccess == true` (user flag)
- Cart contains ≥1 item from early pay vendor
- `PaymentTerms.InDeferralPeriod == false` (not disabled globally)

**Payment Terms Mapping**:
- **Standard**: Store's default payment terms (e.g., "Default", "Net30")
- **Early Pay**: "Net5" with cash discount code applied

**Vendor Check**:
```
earlyPayVendors = GetEarlyPayVendors() // e.g., [123, 456, 789]
cartContainsEarlyPayItems = cart.Any(item =>
    earlyPayVendors.Contains(product.VendorId))
```

### Business Rules
- **Admin Vendor Assignment**: Admins configure which vendors are early pay eligible
- **Payment Method**: Auto-set to "Payments.PurchaseOrder"
- **Ship-to-Same-Address**: Allowed if cart requires shipping
- **Pickup In Store**: Supported if configured
- **Multi-Address Shipping**: Not supported (single shipping address)
- **Minimum Order Total**: Enforced before order placement

---

## 7. WZ.Plugin.Widgets.StoreDrop

### Purpose
Provides store dropdown selector in header and manages active store context with session tracking.

### Functional Inputs

#### User Context
- **USER_MODEL**: User profile from authentication
  - `WZListForRole`: Array of authorized store identifiers
  - `PrimaryWZNumber`: User's primary store

#### Store Selection
- **SelectedWZNumber**: Store identifier chosen by user
- **Purpose**: Sets active store context for session

### Functional Outputs

#### Store Dropdown Widget
- **Available Stores**: List of stores user can access
  - Store identifier (WZNumber)
  - Store name
  - Store location (optional)
- **Current Store**: Currently selected store
- **Widget Zone**: Header or navigation area

#### Session Data
- **CURRENT_STORE**: Active store object
  - `WZNumber`: Store identifier
  - `StoreName`: Display name
  - `PaymentTerms`: Payment terms for store
  - `CompanyId`: Company/tenant identifier
  - Additional store metadata

#### Database Integration
- **twzNOPSessions Table**: Tracks active sessions
  - `UserName`: User identifier
  - `SessionGUID`: Session unique ID
  - `WZNumber`: Active store
  - `CreatedOnUtc`: Session start time
  - `LastActivityUtc`: Last activity timestamp

### Configuration

#### StoreDropConfig
- **WZIntegrations**: Connection string for integration database
- **WZOlympus**: Connection string for user profile database
- **NopDB**: Connection string for nopCommerce database

#### Logging
- **IDebugLogger**: Custom debug logger interface
  - Purpose: Debug-level logging for troubleshooting
  - Methods: `Debug()`, `DebugAsync()`, `Error()`, `ErrorAsync()`

### Integration Points
- **Header Widget**: Rendered in main navigation
- **All Pages**: Store context available globally
- **Authentication Plugin**: Uses USER_MODEL from auth
- **Checkout Plugin**: Uses CURRENT_STORE for payment terms
- **Mass Order Plugin**: Filters orders by store context

### Use Cases

1. **User Logs In**:
   - Input: Authenticated user
   - Process: Load authorized stores from USER_MODEL
   - Output: Store dropdown with available stores
   - Default: Primary store selected

2. **User Switches Store**:
   - Input: User selects different store from dropdown
   - Process:
     - Update CURRENT_STORE in session
     - Update twzNOPSessions record
     - Reload page/context
   - Output: Store context changed
   - Result: Cart, pricing, inventory scoped to new store

3. **Session Tracking**:
   - Input: User activity
   - Process: Update LastActivityUtc in twzNOPSessions
   - Output: Session kept alive
   - Result: Session timeout prevention

4. **Multi-Store Manager**:
   - Scenario: User manages 20 stores
   - Input: WZListForRole = ["0001", "0002", ..., "0020"]
   - Output: Dropdown shows all 20 stores
   - Result: User can switch between stores easily

### Business Rules
- **Store Restriction**: User can ONLY access stores in WZListForRole
- **Primary Store Default**: Auto-select PrimaryWZNumber on login
- **Session Scoping**: All operations scoped to CURRENT_STORE
- **Admin Override**: Admin users ("ATI") can access all stores
- **Store Data Caching**: Store list cached in session (no DB hit per page)

---

## 8. WZ.Plugin.Widgets.StoreRma

### Purpose
Manages Return Merchandise Authorization (RMA) requests for store returns and exchanges.

### Functional Inputs

> **Note**: Detailed implementation not fully analyzed in legacy codebase. This section covers expected functionality based on plugin metadata and common RMA patterns.

#### RMA Request Inputs
- **OrderId**: Original order identifier
- **ProductId**: Product being returned
- **Quantity**: Quantity to return
- **ReasonCode**: Return reason (Defective, Wrong Item, Changed Mind, etc.)
- **Comments**: Customer/store comments
- **RequestedAction**: Refund, Exchange, Store Credit

#### Store Context
- **WZNumber**: Store identifier
- **CustomerId**: Customer requesting return
- **OrderDate**: Original order date (for return window validation)

### Functional Outputs

#### RMA Entity
- **RmaNumber**: Unique RMA identifier
- **Status**: Pending, Approved, Rejected, Completed
- **ReturnLabel**: Shipping label for return (if applicable)
- **RefundAmount**: Calculated refund amount
- **RestockingFee**: Fee deducted (if applicable)

#### RMA Tracking
- **Status Updates**: RMA lifecycle tracking
- **Notifications**: Email notifications to customer/store

### Business Rules (Expected)
- **Return Window**: 30/60/90 days from purchase
- **Restocking Fee**: Percentage-based fee for certain return reasons
- **Product Condition**: Require product to be unopened/unused (for certain categories)
- **Store-Specific Policy**: Different return policies per store
- **Admin Approval**: RMAs above certain value require admin approval

### Integration Points
- **Order Management**: Links to original order
- **Inventory Management**: Restock returned items
- **Accounting**: Process refunds/credits
- **Shipping**: Generate return labels

---

## 9. WZ.Plugin.Widgets.BuildInfo

### Purpose
Displays DevOps pipeline build information on Admin System Information page.

### Functional Inputs

#### Build Metadata (Set by CI/CD Pipeline)
- **BuildId**: Azure DevOps build ID
- **BuildNumber**: Semantic version or build number
- **BuildSummary**: Build description or commit message
- **BuildDate**: Timestamp of build

#### Runtime Information
- **DataSource**: Database server name
- **InitialCatalog**: Database name
- **HostName**: Application server hostname

#### Logging Configuration
- **LogLevelDebugEnabled**: Boolean flag
- **LogLevelInformationEnabled**: Boolean flag
- **LogLevelWarningEnabled**: Boolean flag
- **LogLevelErrorEnabled**: Boolean flag
- **LogLevelFatalEnabled**: Boolean flag

### Functional Outputs

#### Admin System Information Display
- **Build Information Section**:
  - Build ID
  - Build Number
  - Build Summary
  - Build Date

- **Connection Information Section**:
  - Database Server
  - Database Name
  - Application Server

- **Logging Configuration Section**:
  - Active log levels

### Configuration

#### plugin.json Structure
```json
{
  "BuildInfo": {
    "BuildId": "SET BY PIPELINE",
    "BuildNumber": "SET BY PIPELINE",
    "BuildSummary": "SET BY PIPELINE",
    "BuildDate": "SET BY PIPELINE"
  },
  "ConnectionInfo": {
    "DataSource": "SET AT RUNTIME",
    "InitialCatalog": "SET AT RUNTIME",
    "HostName": "SET AT RUNTIME"
  },
  "LoggingLevels": { ... }
}
```

### Integration Points
- **Admin System Information Page**: Widget zone on system info page
- **DevOps Pipeline**: Build script updates plugin.json
- **Application Startup**: Runtime info populated from configuration

### Use Cases

1. **DevOps Build**:
   - Input: Pipeline variables
   - Process: Update plugin.json during build
   - Output: Build metadata embedded in plugin
   - Result: Deployment traceability

2. **Admin Views System Info**:
   - Input: Admin navigates to System → System Information
   - Process: Read plugin.json build info
   - Output: Display build metadata
   - Result: Admin knows exact build deployed

3. **Troubleshooting**:
   - Scenario: Bug report from user
   - Input: Admin checks build info
   - Output: Build number and date
   - Result: Link issue to specific code version

### Business Rules
- **Read-Only Display**: Build info is display-only (not editable)
- **Pipeline Integration**: Automated update during CI/CD (no manual editing)
- **Version Tracking**: Enables version comparison across environments
- **Audit Trail**: Provides deployment history

---

## Cross-Plugin Dependencies

### Dependency Matrix

| Consumer Plugin | Depends On | Dependency Type | Data Flow |
|-----------------|------------|-----------------|-----------|
| SavedCart | ExternalAuth.Office | Session Data | USER_MODEL, CURRENT_STORE |
| MassOrder | SavedCart | Entity | SavedCart records |
| MassOrder | SKUAllocationByOwner | Validation | Allocation checks |
| MassOrder | OrderSubmit | Processing | Order submission |
| OnePageCheckout | ExternalAuth.Office | Session Data | USER_MODEL, CURRENT_STORE |
| OnePageCheckout | StoreDrop | Session Data | CURRENT_STORE |
| OrderSubmit | OnePageCheckout | Event | OrderPlacedEvent |
| SKUAllocationByOwner | StoreDrop | Context | WZNumber |
| StoreRma | Order Management | Entity | Order records |

### Shared Session Data

All plugins rely on session data established by **ExternalAuth.Office**:

```
Session["AD_USER"]       → Username
Session["AD_EMAIL"]      → Email
Session["USER_MODEL"]    → Full user profile
Session["CURRENT_STORE"] → Active store context
Session["COMPANY_ID"]    → Company identifier
Session["CUST_ID"]       → ERP customer ID
Session["STORE_LIST"]    → Authorized stores
Session["SESSION_GUID"]  → Session tracking
```

### Shared Configuration

All plugins use **StoreDropConfig** for database connections:

```
StoreDropConfig.WZOlympus      → User profile database
StoreDropConfig.WZIntegrations → Integration database
StoreDropConfig.NopDB          → nopCommerce database
```

---

## External System Integrations

### 1. Azure Active Directory
- **Plugin**: ExternalAuth.Office
- **Protocol**: OpenID Connect (OIDC)
- **Endpoints**:
  - Authorization: `https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize`
  - Token: `https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token`
  - Logout: `https://login.microsoftonline.com/{tenantId}/oauth2/logout`
- **Data Flow**: Azure AD → Claims → nopCommerce Customer

### 2. WZOlympus Database (SQL Server)
- **Plugin**: ExternalAuth.Office
- **Connection**: Direct SQL connection
- **Stored Procedures**:
  - `spwzGetUserExtendedProfile`: User profile lookup
- **Data Flow**: UserLoginName → User Profile → Session

### 3. WZIntegrations Database (SQL Server)
- **Plugins**: ExternalAuth.Office, StoreDrop
- **Tables**:
  - `twzNOPSessions`: Session tracking
- **Data Flow**: Login → INSERT session | Logout → DELETE session

### 4. VSOD System
- **Plugin**: SavedCart
- **Integration Method**: SQL Server Agent Job
- **Job Name**: `WZ VSOD Create Saved Lists`
- **Data Flow**: VSOD → SQL Job → SavedCart records

### 5. ERP API (Aeolus)
- **Plugin**: OrderSubmit
- **Protocol**: REST API (JSON)
- **Endpoint**: `https://devaeolusapi.wirelesszone.com/` (configurable)
- **Data Flow**: nopCommerce Order → SalesOrder JSON → ERP

### 6. Validation API
- **Plugin**: MassOrder
- **Purpose**: Pre-submission order validation
- **Checks**: Inventory, pricing, credit limit
- **Data Flow**: MassOrder → Validation API → ValidationResult

---

## Data Entities Summary

### Custom Entities to Migrate

| Entity | Plugin | Purpose | Key Fields |
|--------|--------|---------|------------|
| SkuAllocation | SKUAllocationByOwner | SKU allocation management | WZNumber, Sku, AllocationQuantity, FromDateTime, ToDateTime |
| SavedCart | SavedCart | Saved cart header | CustomerId, CartName, CreatedOnUtc |
| SavedCartItem | SavedCart | Saved cart line items | SavedCartId, ProductId, Quantity |
| VSOTransaction | SavedCart | VSOD import tracking | TransactionId, Status, ErrorMessage |
| MassOrder | MassOrder | Bulk order entry | WZNumber, CartName, SKU, Quantity, Status |

### Session Data Entities

| Key | Type | Source Plugin | Purpose |
|-----|------|---------------|---------|
| AD_USER | string | ExternalAuth.Office | Username |
| AD_EMAIL | string | ExternalAuth.Office | Email address |
| USER_MODEL | ADUserModel | ExternalAuth.Office | Full user profile |
| CURRENT_STORE | CurrentStore | StoreDrop | Active store |
| COMPANY_ID | int | ExternalAuth.Office | Company ID |
| CUST_ID | string | ExternalAuth.Office | ERP customer ID |
| STORE_LIST | string | ExternalAuth.Office | Comma-separated stores |
| SESSION_GUID | Guid | ExternalAuth.Office | Session tracking ID |

---

## Security Considerations

### Authentication & Authorization
- **Azure AD Integration**: Secure OIDC authentication
- **Session Management**: HTTP session storage (consider encryption)
- **SQL Injection Risk**: String concatenation in `OfficeDataAccess.RemoveSessionFromTable`
- **Admin Privileges**: Auto-granted for "ATI" warehouse (hard-coded logic)

### Data Access
- **Direct SQL Queries**: Some plugins use raw SQL (vulnerable to SQL injection)
- **Stored Procedures**: Most DB access via stored procedures (safer)
- **Connection Strings**: Stored in configuration (ensure encryption)

### External APIs
- **ERP API**: Authentication mechanism not specified in code
- **Validation API**: Authentication mechanism not specified in code
- **API Endpoints**: Stored in settings (ensure HTTPS)

### Recommendations
1. **Parameterize All SQL**: Replace string concatenation with parameterized queries
2. **Encrypt Session Data**: Sensitive data in session should be encrypted
3. **API Authentication**: Use OAuth 2.0 or API keys for external APIs
4. **Role-Based Permissions**: Implement granular permissions vs. hard-coded admin checks
5. **Audit Logging**: Log all security-relevant events (login, order submission, etc.)

---

## Performance Considerations

### Database Performance
- **N+1 Query Problem**: Multiple plugins load user profile on every request
- **Session Table Growth**: `twzNOPSessions` table requires cleanup job
- **SKU Allocation Lookups**: Index on (WZNumber, Sku, FromDateTime, ToDateTime)

### Caching Opportunities
- **User Profile**: Cache USER_MODEL in distributed cache (vs. session)
- **Store List**: Cache authorized stores per user
- **Early Pay Vendors**: Cache vendor list (infrequently changed)
- **SKU Allocations**: Cache active allocations per store

### External API Calls
- **VSOD Import**: Synchronous SQL Job call (consider async)
- **ERP Submission**: Synchronous REST call (consider async + queue)
- **Validation API**: Synchronous validation (consider batch validation)

### Recommendations
1. **Distributed Cache**: Use Redis for shared session data
2. **Background Jobs**: Use Hangfire for async order submission
3. **API Rate Limiting**: Implement retry logic with exponential backoff
4. **Database Indexing**: Add indexes on commonly queried fields
5. **Query Optimization**: Use projection (select only needed fields)

---

## Functional Summary

### Core Business Flows

#### 1. User Authentication & Authorization
**ExternalAuth.Office + StoreDrop**

1. User authenticates with Azure AD (Office 365)
2. Claims received from Azure AD
3. User profile loaded from WZOlympus database
4. Session data established (stores, permissions, company)
5. User selects active store via StoreDrop
6. Store context applied to all operations

#### 2. Product Allocation Management
**SKUAllocationByOwner**

1. Admin creates SKU allocations per store
2. Customer views product (allocation quantity displayed)
3. Customer adds to cart (allocation decremented)
4. Customer removes from cart (allocation restored)
5. Allocation expires (no longer enforced)

#### 3. Saved Cart Management
**SavedCart + MassOrder**

1. Customer saves current cart with name
2. VSOD import creates additional saved carts
3. Customer manages saved carts (load, delete)
4. Store manager selects multiple saved carts
5. Mass order created from selected carts
6. Mass order validated (inventory, allocations)
7. Mass order submitted as individual nop orders

#### 4. Checkout & Order Submission
**OnePageCheckout + OrderSubmit**

1. Customer proceeds to checkout
2. Billing/shipping addresses entered
3. Shipping method selected
4. Payment terms selected (Standard or Early Pay)
5. Order confirmed
6. nopCommerce order created
7. Order submitted to ERP via API
8. ERP sales order number returned
9. nopCommerce order marked complete

#### 5. RMA Processing
**StoreRma**

1. Customer/store initiates RMA request
2. RMA validated (return window, condition)
3. RMA approved by admin (if required)
4. Return label generated
5. Customer ships product back
6. Store receives return
7. Refund/exchange processed
8. Inventory restocked

---

## Migration Priorities

### High Priority (Core Functionality)
1. **ExternalAuth.Office**: Authentication is foundational
2. **StoreDrop**: Store context required by all other plugins
3. **SKUAllocationByOwner**: Core inventory management
4. **SavedCart**: Saved cart functionality used by MassOrder
5. **OnePageCheckout**: Primary checkout flow

### Medium Priority (Business Operations)
6. **OrderSubmit**: ERP integration (can be manual interim)
7. **MassOrder**: Bulk ordering efficiency

### Low Priority (Auxiliary Features)
8. **StoreRma**: RMA can be handled via standard nop RMA interim
9. **BuildInfo**: Deployment metadata (nice-to-have)

---

## Conclusion

The 9 legacy plugins provide a comprehensive business system for:
- **Authentication**: Azure AD integration with external user profile system
- **Multi-Store Management**: Store selection and context scoping
- **Inventory Control**: SKU allocation management
- **Order Management**: Saved carts, bulk ordering, custom checkout
- **ERP Integration**: Order submission to external system
- **Customer Service**: RMA processing
- **DevOps**: Build tracking

Consolidating these plugins into **Nop.Plugin.Misc.RoundRoom** will:
- Reduce plugin count (9 → 1)
- Eliminate cross-plugin dependencies
- Simplify deployment and maintenance
- Improve performance through shared services
- Enable event-driven architecture
- Modernize code to .NET 9 standards

The next documents will detail the consolidation strategy, event architecture, entity models, and implementation roadmap.

---

**Document Status**: ✅ Complete
**Next Document**: `roundroom-consolidation-strategy.md`
