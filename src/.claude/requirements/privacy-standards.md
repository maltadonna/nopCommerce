# Privacy & GDPR Compliance Standards for nopCommerce Plugins

This document defines mandatory privacy and GDPR (General Data Protection Regulation) compliance requirements for all nopCommerce plugin development that processes personal data.

---

## When GDPR Compliance is Required

**GDPR applies to plugins that:**
- Process personal data of EU residents (regardless of where your business is located)
- Store customer information (names, emails, addresses, phone numbers)
- Track user behavior (analytics, cookies, profiling)
- Process payment information
- Integrate with third-party services that process personal data

**Personal Data Includes:**
- Names, email addresses, phone numbers
- IP addresses, device identifiers, cookies
- Purchase history, browsing behavior
- Location data
- Payment information
- Any data that can identify an individual

---

## GDPR Compliance Checklist

### Article 5: Data Processing Principles

#### Lawfulness, Fairness, and Transparency

- [ ] **Obtain valid consent before processing personal data**
  - Consent must be freely given, specific, informed, and unambiguous
  - Use clear, plain language (no legalese)
  - Separate consent for different processing purposes
  - Pre-ticked boxes are NOT valid consent

**Implementation Pattern:**
```csharp
public class ConsentModel
{
    /// <summary>
    /// Marketing emails consent (opt-in required)
    /// </summary>
    [Display(Name = "I agree to receive marketing emails")]
    public bool MarketingEmailsConsent { get; set; }

    /// <summary>
    /// Third-party data sharing consent (opt-in required)
    /// </summary>
    [Display(Name = "I agree to share my data with [specific partner]")]
    public bool ThirdPartyDataSharingConsent { get; set; }

    /// <summary>
    /// Timestamp when consent was given
    /// </summary>
    public DateTime ConsentGivenOnUtc { get; set; }

    /// <summary>
    /// IP address from which consent was given (for audit)
    /// </summary>
    public string ConsentIpAddress { get; set; }
}
```

- [ ] **Provide clear privacy policy and notices**
  - Explain what data is collected
  - Explain why data is collected (purpose)
  - Explain how data is used
  - Explain who data is shared with (third parties)
  - Explain how long data is retained
  - Link to full privacy policy

#### Purpose Limitation

- [ ] **Collect data only for specified, explicit purposes**
  - Document the purpose for each data field collected
  - Do not use data for purposes beyond original consent
  - Obtain new consent if purpose changes

**Implementation Pattern:**
```csharp
/// <summary>
/// Data processing purposes
/// </summary>
public enum DataProcessingPurpose
{
    /// <summary>
    /// Order fulfillment (necessary for contract)
    /// </summary>
    OrderFulfillment,

    /// <summary>
    /// Marketing communications (requires consent)
    /// </summary>
    Marketing,

    /// <summary>
    /// Analytics and improvement (legitimate interest)
    /// </summary>
    Analytics,

    /// <summary>
    /// Legal compliance (legal obligation)
    /// </summary>
    LegalCompliance
}

/// <summary>
/// Data processing record for GDPR audit
/// </summary>
public class DataProcessingRecord
{
    public string DataType { get; set; }
    public DataProcessingPurpose Purpose { get; set; }
    public string LegalBasis { get; set; }
    public DateTime CollectedOnUtc { get; set; }
    public int CustomerId { get; set; }
}
```

#### Data Minimization

- [ ] **Collect only necessary data**
  - Do not collect data "just in case"
  - Justify each field collected
  - Remove unnecessary fields from forms

**Example:**
```csharp
// BAD - Collecting unnecessary data
public class CustomerRegistrationModel
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string DateOfBirth { get; set; } // ❌ Not necessary for registration
    public string MaritalStatus { get; set; } // ❌ Not necessary
    public string Income { get; set; } // ❌ Not necessary
}

// GOOD - Minimal data collection
public class CustomerRegistrationModel
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    // Optional fields clearly marked
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

#### Accuracy

- [ ] **Keep personal data accurate and up-to-date**
  - Allow customers to update their information
  - Validate data accuracy on collection
  - Implement data correction mechanisms

#### Storage Limitation

- [ ] **Retain data only as long as necessary**
  - Define data retention policies
  - Automatically delete data after retention period
  - Document retention periods in privacy policy

**Implementation Pattern:**
```csharp
/// <summary>
/// Data retention policy
/// </summary>
public class DataRetentionPolicy
{
    /// <summary>
    /// Retention period for customer data
    /// </summary>
    public static readonly TimeSpan CustomerDataRetention = TimeSpan.FromDays(730); // 2 years

