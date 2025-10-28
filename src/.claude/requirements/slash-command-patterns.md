# Slash Command Best Practices

This document provides patterns for creating slash commands with structured user input, validation, and error handling.

## Pattern 1: Structured Information Gathering

**Use AskUserQuestion tool for structured input**

### Example: Plugin Group Selection

```typescript
AskUserQuestion({
  questions: [{
    question: "Which plugin group should this plugin belong to?",
    header: "Plugin Group",
    multiSelect: false,
    options: [
      {
        label: "Payments",
        description: "Payment gateway integrations (PayPal, Stripe, etc.)"
      },
      {
        label: "Shipping",
        description: "Shipping rate calculators (UPS, FedEx, etc.)"
      },
      {
        label: "Tax",
        description: "Tax calculation providers (Avalara, etc.)"
      },
      {
        label: "Widgets",
        description: "UI components and integrations (Analytics, Pixels, etc.)"
      },
      {
        label: "Misc",
        description: "Miscellaneous plugins that don't fit other categories"
      },
      {
        label: "ExternalAuth",
        description: "External authentication providers (Google, Facebook, etc.)"
      }
    ]
  }]
})
```

### Example: Yes/No Questions

```typescript
AskUserQuestion({
  questions: [
    {
      question: "Does your plugin need to store data in database tables?",
      header: "Database",
      multiSelect: false,
      options: [
        {
          label: "Yes",
          description: "Plugin will create custom entities and tables"
        },
        {
          label: "No",
          description: "Plugin only uses existing nopCommerce data"
        }
      ]
    },
    {
      question: "Does your plugin need a configuration page in admin?",
      header: "Configuration",
      multiSelect: false,
      options: [
        {
          label: "Yes",
          description: "Admin can configure plugin settings"
        },
        {
          label: "No",
          description: "No configuration needed"
        }
      ]
    }
  ]
})
```

### Example: Multi-Select Features

```typescript
AskUserQuestion({
  questions: [{
    question: "Which features does your plugin need? (Select all that apply)",
    header: "Features",
    multiSelect: true,
    options: [
      {
        label: "Database Tables",
        description: "Custom entities with EF Core"
      },
      {
        label: "Configuration Page",
        description: "Admin settings UI"
      },
      {
        label: "External API",
        description: "Integration with third-party service"
      },
      {
        label: "Public Widgets",
        description: "Display components on storefront"
      },
      {
        label: "Admin Menu",
        description: "Custom admin navigation"
      },
      {
        label: "Scheduled Tasks",
        description: "Background jobs"
      }
    ]
  }]
})
```

---

## Pattern 2: Input Validation Before Delegation

**Always validate user input before delegating to mission-commander**

### Validation Checklist

```markdown
Before delegating to mission-commander, verify:

1. **Required Fields Provided**:
   - [ ] Plugin name provided (not empty)
   - [ ] Plugin group selected
   - [ ] Functionality description provided
   - [ ] Author name provided

2. **Valid Values**:
   - [ ] Plugin group matches allowed values (Payments/Shipping/Tax/Widgets/Misc/ExternalAuth/Pickup/DiscountRules/Search/MultiFactorAuth)
   - [ ] Plugin name follows naming conventions (no spaces, no special chars except dots)
   - [ ] Version follows semantic versioning (X.Y.Z)

3. **No Conflicts**:
   - [ ] Use Glob to check `Plugins/Nop.Plugin.{Group}.{Name}/` doesn't already exist
   - [ ] If exists, ask user: "Plugin already exists. Overwrite? Modify? Cancel?"

4. **Dependencies Clear**:
   - [ ] If external API selected, API name/endpoint specified
   - [ ] If database selected, entity names provided
   - [ ] If widgets selected, widget zones identified
```

### Validation Implementation Example

