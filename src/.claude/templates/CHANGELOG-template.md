# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- {New features added but not yet released}

### Changed
- {Changes to existing functionality}

### Deprecated
- {Soon-to-be removed features}

### Removed
- {Removed features}

### Fixed
- {Bug fixes}

### Security
- {Security vulnerability fixes}

## [{version}] - {YYYY-MM-DD}

### Added
- Initial release
- {Feature 1 with brief description}
- {Feature 2 with brief description}
- {Feature 3 with brief description}

### Changed
- {Change 1 with brief description}
- {Change 2 with brief description}

### Fixed
- {Bug fix 1 with brief description}
- {Bug fix 2 with brief description}

### Security
- {Security update 1 with brief description}

---

## Version Guidelines

### Semantic Versioning

This plugin follows [Semantic Versioning](https://semver.org/):

- **MAJOR** version (X.0.0): Incompatible API changes or breaking changes
- **MINOR** version (0.X.0): New functionality in a backwards compatible manner
- **PATCH** version (0.0.X): Backwards compatible bug fixes

### Changelog Categories

Use these standard categories for all changes:

- **Added**: New features
- **Changed**: Changes to existing functionality
- **Deprecated**: Features that will be removed in future versions
- **Removed**: Features removed in this version
- **Fixed**: Bug fixes
- **Security**: Security vulnerability fixes

### Example Entry Format

```markdown
## [1.2.0] - 2025-01-15

### Added
- Multi-currency support for payment processing
- New admin dashboard widget showing transaction statistics
- Export transactions to CSV functionality

### Changed
- Improved error handling for failed API requests
- Updated UI to match nopCommerce 4.90 design

### Fixed
- Fixed issue where refunds were not processed correctly
- Resolved decimal rounding error in tax calculations

### Security
- Updated API authentication to use OAuth 2.0
```

---

## Template Instructions

1. **Keep entries chronological**: Newest versions at the top (after Unreleased section)
2. **Use past tense**: "Added feature" not "Add feature"
3. **Be specific**: Include enough detail for users to understand the change
4. **Link to issues**: Reference GitHub issues/PRs where applicable: `(#123)`
5. **Group by category**: Use the standard categories (Added, Changed, Fixed, etc.)
6. **Date format**: Use YYYY-MM-DD format for consistency
7. **Version links**: Add comparison links at the bottom of the file (optional)

---

## Version History Example

```markdown
## [1.1.0] - 2025-02-01

### Added
- Sandbox mode for testing without live transactions
- Webhook support for payment status updates

### Changed
- Improved payment form validation

### Fixed
- Fixed timeout issue with slow API responses

## [1.0.1] - 2025-01-20

### Fixed
- Fixed configuration page not saving settings
- Resolved error when plugin installed on fresh nopCommerce

## [1.0.0] - 2025-01-15

### Added
- Initial release
- Payment processing via {Provider} API
- Admin configuration page
- Transaction logging
```

---

## Version Comparison Links (Optional)

Add these at the bottom of your CHANGELOG for easy comparison between versions:

```markdown
[Unreleased]: https://github.com/{user}/{repo}/compare/v{latest}...HEAD
[{version}]: https://github.com/{user}/{repo}/compare/v{previous}...v{version}
[{previous}]: https://github.com/{user}/{repo}/releases/tag/v{previous}
```

Example:
```markdown
[Unreleased]: https://github.com/user/nop-plugin-paypal/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/user/nop-plugin-paypal/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/user/nop-plugin-paypal/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/user/nop-plugin-paypal/releases/tag/v1.0.0
```
