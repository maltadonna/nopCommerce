---
name: /nop-upgrade
description: Upgrade nopCommerce plugin to newer nopCommerce version (handle breaking changes and API updates)
---

# Upgrade nopCommerce Plugin to Newer Version

You are upgrading a nopCommerce plugin to a newer nopCommerce version. Classify this request:

**This is a COMPLEX MISSION** because:
- Multiple files affected (all plugin files potentially)
- Breaking changes must be identified and fixed
- Multiple agents needed (nopcommerce-migration-specialist, nopcommerce-test-specialist)

## Action Required

**Immediately delegate to mission-commander** for blueprint creation.

## Information to Gather from User (via mission-commander)

Ask the user for:

1. **Version Information**:
   - Current nopCommerce version (e.g., 4.60)
   - Target nopCommerce version (e.g., 4.90)
   - Plugin name and location

2. **Plugin Scope**:
   - Single plugin or multiple plugins?
   - Plugin dependencies?
   - Custom third-party packages used?

3. **Requirements**:
   - Maintain backward compatibility?
   - Update to latest .NET version?
   - Update all packages?

## Delegation Command

Use the Task tool to delegate to mission-commander:

```
Create a comprehensive mission blueprint for upgrading a nopCommerce plugin:

**Version Migration:**
- Source Version: [e.g., nopCommerce 4.60 / .NET 7.0]
- Target Version: [e.g., nopCommerce 4.90 / .NET 9.0]

**Plugin Information:**
- Plugin: [PluginName]
- Path: [Path to plugin]
- Current plugin version: [version]

**Breaking Changes to Handle:**
[Research and list breaking changes between versions]

Common changes by version:
- 4.60 → 4.70: .NET 8.0 migration, EF Core 8.0
- 4.70 → 4.80: .NET 9.0 migration, EF Core 9.0
- 4.80 → 4.90: Latest nopCommerce 4.x, implicit usings

**Migration Tasks:**
1. Update .csproj TargetFramework
2. Update plugin.json SupportedVersions and Version
3. Update NuGet package references
4. Convert synchronous methods to async (if upgrading from pre-4.30)
5. Update DependencyRegistrar signature (if needed)
6. Update Route Provider signature (if needed)
7. Replace deprecated APIs
8. Update namespace imports
9. Update event consumers to async
10. Update Install/Uninstall methods
11. Update controller attributes
12. Update EF Core migrations (if needed)

**Quality Standards:**
- Zero compiler warnings
- All tests pass
- Plugin installs/uninstalls cleanly on new version
- No runtime errors
- Performance maintained or improved
- Coding standards compliance

**Deliverables:**
1. Updated .csproj file
2. Updated plugin.json
3. Updated NuGet packages
4. Migrated code (all async conversions, API updates)
5. Updated tests
6. CHANGELOG.md with migration notes
7. README.md with version compatibility notes

**Agent Assignment:**
- nopcommerce-migration-specialist: All code migration, breaking changes, API updates
- nopcommerce-test-specialist: Verify all tests pass, create migration tests

**Testing Requirements:**
- Plugin builds successfully
- Plugin installs on new version
- Plugin uninstalls cleanly
- All functionality works
- No deprecation warnings
- Performance acceptable

**Risk Assessment:**
- Third-party package compatibility
- Breaking changes impact
- Data migration requirements
- Backward compatibility concerns

Ensure the plugin works flawlessly on the target nopCommerce version.
```

## Expected Outcome

Mission-commander creates blueprint → Team Commander executes → Plugin successfully migrated to new version with all tests passing.
