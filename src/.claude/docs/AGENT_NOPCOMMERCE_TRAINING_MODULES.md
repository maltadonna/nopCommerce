# nopCommerce Agent Training Modules
**Version**: 1.0
**Target**: nopCommerce 4.90 / .NET 9.0
**Purpose**: Comprehensive training modules to inject nopCommerce expertise into specialist agents

---

## Training Module 1: nopCommerce Core Architecture (ALL AGENTS)

### **nopCommerce Solution Structure**
```
nop4.90.0beta/src/
├── Libraries/                    # Core business logic
│   ├── Nop.Core/                # Domain entities, caching, events, interfaces
│   ├── Nop.Data/                # EF Core, repositories, migrations
│   └── Nop.Services/            # Business services (ICustomerService, etc.)
├── Presentation/                 # Web layer
│   ├── Nop.Web/                 # MVC app (public + admin)
│   └── Nop.Web.Framework/       # Web extensions, attributes, helpers
└── Plugins/                      # Plugin-based extensions
    └── Nop.Plugin.{Group}.{Name}/  # Individual plugins
```

### **Key nopCommerce Principles (NON-NEGOTIABLE)**
1. **Plugin-Based Architecture**: ALL customizations via plugins, NEVER modify core
2. **Service Layer Pattern**: Business logic in services (Nop.Services), injected via DI
3. **Repository Pattern**: Data access via IRepository&lt;TEntity&gt;, never direct DbContext
4. **Event-Driven**: Loosely coupled via IEventPublisher and event consumers
5. **Multi-Store/Multi-Tenant**: Store-specific config via IStoreContext and ISettingService
6. **Localization**: All user-facing text via ILocalizationService and resource strings
7. **Caching**: Performance via IStaticCacheManager (Memory/Redis/SQL providers)

### **nopCommerce Dependency Injection (Autofac)**
- Services registered in DependencyRegistrar classes
- Plugins register services via their own DependencyRegistrar
- Injection via constructor (NOT property injection)
- Scoped lifetime for most services

### **nopCommerce Async/Await Convention**
- ALL I/O operations must be async
- Method names end with "Async"
- NEVER use .Result or .Wait() (blocking)
- Return Task or Task&lt;T&gt;

---

## Training Module 2: nopCommerce Domain Model (domain-expert)

### **nopCommerce Entity Base Classes**

#### **BaseEntity**
```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
}
```
- All entities inherit from BaseEntity
- Id is the primary key convention

#### **SoftDeleteEntity**
```csharp
public abstract class SoftDeleteEntity : BaseEntity
{
    public bool Deleted { get; set; }
}
```
- Entities that support soft deletes (Customer, Product, etc.)
- Query filters automatically exclude Deleted = true

### **Core nopCommerce Entities**

#### **Customer Domain**
- **Customer**: User accounts (registered, guests, vendors)
- **CustomerRole**: Roles (Administrators, Registered, Guests, Vendors)
- **CustomerAttribute**: Custom attributes for customers
- **CustomerPassword**: Password history for security

#### **Catalog Domain**
- **Product**: Products with SKU, price, inventory
- **Category**: Product categories (hierarchical)
- **Manufacturer**: Product manufacturers
- **ProductAttribute**: Variant attributes (Size, Color)
- **SpecificationAttribute**: Product specifications

#### **Order Domain**
- **Order**: Customer orders with totals, status, shipping
- **OrderItem**: Line items in orders
- **ShoppingCartItem**: Cart items (active cart or wishlist)
- **ShipmentItem**: Shipped products

### **nopCommerce Entity Patterns**

#### **Multi-Store Support**
```csharp
// Entities that support multi-store use StoreMapping
public class StoreMapping : BaseEntity
{
    public int EntityId { get; set; }
    public string EntityName { get; set; }
    public int StoreId { get; set; }
}
```

#### **Localization Support**
```csharp
// Localizable entities use LocalizedProperty
public class LocalizedProperty : BaseEntity
{
    public int EntityId { get; set; }
    public string LocaleKeyGroup { get; set; }
    public string LocaleKey { get; set; }
    public string LocaleValue { get; set; }
    public int LanguageId { get; set; }
}
```

### **Domain Event Pattern**
```csharp
// Publish domain events after entity changes
await _eventPublisher.EntityInsertedAsync(customer);
await _eventPublisher.EntityUpdatedAsync(product);
await _eventPublisher.EntityDeletedAsync(order);
```

### **Validation Pattern**
- FluentValidation for model validation
- Validators in plugin Validators/ folder
- Validation triggered automatically in controllers

