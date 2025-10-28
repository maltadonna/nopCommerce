# Testing Standards for nopCommerce Plugins

This document defines mandatory testing requirements for all nopCommerce plugin development. These standards ensure consistency across the DEVGRU Mission Framework and harmonize requirements between CLAUDE.md and src/CLAUDE.md.

---

## Mandatory Test Coverage

### Unit Tests (REQUIRED)

**Must have unit tests for**:
- Business logic methods
- Data validation logic
- Calculation/pricing logic
- Custom algorithms
- Utility/helper methods with logic

**Unit test coverage target**: ≥ 70% for business logic classes

**Verify coverage**:
```bash
dotnet test --collect:"XPlat Code Coverage"
# Review coverage report in TestResults/
```

**Unit tests NOT required for**:
- Simple DTOs/models with no logic
- Auto-implemented properties
- EF Core entity classes (covered by integration tests)
- View models with no validation

**Testing framework**: NUnit 4.4.0 with FluentAssertions 7.2.0

---

### Integration Tests (REQUIRED)

**Must have integration tests for**:
- Database operations (EF Core repositories)
- External API calls (payment gateways, shipping APIs)
- Plugin installation/uninstallation
- Event consumers
- Scheduled tasks

**Integration test framework**: Use Nop.Tests patterns with in-memory database (Microsoft.Data.Sqlite 9.0.9)

**Best practices**:
- Use in-memory SQLite for database tests
- Mock external API calls (use Moq 4.20.72)
- Test both success and failure paths
- Verify database cleanup after operations

---

### Manual Testing (REQUIRED)

**Must manually test**:
- Admin UI workflows (all CRUD operations)
- Public store display (if plugin has storefront components)
- Configuration page functionality
- Multi-store scenarios (if plugin is store-scoped)
- Installation process (fresh nopCommerce install)
- Uninstallation process (clean removal, no orphaned data)

**Manual test checklist**:
- [ ] Plugin appears in admin → Configuration → Local Plugins
- [ ] Plugin installs without errors
- [ ] Configuration page loads and saves settings
- [ ] All admin UI features work as expected
- [ ] No JavaScript errors in browser console
- [ ] No exceptions in application logs (check App_Data/logs/)
- [ ] Plugin uninstalls cleanly (no leftover tables/settings)

---

## Test Organization

**Test project structure**:
```
Tests/Nop.Tests/
├── Plugins/
│   └── {Group}.{Name}/
│       ├── {ServiceName}Tests.cs          (unit tests)
│       ├── {RepositoryName}Tests.cs       (integration tests)
│       └── {PluginName}PluginTests.cs     (plugin lifecycle tests)
```

**Test file naming conventions**:
- Unit tests: `{ClassName}Tests.cs`
- Integration tests: `{ClassName}IntegrationTests.cs`
- End-to-end tests: `{FeatureName}E2ETests.cs`

**Test method naming conventions**:
```csharp
[Test]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange-Act-Assert pattern
}
```

Examples:
- `CalculateTotal_WithValidInputs_ReturnsCorrectSum()`
- `ValidateInput_WithNegativeValue_ThrowsException()`
- `GetCustomerByIdAsync_WithNonExistentId_ReturnsNull()`

---

## When Tests Can Be Skipped

**Tests optional for**:
- Proof-of-concept plugins (mark as POC in plugin.json)
- Internal-only tools (not distributed)
- Simple widgets with no logic (display-only)

**Tests NEVER optional for**:
- Payment processing plugins (CRITICAL - money involved)
- Shipping calculation plugins (CRITICAL - customer experience)
- Tax calculation plugins (CRITICAL - legal compliance)
- Authentication plugins (CRITICAL - security)
- Any plugin handling sensitive data (PII, payment info, credentials)

---

## Quality Gate Enforcement

### BLOCK Mission Completion If:
- Payment/shipping/tax/auth plugin has no tests (CRITICAL severity)
- Business logic has < 70% coverage (HIGH severity)
- No integration tests for database operations (HIGH severity)
- Tests are failing (any test failures block completion)

### WARN But Allow If:
- Simple widget has no tests (MEDIUM severity)
- Coverage is 60-69% (MEDIUM severity)
- Manual testing incomplete but documented (MEDIUM severity)

### When to Escalate:
- If test creation would take > 4 hours, escalate to user with options:
  1. Reduce scope to allow time for testing
  2. Accept lower coverage with documented risk
  3. Extend timeline to include comprehensive testing

---

## Test Example Templates

### Unit Test Example (NUnit + FluentAssertions)

