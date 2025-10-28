# Agent Model Selection Strategy

## Overview

Claude Code supports multiple models with different capabilities and costs:
- **Sonnet**: Most capable, slower, higher cost
- **Haiku**: Fast, lower cost, good for straightforward tasks
- **Opus**: Most capable (when available), slowest, highest cost

## Default Behavior

**If agent YAML doesn't specify `model:`**:
- Inherits model from parent (Team Commander's model)
- Team Commander uses model specified by user or system default

**Current DEVGRU Framework Default**: Team Commander runs on Sonnet (most capable model)

## When to Specify Model in Agent YAML

### Use `model: sonnet` When

**Agent requires**:
- Complex reasoning and planning
- Architectural decision-making
- Multi-step analysis with context retention
- High-stakes tasks (security, payments, critical business logic)
- Synthesis of information from multiple sources

**Examples**:
- mission-commander (creates blueprints, makes architectural decisions)
- nopcommerce-plugin-developer (implements complex plugins, enforces quality standards)
- red-team (security analysis requires deep reasoning and vulnerability detection)

**Why sonnet for these agents**:
- **Accuracy**: Fewer errors in complex architectural decisions
- **Context Retention**: Maintains understanding across large codebases (nopCommerce is 100k+ LOC)
- **Standard Compliance**: Better at following nopCommerce coding conventions
- **Risk Mitigation**: High-stakes work (security, payments) needs highest quality

### Use `model: haiku` When

**Agent performs**:
- Straightforward, well-defined tasks
- Template-based work
- Simple CRUD operations
- Read-only analysis (grep, file reading)
- Quick validation checks

**Examples**:
- simple-validator (checks naming conventions, runs grep patterns)
- file-reader (extracts information from files without complex analysis)
- template-generator (fills in README/CHANGELOG templates with provided data)

**Why haiku for these agents**:
- **Speed**: Fast responses for simple tasks
- **Cost**: Lower cost for high-volume, low-complexity work
- **Sufficient**: Task complexity doesn't require sonnet's capabilities

### Omit `model:` (Inherit) When

**Agent is**:
- General-purpose
- User might want to control cost/speed trade-off
- Task complexity varies based on context

**Examples**:
- nopcommerce-data-specialist (sometimes simple entity addition, sometimes complex EF Core migrations)
- nopcommerce-ui-specialist (UI complexity varies: simple form vs. complex admin dashboard)
- Most execution specialists (complexity depends on specific task delegated)

**Why inherit for these agents**:
- **Flexibility**: User controls cost/speed for their specific use case
- **Context-Aware**: If user chose haiku model, simple tasks run faster/cheaper
- **Smart Default**: If user chose sonnet (framework default), complex tasks handled well

## Current Framework Agents

| Agent | Model | Rationale |
|-------|-------|-----------|
| mission-commander | sonnet | Complex planning, architectural decisions, blueprint synthesis requiring deep reasoning |
| nopcommerce-plugin-developer | sonnet | Complex implementation, quality enforcement, nopCommerce expertise, high-stakes work |
| red-team | (inherit) | Security analysis benefits from sonnet, but allow user control for cost |
| All data specialists | (inherit) | Complexity varies: simple entity vs. complex EF Core migrations |
| All UI specialists | (inherit) | Complexity varies: simple form vs. complex admin dashboard |
| All integration specialists | (inherit) | Complexity varies: simple API call vs. complex OAuth flow |
| coa-team | (inherit) | Planning benefits from sonnet, but task breakdown can work with haiku |
| analysis-team | (inherit) | Codebase analysis benefits from sonnet for retention, but simple searches work with haiku |
| debriefing-expert | (inherit) | After-action reviews benefit from sonnet's synthesis, but simple summaries work with haiku |

## Cost vs. Quality Trade-off

### Sonnet
- **Pro**: Best quality, handles complexity, fewer errors, better context retention
- **Con**: Higher cost, slower response
- **Use for**: Critical paths, complex tasks, high-stakes work (security, payments, core architecture)

### Haiku
- **Pro**: Fast, lower cost, good for simple tasks
- **Con**: May struggle with complexity, less context retention, more prone to errors on ambiguous tasks
- **Use for**: Simple tasks, template work, validation, read-only operations

### Opus (when available)
- **Pro**: Best possible quality, handles most complex reasoning
- **Con**: Highest cost, slowest response, not always available
- **Use for**: Extremely complex missions (multi-plugin integration, legacy codebase migration, novel architectural patterns)

## Recommendations

### For New Agents

**Default**: Omit `model:` (inherit)

**Specify `model: sonnet` if**:
- Agent makes architectural decisions
- Agent requires deep nopCommerce expertise
- Errors would be costly (security, payments, data integrity)
- Task requires synthesizing information from 5+ files
- Agent creates mission blueprints or coordinates multiple specialists
- Agent enforces quality standards (code review, compliance checking)

**Specify `model: haiku` if**:
- Agent follows clear templates
- Agent performs simple, well-defined operations
- Speed is more important than perfection
- Task is low-risk (documentation, simple validation)
- Agent does read-only file operations (grep, glob, read)

### Performance Monitoring

**If agent with `model: sonnet` consistently handles simple tasks**:
- Consider removing model specification (inherit)
- Allow user to control cost
- Example: If red-team only checks grep patterns, haiku sufficient

**If agent without model specification frequently fails**:
- Consider specifying `model: sonnet`
- Improves success rate for complex tasks
- Example: If nopcommerce-data-specialist struggles with EF Core mappings, add sonnet

### Agent YAML Syntax

**To specify model in agent YAML**:
```yaml
name: mission-commander
role: Strategic architect and mission planner
model: sonnet  # ← Add this line
instructions: |
  You are the mission-commander for nopCommerce plugin development...
```

**To inherit model (default)**:
```yaml
name: nopcommerce-data-specialist
role: Entity Framework Core data access specialist
# No model: line = inherit from parent (Team Commander)
instructions: |
  You are a data access specialist for nopCommerce...
```

## Framework Model Strategy

### Team Commander (Sonnet)
**Why**:
- Makes critical classification decisions
- Coordinates multiple specialists
- Enforces quality gates
- Handles mission failures and recovery

**Impact**: Team Commander runs on sonnet regardless of user selection

### Critical Agents (Sonnet)
**mission-commander**:
- Creates architectural blueprints
- Makes technology stack decisions
- Designs multi-agent coordination
- High cost of errors (wrong architecture = mission failure)

**nopcommerce-plugin-developer**:
- Implements nopCommerce plugins end-to-end
- Enforces coding standards and compliance
- Integrates complex third-party services
- High-stakes work (production code)

### Flexible Agents (Inherit)
**Most specialists**:
- Complexity varies by task
- User controls cost/speed trade-off
- Framework adapts to user preferences

## Model Selection Examples

| Task | Agent | Model | Reasoning |
|------|-------|-------|-----------|
| Create new payment plugin | mission-commander | sonnet | Complex architecture, security critical, blueprint required |
| Implement plugin from blueprint | nopcommerce-plugin-developer | sonnet | Complex implementation, quality enforcement, high-stakes |
| Add simple entity to plugin | nopcommerce-data-specialist | inherit (sonnet) | Benefits from sonnet for EF Core expertise |
| Fix typo in README | (direct execution) | (Team Commander sonnet) | Simple task, no specialist needed |
| Create unit tests | nopcommerce-test-specialist | inherit (sonnet) | Test quality benefits from sonnet, but simple tests work with haiku |
| Security audit | red-team | inherit (sonnet) | Security analysis requires deep reasoning |
| Fill in README template | template-generator | haiku | Template-based, straightforward, speed matters |
| Search codebase for pattern | (direct execution) | (Team Commander sonnet) | Grep/Glob sufficient |

## Future Optimization

**Potential enhancements**:
- Dynamic model selection based on task complexity (use complexity scoring from CLAUDE.md)
- Mission complexity score → model selection (0-2 points = haiku, 3-5 = sonnet, 6-10 = opus)
- Cost budget tracking (warn user if approaching token limits)
- Model performance metrics by agent (track success rate by model)

**Example dynamic selection**:
```
if mission_complexity_score <= 2:
    use haiku (simple task, speed matters)
elif mission_complexity_score <= 5:
    use sonnet (standard complexity)
else:
    use opus if available, else sonnet (very complex)
```

## Cost Management Tips

### Reduce Costs Without Sacrificing Quality

1. **Use Simple Task routing**: Let Team Commander handle trivial tasks directly (no specialist delegation)
2. **Specify haiku for read-only agents**: File readers, validators, grep searchers don't need sonnet
3. **Use Quick AAR instead of Full AAR**: Save cost on after-action reviews for simple missions
4. **Cache-friendly missions**: Reuse previous agent outputs instead of re-delegating
5. **Batch simple tasks**: Group multiple small tasks into one mission to reduce overhead

### When to Accept Higher Cost (Sonnet/Opus)

1. **High-stakes work**: Security, payments, authentication (cost of errors > cost of model)
2. **Complex architecture**: Multi-plugin integration, novel patterns (rework costs more than sonnet)
3. **Large codebases**: nopCommerce 100k+ LOC (context retention worth the cost)
4. **Critical path missions**: Production bugs, customer-facing features (quality non-negotiable)

## Monitoring Model Usage

**Track metrics** (optional, see mission-metrics.md):
- Average tokens per mission by model
- Success rate by model (sonnet vs. haiku vs. inherit)
- Rework rate by model (indicator of quality)
- Cost per mission by classification (Simple Task, Standard Mission, Complex Custom)

**Analyze trends**:
- Are haiku agents producing more rework? (May need sonnet)
- Are sonnet agents handling trivial tasks? (May be over-specified)
- Is user satisfied with speed/cost/quality balance?

## Conclusion

**Model selection is a cost/quality trade-off.** The DEVGRU framework defaults to sonnet for Team Commander and critical agents to ensure high-quality outcomes. Most specialists inherit model to allow user flexibility.

**Default strategy**: Sonnet for command and architecture, inherit for execution, haiku for simple validation.

**Remember**:
- Start with inherit (let user control cost)
- Specify sonnet only when quality is critical
- Specify haiku only when task is truly simple
- Monitor metrics and adjust based on outcomes

---

**Last Updated**: 2025-10-28
**Status**: Optional Enhancement
**Framework Version**: nopCommerce DEVGRU Mission Command Framework 1.0