    /// <summary>
    /// Retention period for marketing consent
    /// </summary>
    public static readonly TimeSpan MarketingConsentRetention = TimeSpan.FromDays(365); // 1 year

    /// <summary>
    /// Retention period for logs
    /// </summary>
    public static readonly TimeSpan LogRetention = TimeSpan.FromDays(90); // 3 months
}

/// <summary>
/// Scheduled task to delete expired data
/// </summary>
public class DataRetentionTask : IScheduleTask
{
    private readonly ICustomerService _customerService;
    private readonly ILogger _logger;

    public async Task ExecuteAsync()
    {
        // Delete inactive customers after retention period
        var expirationDate = DateTime.UtcNow.Subtract(DataRetentionPolicy.CustomerDataRetention);

        var expiredCustomers = await _customerService.GetInactiveCustomersAsync(expirationDate);

        foreach (var customer in expiredCustomers)
        {
            // Anonymize or delete customer data
            await AnonymizeCustomerAsync(customer);
            await _logger.InformationAsync($"Customer {customer.Id} data anonymized due to retention policy");
        }
    }
}
```

#### Integrity and Confidentiality (Security)

- [ ] **Implement appropriate security measures**
  - See `.claude/requirements/security-standards.md` for detailed security requirements
  - Encrypt sensitive data at rest and in transit
  - Implement access controls
  - Use secure authentication

---

### Article 6: Legal Basis for Processing

Every data processing activity MUST have a valid legal basis. Choose one:

1. **Consent** - User explicitly agreed
2. **Contract** - Necessary for fulfilling a contract (e.g., order fulfillment)
3. **Legal Obligation** - Required by law (e.g., tax records)
4. **Vital Interests** - Protect someone's life
5. **Public Task** - Performing a public task
6. **Legitimate Interest** - Necessary for legitimate business interest (with safeguards)

**Implementation Pattern:**
```csharp
/// <summary>
/// Legal basis for data processing
/// </summary>
public enum LegalBasis
{
    Consent,
    Contract,
    LegalObligation,
    VitalInterests,
    PublicTask,
    LegitimateInterest
}

/// <summary>
/// Data processing activity with legal basis
/// </summary>
public class DataProcessingActivity
{
    public string ActivityName { get; set; }
    public string DataProcessed { get; set; }
    public LegalBasis LegalBasis { get; set; }
    public string LegalBasisJustification { get; set; }
}
```

---

### Article 7: Consent Requirements

- [ ] **Consent must be freely given**
  - No "forced consent" (service can't be conditioned on non-essential consent)
  - Provide genuine choice
  - Allow granular consent (separate consent for different purposes)

- [ ] **Consent must be specific**
  - Separate consent for each purpose
  - No bundled consent

- [ ] **Consent must be informed**
  - Explain what data is collected
  - Explain how data is used
  - Explain who data is shared with

- [ ] **Consent must be unambiguous**
  - Require clear affirmative action (checkbox, button click)
  - Pre-ticked boxes are NOT valid
  - Silence or inactivity is NOT consent

- [ ] **Maintain proof of consent**
  - Record when consent was given
  - Record what was consented to
  - Record how consent was obtained (IP address, timestamp)

**Implementation Pattern:**
```csharp
public class ConsentRecord : BaseEntity
{
    /// <summary>
    /// Customer who gave consent
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// What was consented to (e.g., "Marketing Emails", "Third-Party Sharing")
    /// </summary>
    public string ConsentType { get; set; }

    /// <summary>
    /// Text of consent statement shown to user
    /// </summary>
    public string ConsentText { get; set; }

