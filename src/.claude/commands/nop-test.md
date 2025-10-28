---
name: /nop-test
description: Create comprehensive tests for nopCommerce plugin (unit + integration)
---

# Create Tests for nopCommerce Plugin

You are creating comprehensive tests for a nopCommerce plugin. Classify this request:

**This can be SIMPLE or COMPLEX**:
- **Simple**: Testing a single service or component (≤2 files)
- **Complex**: Full plugin test suite with unit + integration tests

## Action Required

**If COMPLEX (full test suite)**: Delegate to mission-commander for blueprint.
**If SIMPLE (single component)**: Execute directly or delegate to nopcommerce-test-specialist.

## Information to Gather from User (via mission-commander)

Ask the user for:

1. **Test Scope**:
   - Which plugin to test?
   - Full test suite or specific components?
   - Unit tests, integration tests, or both?

2. **Components to Test**:
   - Services
   - Controllers
   - Data access layer
   - Plugin installation/uninstallation
   - External API integrations

3. **Coverage Requirements**:
   - Target code coverage percentage
   - Critical paths that must be tested
   - Edge cases to cover

## Delegation Command (for Complex)

Use the Task tool to delegate to mission-commander:

```
Create a comprehensive mission blueprint for testing a nopCommerce plugin:

**Plugin Under Test:**
- Plugin: [PluginName]
- Namespace: Nop.Plugin.[Group].[Name]

**Test Scope:**
- Unit Tests: [Yes/No]
- Integration Tests: [Yes/No]
- Plugin Installation Tests: [Yes/No]

**Components to Test:**
- Services: [List service classes]
- Controllers: [List controllers]
- Data Access: [List entities/repositories]
- External APIs: [List integrations]
- View Components: [List view components]

**Test Coverage Requirements:**
- Target Coverage: [percentage, e.g., 80%]
- Critical Paths: [List must-test scenarios]
- Edge Cases: [List edge cases]

**Testing Tools:**
- NUnit 4.4.0
- FluentAssertions 7.2.0
- Moq 4.20.72
- Microsoft.Data.Sqlite 9.0.9 (in-memory DB)

**Deliverables:**
1. Service unit tests (Services/{Service}Tests.cs)
2. Controller tests (Controllers/{Controller}Tests.cs)
3. Repository integration tests (Data/{Entity}RepositoryTests.cs)
4. Plugin installation tests (Integration/PluginInstallationTests.cs)
5. Test data builders (Helpers/{Entity}Builder.cs)
6. Test helpers and utilities
7. Test documentation (README.md in test project)

**Test Quality Standards:**
- AAA pattern (Arrange, Act, Assert)
- Clear test names: [Method]_[Scenario]_[ExpectedResult]
- FluentAssertions for readable assertions
- Mocks used appropriately (not over-mocking)
- Tests isolated (no dependencies between tests)
- Fast execution (unit tests < 100ms)
- No flaky tests

**Agent Assignment:**
- nopcommerce-test-specialist: All test implementation

**Acceptance Criteria:**
- All tests pass
- Code coverage meets target
- No flaky tests
- Tests run quickly
- All critical paths covered
- Edge cases handled

Ensure tests follow NUnit and nopCommerce testing best practices.
```

## Delegation Command (for Simple - Single Component)

For testing a single component, delegate directly to nopcommerce-test-specialist:

```
Create unit tests for [ServiceName/ControllerName] in plugin [PluginName]:

**Component**: [Fully qualified class name]
**Methods to test**: [List methods or "all public methods"]
**Coverage target**: [percentage]

Create tests following NUnit best practices with FluentAssertions.
```

## Expected Outcome

- **Complex**: Mission-commander creates blueprint → Team Commander executes → Complete test suite with unit + integration tests
- **Simple**: nopcommerce-test-specialist creates tests → Tests pass and meet coverage target
