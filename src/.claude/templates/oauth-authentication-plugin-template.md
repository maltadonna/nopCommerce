# OAuth Authentication Plugin Template for nopCommerce 4.90

This template provides a complete implementation guide for creating an OAuth external authentication plugin in nopCommerce 4.90.

**Use this template for:** Facebook, Google, Microsoft, GitHub, Twitter, LinkedIn, or any OAuth 2.0 provider.

---

## Quick Start Checklist

- [ ] Replace all `{ProviderName}` placeholders with your provider name (e.g., "GitHub", "LinkedIn")
- [ ] Replace all `{providername}` (lowercase) with lowercase provider name
- [ ] Update OAuth endpoints (authorization, token, user info URLs)
- [ ] Configure OAuth scopes based on provider documentation
- [ ] Obtain Client ID and Client Secret from provider's developer portal
- [ ] Implement user info mapping (provider response → nopCommerce customer)
- [ ] Test OAuth flow in sandbox/development environment
- [ ] Add localization resources
- [ ] Create plugin icon (logo.jpg)

---

## Project Structure

```
Plugins/Nop.Plugin.ExternalAuth.{ProviderName}/
├── plugin.json
├── logo.jpg (100x100 px recommended)
├── Nop.Plugin.ExternalAuth.{ProviderName}.csproj
├── {ProviderName}AuthenticationPlugin.cs           (IExternalAuthenticationMethod)
├── {ProviderName}ExternalAuthSettings.cs           (Settings model)
├── Controllers/
│   └── {ProviderName}AuthenticationController.cs   (OAuth flow controller)
├── Infrastructure/
│   ├── {ProviderName}AuthenticationRegistrar.cs    (IExternalAuthenticationRegistrar)
│   └── DependencyRegistrar.cs                      (Service registration)
├── Services/
│   ├── I{ProviderName}AuthenticationService.cs     (Interface)
│   └── {ProviderName}AuthenticationService.cs      (OAuth logic)
├── Models/
│   ├── ConfigurationModel.cs                       (Admin settings model)
│   └── {ProviderName}UserInfoModel.cs              (Provider user info response)
├── Validators/
│   └── ConfigurationValidator.cs                   (FluentValidation)
├── Components/
│   └── {ProviderName}AuthenticationViewComponent.cs (Login button)
└── Views/
    ├── Configure.cshtml                            (Admin configuration page)
    └── Components/
        └── {ProviderName}Authentication/
            └── Default.cshtml                      (Public login button)
```

---

## Step 1: plugin.json

```json
{
  "Group": "ExternalAuth",
  "FriendlyName": "{ProviderName} External Authentication",
  "SystemName": "ExternalAuth.{ProviderName}",
  "Version": "1.0.0",
  "SupportedVersions": [ "4.90" ],
  "Author": "Your Name/Company",
  "DisplayOrder": 1,
  "FileName": "Nop.Plugin.ExternalAuth.{ProviderName}.dll",
  "Description": "This plugin allows customers to sign in using their {ProviderName} account."
}
```

---

## Step 2: Project File (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <OutputPath>..\..\Presentation\Nop.Web\Plugins\ExternalAuth.{ProviderName}</OutputPath>
    <OutDir>$(OutputPath)</OutDir>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Presentation\Nop.Web.Framework\Nop.Web.Framework.csproj" />
    <ClearPluginAssemblies Include="$(MSBuildProjectDirectory)\..\..\Build\src\ClearPluginAssemblies.proj" />
  </ItemGroup>

  <!-- Copy plugin.json and logo.jpg to output -->
  <ItemGroup>
    <None Update="plugin.json">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
    <None Update="logo.jpg">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
  </ItemGroup>

  <!-- Clean plugin assemblies after build -->
  <Target Name="NopTarget" AfterTargets="Build">
    <MSBuild Projects="@(ClearPluginAssemblies)" Properties="PluginPath=$(MSBuildProjectDirectory)\$(OutDir)" Targets="NopClear" />
  </Target>
</Project>
```

---

## Step 3: Settings Model

**File:** `{ProviderName}ExternalAuthSettings.cs`

```csharp
using Nop.Core.Configuration;