    /// <summary>
    /// When consent was given
    /// </summary>
    public DateTime ConsentGivenOnUtc { get; set; }

    /// <summary>
    /// IP address from which consent was given
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// Whether consent is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When consent was withdrawn (if applicable)
    /// </summary>
    public DateTime? ConsentWithdrawnOnUtc { get; set; }
}
```

- [ ] **Allow easy withdrawal of consent**
  - Withdrawing consent must be as easy as giving it
  - Provide "unsubscribe" links in emails
  - Provide account settings to manage consent

---

### Article 15-22: Data Subject Rights

#### Right to Access (Article 15)

- [ ] **Allow customers to request their data**
  - Provide data in structured, machine-readable format (JSON, CSV)
  - Include all data about the customer
  - Respond within 30 days

**Implementation Pattern:**
```csharp
/// <summary>
/// Export customer data for GDPR data access request
/// </summary>
public async Task<CustomerDataExport> ExportCustomerDataAsync(int customerId)
{
    var customer = await _customerService.GetCustomerByIdAsync(customerId);

    var export = new CustomerDataExport
    {
        PersonalInformation = new
        {
            customer.Email,
            customer.Username,
            customer.FirstName,
            customer.LastName,
            customer.Phone,
            customer.CreatedOnUtc,
            customer.LastActivityDateUtc
        },
        Orders = await GetCustomerOrdersAsync(customerId),
        Addresses = await GetCustomerAddressesAsync(customerId),
        Consents = await GetCustomerConsentsAsync(customerId),
        ActivityLog = await GetCustomerActivityLogAsync(customerId)
    };

    return export;
}
```

#### Right to Rectification (Article 16)

- [ ] **Allow customers to update their data**
  - Provide account settings page
  - Allow data correction
  - Update data across all systems

#### Right to Erasure / "Right to be Forgotten" (Article 17)

- [ ] **Allow customers to request data deletion**
  - Delete or anonymize personal data
  - Notify third parties if data was shared
  - Exceptions: legal obligations, legal claims

**Implementation Pattern:**
```csharp
/// <summary>
/// Delete customer data for GDPR erasure request
/// </summary>
public async Task DeleteCustomerDataAsync(int customerId)
{
    var customer = await _customerService.GetCustomerByIdAsync(customerId);

    // Check if deletion is allowed (no active orders, no legal holds)
    if (await HasActiveOrdersAsync(customerId))
        throw new InvalidOperationException("Cannot delete customer with active orders");

    // Anonymize instead of hard delete (preserves referential integrity)
    customer.Email = $"deleted-{customer.Id}@anonymized.local";
    customer.Username = $"deleted-{customer.Id}";
    customer.FirstName = "Deleted";
    customer.LastName = "User";
    customer.Phone = null;
    customer.Deleted = true;

    await _customerService.UpdateCustomerAsync(customer);

    // Delete addresses
    var addresses = await _customerService.GetAddressesAsync(customerId);
    foreach (var address in addresses)
        await _customerService.DeleteAddressAsync(address);

    // Delete consents
    await DeleteCustomerConsentsAsync(customerId);

    // Log deletion for audit
    await _logger.InformationAsync($"Customer {customerId} data deleted per GDPR erasure request");
}
```

#### Right to Restriction of Processing (Article 18)

- [ ] **Allow customers to restrict data processing**
  - Mark customer data as "restricted"
  - Do not process restricted data except for storage

#### Right to Data Portability (Article 20)

- [ ] **Provide data in portable format**
  - Export data in JSON, CSV, or XML
  - Include all data provided by customer
  - Include data generated by automated processing

**Implementation Pattern:**
```csharp
/// <summary>
/// Export customer data in portable format
/// </summary>
public async Task<string> ExportCustomerDataAsJsonAsync(int customerId)
{
    var data = await ExportCustomerDataAsync(customerId);
    return JsonConvert.SerializeObject(data, Formatting.Indented);
}