```markdown
## Step 1: Gather Information (Structured)

Use AskUserQuestion to collect:
- Plugin group (dropdown)
- Plugin name (text)
- Features needed (multi-select)

## Step 2: Validate Input

1. Check plugin group is valid:
   ```
   If group not in [Payments, Shipping, Tax, Widgets, Misc, ExternalAuth]:
      → Ask again with valid options
   ```

2. Check plugin doesn't exist:
   ```
   Use Glob: "Plugins/Nop.Plugin.{group}.{name}/**"
   If matches found:
      → Ask user: "Plugin exists. Continue? (Yes/No)"
      If No → Cancel mission
   ```

3. Verify required information complete:
   ```
   If "External API" selected but no API name provided:
      → Ask: "Which external service are you integrating?"
   ```

## Step 3: Delegate with Validated Input

Only after validation passes, delegate to mission-commander with:
- Validated plugin group
- Confirmed plugin name
- Complete feature list
- No conflicts
```

---

## Pattern 3: Error-Friendly User Communication

**When validation fails, provide helpful guidance**

### Example: Invalid Group Name

```markdown
❌ Invalid plugin group: "Payment"

Did you mean "Payments"?

Valid plugin groups:
- Payments (payment gateways)
- Shipping (shipping calculators)
- Tax (tax providers)
- Widgets (UI components)
- Misc (general plugins)
- ExternalAuth (authentication providers)
- Pickup (pickup point providers)
- DiscountRules (discount rules)
- Search (search providers)
- MultiFactorAuth (MFA providers)

Please select from the list above.
```

### Example: Plugin Conflict

```markdown
⚠️ Plugin already exists: Nop.Plugin.Payments.PayPal

Found at: Plugins/Nop.Plugin.Payments.PayPal/

Options:
1. **Modify existing plugin** - Add features to existing PayPal plugin
2. **Use different name** - Create new plugin with different name (e.g., PayPalCommerce)
3. **Overwrite** - Delete existing and create new (⚠️ will lose current code)
4. **Cancel** - Abort mission

Which option do you prefer?
```

---

## Pattern 4: Reusable Validation Patterns

These validation helpers provide consistent validation logic and error messages for common input types used across slash commands.

### Validate Plugin Group

**Purpose**: Ensure plugin group matches valid nopCommerce plugin categories

**Valid nopCommerce plugin groups**:
```
Payments, Shipping, Tax, Widgets, Misc, ExternalAuth, Pickup, DiscountRules, Search, MultiFactorAuth
```

**Validation Logic**:
```javascript
// Check if input matches valid group (case-insensitive)
const validGroups = ["Payments", "Shipping", "Tax", "Widgets", "Misc", "ExternalAuth", "Pickup", "DiscountRules", "Search", "MultiFactorAuth"];
const isValid = validGroups.some(group => group.toLowerCase() === input.toLowerCase());
```

**Error Handling**:
```markdown
If user input not in valid groups:
   1. Calculate closest match using Levenshtein distance
   2. Suggest correction with "Did you mean?" message
   3. List all valid groups

Example: User enters "Payment"
→ ❌ Invalid plugin group: "Payment"
→ Did you mean "Payments"?
→ Valid plugin groups: Payments, Shipping, Tax, Widgets, Misc, ExternalAuth, Pickup, DiscountRules, Search, MultiFactorAuth
```

**Example Validation**:
```
Input: "Payment" → Invalid, suggest "Payments"
Input: "Payments" → Valid ✓
Input: "Widget" → Invalid, suggest "Widgets"
Input: "ExternalAuth" → Valid ✓
Input: "OAuth" → Invalid, suggest "ExternalAuth"
```

---

### Validate Plugin Name

**Purpose**: Ensure plugin name follows nopCommerce naming conventions

**Rules**:
- PascalCase (e.g., "PayPalCommerce")
- No spaces (use PascalCase instead)
- No special characters except dots (periods)
- Descriptive (not generic like "Plugin1", "Test", "MyPlugin")
- Reasonable length (3-50 characters)