```csharp
using NUnit.Framework;
using FluentAssertions;

namespace Nop.Tests.Plugins.Misc.MyPlugin
{
    [TestFixture]
    public class MyServiceTests
    {
        private IMyService _myService;

        [SetUp]
        public void SetUp()
        {
            // Arrange: Set up test dependencies
            _myService = new MyService();
        }

        [Test]
        public void CalculateTotal_WithValidInputs_ReturnsCorrectSum()
        {
            // Arrange
            var value1 = 10.5m;
            var value2 = 20.3m;

            // Act
            var result = _myService.CalculateTotal(value1, value2);

            // Assert
            result.Should().Be(30.8m);
        }

        [Test]
        public void CalculateTotal_WithZeroValues_ReturnsZero()
        {
            // Arrange
            var value1 = 0m;
            var value2 = 0m;

            // Act
            var result = _myService.CalculateTotal(value1, value2);

            // Assert
            result.Should().Be(0m);
        }

        [Test]
        public void ValidateInput_WithNegativeValue_ThrowsException()
        {
            // Arrange
            var invalidValue = -5m;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _myService.ValidateInput(invalidValue));
        }

        [Test]
        public void ValidateInput_WithNullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _myService.ValidateInput(null));
        }
    }
}
```

---

### Integration Test Example (In-Memory Database)

```csharp
using NUnit.Framework;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Nop.Tests.Plugins.Misc.MyPlugin
{
    [TestFixture]
    public class MyRepositoryTests
    {
        private SqliteConnection _connection;
        private DbContextOptions<MyDbContext> _options;
        private IMyRepository _repository;

        [SetUp]
        public void SetUp()
        {
            // Use in-memory database for testing
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Configure EF Core to use in-memory SQLite
            _options = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlite(_connection)
                .Options;

            // Create schema
            using var context = new MyDbContext(_options);
            context.Database.EnsureCreated();

            // Set up repository with in-memory DB
            _repository = new MyRepository(context);
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Dispose();
        }

        [Test]
        public async Task InsertAsync_WithValidEntity_PersistsToDatabase()
        {
            // Arrange
            var entity = new MyEntity { Name = "Test", Value = 100 };

            // Act
            await _repository.InsertAsync(entity);

            // Assert - verify persisted by retrieving
            using var context = new MyDbContext(_options);
            var retrieved = await context.MyEntities.FindAsync(entity.Id);
            retrieved.Should().NotBeNull();
            retrieved.Name.Should().Be("Test");
            retrieved.Value.Should().Be(100);
        }

        [Test]
        public async Task GetByIdAsync_WithExistingId_ReturnsEntity()
        {
            // Arrange
            var entity = new MyEntity { Name = "Test", Value = 100 };
            await _repository.InsertAsync(entity);

            // Act
            var retrieved = await _repository.GetByIdAsync(entity.Id);

            // Assert
            retrieved.Should().NotBeNull();
            retrieved.Id.Should().Be(entity.Id);
        }

        [Test]
        public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Act
            var retrieved = await _repository.GetByIdAsync(999);

            // Assert
            retrieved.Should().BeNull();
        }

        [Test]
        public async Task DeleteAsync_WithExistingEntity_RemovesFromDatabase()
        {
            // Arrange
            var entity = new MyEntity { Name = "Test", Value = 100 };
            await _repository.InsertAsync(entity);

            // Act
            await _repository.DeleteAsync(entity);

            // Assert - verify deleted
            using var context = new MyDbContext(_options);
            var retrieved = await context.MyEntities.FindAsync(entity.Id);
            retrieved.Should().BeNull();
        }
    }
}
```

---

### Plugin Lifecycle Test Example

```csharp
using NUnit.Framework;
using FluentAssertions;

namespace Nop.Tests.Plugins.Misc.MyPlugin
{
    [TestFixture]
    public class MyPluginTests
    {
        private IMyPlugin _plugin;
        private IServiceProvider _serviceProvider;

        [SetUp]
        public void SetUp()
        {
            // Set up service provider with required dependencies
            var services = new ServiceCollection();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<ILocalizationService, LocalizationService>();
            _serviceProvider = services.BuildServiceProvider();

            _plugin = new MyPlugin();
        }

        [Test]
        public async Task Install_CreatesRequiredSettings()
        {
            // Act
            await _plugin.InstallAsync();

            // Assert
            var settingService = _serviceProvider.GetRequiredService<ISettingService>();
            var settings = await settingService.LoadSettingAsync<MyPluginSettings>();
            settings.Should().NotBeNull();
        }

        [Test]
        public async Task Install_CreatesLocalizationResources()
        {
            // Act
            await _plugin.InstallAsync();

            // Assert
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var resource = await localizationService.GetResourceAsync("Plugins.Misc.MyPlugin.SettingName");
            resource.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task Uninstall_RemovesSettings()
        {
            // Arrange
            await _plugin.InstallAsync();

            // Act
            await _plugin.UninstallAsync();

            // Assert
            var settingService = _serviceProvider.GetRequiredService<ISettingService>();
            var settings = await settingService.LoadSettingAsync<MyPluginSettings>();
            settings.ApiKey.Should().BeNullOrEmpty(); // Settings should be cleared
        }

        [Test]
        public async Task Uninstall_RemovesLocalizationResources()
        {
            // Arrange
            await _plugin.InstallAsync();

            // Act
            await _plugin.UninstallAsync();

            // Assert
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var resource = await localizationService.GetResourceAsync("Plugins.Misc.MyPlugin.SettingName");
            resource.Should().BeNullOrEmpty(); // Resources should be removed
        }
    }
}
```