public async Task<byte[]> ExportCustomerDataAsCsvAsync(int customerId)
{
    var data = await ExportCustomerDataAsync(customerId);
    // Convert to CSV format
    return ConvertToCsv(data);
}
```

#### Right to Object (Article 21)

- [ ] **Allow customers to object to data processing**
  - Especially for marketing purposes
  - Provide "opt-out" mechanisms
  - Stop processing if objection is valid

---

### Article 25: Data Protection by Design and by Default

- [ ] **Privacy by design**
  - Build privacy into plugin architecture
  - Minimize data collection from the start
  - Use pseudonymization where possible
  - Implement security measures from the beginning

- [ ] **Privacy by default**
  - Default settings should be privacy-friendly
  - Minimal data collection by default
  - Privacy-preserving options enabled by default

**Example:**
```csharp
public class PrivacySettings : ISettings
{
    /// <summary>
    /// Anonymize IP addresses in logs (privacy by default)
    /// </summary>
    public bool AnonymizeIpAddresses { get; set; } = true; // Default: ON

    /// <summary>
    /// Automatically delete inactive customer data (privacy by default)
    /// </summary>
    public bool AutoDeleteInactiveCustomers { get; set; } = true; // Default: ON

    /// <summary>
    /// Retention period for inactive customers (days)
    /// </summary>
    public int InactiveCustomerRetentionDays { get; set; } = 730; // 2 years
}
```

---

### Article 30: Records of Processing Activities

- [ ] **Maintain records of data processing activities**
  - What data is processed
  - Why it's processed (purpose)
  - Who has access to it
  - How long it's retained
  - Security measures in place

**Implementation Pattern:**
```csharp
/// <summary>
/// Record of processing activities (GDPR Article 30)
/// </summary>
public class ProcessingActivityRecord
{
    public string ActivityName { get; set; }
    public string Controller { get; set; } // Data controller (merchant)
    public string Purpose { get; set; }
    public string LegalBasis { get; set; }
    public List<string> DataCategories { get; set; } // "Contact data", "Payment data"
    public List<string> DataSubjects { get; set; } // "Customers", "Employees"
    public List<string> Recipients { get; set; } // Third parties who receive data
    public string RetentionPeriod { get; set; }
    public string SecurityMeasures { get; set; }
}
```

---

### Article 32: Security of Processing

- [ ] **Implement appropriate technical and organizational measures**
  - Encryption at rest and in transit
  - Access controls
  - Regular security testing
  - Incident response plan
  - See `.claude/requirements/security-standards.md` for detailed requirements

---

### Article 33-34: Data Breach Notification

- [ ] **Implement data breach detection**
  - Monitor for unauthorized access
  - Log access attempts
  - Alert on suspicious activity

- [ ] **Data breach response plan**
  - Notify supervisory authority within 72 hours
  - Notify affected customers if high risk
  - Document breach details

**Implementation Pattern:**
```csharp
/// <summary>
/// Data breach record
/// </summary>
public class DataBreachRecord : BaseEntity
{
    public DateTime BreachDetectedOnUtc { get; set; }
    public string BreachDescription { get; set; }
    public string DataAffected { get; set; }
    public int CustomersAffected { get; set; }
    public string RiskAssessment { get; set; } // Low, Medium, High
    public bool AuthorityNotified { get; set; }
    public DateTime? AuthorityNotifiedOnUtc { get; set; }
    public bool CustomersNotified { get; set; }
    public DateTime? CustomersNotifiedOnUtc { get; set; }
    public string MitigationMeasures { get; set; }
}
```

---

### Article 35: Data Protection Impact Assessment (DPIA)

**Required when processing is likely to result in high risk**, such as:
- Large-scale processing of sensitive data
- Systematic monitoring (tracking, profiling)
- Automated decision-making with legal effects

- [ ] **Conduct DPIA for high-risk processing**
  - Describe processing operations
  - Assess necessity and proportionality
  - Identify risks to rights and freedoms
  - Document mitigation measures

---

## Third-Party Data Sharing

### Requirements for Sharing Data with Third Parties

- [ ] **Obtain explicit consent for data sharing**
  - Name the specific third party
  - Explain what data is shared
  - Explain why data is shared

- [ ] **Data Processing Agreements (DPA)**
  - Require DPA with all third-party processors
  - Ensure third party is GDPR-compliant
  - Define data processing terms

- [ ] **International data transfers**
  - Ensure adequate protection (adequacy decision, standard contractual clauses, etc.)
  - Document safeguards for international transfers

**Implementation Pattern:**
```csharp
/// <summary>
/// Third-party data sharing record
/// </summary>
public class ThirdPartyDataSharing
{
    public string ThirdPartyName { get; set; }
    public string DataShared { get; set; }
    public string Purpose { get; set; }
    public bool ConsentObtained { get; set; }
    public DateTime? ConsentDate { get; set; }
    public bool DpaInPlace { get; set; }
    public string DpaReference { get; set; }
}
```

---

## Cookies and Tracking

### Cookie Consent (ePrivacy Directive)

- [ ] **Obtain consent before setting non-essential cookies**
  - Essential cookies (session, security) do not require consent
  - Analytics, marketing, advertising cookies REQUIRE consent
  - Provide granular cookie control

- [ ] **Cookie banner requirements**
  - Explain what cookies are used
  - Explain purpose of each cookie
  - Allow accept/reject
  - Link to cookie policy

**Implementation Pattern:**
```csharp
public enum CookieCategory
{
    Essential,      // No consent required
    Analytics,      // Requires consent
    Marketing,      // Requires consent
    Advertising     // Requires consent
}

