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

## Step 1: Gather Structured Information from User

Use the AskUserQuestion tool to collect plugin details with structured options:

**Question 1: Plugin Group**
```typescript
AskUserQuestion({
  questions: [{
    question: "Which plugin group should this plugin belong to?",
    header: "Plugin Group",
    multiSelect: false,
    options: [
      {
        label: "Payments",
        description: "Payment gateway integrations (PayPal, Stripe, Square, etc.)"
      },
      {
        label: "Shipping",
        description: "Shipping rate calculators (UPS, FedEx, custom rates)"
      },
      {
        label: "Tax",
        description: "Tax calculation providers (Avalara, manual rules)"
      },
      {
        label: "Widgets",
        description: "UI components and integrations (Analytics, Pixels, Sliders)"
      },
      {
        label: "Misc",
        description: "Miscellaneous plugins that don't fit other categories"
      },
      {
        label: "ExternalAuth",
        description: "External authentication providers (Google, Facebook, Microsoft)"
      },
      {
        label: "Pickup",
        description: "Pickup point providers"
      },
      {
        label: "MultiFactorAuth",
        description: "Multi-factor authentication providers"
      }
    ]
  }]
})
```

**Question 2: Features Needed**
```typescript
AskUserQuestion({
  questions: [{
    question: "Which features does your plugin need? (Select all that apply)",
    header: "Features",
    multiSelect: true,
    options: [
      {
        label: "Database Tables",
        description: "Custom entities with EF Core data access"
      },
      {
        label: "Configuration Page",
        description: "Admin settings UI for plugin configuration"
      },
      {
        label: "External API",
        description: "Integration with third-party service (REST/SOAP)"
      },
      {
        label: "Public Widgets",
        description: "Display components on storefront pages"
      },
      {
        label: "Admin Menu",
        description: "Custom admin navigation items"
      },
      {
        label: "Scheduled Tasks",
        description: "Background jobs (cron-style)"
      },
      {
        label: "Custom Routes",
        description: "Custom URL patterns and routing"
      },
      {
        label: "Event Consumers",
        description: "React to nopCommerce domain events"
      }
    ]
  }]
})
```

## Step 2: Collect Text Inputs with Guidance

After structured questions, ask user for text inputs with clear formatting guidance:

**Prompt for Plugin Name:**
```
What is your plugin name? (Use PascalCase, e.g., 'PayPalCommerce', 'CustomShipping')
- No spaces
- No special characters except dots
- Descriptive name (not generic like 'Plugin1')
```

**Prompt for Description:**
```
Describe what your plugin does (1-3 sentences):
- Clear business value statement
- Not just a list of features
- Minimum 20 characters
```

**Prompt for Author:**
```
Author name or company for plugin.json:
- Your name or company name
- Will appear in admin plugin list
```

**If "External API" was selected, ask:**
```
Which external service are you integrating? (e.g., PayPal REST API, Stripe, FedEx Web Services)
- Service name
- API type (REST/SOAP/GraphQL)
- Documentation URL (if available)
```

## Step 3: Validate All Inputs

**Before delegating to mission-commander, validate:**

### Plugin Group Validation
- ✅ Already validated (from dropdown selection)
- Valid groups: Payments, Shipping, Tax, Widgets, Misc, ExternalAuth, Pickup, MultiFactorAuth

### Plugin Name Validation
**Rules:**
- PascalCase (e.g., "PayPalCommerce")
- No spaces (fail if spaces found)
- No special characters except dots (fail if found)
- Not generic (warn if "Plugin", "Test", "Sample")
- Descriptive and meaningful

**Validation:**
```markdown
If name contains spaces:
  → Error: "Plugin names must be PascalCase with no spaces. Example: 'PayPalCommerce'"

If name contains special characters (except dots):
  → Error: "Plugin names can only contain letters, numbers, and dots. Example: 'PayPal.Commerce'"

If name is generic ("Plugin", "Test", "MyPlugin"):
  → Warning: "Use a descriptive name that indicates functionality. Example: 'PayPalCommerce' instead of 'Payment'"
```

### Conflict Detection
**Use Glob to check if plugin already exists:**
```bash
Glob: Plugins/Nop.Plugin.{Group}.{Name}/**
```

**If matches found:**
```markdown
⚠️ Plugin already exists: Nop.Plugin.{Group}.{Name}

Found at: Plugins/Nop.Plugin.{Group}.{Name}/

Options:
1. **Use different name** - Create new plugin with different name
2. **Modify existing** - Add features to existing plugin (not recommended via this command)
3. **Overwrite** - Delete existing and create new (⚠️ DESTRUCTIVE - will lose current code)
4. **Cancel** - Abort plugin creation

Which option do you prefer?
```