---

## Training Module 3: nopCommerce Data Access (efcore-expert)

### **nopCommerce EF Core Architecture**

#### **Repository Pattern (CRITICAL)**
```csharp
// ALWAYS use IRepository<TEntity>, NEVER DbContext directly
private readonly IRepository<Customer> _customerRepository;

// Query pattern
var customer = await _customerRepository.Table
    .FirstOrDefaultAsync(c => c.Email == email);

// Insert pattern
await _customerRepository.InsertAsync(customer);

// Update pattern
await _customerRepository.UpdateAsync(customer);

// Delete pattern (soft delete preferred)
customer.Deleted = true;
await _customerRepository.UpdateAsync(customer);
```

### **Entity Configuration Pattern**
```csharp
// Plugins create entity configs using IEntityTypeConfiguration
public class MyEntityBuilder : NopEntityBuilder<MyEntity>
{
    public override void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ToTable(nameof(MyEntity));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(400)
            .IsRequired();

        builder.Property(e => e.CreatedOnUtc)
            .IsRequired();
    }
}
```

### **Migration Pattern for Plugins**
```csharp
[NopMigration("2025/10/27 12:00:00", "Nop.Plugin.Misc.MyPlugin 1.0.0")]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<MyEntity>();
    }
}
```

### **Query Optimization Patterns**

#### **AsNoTracking for Read-Only**
```csharp
var products = await _productRepository.Table
    .AsNoTracking()
    .Where(p => !p.Deleted)
    .ToListAsync();
```

#### **Include for Eager Loading**
```csharp
var customer = await _customerRepository.Table
    .Include(c => c.CustomerRoles)
    .FirstOrDefaultAsync(c => c.Id == customerId);
```

#### **Select for Projection**
```csharp
var productNames = await _productRepository.Table
    .Where(p => !p.Deleted)
    .Select(p => new { p.Id, p.Name })
    .ToListAsync();
```

### **Caching Integration**
```csharp
// ALWAYS cache frequently accessed data
var customer = await _staticCacheManager.GetAsync(
    _staticCacheManager.PrepareKeyForDefaultCache(NopCustomerDefaults.CustomerByIdCacheKey, customerId),
    async () => await _customerRepository.GetByIdAsync(customerId)
);

// Clear cache on updates
await _staticCacheManager.RemoveAsync(NopCustomerDefaults.CustomerByIdCacheKey, customerId);
```

---

## Training Module 4: nopCommerce MVC Patterns (mvc-expert)

### **nopCommerce Controller Patterns**

#### **Admin Controller**
```csharp
[AuthorizeAdmin]  // MANDATORY for admin controllers
[Area(AreaNames.Admin)]
public class MyPluginController : BasePluginController
{
    private readonly IMyService _myService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;

    public MyPluginController(
        IMyService myService,
        ILocalizationService localizationService,
        INotificationService notificationService)
    {
        _myService = myService;
        _localizationService = localizationService;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Configure()
    {
        var model = await _myService.GetConfigurationModelAsync();
        return View("~/Plugins/Misc.MyPlugin/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        await _myService.SaveConfigurationAsync(model);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }
}
```

#### **Public Controller**
```csharp
public class MyPluginPublicController : BasePublicController
{
    // Public store functionality
}
```

### **Admin-LTE 3.2 View Patterns**

#### **Admin Configuration View**
```cshtml
@model ConfigurationModel

<form asp-controller="MyPlugin" asp-action="Configure" method="post">
    <div class="card card-default">
        <div class="card-header">
            @T("Plugins.Misc.MyPlugin.Configuration")
        </div>
        <div class="card-body">
            <div class="form-group row">
                <div class="col-md-3">
                    <nop-label asp-for="ApiKey" />
                </div>
                <div class="col-md-9">
                    <nop-editor asp-for="ApiKey" />
                    <span asp-validation-for="ApiKey"></span>
                </div>
            </div>
        </div>
        <div class="card-footer">
            <button type="submit" name="save" class="btn btn-primary">
                @T("Admin.Common.Save")
            </button>
        </div>
    </div>
</form>
```

### **Localization in Views**
```cshtml
@* Resource key translation *@
@T("Plugins.Misc.MyPlugin.Fields.ApiKey")

@* With parameters *@
@T("Plugins.Misc.MyPlugin.OrderTotal", Model.OrderTotal)
```

