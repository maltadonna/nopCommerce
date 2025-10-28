---
name: /nop-optimize
description: Optimize nopCommerce plugin performance (database queries, caching, async operations)
---

# Optimize nopCommerce Plugin Performance

You are optimizing the performance of a nopCommerce plugin. This is typically a **SIMPLE TASK** that delegates directly to the performance specialist.

## Action Required

**If SIMPLE (specific performance issue)**: Delegate directly to nopcommerce-performance-specialist.
**If COMPLEX (systemic performance issues)**: Delegate to mission-commander for blueprint.

## Information to Gather from User

Ask the user for:

1. **Performance Issue**:
   - What is slow? (specific page, action, or operation)
   - How slow? (response time in ms or seconds)
   - When does it occur? (always, under load, with specific data)
   - How many users affected?

2. **Metrics** (if available):
   - Current response time
   - Database query count per request
   - Memory usage
   - Error rate (if timeouts occurring)

3. **Context**:
   - nopCommerce version (e.g., 4.90)
   - Plugin name
   - Environment (production, staging)
   - Scale (number of products, customers, orders)

4. **Performance Goals**:
   - Target response time (e.g., "under 500ms")
   - Acceptable query count (e.g., "less than 10")
   - Load requirements (e.g., "100 concurrent users")

## Delegation Command (for Simple)

Use the Task tool to delegate to nopcommerce-performance-specialist:

```
Optimize performance for the following nopCommerce plugin:

**Performance Issue**:
- Symptom: [e.g., "Product list page takes 5 seconds to load"]
- Where: [e.g., "Admin > Catalog > Products"]
- When: [e.g., "When viewing all products (10,000+ items)"]
- Impact: [e.g., "All admin users affected"]

**Current Metrics** (if available):
- Response Time: [e.g., 5000ms]
- Query Count: [e.g., 250 queries per request]
- Memory Usage: [e.g., 500MB]

**Context**:
- nopCommerce: [4.90]
- Plugin: [Nop.Plugin.{Group}.{Name}]
- Environment: [Production]
- Scale: [10,000 products, 1,000 customers]

**Performance Goals**:
- Target Response Time: [e.g., <500ms]
- Max Query Count: [e.g., <10]
- Load Requirement: [e.g., 50 concurrent users]

**Optimization Focus**:
[Database Queries / Caching / Async Operations / Resource Management / API Calls]

**Deliverables**:
1. Bottleneck analysis (what's causing slowness)
2. Optimization implementation (code changes)
3. Before/after metrics comparison
4. Load testing results
5. Monitoring recommendations

Apply nopCommerce-specific optimization patterns (IStaticCacheManager, .AsNoTracking(), pagination, etc.)
```

## Delegation Command (for Complex)

Use the Task tool to delegate to mission-commander:

```
Create comprehensive performance optimization blueprint for multiple issues:

**Performance Issues**:
1. [Issue 1: e.g., Slow product listing]
2. [Issue 2: e.g., High memory usage]
3. [Issue 3: e.g., Timeout under load]

**System Context**:
- nopCommerce: [4.90]
- Plugins: [list affected plugins]
- Infrastructure: [server specs, database]
- Scale: [data volumes, user load]

**Performance Goals**:
- Overall system response time: [target]
- Concurrent user capacity: [target]
- Reliability: [uptime target]

**Investigation Required**:
- Identify systemic bottlenecks
- Determine if infrastructure scaling needed
- Prioritize optimizations by impact

**Deliverables**:
- Comprehensive performance analysis
- Phased optimization plan
- Implementation of all optimizations
- Load testing validation
- Infrastructure recommendations

**Agent Assignment**:
- nopcommerce-performance-specialist: Database/caching/async optimization
- [Other specialists as needed]

Ensure performance meets targets at expected scale.
```

## Common Performance Issues Quick Reference

| Symptom | Likely Cause | Solution |
|---------|--------------|----------|
| Slow page load | N+1 queries | Fix with .Include() |
| High query count | No eager loading | Use .Include() |
| Repeated slow queries | No caching | Add IStaticCacheManager |
| Slow query | Missing index | Add index in EntityTypeBuilder |
| Memory growth | No .AsNoTracking() | Add for read-only queries |
| Large result set | No pagination | Use ToPagedListAsync() |
| Slow API calls | No timeout | Configure HttpClient timeout |
| Thread starvation | Blocking async | Remove .Result/.Wait() |
| Socket exhaustion | New HttpClient | Register in DI |
| Multiple issues | Systemic problem | mission-commander |

## Expected Outcome

- **Simple**: nopcommerce-performance-specialist analyzes, optimizes, and provides metrics
- **Complex**: mission-commander creates blueprint → Team Commander executes → Performance meets goals

## Performance Optimization Priorities

### 1. Database Queries (Highest Impact)
- Fix N+1 queries first
- Add missing indexes
- Use .AsNoTracking() for reads
- Implement pagination

### 2. Caching (High Impact)
- Cache frequently accessed data
- Use IStaticCacheManager
- Invalidate on updates
- Cache keys with entity IDs

### 3. Async Operations (Medium Impact)
- Remove blocking calls
- Use async all the way
- Configure timeouts

### 4. Resource Management (Medium Impact)
- Reuse HttpClient
- Dispose IDisposable
- Manage memory allocations

## Example Usage

### Example 1: Slow Database Queries
```
User: "My product list page is taking 5 seconds to load"

You:
[Gather information about query count, data volume]
[Classify as SIMPLE - specific performance issue]
[Delegate to nopcommerce-performance-specialist with metrics]

Expected Fix: N+1 query fix with .Include(), add caching
Result: 5000ms → 200ms
```

### Example 2: No Caching
```
User: "Category page queries database every time"

You:
[Classify as SIMPLE - caching issue]
[Delegate to nopcommerce-performance-specialist]

Expected Fix: Add IStaticCacheManager with proper cache keys
Result: Database hits reduced by 90%
```

### Example 3: Multiple Performance Issues
```
User: "Site is slow, timeouts under load, high memory usage"

You:
[Multiple issues - systemic problem]
[Classify as COMPLEX]
[Delegate to mission-commander for comprehensive analysis]

Expected: Phased optimization plan addressing all issues
```

---

**Remember**: Measure before and after optimization to prove improvement. Always load test at expected scale to ensure optimization holds under pressure.
