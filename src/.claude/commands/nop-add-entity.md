---
name: /nop-add-entity
description: Add a new entity with data access layer to an existing nopCommerce plugin
---

# Add Entity to nopCommerce Plugin

You are adding a new entity with data access layer to an existing nopCommerce plugin. Classify this request:

**This is a COMPLEX MISSION** because:
- Multiple files affected (entity, configuration, migration, repository, service)
- Architectural decisions required (indexing strategy, relationships)
- Multiple agents needed (nopcommerce-data-specialist, nopcommerce-test-specialist)

## Action Required

**Immediately delegate to mission-commander** for blueprint creation.

## Information to Gather from User (via mission-commander)

Ask the user for:

1. **Plugin Context**:
   - Which plugin? (name or path)
   - Plugin namespace

2. **Entity Details**:
   - Entity name (e.g., "ProductViewTrackerRecord", "CustomShippingRate")
   - Entity purpose/description

3. **Entity Properties**:
   - List of properties with data types
   - Which properties are required?
   - Which properties need indexes?
   - Relationships to other entities?

4. **Additional Requirements**:
   - Should entity support soft delete?
   - Should entity track creation/update timestamps?
   - Is caching needed for this entity?
   - What's the expected data volume? (for indexing strategy)

## Delegation Command

Use the Task tool to delegate to mission-commander:

```
Create a comprehensive mission blueprint for adding a new entity to a nopCommerce plugin:

**Plugin Context:**
- Plugin: [PluginName]
- Namespace: Nop.Plugin.[Group].[Name]

**Entity Specification:**
- Entity Name: [EntityName]
- Purpose: [What this entity represents]
- Base Class: [BaseEntity / BaseEntityWithDate / SoftDeleteEntity]

**Properties:**
[List all properties with types, e.g.:]
- Name (string, max 400, required, indexed)
- Description (string, max 4000, nullable)
- CustomerId (int, required, foreign key to Customer, indexed)
- DisplayOrder (int, required, indexed)
- IsActive (bool, required, indexed)
- CreatedOnUtc (DateTime, required)

**Relationships:**
[e.g., Many-to-one with Customer table]

**Performance Requirements:**
- Expected record count: [number]
- Indexing strategy: [which columns need indexes]
- Caching needed: [Yes/No]

**Deliverables:**
1. Domain entity class (Domain/{EntityName}.cs)
2. Entity configuration (Data/{EntityName}RecordBuilder.cs)
3. FluentMigrator migration (Data/Migrations/{Timestamp}_{EntityName}.cs)
4. Service interface (Services/I{EntityName}Service.cs)
5. Service implementation (Services/{EntityName}Service.cs)
6. Service registration in DependencyRegistrar
7. Unit tests for service
8. Integration tests for repository/database

**Agent Assignment:**
- nopcommerce-data-specialist: Entity, configuration, migration, repository, service
- nopcommerce-test-specialist: Unit and integration tests

**Quality Standards:**
- XML documentation on all public members
- Proper async/await usage
- Event publisher integration (EntityInserted/Updated/Deleted)
- Proper indexing for performance
- Zero compiler warnings

Ensure the entity follows nopCommerce 4.90 data access patterns and is production-ready.
```

## Expected Outcome

Mission-commander creates blueprint → Team Commander executes → Complete data access layer ready with tests.