### **Ajax Form Pattern**
```cshtml
<form asp-controller="MyPlugin" asp-action="AjaxAction"
      data-ajax="true"
      data-ajax-method="POST"
      data-ajax-success="onSuccess">
    @* Form fields *@
</form>

<script>
function onSuccess(response) {
    if (response.success) {
        displaySuccessMessage(response.message);
    }
}
</script>
```

### **Widget/ViewComponent Pattern**
```csharp
// ViewComponent for widget rendering
[ViewComponent(Name = "MyPluginWidget")]
public class MyPluginWidgetViewComponent : NopViewComponent
{
    private readonly IMyService _myService;

    public MyPluginWidgetViewComponent(IMyService myService)
    {
        _myService = myService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var model = await _myService.GetWidgetModelAsync(widgetZone);
        return View("~/Plugins/Misc.MyPlugin/Views/Shared/Components/Widget/Default.cshtml", model);
    }
}
```

---

## Training Module 5: nopCommerce Service Layer (ALL AGENTS)

### **Common nopCommerce Services**

#### **ICustomerService**
- Customer CRUD operations
- Customer authentication
- Customer role management
- Customer attribute handling

#### **ISettingService**
- Plugin configuration storage
- Store-specific settings
- Setting CRUD operations

```csharp
// Save plugin settings
var settings = new MyPluginSettings
{
    ApiKey = model.ApiKey,
    Enabled = model.Enabled
};
await _settingService.SaveSettingAsync(settings);

// Load plugin settings
var settings = await _settingService.LoadSettingAsync<MyPluginSettings>();

// Delete plugin settings
await _settingService.DeleteSettingAsync<MyPluginSettings>();
```

#### **ILocalizationService**
- Add/update locale resources
- Get localized strings
- Manage translations

```csharp
// Add localization resources during plugin installation
await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
{
    ["Plugins.Misc.MyPlugin.Fields.ApiKey"] = "API Key",
    ["Plugins.Misc.MyPlugin.Fields.ApiKey.Hint"] = "Enter your API key"
});

// Delete resources during uninstallation
await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.MyPlugin");

// Get localized string
var message = await _localizationService.GetResourceAsync("Plugins.Misc.MyPlugin.Success");
```

#### **IStaticCacheManager**
- Cache data for performance
- Prepare cache keys
- Clear cache on updates

#### **ILogger**
- Error logging
- Information logging
- Debug logging

```csharp
await _logger.InformationAsync("MyPlugin action completed");
await _logger.WarningAsync("MyPlugin configuration issue");
await _logger.ErrorAsync("MyPlugin error occurred", exception);
```

#### **INotificationService**
- Success notifications in admin
- Error notifications
- Warning notifications

```csharp
_notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
_notificationService.ErrorNotification("An error occurred");
```

#### **IWorkContext**
- Current customer
- Current language
- Current currency
- Working language

#### **IStoreContext**
- Current store
- Multi-store operations

---

## Training Module 6: nopCommerce Plugin Development (ALL AGENTS)

### **Plugin Naming Convention (MANDATORY)**
```
Nop.Plugin.{Group}.{Name}
```

**Valid Groups**:
- **ExternalAuth**: Authentication providers (Facebook, Google)
- **Payments**: Payment gateways (PayPal, Stripe)
- **Shipping**: Shipping providers (UPS, FedEx)
- **Tax**: Tax calculation providers (Avalara, FixedOrByCountryStateZip)
- **Widgets**: UI widgets (GoogleAnalytics, FacebookPixel, Swiper)
- **Misc**: Miscellaneous plugins (WebAPI, Azure, RFQ)
- **Pickup**: Pickup point providers
- **DiscountRules**: Discount rule providers
- **Search**: Search providers (Lucene)
- **MultiFactorAuth**: MFA providers

### **plugin.json Structure (REQUIRED)**
```json
{
  "Group": "Misc",
  "FriendlyName": "My Plugin Name",
  "SystemName": "Nop.Plugin.Misc.MyPlugin",
  "Version": "1.0.0",
  "SupportedVersions": [ "4.90" ],
  "Author": "Your Name",
  "DisplayOrder": 1,
  "FileName": "Nop.Plugin.Misc.MyPlugin.dll",
  "Description": "Brief description of plugin functionality",
  "LimitedToStores": [],
  "LimitedToCustomerRoles": []
}
```

