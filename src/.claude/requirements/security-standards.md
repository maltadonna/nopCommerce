# Security Standards for nopCommerce Plugin Development

This document defines mandatory security requirements for all nopCommerce plugin development, with specialized checklists for payment processing, authentication, and data handling plugins.

---

## PCI-DSS Compliance Checklist for Payment Plugins

**CRITICAL**: All payment gateway plugins MUST comply with PCI-DSS (Payment Card Industry Data Security Standard) requirements. Failure to meet these standards is a **CRITICAL quality gate failure** that BLOCKS mission completion.

### PCI-DSS Requirement 1: Cardholder Data Protection

**Cardholder Data Environment (CDE) Rules:**

- [ ] **NEVER store full magnetic stripe data, CAV2/CVC2/CVV2/CID codes, or PIN data**
  - ❌ PROHIBITED: Storing CVV codes in database
  - ❌ PROHIBITED: Logging full credit card numbers
  - ❌ PROHIBITED: Storing track data from card swipes
  - ✅ ALLOWED: Storing masked PAN (e.g., "************1234") for display purposes only

- [ ] **Minimize cardholder data storage**
  - Store only what is necessary for business purposes
  - Implement data retention policies (auto-delete after X days)
  - Document justification for any stored cardholder data

- [ ] **Mask PAN (Primary Account Number) when displayed**
  - Display only first 6 and last 4 digits: "424242******4242"
  - Use nopCommerce's built-in card masking utilities
  - Never display full PAN in admin panel, logs, or emails

### PCI-DSS Requirement 2: Secure Transmission

- [ ] **Encrypt cardholder data during transmission**
  - ✅ Use HTTPS/TLS 1.2+ for all API calls
  - ✅ Verify SSL certificates in HttpClient
  - ❌ NEVER send card data via HTTP (unencrypted)
  - ❌ NEVER send card data in URL parameters or query strings

**Implementation Pattern:**
```csharp
// CORRECT - Secure HTTPS transmission
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, sslErrors) =>
    {
        // In production, validate certificate properly
        return sslErrors == SslPolicyErrors.None;
    }
};
var httpClient = new HttpClient(handler);

// WRONG - Insecure HTTP
var url = "http://payment-gateway.com/charge"; // ❌ NO HTTPS!
```

- [ ] **Use tokenization or point-to-point encryption (P2PE)**
  - Prefer hosted payment pages (redirect customer to gateway)
  - Use payment gateway's JavaScript SDK for client-side tokenization
  - Avoid handling raw card data on nopCommerce server

### PCI-DSS Requirement 3: Secure Credential Storage

- [ ] **Store API keys and secrets securely**
  - ✅ Use nopCommerce `ISettingService` (encrypted by default)
  - ✅ Use `[DataType(DataType.Password)]` attribute on settings
  - ❌ NEVER hardcode API keys in source code
  - ❌ NEVER commit credentials to version control

**Implementation Pattern:**
```csharp
/// <summary>
/// Payment gateway settings
/// </summary>
public class PaymentGatewaySettings : ISettings
{
    /// <summary>
    /// Gets or sets the API key (stored encrypted)
    /// </summary>
    [DataType(DataType.Password)]
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the API secret (stored encrypted)
    /// </summary>
    [DataType(DataType.Password)]
    public string ApiSecret { get; set; }
}
```

### PCI-DSS Requirement 4: Logging and Monitoring

- [ ] **NEVER log sensitive authentication data**
  - ❌ PROHIBITED: Full credit card numbers in logs
  - ❌ PROHIBITED: CVV codes in logs
  - ❌ PROHIBITED: API secrets/keys in logs
  - ✅ ALLOWED: Masked PAN for audit trail ("************1234")
  - ✅ ALLOWED: Transaction IDs, order IDs, customer IDs (non-sensitive)

**Implementation Pattern:**
```csharp
// CORRECT - Log masked card number
await _logger.InformationAsync($"Payment processed for card ending in {card.Last4Digits}");

// WRONG - Log full card number
await _logger.InformationAsync($"Payment processed for card {card.FullNumber}"); // ❌ PCI VIOLATION!
```

