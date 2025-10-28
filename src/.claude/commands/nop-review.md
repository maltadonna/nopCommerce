---
name: /nop-review
description: Perform pre-release code review and quality assurance on nopCommerce plugin
---

# Review nopCommerce Plugin

You are performing a quality assurance review of a nopCommerce plugin. This is ALWAYS a **SIMPLE TASK** that delegates directly to the QA specialist.

## Action Required

**ALWAYS delegate directly to nopcommerce-qa-specialist** (no mission-commander needed).

## Information to Gather from User

Ask the user for:

1. **Plugin Information**:
   - Plugin name (Nop.Plugin.{Group}.{Name})
   - Plugin version being reviewed
   - Target nopCommerce version (e.g., 4.90)
   - Plugin type (Payment, Widget, Misc, etc.)

2. **Review Scope**:
   - Full pre-release review (comprehensive - recommended)
   - Security audit only
   - Performance review only
   - Compliance check only

3. **Context**:
   - Is this for initial release or update?
   - Any known issues or concerns?
   - Deadline for review?

## Delegation Command

Use the Task tool to delegate to nopcommerce-qa-specialist:

```
Perform a comprehensive quality assurance review of the following nopCommerce plugin:

**Plugin Information**:
- Name: Nop.Plugin.{Group}.{Name}
- Version: [e.g., 1.0.0]
- Target nopCommerce: [e.g., 4.90]
- Plugin Type: [Payment / Widget / Shipping / Misc / etc.]

**Review Scope**:
[Full Pre-Release Review / Security Audit / Performance Review / Compliance Check]

**Context**:
- Release Type: [Initial Release / Update from v{version}]
- Known Concerns: [Any specific areas of concern]
- Deadline: [When review needed]

**Review Categories**:
1. nopCommerce Compliance (plugin.json, structure, interfaces)
2. Code Quality (XML docs, naming, standards)
3. Security (input validation, XSS, credentials, authorization)
4. Performance (queries, caching, async operations)
5. Testing (unit tests, integration tests, coverage)
6. Build & Deployment (compilation, dependencies, output)
7. Multi-Store Support (settings, store scope)
8. User Experience (admin UI, error messages)
9. Documentation (comments, README, CHANGELOG)

**Deliverables**:
1. Comprehensive QA report with issue list
2. Issues categorized by severity (Critical / High / Medium / Low)
3. Fix recommendations for all issues
4. Release readiness verdict (Ready / Conditional / Not Ready)
5. Positive findings (what was done well)
6. Next steps for release

**Quality Standards**:
- Zero Critical issues for release
- Zero compiler warnings
- All tests passing
- Security vulnerabilities: None
- nopCommerce compliance: 100%

Provide detailed, actionable recommendations for any issues found.
```

## Review Types

### 1. Full Pre-Release Review (Recommended)
**Scope**: All 9 categories
**When**: Before releasing plugin to production or marketplace
**Output**: Comprehensive report with all issues

### 2. Security Audit
**Scope**: Category 3 only (Security)
**When**: Handling sensitive data or integrating payment gateways
**Output**: Security-focused report with vulnerabilities

### 3. Performance Review
**Scope**: Category 4 only (Performance)
**When**: Plugin is slow or handles large data sets
**Output**: Performance-focused report with optimization recommendations

### 4. Compliance Check
**Scope**: Category 1 only (nopCommerce Compliance)
**When**: Plugin not loading or installation issues
**Output**: Compliance-focused report

## Expected Outcome

nopcommerce-qa-specialist will:
1. Review plugin code against all applicable categories
2. Document all issues with severity levels
3. Provide fix recommendations
4. Deliver release readiness verdict
5. Highlight what was done well

## Example Usage

### Example 1: Full Pre-Release Review
```
User: "Review my PayPal plugin before I release it"

You:
Plugin: Nop.Plugin.Payments.PayPalCommerce
Version: 1.0.0
Target: 4.90
Scope: Full Pre-Release Review

[Delegate to nopcommerce-qa-specialist with full context]
```

### Example 2: Security Audit
```
User: "Audit my payment plugin for security issues"

You:
Plugin: Nop.Plugin.Payments.Stripe
Scope: Security Audit Only

[Delegate to nopcommerce-qa-specialist focusing on security category]
```

### Example 3: Performance Review
```
User: "My catalog plugin is slow, can you review performance?"

You:
Plugin: Nop.Plugin.Misc.ProductCatalog
Scope: Performance Review Only

[Delegate to nopcommerce-qa-specialist focusing on performance category]
```

---

**Remember**: QA reviews should happen BEFORE releasing plugins. Catching issues early prevents production bugs and security vulnerabilities.

## Release Readiness Criteria

Plugin is **READY FOR RELEASE** when:
- ✅ Zero Critical issues
- ✅ Zero compiler warnings
- ✅ All tests passing
- ✅ Security audit passed
- ✅ nopCommerce compliance 100%

Plugin is **CONDITIONALLY READY** when:
- ⚠️ High priority issues exist but can be fixed quickly
- ⚠️ Medium/Low priority issues exist (fix in next version)

Plugin is **NOT READY** when:
- ❌ Critical security vulnerabilities exist
- ❌ Does not meet nopCommerce compliance
- ❌ Build fails or has errors
- ❌ Multiple High priority issues exist