### **IPlugin Implementation (REQUIRED)**
```csharp
/// <summary>
/// Represents the MyPlugin plugin
/// </summary>
public class MyPlugin : BasePlugin, IMiscPlugin
{
    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;

    /// <summary>
    /// Ctor
    /// </summary>
    public MyPlugin(
        ILocalizationService localizationService,
        IWebHelper webHelper)
    {
        _localizationService = localizationService;
        _webHelper = webHelper;
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        // Add localization resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.MyPlugin.Fields.Setting1"] = "Setting 1"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        // Delete localization resources
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.MyPlugin");

        // Delete settings
        await _settingService.DeleteSettingAsync<MyPluginSettings>();

        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/MyPlugin/Configure";
    }
}
```

### **DependencyRegistrar (REQUIRED)**
```csharp
/// <summary>
/// Dependency registrar
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

---

## Training Module 7: nopCommerce Coding Standards (ALL AGENTS)

### **XML Documentation (MANDATORY)**
ALL public members MUST have XML documentation:
```csharp
/// <summary>
/// Gets a customer by email
/// </summary>
/// <param name="email">The email address</param>
/// <returns>
/// A task that represents the asynchronous operation
/// The task result contains the customer
/// </returns>
public async Task<Customer> GetCustomerByEmailAsync(string email)
{
    // Implementation
}
```

### **Language Keywords vs Type Names (MANDATORY)**
```csharp
// CORRECT - use language keywords
string name = "John";
int count = 10;
bool isActive = true;
decimal price = 99.99m;

// WRONG - don't use type names
String name = "John";      // NO!
Int32 count = 10;          // NO!
Boolean isActive = true;   // NO!
Decimal price = 99.99m;    // NO!
```

### **Async/Await Pattern (MANDATORY)**
```csharp
// CORRECT - async all the way
public async Task<Order> GetOrderAsync(int orderId)
{
    return await _orderRepository.GetByIdAsync(orderId);
}

// WRONG - blocking async call
public Order GetOrder(int orderId)
{
    return _orderRepository.GetByIdAsync(orderId).Result;  // NEVER DO THIS
}
```

### **Error Handling Pattern**
```csharp
private readonly ILogger _logger;

public async Task<bool> ProcessAsync(Order order)
{
    try
    {
        // Business logic
        await _orderService.ProcessOrderAsync(order);
        return true;
    }
    catch (Exception ex)
    {
        await _logger.ErrorAsync($"Error processing order {order.Id}", ex);
        throw; // Re-throw or handle appropriately
    }
}
```

### **Caching Pattern**
```csharp
public async Task<Customer> GetCustomerByIdAsync(int customerId)
{
    var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
        NopCustomerDefaults.CustomerByIdCacheKey,
        customerId);

    return await _staticCacheManager.GetAsync(cacheKey, async () =>
    {
        return await _customerRepository.GetByIdAsync(customerId);
    });
}
```

---

## Training Module 8: nopCommerce Widget Development (widget-specialist)

### **IWidgetPlugin Interface**
```csharp
public class MyWidgetPlugin : BasePlugin, IWidgetPlugin
{
    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the widget zones
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.HomepageTop,
            PublicWidgetZones.ProductDetailsTop,
            AdminWidgetZones.OrderDetailsBlock
        });
    }

    /// <summary>
    /// Gets a view component name for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component name</returns>
    public string GetWidgetViewComponentName(string widgetZone)
    {
        return "MyWidget";
    }
}
```

### **Common Widget Zones**

**Public Store**:
- `PublicWidgetZones.HomepageTop`
- `PublicWidgetZones.HomepageBottom`
- `PublicWidgetZones.HeaderBefore`
- `PublicWidgetZones.HeaderAfter`
- `PublicWidgetZones.ProductDetailsTop`
- `PublicWidgetZones.ProductDetailsBottom`
- `PublicWidgetZones.Footer`

**Admin**:
- `AdminWidgetZones.OrderDetailsBlock`
- `AdminWidgetZones.CustomerDetailsBlock`
- `AdminWidgetZones.ProductDetailsBlock`

### **ViewComponent Implementation**
```csharp
[ViewComponent(Name = "MyWidget")]
public class MyWidgetViewComponent : NopViewComponent
{
    private readonly IMyService _myService;
    private readonly MyWidgetSettings _settings;

    public MyWidgetViewComponent(
        IMyService myService,
        MyWidgetSettings settings)
    {
        _myService = myService;
        _settings = settings;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!_settings.Enabled)
            return Content("");

        var model = await _myService.PrepareWidgetModelAsync(widgetZone);

