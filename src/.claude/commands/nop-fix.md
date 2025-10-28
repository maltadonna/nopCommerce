---
name: /nop-fix
description: Troubleshoot and fix nopCommerce plugin issues, errors, bugs, and unexpected behavior
---

# Fix nopCommerce Plugin Issue

You are troubleshooting a nopCommerce plugin issue. Classify this request:

**This is a SIMPLE or COMPLEX task**:
- **Simple**: Single, isolated issue with clear symptoms (e.g., "widget not appearing", "null reference in controller")
- **Complex**: Multiple interrelated issues, systemic problems, or requires deep investigation

## Action Required

**If SIMPLE (single isolated issue)**: Delegate directly to nopcommerce-troubleshooter.
**If COMPLEX (multiple issues or systemic)**: Delegate to mission-commander for blueprint.

## Information to Gather from User

Ask the user for:

1. **Problem Description**:
   - What is happening? (exact error message or behavior)
   - What did you expect to happen?
   - When does it occur? (installation, runtime, specific action)
   - Can you reproduce it? If yes, what are the steps?

2. **Context**:
   - nopCommerce version (e.g., 4.90)
   - Plugin name and type
   - When did it start? (after deployment, after changes, always)
   - What changed recently?

3. **Available Information**:
   - Error logs (from App_Data/Logs/)
   - Stack traces
   - Screenshots (if applicable)
   - Configuration details

## Delegation Command (for Simple)

Use the Task tool to delegate to nopcommerce-troubleshooter:

```
Diagnose and fix the following nopCommerce plugin issue:

**Problem**:
[Exact description of issue]

**Symptoms**:
- Error message: [exact error]
- Occurs when: [action/timing]
- Reproduction steps: [1, 2, 3...]

**Context**:
- nopCommerce Version: [4.90]
- Plugin: [Nop.Plugin.{Group}.{Name}]
- Environment: [Development/Production]
- Recent changes: [what changed]

**Available Information**:
- Error logs: [paste relevant log entries]
- Stack trace: [paste if available]

**Expected Outcome**:
- Root cause identified
- Issue fixed and verified
- Prevention recommendations provided
```

## Delegation Command (for Complex)

Use the Task tool to delegate to mission-commander:

```
Create a comprehensive troubleshooting blueprint for multiple plugin issues:

**Issues Identified**:
1. [Issue 1 description]
2. [Issue 2 description]
3. [Issue 3 description]

**System Context**:
- nopCommerce Version: [4.90]
- Affected Plugins: [list]
- Environment: [context]

**Investigation Required**:
- Determine if issues are related
- Identify systemic root causes
- Prioritize fixes by impact

**Deliverables**:
- Root cause analysis for each issue
- Fixes for all issues
- Verification that no regressions introduced
- Documentation of fixes

**Agent Assignment**:
- nopcommerce-troubleshooter: Diagnostic and fixes
- [Other specialists as needed based on issue type]

Ensure all issues are resolved and system is stable.
```

## Common Issue Quick Reference

| Symptom | Likely Cause | Delegate To |
|---------|--------------|-------------|
| Plugin not appearing in admin | plugin.json issue | nopcommerce-troubleshooter |
| "No service for type" error | DependencyRegistrar issue | nopcommerce-troubleshooter |
| Route returns 404 | RouteProvider issue | nopcommerce-troubleshooter |
| Widget not showing | Widget zone / settings | nopcommerce-troubleshooter |
| Database error | Migration not run | nopcommerce-troubleshooter |
| Settings not saving | Multi-store / cache issue | nopcommerce-troubleshooter |
| Null reference | Service injection / null check | nopcommerce-troubleshooter |
| Payment not processing | API credentials / webhook | nopcommerce-troubleshooter |
| Multiple unrelated issues | Systemic problem | mission-commander |
| Performance degradation | Needs profiling | mission-commander → performance specialist |

## Expected Outcome

- **Simple**: nopcommerce-troubleshooter diagnoses, fixes, and verifies resolution
- **Complex**: mission-commander creates blueprint → Team Commander executes → All issues resolved

## Example Usage

### Example 1: Simple Issue
```
User: "My widget isn't showing on the homepage"

You:
[Gather information: Which widget? Enabled in admin? Widget zone correct?]
[Classify as SIMPLE - single isolated issue]
[Delegate to nopcommerce-troubleshooter with full context]
```

### Example 2: Complex Issue
```
User: "After upgrading to 4.90, my payment plugin doesn't work, orders are failing, and the admin panel is slow"

You:
[Three separate issues - payment, orders, performance]
[Classify as COMPLEX - multiple interrelated issues]
[Delegate to mission-commander for systematic investigation]
```

---

**Remember**: Most plugin issues are SIMPLE and can be handled directly by nopcommerce-troubleshooter. Only escalate to mission-commander when multiple issues are interrelated or systemic.
