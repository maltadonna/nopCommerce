---
name: /nop-new-plugin
description: Create a new nopCommerce plugin with complete structure and boilerplate code
---

# New nopCommerce Plugin Creation

You are creating a new nopCommerce plugin. Classify this request according to the Mission Command Framework in `.claude/CLAUDE.md`:

**This is a COMPLEX MISSION** because:
- Multiple files need to be created (>2 files)
- Architectural decisions required (plugin structure, dependencies)
- Multiple agents needed (nopcommerce-plugin-developer, nopcommerce-data-specialist if data access needed)

## Action Required

**Immediately delegate to mission-commander** for blueprint creation. Do NOT execute directly.

## Information to Gather from User (via mission-commander)

Ask the user for the following information:

1. **Plugin Details**:
   - Plugin Name (e.g., "PayPalCommerce", "CustomShipping")
   - Plugin Group (Payments, Shipping, Tax, Widgets, Misc, ExternalAuth)
   - Plugin Description
   - Author Name

2. **Functionality Requirements**:
   - What should the plugin do?
   - Does it need data access (database tables)?
   - Does it need a configuration page?
   - Does it integrate with external APIs?
   - Does it need to display UI in public store or admin?

3. **Technical Requirements**:
   - Which nopCommerce services does it use?
   - Does it need custom routes?
   - Does it need event consumers?
   - Does it need widgets?
   - Does it need scheduled tasks?

## Delegation Command

Use the Task tool to delegate to mission-commander:

```
Create a comprehensive mission blueprint for a new nopCommerce plugin with the following requirements:

**Plugin Information:**
- Name: [PluginName]
- Group: [Group] (Payments/Shipping/Tax/Widgets/Misc/ExternalAuth)
- Description: [Description]
- Author: [Author]

**Functionality:**
[Describe what the plugin should do]

**Technical Requirements:**
- Data access needed: [Yes/No]
- Configuration page needed: [Yes/No]
- External API integration: [Yes/No]
- Public store UI: [Yes/No]
- Admin UI customization: [Yes/No]

**Quality Standards:**
- nopCommerce 4.90 compliance
- .NET 9.0
- All coding standards enforced
- XML documentation required
- Zero compiler warnings
- Proper async/await usage

**Deliverables:**
1. Complete plugin structure (folders, files)
2. plugin.json with metadata
3. .csproj with proper configuration
4. IPlugin implementation with Install/Uninstall
5. DependencyRegistrar for service registration
6. Entity/data access layer (if needed)
7. Admin configuration page (if needed)
8. Localization resources
9. README.md with usage instructions

**Agent Assignment:**
- nopcommerce-plugin-developer: Core plugin structure and IPlugin implementation
- nopcommerce-data-specialist: Data access layer (if needed)
- nopcommerce-integration-specialist: External API integration (if needed)
- nopcommerce-widget-specialist: Widget implementation (if needed)
- nopcommerce-ui-specialist: Admin configuration views
- nopcommerce-test-specialist: Unit and integration tests

Ensure all nopCommerce 4.90 standards are met and the plugin is production-ready.
```

## Expected Outcome

Mission-commander will create a detailed blueprint, then Team Commander will execute it by delegating to specialized agents. The result will be a complete, working nopCommerce plugin ready for use.