- [ ] **Implement audit logging for security events**
  - Log failed payment attempts
  - Log configuration changes (API key updates)
  - Log refund/void operations
  - Include timestamp, user ID, IP address (if available)

### PCI-DSS Requirement 5: Error Handling

- [ ] **Sanitize error messages sent to customers**
  - ❌ NEVER expose internal error details to customers
  - ❌ NEVER include card data in error messages
  - ✅ Show generic error: "Payment processing failed. Please try again."
  - ✅ Log detailed error internally for troubleshooting

**Implementation Pattern:**
```csharp
try
{
    var response = await _paymentGateway.ChargeAsync(request);
}
catch (Exception ex)
{
    // Log detailed error for developers
    await _logger.ErrorAsync($"Payment gateway error: {ex.Message}", ex);

    // Return generic error to customer
    result.AddError("Payment processing failed. Please contact support.");
}
```

### PCI-DSS Requirement 6: Access Control

- [ ] **Restrict access to cardholder data**
  - Use `[AuthorizeAdmin]` attribute on admin controllers
  - Verify user permissions before displaying payment data
  - Implement role-based access control (RBAC) for sensitive operations

- [ ] **Require strong authentication for payment operations**
  - Refunds/voids should require admin authentication
  - Configuration changes should require admin authentication
  - Consider two-factor authentication for high-value operations

### PCI-DSS Requirement 7: Secure Development Practices

- [ ] **Input validation on all payment fields**
  - Validate card number format (Luhn algorithm)
  - Validate expiration date (not expired, valid month/year)
  - Validate CVV length (3-4 digits)
  - Sanitize all inputs to prevent injection attacks

- [ ] **Use parameterized queries (EF Core LINQ only)**
  - ❌ NEVER use raw SQL with string concatenation
  - ✅ Use EF Core LINQ queries (parameterized by default)

### PCI-DSS Requirement 8: Testing and Validation

- [ ] **Test payment plugin in sandbox/test mode**
  - Verify no real cards are processed during development
  - Use payment gateway's test API keys
  - Use test card numbers (e.g., 4111111111111111 for Visa)

- [ ] **Verify PCI compliance before production deployment**
  - Run security scan on payment endpoints
  - Verify SSL/TLS configuration (TLS 1.2+ only)
  - Review logs for sensitive data leakage
  - Confirm no card data stored locally

### PCI-DSS Requirement 9: Vulnerability Management

- [ ] **Keep payment gateway SDK up to date**
  - Monitor for security updates from payment provider
  - Apply patches within 30 days of release
  - Document SDK version in plugin.json

- [ ] **Implement rate limiting on payment endpoints**
  - Prevent brute-force card testing attacks
  - Limit failed payment attempts per customer/IP
  - See Rate Limiting pattern in this document

---

## GDPR/Privacy Compliance Checklist

See `.claude/requirements/privacy-standards.md` for comprehensive GDPR requirements.

---

## General Security Requirements (All Plugins)

### Input Validation (MANDATORY)

- [ ] **Validate all user inputs**
  - Use Data Annotations (`[Required]`, `[StringLength]`, `[Range]`)
  - Use FluentValidation for complex validation logic
  - Sanitize inputs to prevent injection attacks

**Implementation Pattern:**
```csharp
public class ConfigurationModel
{
    [Required]
    [StringLength(100)]
    public string ApiKey { get; set; }

    [Required]
    [Url]
    public string WebhookUrl { get; set; }

    [Range(1, 100)]
    public int Timeout { get; set; }
}
```

### SQL Injection Prevention (MANDATORY)

- [ ] **NEVER use raw SQL with string concatenation**
  - ✅ Use EF Core LINQ queries (parameterized automatically)
  - ✅ Use stored procedures with parameters
  - ❌ NEVER concatenate user input into SQL strings

**Implementation Pattern:**
```csharp
// CORRECT - Parameterized query via LINQ
var customers = await _customerRepository.Table
    .Where(c => c.Email == userEmail) // Parameterized automatically
    .ToListAsync();

// WRONG - SQL injection vulnerability
var sql = $"SELECT * FROM Customer WHERE Email = '{userEmail}'"; // ❌ VULNERABLE!
```