namespace Nop.Plugin.ExternalAuth.{ProviderName}
{
    /// <summary>
    /// Represents settings for {ProviderName} external authentication
    /// </summary>
    public class {ProviderName}ExternalAuthSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the OAuth Client ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the OAuth Client Secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable the plugin
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
```

---

## Step 4: Authentication Method Plugin

**File:** `{ProviderName}AuthenticationPlugin.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.ExternalAuth.{ProviderName}
{
    /// <summary>
    /// Represents the {ProviderName} external authentication method
    /// </summary>
    public class {ProviderName}AuthenticationPlugin : BasePlugin, IExternalAuthenticationMethod
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        /// <summary>
        /// Ctor
        /// </summary>
        public {ProviderName}AuthenticationPlugin(
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/{ProviderName}Authentication/Configure";
        }

        /// <summary>
        /// Gets a view component name for displaying plugin in public store
        /// </summary>
        public string GetPublicViewComponentName()
        {
            return "{ProviderName}Authentication";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            // Install default settings
            await _settingService.SaveSettingAsync(new {ProviderName}ExternalAuthSettings
            {
                Enabled = false,
                DisplayOrder = 1
            });

            // Install localization resources
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.ExternalAuth.{ProviderName}.Instructions"] = "Configure {ProviderName} OAuth authentication. You need to obtain a Client ID and Client Secret from {ProviderName} Developer Portal.",
                ["Plugins.ExternalAuth.{ProviderName}.ClientId"] = "Client ID",
                ["Plugins.ExternalAuth.{ProviderName}.ClientId.Hint"] = "Enter your {ProviderName} OAuth Client ID.",
                ["Plugins.ExternalAuth.{ProviderName}.ClientSecret"] = "Client Secret",
                ["Plugins.ExternalAuth.{ProviderName}.ClientSecret.Hint"] = "Enter your {ProviderName} OAuth Client Secret.",
                ["Plugins.ExternalAuth.{ProviderName}.Enabled"] = "Enabled",
                ["Plugins.ExternalAuth.{ProviderName}.Enabled.Hint"] = "Enable or disable {ProviderName} authentication.",
                ["Plugins.ExternalAuth.{ProviderName}.DisplayOrder"] = "Display Order",
                ["Plugins.ExternalAuth.{ProviderName}.DisplayOrder.Hint"] = "The display order for this authentication method.",
                ["Plugins.ExternalAuth.{ProviderName}.Login"] = "Sign in with {ProviderName}",
                ["Plugins.ExternalAuth.{ProviderName}.RedirectUri"] = "Redirect URI",
                ["Plugins.ExternalAuth.{ProviderName}.RedirectUri.Hint"] = "Use this redirect URI when configuring your {ProviderName} OAuth app: {0}"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            // Delete settings
            await _settingService.DeleteSettingAsync<{ProviderName}ExternalAuthSettings>();

            // Delete localization resources
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.ExternalAuth.{ProviderName}");

            await base.UninstallAsync();
        }
    }
}
```

---

## Step 5: External Authentication Registrar

**File:** `Infrastructure/{ProviderName}AuthenticationRegistrar.cs`

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication.External;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nop.Plugin.ExternalAuth.{ProviderName}.Infrastructure
{
    /// <summary>
    /// Represents registrar for {ProviderName} external authentication
    /// </summary>
    public class {ProviderName}AuthenticationRegistrar : IExternalAuthenticationRegistrar
    {
        /// <summary>
        /// Configure authentication
        /// </summary>
        /// <param name="builder">Authentication builder</param>
        public void Configure(AuthenticationBuilder builder)
        {
            builder.AddOAuth("{ProviderName}", "{ProviderName}", options =>
            {
                // Load settings dynamically at runtime
                var settings = EngineContext.Current.Resolve<{ProviderName}ExternalAuthSettings>();

                // Set client credentials
                options.ClientId = settings.ClientId;
                options.ClientSecret = settings.ClientSecret;

                // Set callback path (must match redirect URI in provider app settings)
                options.CallbackPath = "/signin-{providername}";

                // Configure OAuth endpoints (REPLACE WITH ACTUAL PROVIDER URLS)
                options.AuthorizationEndpoint = "https://provider.com/oauth/authorize";
                options.TokenEndpoint = "https://provider.com/oauth/token";
                options.UserInformationEndpoint = "https://api.provider.com/user";

                // Request OAuth scopes (adjust based on provider)
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                // Save tokens for later use (optional)
                options.SaveTokens = true;

                // Map user claims from provider response
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url"); // Provider-specific claims

                // Handle token creation (map provider user data to nopCommerce customer)
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        // User info already retrieved by ASP.NET Core OAuth handler
                        // Claims are automatically mapped via ClaimActions above
                        await Task.CompletedTask;
                    }
                };
            });
        }
    }
}
```

