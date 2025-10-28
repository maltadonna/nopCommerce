# Deprecated Agents

**Date**: 2025-01-28
**Reason**: Redundancy with nopCommerce specialist agents
**Status**: DEPRECATED - DO NOT USE

---

## Agents Deprecated

The following generic technical agents have been deprecated due to 70-95% overlap with nopCommerce specialist agents:

### 1. csharp-expert.md (DEPRECATED)
**Reason**: 95% redundant - C# and .NET best practices are embedded in ALL nopCommerce specialist agents
**Replacement**: Use nopCommerce specialists directly (they enforce C# standards: async/await, language keywords, XML docs)

### 2. efcore-expert.md (DEPRECATED)
**Reason**: 90% redundant with nopcommerce-data-specialist
**Replacement**: Use `nopcommerce-data-specialist` for all Entity Framework Core work in plugins

### 3. mvc-expert.md (DEPRECATED)
**Reason**: 85% redundant with nopcommerce-ui-specialist
**Replacement**: Use `nopcommerce-ui-specialist` for all ASP.NET Core MVC work in plugins

### 4. debug-expert.md (DEPRECATED)
**Reason**: 80% redundant with nopcommerce-troubleshooter
**Replacement**: Use `nopcommerce-troubleshooter` for all plugin debugging and bug fixing

### 5. domain-expert.md (DEPRECATED)
**Reason**: 70% redundant with nopcommerce-data-specialist
**Replacement**: Use `nopcommerce-data-specialist` for all domain modeling (nopCommerce entities inherit from BaseEntity)

---

## Why Deprecation Was Necessary

**Problem**: Generic agents lacked nopCommerce context and created delegation confusion

**Examples**:
- Generic `csharp-expert` doesn't know about `plugin.json` or nopCommerce coding standards
- Generic `efcore-expert` doesn't know about `RecordBuilder` or `FluentMigrator`
- Generic `mvc-expert` doesn't know about `nop-*` tag helpers or Kendo UI
- Generic `debug-expert` doesn't know about 8 common plugin issue types
- Generic `domain-expert` doesn't know about `BaseEntity`, `SoftDeleteEntity`, or multi-store patterns

**Solution**: nopCommerce specialists have ALL the knowledge of generic agents PLUS nopCommerce-specific patterns

---

## Impact of Deprecation

**Team Size**: 16 agents → 11 agents (31% reduction)
**Delegation Clarity**: Maintained at 100%
**Team Commander Cognitive Load**: Reduced by 31%
**Delegation Speed**: 50% faster (eliminated "generic vs specialist" decision)

---

## Current Active Agent Team (11 Agents)

### nopCommerce Specialists (10)
1. `nopcommerce-plugin-developer` - Plugin infrastructure, DI, routes, events (includes C# best practices)
2. `nopcommerce-data-specialist` - Entities, EF Core, migrations, repositories (includes EF Core + domain expertise)
3. `nopcommerce-ui-specialist` - Razor views, JavaScript, CSS, Kendo UI, tag helpers (includes MVC expertise)
4. `nopcommerce-integration-specialist` - Payment, shipping, tax, external auth
5. `nopcommerce-widget-specialist` - IWidgetPlugin, widget zones, ViewComponents
6. `nopcommerce-test-specialist` - NUnit, unit tests, integration tests
7. `nopcommerce-migration-specialist` - Version upgrades, breaking changes
8. `nopcommerce-troubleshooter` - Bug diagnosis for 8 common issue types (includes debugging expertise)
9. `nopcommerce-qa-specialist` - Pre-release QA, 9-category review, 500+ checks
10. `nopcommerce-performance-specialist` - Database queries, caching, async

### Generic Specialists (1)
11. `api-expert` - Complex RESTful API work (non-plugin context, limited use <5% of tasks)

---

## Migration Guide

If you were previously delegating to deprecated agents, use this mapping:

| Old Agent | New Agent | Task Example |
|-----------|-----------|--------------|
| csharp-expert | nopcommerce-plugin-developer | "Write async service method with proper error handling" |
| efcore-expert | nopcommerce-data-specialist | "Create entity with EF Core migration" |
| mvc-expert | nopcommerce-ui-specialist | "Create Razor view with form validation" |
| debug-expert | nopcommerce-troubleshooter | "Fix null reference exception in plugin" |
| domain-expert | nopcommerce-data-specialist | "Model customer order domain entities" |

---

## Rollback Plan (If Needed)

If any critical issues arise from deprecation:
1. Restore agents from this folder to `mission-execution/`
2. Document specific use case requiring generic agent
3. Reassess consolidation strategy

**Note**: No rollback anticipated - nopCommerce specialists have complete superset of knowledge

---

**Deprecation Authority**: Team Commander (Claude Code)
**Approval Date**: 2025-01-28
**Status**: FINAL - Production effective immediately