### XSS (Cross-Site Scripting) Prevention (MANDATORY)

- [ ] **Encode output in Razor views**
  - Razor automatically encodes `@Model.Property`
  - Use `@Html.Raw()` only for trusted content
  - Use `[AllowHtml]` attribute sparingly

**Implementation Pattern:**
```cshtml
<!-- CORRECT - Razor automatically encodes -->
<p>@Model.UserName</p>

<!-- WRONG - Raw HTML (XSS risk) -->
<p>@Html.Raw(Model.UserInput)</p> <!-- ❌ Only if trusted! -->
```

- [ ] **Sanitize rich text inputs**
  - Use HTML sanitizer library (e.g., HtmlSanitizer)
  - Whitelist allowed HTML tags
  - Remove JavaScript event handlers

### CSRF (Cross-Site Request Forgery) Prevention (MANDATORY)

- [ ] **Use anti-forgery tokens on forms**
  - Add `@Html.AntiForgeryToken()` to forms
  - Add `[ValidateAntiForgeryToken]` attribute to POST actions

**Implementation Pattern:**
```cshtml
<!-- View -->
<form method="post">
    @Html.AntiForgeryToken()
    <!-- form fields -->
</form>
```

```csharp
// Controller
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Configure(ConfigurationModel model)
{
    // Process form
}
```

### Authentication & Authorization (MANDATORY)

- [ ] **Secure admin controllers**
  - Use `[AuthorizeAdmin]` attribute on all admin controllers
  - Verify user permissions for sensitive operations
  - Use `IPermissionService` for custom permissions

**Implementation Pattern:**
```csharp
[AuthorizeAdmin]
[Area(AreaNames.Admin)]
public class MyPluginController : BasePluginController
{
    // All actions require admin authentication
}
```

- [ ] **Validate API requests**
  - Require API keys for public-facing endpoints
  - Validate webhook signatures (HMAC-SHA256)
  - Implement IP whitelisting for sensitive endpoints

### Secrets Management (MANDATORY)

- [ ] **NEVER hardcode secrets in source code**
  - ❌ PROHIBITED: API keys in code
  - ❌ PROHIBITED: Passwords in code
  - ❌ PROHIBITED: Connection strings in code
  - ✅ REQUIRED: Use ISettingService for plugin settings
  - ✅ REQUIRED: Use appsettings.json for infrastructure settings

**Implementation Pattern:**
```csharp
// CORRECT - Load from settings
var settings = await _settingService.LoadSettingAsync<MyPluginSettings>();
var apiKey = settings.ApiKey;

// WRONG - Hardcoded secret
var apiKey = "sk_live_abc123"; // ❌ NEVER DO THIS!
```

### Error Handling & Information Disclosure (MANDATORY)

- [ ] **Sanitize error messages**
  - ❌ NEVER expose stack traces to end users
  - ❌ NEVER expose internal paths or database details
  - ✅ Log detailed errors internally (ILogger)
  - ✅ Show generic error messages to users

**Implementation Pattern:**
```csharp
try
{
    await _service.ProcessAsync(data);
}
catch (Exception ex)
{
    // Log detailed error for developers
    await _logger.ErrorAsync($"Processing failed: {ex.Message}", ex);

    // Show generic error to user
    _notificationService.ErrorNotification("An error occurred. Please try again.");
}
```

---

## Security Testing Requirements

### Penetration Testing (Recommended for Production Plugins)

- [ ] **SQL injection testing**
  - Test all input fields with SQL injection payloads
  - Use tools: SQLMap, Burp Suite

- [ ] **XSS testing**
  - Test all input fields with XSS payloads
  - Verify output encoding in all views

- [ ] **Authentication testing**
  - Verify admin-only endpoints require authentication
  - Test session management and logout

- [ ] **Authorization testing**
  - Verify users can only access their own data
  - Test privilege escalation scenarios

### Static Code Analysis (Recommended)

- [ ] **Run security linters**
  - Use: SonarQube, CodeQL, Roslyn analyzers
  - Address all security warnings

- [ ] **Dependency scanning**
  - Check for known vulnerabilities in NuGet packages
  - Use: OWASP Dependency-Check, Snyk