---

## Step 6: OAuth Service Implementation

**File:** `Services/I{ProviderName}AuthenticationService.cs`

```csharp
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.ExternalAuth.{ProviderName}.Services
{
    /// <summary>
    /// Interface for {ProviderName} authentication service
    /// </summary>
    public interface I{ProviderName}AuthenticationService
    {
        /// <summary>
        /// Get or create customer from external authentication data
        /// </summary>
        /// <param name="providerUserId">Provider user ID</param>
        /// <param name="email">Email address</param>
        /// <param name="name">Full name</param>
        /// <returns>Customer</returns>
        Task<Customer> GetOrCreateCustomerAsync(string providerUserId, string email, string name);
    }
}
```

**File:** `Services/{ProviderName}AuthenticationService.cs`

```csharp
using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;

namespace Nop.Plugin.ExternalAuth.{ProviderName}.Services
{
    /// <summary>
    /// Service for {ProviderName} authentication
    /// </summary>
    public class {ProviderName}AuthenticationService : I{ProviderName}AuthenticationService
    {
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Ctor
        /// </summary>
        public {ProviderName}AuthenticationService(
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILogger logger)
        {
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
        }

        /// <summary>
        /// Get or create customer from external authentication data
        /// </summary>
        public async Task<Customer> GetOrCreateCustomerAsync(string providerUserId, string email, string name)
        {
            // Try to find existing customer by external authentication record
            var customer = await FindCustomerByProviderUserIdAsync(providerUserId);

            if (customer != null)
            {
                await _logger.InformationAsync($"Existing customer found for {ProviderName} user {providerUserId}");
                return customer;
            }

            // Try to find existing customer by email
            if (!string.IsNullOrWhiteSpace(email))
            {
                customer = await _customerService.GetCustomerByEmailAsync(email);

                if (customer != null)
                {
                    // Link existing customer to {ProviderName} account
                    await LinkCustomerToProviderAsync(customer, providerUserId);
                    await _logger.InformationAsync($"Linked existing customer (email: {email}) to {ProviderName} user {providerUserId}");
                    return customer;
                }
            }

            // Create new customer
            customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = email,
                Username = email,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = 0 // Default store
            };

            // Parse name into first/last name
            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameParts = name.Split(' ', 2);
                customer.FirstName = nameParts[0];
                customer.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
            }

            await _customerService.InsertCustomerAsync(customer);

            // Link to {ProviderName} account
            await LinkCustomerToProviderAsync(customer, providerUserId);

            await _logger.InformationAsync($"Created new customer for {ProviderName} user {providerUserId}");

            return customer;
        }

        /// <summary>
        /// Find customer by {ProviderName} user ID
        /// </summary>
        private async Task<Customer> FindCustomerByProviderUserIdAsync(string providerUserId)
        {
            var attributeKey = $"{ProviderName}UserId";
            return await _customerService.GetCustomerByGenericAttributeAsync(attributeKey, providerUserId);
        }

        /// <summary>
        /// Link customer to {ProviderName} account
        /// </summary>
        private async Task LinkCustomerToProviderAsync(Customer customer, string providerUserId)
        {
            var attributeKey = $"{ProviderName}UserId";
            await _genericAttributeService.SaveAttributeAsync(customer, attributeKey, providerUserId);
        }
    }
}
```

---

## Step 7: OAuth Flow Controller

**File:** `Controllers/{ProviderName}AuthenticationController.cs`