**Validation Regex**:
```regex
^[A-Z][a-zA-Z0-9\.]*$
```

**Validation Logic**:
```javascript
// Check for spaces
if (name.includes(' ')) → Error: "No spaces allowed"

// Check for special characters (except dots)
if (!/^[A-Za-z0-9\.]+$/.test(name)) → Error: "Only letters, numbers, and dots allowed"

// Check for PascalCase (starts with uppercase)
if (!/^[A-Z]/.test(name)) → Warning: "Should start with uppercase letter"

// Check for generic names
const genericNames = ["Plugin", "Test", "Sample", "MyPlugin", "NewPlugin"];
if (genericNames.some(generic => name.includes(generic))) → Warning: "Use descriptive name"

// Check length
if (name.length < 3) → Error: "Name too short (minimum 3 characters)"
if (name.length > 50) → Warning: "Name too long (maximum 50 characters recommended)"
```

**Error Message Templates**:
```markdown
If contains spaces:
"Plugin names must be PascalCase with no spaces. Example: 'PayPalCommerce'"

If has special characters:
"Plugin names can only contain letters, numbers, and dots. Example: 'PayPal.Commerce'"

If not PascalCase:
"Plugin name should be PascalCase (start with uppercase). Example: 'PaypalCommerce' not 'paypalcommerce'"

If generic name:
"Use a descriptive name that indicates functionality. Example: 'PayPalCommerce' instead of 'PaymentPlugin'"

If too short:
"Plugin name must be at least 3 characters long"

If too long:
"Plugin name is too long. Keep it under 50 characters for readability"
```

**Example Validation**:
```
Input: "PayPalCommerce" → Valid ✓
Input: "PayPal Commerce" → Invalid (spaces): "Plugin names must be PascalCase with no spaces"
Input: "paypalcommerce" → Warning (not PascalCase): "Should start with uppercase letter"
Input: "PayPal-Commerce" → Invalid (special char): "Only letters, numbers, and dots allowed"
Input: "PayPal.Commerce" → Valid ✓ (dots allowed)
Input: "Plugin1" → Warning (generic): "Use descriptive name like 'PayPalCommerce'"
Input: "PP" → Invalid (too short): "Name must be at least 3 characters"
```

---

### Validate Version Number

**Purpose**: Ensure version follows semantic versioning (SemVer) standard

**Format**: Semantic versioning (X.Y.Z)
- X = Major version (breaking changes)
- Y = Minor version (new features, backward compatible)
- Z = Patch version (bug fixes, backward compatible)

**Validation Regex**:
```regex
^\d+\.\d+\.\d+$
```

**Extended SemVer Support** (optional):
```regex
^\d+\.\d+\.\d+(-[a-zA-Z0-9\-\.]+)?(\+[a-zA-Z0-9\-\.]+)?$
```
Supports pre-release (e.g., "1.0.0-alpha.1") and build metadata (e.g., "1.0.0+20250101")

**Validation Logic**:
```javascript
// Basic validation
const semverPattern = /^\d+\.\d+\.\d+$/;
if (!semverPattern.test(version)) → Error: "Invalid semantic version format"

// Parse version parts
const [major, minor, patch] = version.split('.').map(Number);

// Check each part is valid number
if (isNaN(major) || isNaN(minor) || isNaN(patch)) → Error: "Version parts must be numbers"

// Optional: Check version isn't 0.0.0
if (major === 0 && minor === 0 && patch === 0) → Warning: "Version 0.0.0 is not recommended"
```

**Error Message Templates**:
```markdown
If invalid format:
"Version should follow semantic versioning (X.Y.Z). Examples: '1.0.0', '2.1.3', '10.5.12'"

If contains letters:
"Version parts must be numbers only. Example: '1.0.0' not '1.0.a'"

If missing parts:
"Version must have three parts (Major.Minor.Patch). Example: '1.0.0' not '1.0'"

If too many parts:
"Version should have exactly three parts. Example: '1.0.0' not '1.0.0.1'"
```