---

## Rate Limiting Pattern (API Protection)

### Implementation Pattern for API Endpoints

**Purpose:** Prevent brute-force attacks, DoS attacks, and API abuse.

```csharp
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Nop.Plugin.{Group}.{Name}.Infrastructure
{
    /// <summary>
    /// Rate limiting attribute for API endpoints
    /// </summary>
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimits = new();
        private readonly int _maxRequests;
        private readonly int _timeWindowSeconds;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="maxRequests">Maximum requests allowed</param>
        /// <param name="timeWindowSeconds">Time window in seconds</param>
        public RateLimitAttribute(int maxRequests = 100, int timeWindowSeconds = 60)
        {
            _maxRequests = maxRequests;
            _timeWindowSeconds = timeWindowSeconds;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get client identifier (IP address or API key)
            var clientId = GetClientIdentifier(context);

            var rateLimitInfo = _rateLimits.GetOrAdd(clientId, _ => new RateLimitInfo());

            lock (rateLimitInfo)
            {
                // Reset if time window expired
                if (DateTime.UtcNow > rateLimitInfo.WindowStart.AddSeconds(_timeWindowSeconds))
                {
                    rateLimitInfo.RequestCount = 0;
                    rateLimitInfo.WindowStart = DateTime.UtcNow;
                }

                rateLimitInfo.RequestCount++;

                // Check if rate limit exceeded
                if (rateLimitInfo.RequestCount > _maxRequests)
                {
                    context.Result = new StatusCodeResult(429); // Too Many Requests
                    context.HttpContext.Response.Headers["Retry-After"] = _timeWindowSeconds.ToString();
                    return;
                }
            }

            await next();
        }

        private string GetClientIdentifier(ActionExecutingContext context)
        {
            // Try to get API key from header
            if (context.HttpContext.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
                return $"apikey:{apiKey}";

            // Fall back to IP address
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"ip:{ipAddress}";
        }

        private class RateLimitInfo
        {
            public int RequestCount { get; set; }
            public DateTime WindowStart { get; set; } = DateTime.UtcNow;
        }
    }
}
```

**Usage:**
```csharp
[HttpPost]
[RateLimit(maxRequests: 10, timeWindowSeconds: 60)] // 10 requests per minute
public async Task<IActionResult> ProcessPayment(PaymentRequest request)
{
    // Payment processing logic
}
```

---

## Webhook Signature Verification Pattern (CRITICAL)

**Purpose:** Verify webhook requests are from legitimate sources, not attackers.

### HMAC-SHA256 Signature Verification

```csharp
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.{Group}.{Name}.Controllers
{
    /// <summary>
    /// Webhook controller with signature verification
    /// </summary>
    public class WebhookController : Controller
    {
        private readonly ILogger _logger;
        private readonly MyPluginSettings _settings;

        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                // Read request body
                string payload;
                using (var reader = new StreamReader(Request.Body))
                {
                    payload = await reader.ReadToEndAsync();
                }

                // Get signature from header
                var signature = Request.Headers["X-Signature"].FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                {
                    await _logger.WarningAsync("Webhook received without signature");
                    return Unauthorized("Missing signature");
                }

                // Verify signature
                if (!VerifySignature(payload, signature, _settings.WebhookSecret))
                {
                    await _logger.WarningAsync("Webhook signature verification failed");
                    return Unauthorized("Invalid signature");
                }

                // Process webhook (signature verified)
                await ProcessWebhookAsync(payload);

                return Ok();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Webhook processing error", ex);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Verify HMAC-SHA256 signature
        /// </summary>
        private bool VerifySignature(string payload, string signature, string secret)
        {
            var expectedSignature = ComputeHmacSha256(payload, secret);

            // Use constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signature),
                Encoding.UTF8.GetBytes(expectedSignature));
        }

        /// <summary>
        /// Compute HMAC-SHA256 hash
        /// </summary>
        private string ComputeHmacSha256(string payload, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(payloadBytes);

            return Convert.ToBase64String(hashBytes);
            // Or hex: return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private async Task ProcessWebhookAsync(string payload)
        {
            // Parse and process webhook data
        }
    }
}
```