```csharp
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.ExternalAuth.{ProviderName}.Models;
using Nop.Plugin.ExternalAuth.{ProviderName}.Services;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.ExternalAuth.{ProviderName}.Controllers
{
    /// <summary>
    /// Controller for {ProviderName} OAuth authentication
    /// </summary>
    public class {ProviderName}AuthenticationController : BasePluginController
    {
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly I{ProviderName}AuthenticationService _{providername}AuthenticationService;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly {ProviderName}ExternalAuthSettings _settings;

        /// <summary>
        /// Ctor
        /// </summary>
        public {ProviderName}AuthenticationController(
            IAuthenticationPluginManager authenticationPluginManager,
            IAuthenticationService authenticationService,
            I{ProviderName}AuthenticationService {providername}AuthenticationService,
            IExternalAuthenticationService externalAuthenticationService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            {ProviderName}ExternalAuthSettings settings)
        {
            _authenticationPluginManager = authenticationPluginManager;
            _authenticationService = authenticationService;
            _{providername}AuthenticationService = {providername}AuthenticationService;
            _externalAuthenticationService = externalAuthenticationService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _settings = settings;
        }

        /// <summary>
        /// Configure plugin
        /// </summary>
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {
            var model = new ConfigurationModel
            {
                ClientId = _settings.ClientId,
                ClientSecret = _settings.ClientSecret,
                Enabled = _settings.Enabled,
                DisplayOrder = _settings.DisplayOrder
            };

            // Generate redirect URI for display
            var storeLocation = (await _storeContext.GetCurrentStoreAsync()).Url;
            model.RedirectUri = $"{storeLocation}signin-{providername}";

            return View("~/Plugins/ExternalAuth.{ProviderName}/Views/Configure.cshtml", model);
        }

        /// <summary>
        /// Configure plugin (POST)
        /// </summary>
        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            // Save settings
            _settings.ClientId = model.ClientId;
            _settings.ClientSecret = model.ClientSecret;
            _settings.Enabled = model.Enabled;
            _settings.DisplayOrder = model.DisplayOrder;

            await _settingService.SaveSettingAsync(_settings);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        /// <summary>
        /// Initiate OAuth login
        /// </summary>
        public IActionResult Login(string returnUrl)
        {
            // Check if plugin is enabled
            if (!_settings.Enabled)
                return RedirectToRoute("Login");

            // Store return URL in authentication properties
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items = { { "returnUrl", returnUrl ?? "/" } }
            };

            // Challenge {ProviderName} authentication
            return Challenge(properties, "{ProviderName}");
        }

        /// <summary>
        /// OAuth callback (after user authorizes on {ProviderName})
        /// </summary>
        public async Task<IActionResult> Callback()
        {
            // Authenticate with {ProviderName}
            var authenticateResult = await HttpContext.AuthenticateAsync("{ProviderName}");

            if (!authenticateResult.Succeeded)
            {
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync("Plugins.ExternalAuth.{ProviderName}.AuthenticationFailed"));
                return RedirectToRoute("Login");
            }

            // Extract user claims
            var claims = authenticateResult.Principal.Claims.ToList();
            var providerUserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(providerUserId))
            {
                _notificationService.ErrorNotification("Failed to retrieve user ID from {ProviderName}");
                return RedirectToRoute("Login");
            }

            // Get or create customer
            var customer = await _{providername}AuthenticationService.GetOrCreateCustomerAsync(providerUserId, email, name);

            // Sign in customer
            await _authenticationService.SignInAsync(customer, isPersistent: true);

            // Get return URL
            var returnUrl = authenticateResult.Properties.Items.TryGetValue("returnUrl", out var url) ? url : "/";

            return Redirect(returnUrl);
        }
    }
}
```

---

## Step 8: Configuration Model & Validator

**File:** `Models/ConfigurationModel.cs`

```csharp
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.ExternalAuth.{ProviderName}.Models
{
    /// <summary>
    /// Configuration model for {ProviderName} authentication
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        /// <summary>
        /// Gets or sets the Client ID
        /// </summary>
        [NopResourceDisplayName("Plugins.ExternalAuth.{ProviderName}.ClientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Client Secret
        /// </summary>
        [NopResourceDisplayName("Plugins.ExternalAuth.{ProviderName}.ClientSecret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets whether the plugin is enabled
        /// </summary>
        [NopResourceDisplayName("Plugins.ExternalAuth.{ProviderName}.Enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        [NopResourceDisplayName("Plugins.ExternalAuth.{ProviderName}.DisplayOrder")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI (read-only, for display)
        /// </summary>
        [NopResourceDisplayName("Plugins.ExternalAuth.{ProviderName}.RedirectUri")]
        public string RedirectUri { get; set; }
    }
}
```

**File:** `Validators/ConfigurationValidator.cs`

```csharp
using FluentValidation;
using Nop.Plugin.ExternalAuth.{ProviderName}.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.ExternalAuth.{ProviderName}.Validators
{
    /// <summary>
    /// Validator for configuration model
    /// </summary>
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .When(x => x.Enabled)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.ExternalAuth.{ProviderName}.ClientId.Required"));

            RuleFor(x => x.ClientSecret)
                .NotEmpty()
                .When(x => x.Enabled)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.ExternalAuth.{ProviderName}.ClientSecret.Required"));
        }
    }
}
```

