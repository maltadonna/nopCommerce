---
name: /nop-add-integration
description: Add third-party integration (payment/shipping/tax/external auth) to nopCommerce plugin
---

# Add Third-Party Integration to nopCommerce Plugin

You are adding a third-party integration (payment gateway, shipping provider, tax provider, or external authentication) to nopCommerce. Classify this request:

**This is a COMPLEX MISSION** because:
- Multiple integration points (API, webhooks, configuration)
- Security requirements (credential storage, encryption)
- Multiple agents needed (nopcommerce-integration-specialist, nopcommerce-ui-specialist, nopcommerce-test-specialist)

## Action Required

**Immediately delegate to mission-commander** for blueprint creation.

## Information to Gather from User (via mission-commander)

Ask the user for:

1. **Integration Type**:
   - Payment Gateway (IPaymentMethod)
   - Shipping Provider (IShippingRateComputationMethod)
   - Tax Provider (ITaxProvider)
   - External Authentication (IExternalAuthenticationMethod)
   - Other API integration

2. **Provider Details**:
   - Provider name (e.g., "Stripe", "UPS", "Avalara")
   - API documentation URL
   - Authentication method (API key, OAuth, Basic Auth)

3. **API Specifications**:
   - API base URL (production and sandbox)
   - Required API credentials
   - API endpoints to integrate
   - Webhook support?

4. **Functionality Requirements**:
   - Which operations are needed? (authorize, capture, refund, void for payments)
   - Configuration options for merchants
   - Multi-store support needed?
   - Multi-currency support needed?

## Delegation Command

Use the Task tool to delegate to mission-commander:

```
Create a comprehensive mission blueprint for integrating a third-party service with nopCommerce:

**Integration Type:**
[Payment Gateway / Shipping Provider / Tax Provider / External Auth]

**Provider Information:**
- Name: [ProviderName]
- Interface to implement: [IPaymentMethod / IShippingRateComputationMethod / ITaxProvider]
- API Documentation: [URL]

**API Specifications:**
- Authentication: [API Key / OAuth 2.0 / Basic Auth]
- Base URL (Production): [URL]
- Base URL (Sandbox): [URL]
- Endpoints to integrate:
  [List main endpoints]

**Functionality Requirements:**
[For Payment Gateway:]
- Supports: Authorize, Capture, Refund, Void, Recurring
- Payment flow: Standard / Redirect
- Supports partial refund: Yes/No

[For Shipping Provider:]
- Rate calculation
- Shipment tracking
- Label generation (if applicable)

[For Tax Provider:]
- Tax rate calculation
- Address validation

**Configuration Requirements:**
- API credentials (encrypted storage)
- Mode selector (sandbox/production)
- Additional settings: [list]

**Security Requirements:**
- PCI compliance (for payment gateways)
- Credential encryption
- Webhook signature verification
- HTTPS enforcement

**Deliverables:**
1. Interface implementation ({Provider}Processor.cs / {Provider}ComputationMethod.cs)
2. API service layer ({Provider}Service.cs)
3. Settings model ({Provider}Settings.cs)
4. Admin configuration controller
5. Admin configuration view
6. Webhook handler (if applicable)
7. Route provider for webhooks
8. Localization resources
9. Unit tests for service layer
10. Integration tests for API calls
11. Installation/uninstallation logic

**Agent Assignment:**
- nopcommerce-integration-specialist: Interface implementation, API service, webhook handling
- nopcommerce-ui-specialist: Admin configuration views
- nopcommerce-test-specialist: Unit and integration tests
- nopcommerce-plugin-developer: Plugin infrastructure, settings, localization

**Quality Standards:**
- Secure credential storage (encrypted)
- Comprehensive error handling
- Logging all API calls
- Rate limiting awareness
- Sandbox mode for testing
- Multi-store configuration support
- Zero compiler warnings

Ensure the integration follows nopCommerce 4.90 standards and industry best practices for security.
```

## Expected Outcome

Mission-commander creates blueprint → Team Commander executes → Complete third-party integration ready with security, error handling, and tests.