public class CookieConsent
{
    public bool EssentialCookies { get; set; } = true; // Always enabled
    public bool AnalyticsCookies { get; set; }
    public bool MarketingCookies { get; set; }
    public bool AdvertisingCookies { get; set; }
    public DateTime ConsentGivenOnUtc { get; set; }
}
```

---

## Quality Gate Integration

**Critical Privacy Gates (BLOCK Mission Completion):**

1. ✅ **Data Minimization** - Collect only necessary data
2. ✅ **Consent Mechanism** - Valid consent obtained before processing (where required)
3. ✅ **Data Retention Policy** - Documented retention periods, auto-deletion implemented
4. ✅ **Data Subject Rights** - Access, rectification, erasure implemented
5. ✅ **Privacy Policy** - Clear, accessible privacy notice provided
6. ✅ **Third-Party Sharing** - Explicit consent + DPA in place
7. ✅ **Cookie Consent** - Banner with granular control (if cookies used)

**Privacy Verification Checklist:**
```
For plugins that process personal data:
- [ ] Documented legal basis for each data processing activity
- [ ] Consent mechanism implemented (if consent is legal basis)
- [ ] Data retention policy defined and implemented
- [ ] Customer data export functionality (data portability)
- [ ] Customer data deletion functionality (right to erasure)
- [ ] Privacy notice / policy accessible to users
- [ ] Third-party data sharing disclosed + DPA in place
- [ ] Cookie consent banner (if non-essential cookies used)
```

---

## GDPR Compliance Testing

### Manual Testing

- [ ] **Test data access request**
  - Request customer data export
  - Verify all data is included
  - Verify format is machine-readable

- [ ] **Test data deletion request**
  - Request customer data deletion
  - Verify data is anonymized or deleted
  - Verify no orphaned data remains

- [ ] **Test consent withdrawal**
  - Withdraw consent for marketing
  - Verify processing stops
  - Verify easy to withdraw

- [ ] **Test cookie consent**
  - Reject non-essential cookies
  - Verify cookies are not set
  - Verify functionality still works

---

## References

- **GDPR Official Text**: https://gdpr-info.eu/
- **ICO GDPR Guidance** (UK): https://ico.org.uk/for-organisations/guide-to-data-protection/guide-to-the-general-data-protection-regulation-gdpr/
- **EDPB Guidelines**: https://edpb.europa.eu/our-work-tools/general-guidance/gdpr-guidelines-recommendations-best-practices_en
- **nopCommerce GDPR Features**: https://docs.nopcommerce.com/en/running-your-store/gdpr-settings.html

---

**GDPR compliance is mandatory for processing EU residents' data. Non-compliance can result in fines up to €20 million or 4% of global annual turnover.**