---

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Tests in Specific Project
```bash
dotnet test Tests/Nop.Tests/Nop.Tests.csproj
```

### Run Tests with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~MyServiceTests.CalculateTotal_WithValidInputs_ReturnsCorrectSum"
```

---

## Common Testing Patterns

### Mocking External Dependencies

```csharp
using Moq;

[SetUp]
public void SetUp()
{
    // Mock external service
    var mockExternalService = new Mock<IExternalService>();
    mockExternalService
        .Setup(x => x.GetDataAsync(It.IsAny<int>()))
        .ReturnsAsync(new ExternalData { Value = 100 });

    _service = new MyService(mockExternalService.Object);
}
```

### Testing Async Methods

```csharp
[Test]
public async Task GetCustomerAsync_WithValidId_ReturnsCustomer()
{
    // Arrange
    var customerId = 1;

    // Act
    var customer = await _customerService.GetCustomerByIdAsync(customerId);

    // Assert
    customer.Should().NotBeNull();
    customer.Id.Should().Be(customerId);
}
```

### Testing Exception Handling

```csharp
[Test]
public void ProcessPayment_WithInvalidCard_ThrowsPaymentException()
{
    // Arrange
    var invalidCard = new CreditCard { Number = "0000" };

    // Act & Assert
    Assert.ThrowsAsync<PaymentException>(async () =>
        await _paymentService.ProcessPaymentAsync(invalidCard));
}
```

### Testing Collections

```csharp
[Test]
public async Task GetAllAsync_ReturnsNonEmptyCollection()
{
    // Act
    var results = await _repository.GetAllAsync();

    // Assert
    results.Should().NotBeNull();
    results.Should().NotBeEmpty();
    results.Should().HaveCountGreaterThan(0);
}
```

---

## Test Coverage Targets by Plugin Type

| Plugin Type | Unit Test Coverage | Integration Tests | Manual Tests |
|-------------|-------------------|-------------------|--------------|
| Payment | ≥ 80% | Required | Required |
| Shipping | ≥ 80% | Required | Required |
| Tax | ≥ 80% | Required | Required |
| Authentication | ≥ 90% | Required | Required |
| Widgets (display-only) | ≥ 50% | Optional | Required |
| Widgets (with logic) | ≥ 70% | Required | Required |
| Misc (data processing) | ≥ 70% | Required | Required |
| Misc (simple tools) | ≥ 60% | Optional | Required |

---

## Troubleshooting Test Issues

### Tests Fail Locally But Pass in CI
- Check .NET version matches (dotnet --version)
- Verify NuGet packages are restored
- Clear bin/obj folders and rebuild

### In-Memory Database Tests Fail
- Ensure SQLite connection is opened in SetUp
- Verify database schema is created (EnsureCreated)
- Check connection is disposed in TearDown

### Coverage Report Missing
- Install coverage tool: `dotnet tool install --global dotnet-coverage`
- Verify --collect:"XPlat Code Coverage" flag is used
- Check TestResults/ directory for coverage files

### Mocked Services Not Working
- Verify Mock<T> is set up correctly
- Use It.IsAny<T>() for parameter matching
- Check Setup() is called before test execution

---

## References

- **NUnit Documentation**: https://docs.nunit.org/
- **FluentAssertions Documentation**: https://fluentassertions.com/
- **Moq Documentation**: https://github.com/moq/moq4
- **EF Core Testing**: https://learn.microsoft.com/en-us/ef/core/testing/
- **nopCommerce Testing Patterns**: See existing tests in Tests/Nop.Tests/

---

## Quality Standards Summary

**Minimum requirements for all plugins**:
- ✅ Unit tests for business logic (≥ 70% coverage)
- ✅ Integration tests for database operations and external APIs
- ✅ Manual testing completed (admin UI, public store, install/uninstall)
- ✅ All tests passing (100% pass rate)
- ✅ No skipped/ignored tests without documented reason

**Critical plugins (payment/shipping/tax/auth) require**:
- ✅ Higher coverage targets (≥ 80-90%)
- ✅ Comprehensive integration tests (all scenarios)
- ✅ Security testing (if applicable)
- ✅ Load testing for performance-critical operations

**Remember**: Testing is not optional. It's a quality gate that ensures plugin reliability and maintainability.