**Example Validation**:
```
Input: "1.0.0" → Valid ✓
Input: "2.1.3" → Valid ✓
Input: "10.5.12" → Valid ✓
Input: "1.0" → Invalid: "Version must have three parts (Major.Minor.Patch)"
Input: "1.0.0.1" → Invalid: "Version should have exactly three parts"
Input: "1.0.a" → Invalid: "Version parts must be numbers only"
Input: "v1.0.0" → Invalid: "Do not include 'v' prefix. Use '1.0.0'"
Input: "0.0.0" → Warning: "Version 0.0.0 is not recommended. Use '1.0.0' for initial release"
```

---

### Check Plugin Exists (Conflict Detection)

**Purpose**: Prevent accidentally overwriting existing plugins

**Check Method**:
```bash
Use Glob tool: Plugins/Nop.Plugin.{Group}.{Name}/**
```

**Validation Logic**:
```javascript
// Check if plugin directory exists
const pluginPath = `Plugins/Nop.Plugin.${group}.${name}/**`;
const matches = await Glob(pluginPath);

if (matches.length > 0) {
   // Plugin exists - present options to user
   showConflictResolutionOptions(group, name, matches);
}
```

**Conflict Resolution Options**:
```markdown
If matches found:
   Display:
   ⚠️ Plugin already exists: Nop.Plugin.{Group}.{Name}

   Found at: Plugins/Nop.Plugin.{Group}.{Name}/

   Files detected:
   - plugin.json
   - {Name}Plugin.cs
   - [list other key files found]

   Options:
   1. **Use different name** - Create new plugin with different name (recommended)
   2. **Modify existing** - Add features to existing plugin (not via /nop-new-plugin)
   3. **Overwrite** - Delete existing and create new (⚠️ DESTRUCTIVE - will lose current code)
   4. **Cancel** - Abort plugin creation

   Which option do you prefer?
```

**Example Validation**:
```
Check: Plugins/Nop.Plugin.Payments.PayPal/**
Result: Matches found → Conflict detected

User sees:
⚠️ Plugin already exists: Nop.Plugin.Payments.PayPal
Found at: Plugins/Nop.Plugin.Payments.PayPal/
Files detected:
- plugin.json
- PayPalPlugin.cs
- PayPalPaymentProcessor.cs
Options: [Use different name / Modify existing / Overwrite / Cancel]

User chooses: "Use different name"
Follow-up: "What name would you like to use instead? (e.g., 'PayPalCommerce')"
```

---

### Validate Email Address

**Purpose**: Ensure email address has valid format

**Format**: user@domain.tld

**Validation Regex**:
```regex
^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
```

**Simplified Regex** (more permissive):
```regex
^[^@]+@[^@]+\.[^@]+$
```

**Validation Logic**:
```javascript
const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
if (!emailPattern.test(email)) → Error: "Invalid email format"

// Additional checks
if (!email.includes('@')) → Error: "Email must contain @ symbol"
if (!email.includes('.')) → Error: "Email must contain domain extension"
if (email.startsWith('@')) → Error: "Email must have username before @"
if (email.endsWith('@')) → Error: "Email must have domain after @"
```

**Error Message Templates**:
```markdown
If invalid format:
"Email should be in format: user@domain.com"

If missing @ symbol:
"Email must contain @ symbol. Example: 'user@example.com'"

If missing domain:
"Email must have domain after @. Example: 'user@example.com'"

If missing extension:
"Email must have domain extension. Example: 'user@example.com' not 'user@example'"
```

**Example Validation**:
```
Input: "user@example.com" → Valid ✓
Input: "john.doe@company.co.uk" → Valid ✓
Input: "support+tag@domain.com" → Valid ✓
Input: "userexample.com" → Invalid: "Email must contain @ symbol"
Input: "user@" → Invalid: "Email must have domain after @"
Input: "@example.com" → Invalid: "Email must have username before @"
Input: "user@example" → Warning: "Email should have domain extension (.com, .org, etc.)"
Input: "user name@example.com" → Invalid: "Email cannot contain spaces"
```

---

### Validate URL

**Purpose**: Ensure URL has valid format and protocol

**Format**: https://domain.com or http://domain.com

**Validation Regex**:
```regex
^(https?):\/\/([\w\-]+(\.[\w\-]+)+)([\w\-\.,@?^=%&:/~\+#]*)?$
```

**Simplified Regex** (more permissive):
```regex
^https?:\/\/.+$
```

**Validation Logic**:
```javascript
// Check protocol
if (!url.startsWith('http://') && !url.startsWith('https://')) {
   → Error: "URL must start with http:// or https://"
}

// Security warning for HTTP
if (url.startsWith('http://') && !url.startsWith('https://')) {
   → Warning: "Consider using https:// for security"
}

// Check domain exists
const urlPattern = /^https?:\/\/.+\..+/;
if (!urlPattern.test(url)) {
   → Error: "URL must have valid domain"
}
```

**Error Message Templates**:
```markdown
If no protocol:
"URL must start with http:// or https://. Example: 'https://example.com' not 'example.com'"

If invalid protocol:
"URL must use http:// or https:// protocol. Example: 'https://example.com' not 'ftp://example.com'"

If http (not https):
"⚠️ Consider using https:// for security. Example: 'https://example.com' instead of 'http://example.com'"

If missing domain:
"URL must have valid domain. Example: 'https://example.com' not 'https://'"

If has spaces:
"URL cannot contain spaces. Use %20 for spaces in paths."
```

**Example Validation**:
```
Input: "https://example.com" → Valid ✓
Input: "https://api.example.com/v1/endpoint" → Valid ✓
Input: "http://example.com" → Valid with warning: "Consider using https://"
Input: "example.com" → Invalid: "URL must start with http:// or https://"
Input: "ftp://example.com" → Invalid: "URL must use http:// or https:// protocol"
Input: "https://" → Invalid: "URL must have valid domain"
Input: "https://my site.com" → Invalid: "URL cannot contain spaces"
Input: "https://localhost:3000" → Valid ✓ (localhost allowed)
```

---

## Pattern 5: Multi-Question Workflows

**Collect related information in a single AskUserQuestion call**

### Example: Complete Plugin Information

```typescript
AskUserQuestion({
  questions: [
    {
      question: "Which plugin group should this plugin belong to?",
      header: "Group",
      multiSelect: false,
      options: [
        { label: "Payments", description: "Payment gateways" },
        { label: "Shipping", description: "Shipping calculators" },
        { label: "Tax", description: "Tax providers" },
        { label: "Widgets", description: "UI components" },
        { label: "Misc", description: "Other plugins" },
        { label: "ExternalAuth", description: "Authentication providers" }
      ]
    },
    {
      question: "Which features does your plugin need? (Select all that apply)",
      header: "Features",
      multiSelect: true,
      options: [
        { label: "Database Tables", description: "Custom entities" },
        { label: "Configuration Page", description: "Admin settings" },
        { label: "External API", description: "Third-party integration" },
        { label: "Public Widgets", description: "Storefront display" }
      ]
    },
    {
      question: "Does your plugin need a configuration page?",
      header: "Config",
      multiSelect: false,
      options: [
        { label: "Yes", description: "Admin can configure settings" },
        { label: "No", description: "No configuration needed" }
      ]
    }
  ]
})
```

**Benefits**:
- User answers all questions at once
- Reduces back-and-forth
- Better user experience
- Context preserved across questions

---

## Pattern 6: Progressive Disclosure

**Ask follow-up questions based on previous answers**

### Example: Conditional Questions

```markdown
## Initial Question

AskUserQuestion: "Does your plugin integrate with external API?"
- Yes
- No

## If Yes → Ask Follow-Up

Ask user for text input:
- "Which external service are you integrating?" (e.g., PayPal, Stripe)
- "Do you have API documentation URL?"

## If No → Skip Follow-Up

Proceed with plugin creation without API integration
```

### Example: Feature-Dependent Questions

```markdown
## Ask: "Which features does your plugin need?"
- [x] Database Tables
- [x] Configuration Page
- [ ] External API

## Because "Database Tables" selected → Ask:

"What data will your plugin store?"
(Free text: brief description)

## Because "Configuration Page" selected → Ask:

"What settings will be configurable?"
- [x] API Keys
- [x] Enable/Disable
- [ ] Custom Styling
```

---

## Pattern 7: Text Input Best Practices

**When structured options don't work, collect text input with validation**

### Example: Plugin Name Collection

```markdown
**Prompt**: "What is your plugin name? (Use PascalCase, e.g., 'PayPalCommerce')"

**After user provides input**:

1. Validate format (no spaces, PascalCase)
2. Check for conflicts (use Glob)
3. Verify it's descriptive (not generic)

If validation fails:
- Show specific error with example
- Ask user to provide corrected input
- Explain why it failed
```

### Example: Description Collection

```markdown
**Prompt**: "Describe what your plugin does (1-2 sentences)"

**After user provides input**:

1. Check minimum length (at least 20 characters)
2. Check it's not just keywords (should be a sentence)

If too short:
"Please provide a more detailed description (at least 20 characters)"

If not descriptive:
"Please describe what the plugin does, not just list features"
```

---

## Complete Example: /nop-new-plugin Workflow

### Step 1: Structured Questions

```typescript
AskUserQuestion({
  questions: [
    {
      question: "Which plugin group should this plugin belong to?",
      header: "Plugin Group",
      multiSelect: false,
      options: [
        { label: "Payments", description: "Payment gateway integrations" },
        { label: "Shipping", description: "Shipping rate calculators" },
        { label: "Tax", description: "Tax calculation providers" },
        { label: "Widgets", description: "UI components" },
        { label: "Misc", description: "General plugins" },
        { label: "ExternalAuth", description: "Authentication providers" }
      ]
    },
    {
      question: "Which features does your plugin need? (Select all that apply)",
      header: "Features",
      multiSelect: true,
      options: [
        { label: "Database Tables", description: "Custom entities with EF Core" },
        { label: "Configuration Page", description: "Admin settings UI" },
        { label: "External API", description: "Third-party service integration" },
        { label: "Public Widgets", description: "Storefront components" },
        { label: "Admin Menu", description: "Custom admin navigation" },
        { label: "Scheduled Tasks", description: "Background jobs" }
      ]
    }
  ]
})
```

### Step 2: Text Input with Guidance

```markdown
Ask user for:
1. **Plugin Name**: "What is your plugin name? (PascalCase, e.g., 'PayPalCommerce')"
2. **Description**: "Describe what your plugin does (1-2 sentences)"
3. **Author**: "Author name for plugin.json"

If "External API" was selected in features:
4. **API Name**: "Which external service are you integrating? (e.g., PayPal, Stripe)"
```

### Step 3: Validation

```markdown
Validate all inputs:

1. Plugin Group: Already validated (from dropdown)
2. Features: Already validated (from multi-select)
3. Plugin Name:
   - Check no spaces (fail if spaces)
   - Check PascalCase (warn if not)
   - Check not generic (warn if "Plugin", "Test", etc.)
   - Check no conflict (use Glob)
4. Description:
   - Check min length 20 chars
   - Check is sentence (not keywords)
5. Author:
   - Check not empty

If any validation fails:
- Show specific error
- Ask user to provide corrected input
- Continue validation loop until all pass
```

### Step 4: Delegate with Validated Input

```markdown
Only after all validation passes:

Delegate to mission-commander with:
```
Create a comprehensive mission blueprint for a new nopCommerce plugin:

**Plugin Information:**
- Name: {validated name}
- Group: {selected group}
- Description: {validated description}
- Author: {provided author}

**Features (from multi-select):**
- Database Tables: {Yes/No}
- Configuration Page: {Yes/No}
- External API Integration: {Yes if selected, with API name}
- Public Widgets: {Yes/No}
- Admin Menu: {Yes/No}
- Scheduled Tasks: {Yes/No}

**Technical Requirements:**
- nopCommerce 4.90 compliance
- .NET 9.0
- All coding standards enforced
- XML documentation required
- Zero compiler warnings

All inputs have been validated and conflicts checked.
```
```

---

## Best Practices Summary

### DO:
- ✅ Use AskUserQuestion for structured choices (dropdowns, multi-select)
- ✅ Validate ALL user input before delegation
- ✅ Check for conflicts (existing plugins, files)
- ✅ Provide helpful error messages with examples
- ✅ Show user what went wrong and how to fix it
- ✅ Use progressive disclosure (ask follow-ups based on answers)
- ✅ Collect related information in single question batch

### DON'T:
- ❌ Use free-form text when structured options work better
- ❌ Skip validation and hope for the best
- ❌ Give vague error messages ("Invalid input")
- ❌ Delegate with unvalidated input
- ❌ Ask same question multiple times if user makes mistake
- ❌ Proceed when required information is missing

### Error Handling:
- Show what was wrong
- Show what was expected
- Show example of correct format
- Allow user to correct and retry
- Don't make user start over completely

---

## Validation Helper Functions (Conceptual)

These patterns can be reused across slash commands:

```markdown
### ValidatePluginGroup(input)
Returns: { valid: boolean, suggestion?: string }
Checks if input matches valid groups, suggests closest match if not

### ValidatePluginName(input)
Returns: { valid: boolean, errors: string[] }
Checks: no spaces, PascalCase, not generic, descriptive

### CheckPluginConflict(group, name)
Returns: { exists: boolean, files: string[] }
Uses Glob to check if plugin already exists

### ValidateVersion(input)
Returns: { valid: boolean, message?: string }
Checks semantic versioning format (X.Y.Z)

### ValidateEmail(input)
Returns: { valid: boolean }
Basic email format validation

### ValidateUrl(input)
Returns: { valid: boolean, warning?: string }
Checks http/https, warns if http not https
```

---

## Migration Strategy for Existing Commands

**For each slash command**:

1. **Identify information gathering**
   - What does the command currently ask for?
   - Which can be structured (dropdown/multi-select)?
   - Which must be text input?

2. **Design structured questions**
   - Create AskUserQuestion with options
   - Group related questions together
   - Use progressive disclosure for follow-ups

3. **Add validation**
   - Define validation rules for each input
   - Add conflict detection where needed
   - Create helpful error messages

4. **Update delegation**
   - Only delegate after full validation
   - Include validated inputs in delegation
   - Document what was validated

---

## Testing Validation

**For each validation rule, test**:

- ✅ Valid input passes
- ✅ Invalid input fails with helpful message
- ✅ Edge cases handled (empty, special chars, etc.)
- ✅ User can correct and retry
- ✅ Conflict detection works

**Example Test Cases for Plugin Name**:

| Input | Expected Result | Message |
|-------|----------------|---------|
| "PayPalCommerce" | ✅ Pass | Valid |
| "PayPal Commerce" | ❌ Fail | "No spaces allowed - use PascalCase" |
| "paypalcommerce" | ⚠️ Warn | "Should be PascalCase - use 'PaypalCommerce'" |
| "Plugin1" | ⚠️ Warn | "Use descriptive name like 'PayPalCommerce'" |
| "Payment" (exists) | ❌ Fail | "Plugin exists - choose: Modify/Rename/Overwrite/Cancel" |
| "" (empty) | ❌ Fail | "Plugin name required" |
| "PayPal@Commerce" | ❌ Fail | "Only letters, numbers, and dots allowed" |