---

## Step 9: View Component (Login Button)

**File:** `Components/{ProviderName}AuthenticationViewComponent.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.ExternalAuth.{ProviderName}.Components
{
    /// <summary>
    /// View component for {ProviderName} login button
    /// </summary>
    public class {ProviderName}AuthenticationViewComponent : NopViewComponent
    {
        private readonly {ProviderName}ExternalAuthSettings _settings;

        /// <summary>
        /// Ctor
        /// </summary>
        public {ProviderName}AuthenticationViewComponent({ProviderName}ExternalAuthSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Invoke view component
        /// </summary>
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            // Don't display if plugin is disabled
            if (!_settings.Enabled)
                return Content(string.Empty);

            return View("~/Plugins/ExternalAuth.{ProviderName}/Views/Components/{ProviderName}Authentication/Default.cshtml");
        }
    }
}
```

---

## Step 10: Views

**File:** `Views/Configure.cshtml` (Admin Configuration Page)

```cshtml
@model ConfigurationModel
@{
    Layout = "_ConfigurePlugin";
}

<div class="content-header clearfix">
    <h1 class="float-left">
        @T("Plugins.ExternalAuth.{ProviderName}.Configure")
    </h1>
</div>

<section class="content">
    <div class="container-fluid">
        <form asp-controller="{ProviderName}Authentication" asp-action="Configure" method="post">
            <div class="card card-default">
                <div class="card-body">
                    <p>
                        @T("Plugins.ExternalAuth.{ProviderName}.Instructions")
                    </p>

                    <div class="form-group row">
                        <div class="col-md-3">
                            <nop-label asp-for="ClientId" />
                        </div>
                        <div class="col-md-9">
                            <nop-editor asp-for="ClientId" />
                            <span asp-validation-for="ClientId"></span>
                        </div>
                    </div>

                    <div class="form-group row">
                        <div class="col-md-3">
                            <nop-label asp-for="ClientSecret" />
                        </div>
                        <div class="col-md-9">
                            <nop-editor asp-for="ClientSecret" asp-template="Password" />
                            <span asp-validation-for="ClientSecret"></span>
                        </div>
                    </div>

                    <div class="form-group row">
                        <div class="col-md-3">
                            <nop-label asp-for="Enabled" />
                        </div>
                        <div class="col-md-9">
                            <nop-editor asp-for="Enabled" />
                            <span asp-validation-for="Enabled"></span>
                        </div>
                    </div>

                    <div class="form-group row">
                        <div class="col-md-3">
                            <nop-label asp-for="DisplayOrder" />
                        </div>
                        <div class="col-md-9">
                            <nop-editor asp-for="DisplayOrder" />
                            <span asp-validation-for="DisplayOrder"></span>
                        </div>
                    </div>

                    <div class="form-group row">
                        <div class="col-md-3">
                            <nop-label asp-for="RedirectUri" />
                        </div>
                        <div class="col-md-9">
                            <div class="form-text-row">
                                <code>@Model.RedirectUri</code>
                            </div>
                            <em>@T("Plugins.ExternalAuth.{ProviderName}.RedirectUri.Hint", Model.RedirectUri)</em>
                        </div>
                    </div>

                    <div class="form-group row">
                        <div class="col-md-9 offset-md-3">
                            <button type="submit" name="save" class="btn btn-primary">
                                @T("Admin.Common.Save")
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </form>
    </div>
</section>
```

**File:** `Views/Components/{ProviderName}Authentication/Default.cshtml` (Public Login Button)

```cshtml
@{
    var returnUrl = Context.Request.Query["returnUrl"].ToString();
}

<div class="external-authentication-button">
    <a href="@Url.Action("Login", "{ProviderName}Authentication", new { returnUrl })" class="btn btn-block btn-{providername}">
        <i class="fab fa-{providername}"></i>
        @T("Plugins.ExternalAuth.{ProviderName}.Login")
    </a>
</div>
```

---

## Step 11: Dependency Registration

**File:** `Infrastructure/DependencyRegistrar.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.ExternalAuth.{ProviderName}.Services;

namespace Nop.Plugin.ExternalAuth.{ProviderName}.Infrastructure
{
    /// <summary>
    /// Dependency registrar for {ProviderName} authentication
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<I{ProviderName}AuthenticationService, {ProviderName}AuthenticationService>();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 1;
    }
}
```