### Description Validation
**Rules:**
- Minimum 20 characters
- Should be sentence(s), not just keywords
- Describes what plugin does, not how

**Validation:**
```markdown
If length < 20 characters:
  → Error: "Please provide a more detailed description (at least 20 characters)"

If description is just keywords (no sentence structure):
  → Warning: "Please describe what the plugin does in 1-3 complete sentences"
```

### Author Validation
**Rules:**
- Not empty
- Reasonable length (< 100 characters)

**Validation:**
```markdown
If empty:
  → Error: "Author name is required for plugin.json"
```

### Feature Validation
- ✅ Already validated (from multi-select)
- At least one feature selected (validated by user selection)

## Step 4: Validation Error Handling

**If ANY validation fails:**
1. Show specific error message with example
2. Ask user to provide corrected input
3. Re-validate until all checks pass
4. DO NOT proceed to delegation until validated

**Example Error Message:**
```markdown
❌ Validation Failed

Plugin Name: "My Plugin"
Error: Plugin names must be PascalCase with no spaces.

Did you mean: "MyPlugin"?

Please provide a corrected plugin name (PascalCase, e.g., 'PayPalCommerce'):
```

## Step 5: Delegate to Mission Commander (Only After Validation Passes)

**CRITICAL**: Do NOT execute this delegation until ALL validation checks pass.

Once all inputs are validated and any conflicts resolved, use the Task tool to delegate to mission-commander:

```
Create a comprehensive mission blueprint for a new nopCommerce plugin with the following requirements:

**Plugin Information (VALIDATED):**
- Name: [PluginName] (validated: PascalCase, no spaces, no conflicts)
- Group: [Group] (validated: from dropdown)
- Description: [Description] (validated: ≥20 chars, sentence format)
- Author: [Author] (validated: not empty)

**Features Requested (VALIDATED from multi-select):**
- Database Tables: [Yes/No]
- Configuration Page: [Yes/No]
- External API Integration: [Yes/No] (if Yes, service: [ServiceName])
- Public Widgets: [Yes/No]
- Admin Menu: [Yes/No]
- Scheduled Tasks: [Yes/No]
- Custom Routes: [Yes/No]
- Event Consumers: [Yes/No]

**Technical Context:**
- nopCommerce Version: 4.90
- .NET Version: 9.0
- Target Framework: net9.0
- Location: Plugins/Nop.Plugin.[Group].[Name]/

**Quality Standards (NON-NEGOTIABLE):**
- nopCommerce 4.90 compliance
- Plugin naming convention: Nop.Plugin.[Group].[Name]
- All coding standards enforced (async/await, language keywords, etc.)
- XML documentation required on all public members
- Zero compiler warnings
- Proper error handling and logging
- Caching where appropriate
- Localization support
- Multi-store compatibility

**Deliverables Required:**
1. Complete plugin structure (folders, files)
2. plugin.json with validated metadata
3. .csproj with proper configuration
4. IPlugin implementation with Install/Uninstall logic
5. DependencyRegistrar for service registration
6. Entity/data access layer (if "Database Tables" selected)
7. Admin configuration page (if "Configuration Page" selected)
8. External API integration (if "External API" selected)
9. Public widgets (if "Public Widgets" selected)
10. Admin menu items (if "Admin Menu" selected)
11. Scheduled tasks (if "Scheduled Tasks" selected)
12. Custom routes (if "Custom Routes" selected)
13. Event consumers (if "Event Consumers" selected)
14. Localization resources (en-US.xml)
15. README.md with installation and usage instructions
16. Unit tests for business logic

**Agent Coordination Plan:**
- nopcommerce-plugin-developer: Core plugin structure, IPlugin, DependencyRegistrar
- nopcommerce-data-specialist: Entity/data layer (if needed)
- nopcommerce-integration-specialist: External API integration (if needed)
- nopcommerce-widget-specialist: Widget implementation (if needed)
- nopcommerce-ui-specialist: Admin configuration views
- nopcommerce-test-specialist: Unit and integration tests

**Pre-Execution Verification:**
- All inputs validated ✓
- No plugin name conflicts (or conflict resolved) ✓
- User requirements clear ✓
- Quality standards defined ✓

Ensure all nopCommerce 4.90 standards are met, all selected features are implemented, and the plugin is production-ready with passing tests.
```

## Expected Outcome

Mission-commander will create a detailed blueprint, then Team Commander will execute it by delegating to specialized agents. The result will be a complete, working nopCommerce plugin ready for use.