---

## Log Scrubbing Pattern (Prevent Secrets in Logs)

### Automated Secret Detection

**Purpose:** Prevent accidental logging of sensitive data (API keys, passwords, card numbers).

```csharp
using System.Text.RegularExpressions;

namespace Nop.Plugin.{Group}.{Name}.Infrastructure
{
    /// <summary>
    /// Log scrubber to remove sensitive data from log messages
    /// </summary>
    public static class LogScrubber
    {
        private static readonly Regex CreditCardRegex = new(@"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b");
        private static readonly Regex ApiKeyRegex = new(@"\b(api[_-]?key|apikey|api[_-]?secret|apisecret)[\"']?\s*[:=]\s*[\"']?([a-zA-Z0-9_\-]{20,})[\"']?", RegexOptions.IgnoreCase);
        private static readonly Regex PasswordRegex = new(@"\b(password|pwd|passwd)[\"']?\s*[:=]\s*[\"']?([^\s\""']{4,})[\"']?", RegexOptions.IgnoreCase);
        private static readonly Regex EmailRegex = new(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b");

        /// <summary>
        /// Scrub sensitive data from log message
        /// </summary>
        public static string Scrub(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            // Mask credit card numbers
            message = CreditCardRegex.Replace(message, m => MaskCreditCard(m.Value));

            // Mask API keys
            message = ApiKeyRegex.Replace(message, m => $"{m.Groups[1].Value}=***REDACTED***");

            // Mask passwords
            message = PasswordRegex.Replace(message, m => $"{m.Groups[1].Value}=***REDACTED***");

            // Optionally mask email addresses (depends on privacy requirements)
            // message = EmailRegex.Replace(message, "***EMAIL***");

            return message;
        }

        private static string MaskCreditCard(string cardNumber)
        {
            var digits = Regex.Replace(cardNumber, @"[\s-]", "");
            if (digits.Length < 8)
                return "***CARD***";

            return $"{digits.Substring(0, 4)}********{digits.Substring(digits.Length - 4)}";
        }
    }
}
```

**Usage in Logger Wrapper:**
```csharp
public class SecureLogger : ILogger
{
    private readonly ILogger _innerLogger;

    public async Task InformationAsync(string message, Exception exception = null)
    {
        // Scrub before logging
        var scrubbedMessage = LogScrubber.Scrub(message);
        await _innerLogger.InformationAsync(scrubbedMessage, exception);
    }

    public async Task ErrorAsync(string message, Exception exception = null)
    {
        // Scrub before logging
        var scrubbedMessage = LogScrubber.Scrub(message);
        await _innerLogger.ErrorAsync(scrubbedMessage, exception);
    }
}
```

---

## Quality Gate Integration

**Critical Security Gates (BLOCK Mission Completion):**

1. ✅ **PCI Compliance** - Payment plugins must pass all PCI checklist items
2. ✅ **Webhook Signature Verification** - All webhooks must verify signatures
3. ✅ **SQL Injection Prevention** - No raw SQL with string concatenation
4. ✅ **XSS Prevention** - All output must be encoded
5. ✅ **Secrets Management** - No hardcoded credentials
6. ✅ **Log Scrubbing** - Automated scan for secrets in log statements

**Security Verification Commands:**
```bash
# Scan for hardcoded secrets
grep -r "api_key\s*=\s*['\"][a-zA-Z0-9]" --include="*.cs"

# Check for raw SQL
grep -r "SqlCommand\|ExecuteSqlRaw" --include="*.cs"

# Verify HTTPS enforcement
grep -r "http://" --include="*.cs" --include="*.json"
```

---

## References

- **PCI-DSS Official Documentation**: https://www.pcisecuritystandards.org/
- **OWASP Top 10**: https://owasp.org/www-project-top-ten/
- **GDPR Requirements**: See `.claude/requirements/privacy-standards.md`
- **nopCommerce Security Best Practices**: https://docs.nopcommerce.com/en/developer/tutorials/security.html

---

**Security is non-negotiable. These standards protect customers, merchants, and the reputation of nopCommerce plugins.**