---

## Provider-Specific Configuration Examples

### GitHub OAuth

```csharp
options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
options.TokenEndpoint = "https://github.com/login/oauth/access_token";
options.UserInformationEndpoint = "https://api.github.com/user";

options.Scope.Add("user:email");
options.Scope.Add("read:user");
```

**Developer Portal:** https://github.com/settings/developers

---

### Google OAuth

```csharp
options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
options.TokenEndpoint = "https://oauth2.googleapis.com/token";
options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

options.Scope.Add("openid");
options.Scope.Add("profile");
options.Scope.Add("email");
```

**Developer Portal:** https://console.cloud.google.com/

---

### Microsoft OAuth

```csharp
options.AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
options.TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
options.UserInformationEndpoint = "https://graph.microsoft.com/v1.0/me";

options.Scope.Add("openid");
options.Scope.Add("profile");
options.Scope.Add("email");
```

**Developer Portal:** https://portal.azure.com/

---

### LinkedIn OAuth

```csharp
options.AuthorizationEndpoint = "https://www.linkedin.com/oauth/v2/authorization";
options.TokenEndpoint = "https://www.linkedin.com/oauth/v2/accessToken";
options.UserInformationEndpoint = "https://api.linkedin.com/v2/me";

options.Scope.Add("r_liteprofile");
options.Scope.Add("r_emailaddress");
```

**Developer Portal:** https://www.linkedin.com/developers/apps

---

## Testing Checklist

- [ ] **Obtain OAuth credentials**
  - Register OAuth app in provider's developer portal
  - Copy Client ID and Client Secret
  - Configure redirect URI: `https://yourstore.com/signin-{providername}`

- [ ] **Test OAuth flow**
  - Click "Sign in with {ProviderName}" button
  - Redirected to {ProviderName} authorization page
  - Authorize app
  - Redirected back to nopCommerce
  - Automatically signed in as customer

- [ ] **Test existing customer linking**
  - Sign in with existing email via OAuth
  - Verify customer account is linked (not duplicate created)

- [ ] **Test new customer creation**
  - Sign in with new email via OAuth
  - Verify new customer created
  - Verify first name and last name populated

- [ ] **Test configuration validation**
  - Enable plugin without Client ID → should show error
  - Enable plugin without Client Secret → should show error

- [ ] **Test multi-store**
  - Configure different Client ID/Secret per store
  - Verify correct credentials used per store

---

## Troubleshooting

### "Invalid redirect URI" error
- **Cause:** Redirect URI in provider app doesn't match nopCommerce callback URL
- **Fix:** Ensure redirect URI is exactly `https://yourstore.com/signin-{providername}`

### "Invalid client credentials" error
- **Cause:** Wrong Client ID or Client Secret
- **Fix:** Double-check credentials in provider's developer portal

### User info not mapping correctly
- **Cause:** Provider response format doesn't match claim mapping
- **Fix:** Inspect OAuth response JSON and update `ClaimActions.MapJsonKey()` paths

### Customer created without email
- **Cause:** Email scope not requested or email not provided by provider
- **Fix:** Add email scope and mark email as required in provider app settings

---

## Security Best Practices

- [ ] **Use HTTPS in production** (OAuth requires secure callback URLs)
- [ ] **Store Client Secret securely** (ISettingService encrypts automatically)
- [ ] **Validate state parameter** (ASP.NET Core OAuth handler does this automatically)
- [ ] **Verify callback origin** (use `options.Events.OnRemoteFailure` to log suspicious attempts)
- [ ] **Implement GDPR consent** (if storing additional provider data beyond email/name)
- [ ] **Log OAuth failures** (for security monitoring)

---

## Additional Resources

- **nopCommerce OAuth Documentation:** https://docs.nopcommerce.com/en/developer/plugins/how-to-write-plugin-4.70.html
- **ASP.NET Core OAuth:** https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/
- **OAuth 2.0 RFC:** https://datatracker.ietf.org/doc/html/rfc6749
- **OWASP OAuth Cheat Sheet:** https://cheatsheetseries.owasp.org/cheatsheets/OAuth2_Cheat_Sheet.html

---

**Template Version:** 1.0 (nopCommerce 4.90)
**Last Updated:** 2025-10-28