        return View("~/Plugins/Widgets.MyWidget/Views/PublicInfo.cshtml", model);
    }
}
```

---

## Training Module 9: nopCommerce Integration Development (integration-specialist)

### **Payment Gateway Integration (IPaymentMethod)**
```csharp
public class MyPaymentProcessor : BasePlugin, IPaymentMethod
{
    /// <summary>
    /// Process a payment
    /// </summary>
    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        var result = new ProcessPaymentResult();

        // Call payment gateway API
        var response = await _paymentApi.ChargeAsync(processPaymentRequest);

        if (response.Success)
        {
            result.NewPaymentStatus = PaymentStatus.Paid;
            result.CaptureTransactionId = response.TransactionId;
        }
        else
        {
            result.AddError(response.ErrorMessage);
        }

        return result;
    }

    /// <summary>
    /// Gets payment method type
    /// </summary>
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

    /// <summary>
    /// Gets a value indicating whether capture is supported
    /// </summary>
    public async Task<bool> SupportCaptureAsync()
    {
        return true;
    }

    /// <summary>
    /// Gets a value indicating whether refund is supported
    /// </summary>
    public async Task<bool> SupportRefundAsync()
    {
        return true;
    }
}
```

### **Shipping Provider Integration (IShippingRateComputationMethod)**
```csharp
public class MyShippingProvider : BasePlugin, IShippingRateComputationMethod
{
    /// <summary>
    /// Gets shipping rates
    /// </summary>
    public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
    {
        var response = new GetShippingOptionResponse();

        // Call shipping provider API
        var rates = await _shippingApi.GetRatesAsync(getShippingOptionRequest);

        foreach (var rate in rates)
        {
            response.ShippingOptions.Add(new ShippingOption
            {
                Name = rate.ServiceName,
                Rate = rate.Price,
                Description = rate.DeliveryDays
            });
        }

        return response;
    }
}
```

### **Tax Provider Integration (ITaxProvider)**
```csharp
public class MyTaxProvider : BasePlugin, ITaxProvider
{
    /// <summary>
    /// Gets tax rate
    /// </summary>
    public async Task<TaxRateResult> GetTaxRateAsync(TaxRateRequest taxRateRequest)
    {
        var result = new TaxRateResult();

        // Calculate or retrieve tax rate
        result.TaxRate = await _taxApi.GetTaxRateAsync(taxRateRequest);

        return result;
    }
}
```

---

## Training Module 10: nopCommerce Event System (ALL AGENTS)

### **Event Consumer Pattern**
```csharp
/// <summary>
/// Customer registered event consumer
/// </summary>
public class CustomerRegisteredEventConsumer : IConsumer<CustomerRegisteredEvent>
{
    private readonly IMyService _myService;
    private readonly ILogger _logger;

    public CustomerRegisteredEventConsumer(
        IMyService myService,
        ILogger logger)
    {
        _myService = myService;
        _logger = logger;
    }

    /// <summary>
    /// Handle the event
    /// </summary>
    public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
    {
        try
        {
            await _myService.OnCustomerRegisteredAsync(eventMessage.Customer);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Error handling customer registered event", ex);
        }
    }
}
```

### **Common Events**
- `EntityInsertedEvent<TEntity>`
- `EntityUpdatedEvent<TEntity>`
- `EntityDeletedEvent<TEntity>`
- `OrderPlacedEvent`
- `OrderPaidEvent`
- `OrderCancelledEvent`
- `CustomerRegisteredEvent`
- `CustomerLoggedInEvent`

---

## Quick Reference Checklists

### **Plugin Development Checklist**
- [ ] Plugin naming: `Nop.Plugin.{Group}.{Name}`
- [ ] plugin.json created with correct structure
- [ ] IPlugin interface implemented
- [ ] DependencyRegistrar created
- [ ] XML documentation on all public members
- [ ] Language keywords used (not type names)
- [ ] All I/O operations async with proper naming
- [ ] Error handling and logging implemented
- [ ] Localization resources added/removed
- [ ] Settings saved/loaded correctly
- [ ] InstallAsync/UninstallAsync tested
- [ ] Multi-store compatible (if applicable)
- [ ] No core file modifications

### **Code Quality Checklist**
- [ ] Zero compiler warnings
- [ ] XML documentation complete
- [ ] Async/await properly implemented
- [ ] Language keywords used
- [ ] Error handling comprehensive
- [ ] Logging implemented
- [ ] Caching where appropriate
- [ ] Input validation implemented
- [ ] XSS protection in views
- [ ] SQL injection prevention (use EF Core)

---

**End of Training Modules**

These modules should be integrated into each agent's knowledge base according to their domain expertise. Agents should reference these patterns when executing tasks related to nopCommerce plugin development.
