# Agent Framework Corrections Blueprint

**Document Version**: 1.0
**Created**: 2025-10-28
**Status**: Ready for Implementation
**Estimated Total Effort**: 12-16 hours

## Executive Summary

This blueprint addresses 15 identified issues across the DEVGRU Mission Command Framework discovered during a comprehensive production readiness review. Issues are categorized by priority and include detailed implementation steps, acceptance criteria, and affected files.

**Framework Context**:
- **Primary Command Authority**: `.claude/CLAUDE.md` (333 lines)
- **Technical Reference**: `src/CLAUDE.md` (comprehensive nopCommerce standards)
- **Operational Protocols**: 10 slash commands in `.claude/commands/`
- **Specialist Agents**: 18 agents in `.claude/agents/`
- **Current State**: Production ready with recommended improvements

**Overall Assessment**: 8.0/10 - Framework is functional and well-designed, but improvements will enhance reliability, efficiency, and error handling.

---

## Priority Matrix

| Priority | Count | Total Effort | Status |
|----------|-------|--------------|--------|
| **CRITICAL** | 4 | 4-6 hours | Must complete before production |
| **IMPORTANT** | 5 | 5-7 hours | Complete within 2 weeks of production |
| **ENHANCEMENT** | 6 | 3-4 hours | Optimize as time permits |

---

## CRITICAL PRIORITY ISSUES

### CRIT-1: Mission State Persistence Not Implemented

**Problem Statement**:
The framework lacks mission state tracking and persistence. If a mission is interrupted (user closes terminal, system crash, agent timeout), there's no way to resume from where it stopped. Team Commander responsibilities include monitoring progress, but no mechanism exists to track state.

**Impact**:
- ❌ Lost work if mission interrupted
- ❌ No visibility into multi-step mission progress
- ❌ Can't resume after blocker resolution
- ❌ Difficult to track which blueprint tasks are complete

**Current Behavior**:
```markdown
Team Commander delegates to mission-commander → mission-commander creates blueprint →
Team Commander delegates to specialists → ??? (no state tracking)
```

**Root Cause**:
- `.claude/CLAUDE.md` mentions TodoWrite in tools list but provides no guidance on when/how to use it
- `mission-commander.md` line 4 lists TodoWrite in tools but has zero usage instructions
- No requirement for Team Commander to track mission state

**Solution**:
Add mandatory TodoWrite usage requirements to `.claude/CLAUDE.md` and update mission-commander agent specification.

**Implementation Steps**:

1. **Update `.claude/CLAUDE.md` - Add Mission State Tracking Section**

   **Location**: After line 98 (end of Phase 2), before Phase 3: Quality Gates

   **Insert New Section**:
   ```markdown
   ### Mission State Tracking (TodoWrite)

   **MANDATORY for ALL missions** (Simple Tasks may skip if <5 minutes):

   **At Mission Start:**
   1. Create parent mission todo: `"Mission: {User's objective}"`
   2. Set status: `in_progress`
   3. Add child todos for each major step (classification, delegation, verification)

   **During Execution:**
   1. Mark current task as `in_progress` BEFORE starting
   2. Mark completed tasks as `completed` IMMEDIATELY after finishing
   3. Create new todos if additional steps discovered
   4. Update todos if blockers encountered

   **Mission Completion:**
   1. Ensure all child todos are `completed`
   2. Mark parent mission todo as `completed`

   **Example Mission Todo Structure:**
   ```
   ☑ Mission: Create new PayPal payment plugin (completed)
     ☑ Classify mission complexity (completed)
     ☑ Delegate to mission-commander for blueprint (completed)
     ☑ Execute blueprint with nopcommerce-plugin-developer (completed)
     ☑ Verify quality gates (completed)
     ☑ Confirm filesystem changes (completed)
   ```

   **Interruption Handling:**
   If mission interrupted, todo state preserves:
   - What was completed
   - What's in progress
   - What remains pending
   - User can ask: "What's the status?" and you can reference todos
   ```

2. **Update `mission-commander.md` - Add TodoWrite Requirements**

   **Location**: After line 32 (Phase 1: Discovery & Analysis)

   **Insert**:
   ```markdown

   **Track Blueprint Progress:**
   Use TodoWrite to create blueprint task list:
   1. Create parent: "Blueprint: {Mission objective}"
   2. Add child todos for each investigation area:
      - [ ] Investigate existing implementations
      - [ ] Research nopCommerce compatibility
      - [ ] Identify required specialists
      - [ ] Define architectural approach
      - [ ] Document quality standards
      - [ ] Create task breakdown
   3. Mark tasks complete as you progress
   4. Final blueprint includes task list for Team Commander execution
   ```

3. **Update `nopcommerce-plugin-developer.md` - Add Self-Tracking**

   **Location**: After line 74 (Step 2: Validate Understanding)

   **Insert**:
   ```markdown

   #### TodoWrite Self-Tracking (for multi-step implementations)

   If implementation has >3 major steps, create todos:
   ```
   ☐ Project structure created
   ☐ Core implementation (IPlugin, DependencyRegistrar)
   ☐ Integration points (events, widgets, routes)
   ☐ Configuration & admin UI
   ☐ Quality implementation (docs, error handling, logging)
   ☐ Installation logic
   ```

   Mark complete as you finish each step.
   ```

**Files Affected**:
- `.claude/CLAUDE.md` (lines 98-99, insert new section)
- `.claude/agents/mission-command/mission-commander.md` (line 32, insert tracking requirements)
- `.claude/agents/mission-execution/nopcommerce-plugin-developer.md` (line 74, insert self-tracking)

**Acceptance Criteria**:
- [ ] CLAUDE.md explicitly requires TodoWrite for all missions
- [ ] mission-commander.md includes blueprint progress tracking
- [ ] Example todo structures provided
- [ ] Interruption handling documented
- [ ] Self-tracking guidance added to nopcommerce-plugin-developer

**Dependencies**: None

**Estimated Effort**: 1.5 hours

---

### CRIT-2: Insufficient Bash Permissions for Quality Gate Verification

**Problem Statement**:
`.claude/settings.local.json` only allows `Bash(claude mcp add:*)`, but quality gates require filesystem verification and build verification:

```markdown
CLAUDE.md lines 133-136:
- [ ] Use Read tool to verify file changes exist
- [ ] Use Bash (dotnet build) to confirm compilation succeeds  ← BLOCKED
- [ ] Confirm plugin appears in expected location
```

Current permissions prevent `dotnet build`, making quality gate verification impossible.

**Impact**:
- ❌ **CRITICAL**: Cannot verify builds succeed (required quality gate)
- ❌ Cannot verify no compiler warnings
- ❌ Cannot run tests
- ❌ Quality gates are unenforceable

**Current State**:
```json
{
  "permissions": {
    "allow": [
      "Bash(claude mcp add:*)",
      "WebFetch(domain:docs.nopcommerce.com)",
      "WebSearch"
    ]
  }
}
```

**Root Cause**:
Overly restrictive permissions during initial setup. Security-conscious approach, but prevents core functionality.

**Solution**:
Expand bash permissions to include build, test, and git commands needed for quality verification.

**Implementation Steps**:

1. **Update `.claude/settings.local.json`**

   **Replace entire file with**:
   ```json
   {
     "permissions": {
       "allow": [
         "Bash(claude mcp add:*)",
         "Bash(dir:*)",
         "Bash(dotnet build*)",
         "Bash(dotnet clean*)",
         "Bash(dotnet test*)",
         "Bash(dotnet restore*)",
         "Bash(git status*)",
         "Bash(git diff*)",
         "Bash(git log*)",
         "Bash(git show*)",
         "Bash(npm install*)",
         "Bash(npx gulp*)",
         "WebFetch(domain:docs.nopcommerce.com)",
         "WebSearch"
       ],
       "deny": [
         "Bash(git push*)",
         "Bash(git reset --hard*)",
         "Bash(rm -rf*)",
         "Bash(del /f*)"
       ],
       "ask": [
         "Bash(git commit*)",
         "Bash(git add*)"
       ]
     }
   }
   ```

2. **Document Permission Rationale**

   Create `.claude/requirements/permissions-rationale.md`:
   ```markdown
   # Bash Permissions Rationale

   ## Allowed Commands

   **Build & Test** (required for quality gates):
   - `dotnet build*` - Verify compilation succeeds, no warnings
   - `dotnet test*` - Run unit/integration tests
   - `dotnet clean*` - Clean build artifacts
   - `dotnet restore*` - Restore NuGet packages

   **Version Control** (read-only, for context):
   - `git status*` - Check working tree state
   - `git diff*` - View code changes
   - `git log*` - View commit history
   - `git show*` - View specific commits

   **Frontend Build** (required for plugin assets):
   - `npm install*` - Install dependencies
   - `npx gulp*` - Build frontend assets

   **File Operations**:
   - `dir:*` - Directory listing (already allowed)

   ## Denied Commands (destructive)

   - `git push*` - Prevent accidental remote pushes
   - `git reset --hard*` - Prevent data loss
   - `rm -rf*` / `del /f*` - Prevent accidental file deletion

   ## Ask Before Running (require user confirmation)

   - `git commit*` - User should review changes before commit
   - `git add*` - User should confirm what's staged

   ## Security Considerations

   All allowed commands are:
   - Read-only OR
   - Local build/test operations OR
   - Explicitly required by quality gates

   No commands can:
   - Modify remote repositories
   - Delete files permanently
   - Expose credentials
   ```

**Files Affected**:
- `.claude/settings.local.json` (replace entire file)
- `.claude/requirements/permissions-rationale.md` (create new)

**Acceptance Criteria**:
- [ ] Can run `dotnet build` to verify compilation
- [ ] Can run `dotnet test` to verify tests pass
- [ ] Can run `git status` and `git diff` for context
- [ ] Cannot run destructive commands (git push, reset --hard)
- [ ] Permissions documented with rationale

**Dependencies**: None

**Estimated Effort**: 0.5 hours

---

### CRIT-3: No Failure Recovery or Rollback Protocols

**Problem Statement**:
Framework defines blocker escalation (CLAUDE.md lines 174-195) but provides no guidance for:
- What happens if a specialist fails to complete a task?
- What happens if mission-commander produces an infeasible blueprint?
- How to clean up partial changes if mission fails mid-execution?
- How to restore previous working state?

**Impact**:
- ❌ **CRITICAL**: Codebase can be left in broken state
- ❌ No systematic way to abort failed missions
- ❌ Partial file changes with no cleanup
- ❌ User confusion about next steps

**Current Behavior**:
```
Mission fails mid-execution → Files partially created → No cleanup →
User left with broken plugin structure → Manual cleanup required
```

**Example Failure Scenario**:
1. `/nop-new-plugin` creates plugin folder structure
2. nopcommerce-plugin-developer creates 5 of 10 required files
3. Agent encounters blocker (missing dependency, invalid configuration)
4. Mission escalated to user
5. **No guidance on what to do with partial files**

**Root Cause**:
Framework focused on success path, didn't account for failure modes.

**Solution**:
Add comprehensive failure recovery and rollback protocols to CLAUDE.md.

**Implementation Steps**:

1. **Add New Section to `.claude/CLAUDE.md`**

   **Location**: After line 195 (end of Blocker Escalation Protocol), before Phase 5

   **Insert New Section**:
   ```markdown

   ### Mission Failure Recovery Protocol

   If mission **cannot proceed** due to blockers, critical errors, or infeasible requirements:

   #### Step 1: STOP Execution Immediately

   - Mark current todo as blocked (update status with blocker info)
   - DO NOT create additional files or make further changes
   - DO NOT delegate to more agents
   - Preserve current state for assessment

   #### Step 2: ASSESS Impact & State

   **Determine what was completed**:
   1. Use TodoWrite to review what's `completed` vs `in_progress` vs `pending`
   2. Use `git status` to see what files were modified/created
   3. Use `git diff` to see specific changes

   **Categorize the failure**:
   - **Recoverable**: Issue can be fixed with user input or alternative approach
   - **Partially Complete**: Some deliverables finished, can save progress
   - **Critical Failure**: Must rollback all changes

   #### Step 3: DOCUMENT Failure State

   **Create failure report**:
   ```
   MISSION FAILURE REPORT

   Mission: [Original objective]
   Classification: [Simple Task / Standard Mission / Complex Custom]
   Failure Point: [Where mission failed]

   Completed Work:
   - [List completed tasks from todos]

   Partial Work:
   - [List in-progress items]

   Blocker:
   - Issue: [What went wrong]
   - Root Cause: [Technical reason]
   - Impact: [Why mission cannot proceed]

   File Changes:
   - Created: [List new files]
   - Modified: [List changed files]
   - Deleted: [List removed files]
   ```

   #### Step 4: OFFER User Options

   **Present recovery options**:

   ```
   RECOVERY OPTIONS

   Option 1: ROLLBACK (Clean slate)
   - Revert all changes: git restore .
   - Delete new files: [list files to delete]
   - State: Back to pre-mission condition
   - Recommendation: [When to choose this]

   Option 2: SAVE PROGRESS (Partial completion)
   - Keep completed work: [list what's done]
   - Document remaining work: [list what's needed]
   - State: Partial implementation saved
   - Recommendation: [When to choose this]

   Option 3: FIX AND CONTINUE (Resume mission)
   - Required fixes: [list what user must provide/fix]
   - Resume point: [which task to restart from]
   - Estimated time: [how long to complete]
   - Recommendation: [When to choose this]

   Option 4: ALTERNATIVE APPROACH (Pivot)
   - Alternative strategy: [describe different approach]
   - Rollback required: [Yes/No]
   - Trade-offs: [what's different]
   - Recommendation: [When to choose this]

   Recommended Option: [Your assessment with justification]
   ```

   #### Step 5: EXECUTE User Decision

   **Option 1 - Rollback**:
   ```bash
   # Revert modified files
   git restore .

   # Remove new files (if not tracked)
   # Use Read tool to confirm files exist before deletion
   # List files for user to manually delete (safer than rm)
   ```

   **Option 2 - Save Progress**:
   - Create `INCOMPLETE.md` documenting remaining work
   - Update todos to mark what's done vs pending
   - Provide user with next steps

   **Option 3 - Fix and Continue**:
   - Wait for user to provide fixes
   - Update mission context with new information
   - Resume from failed task (not from beginning)

   **Option 4 - Alternative Approach**:
   - Rollback if needed
   - Reclassify mission with new approach
   - Execute alternative protocol

   #### Critical Failures Requiring Immediate Rollback

   **ALWAYS rollback if**:
   - Build is broken (compilation errors)
   - Core nopCommerce files modified (violation of standards)
   - Database corruption risk (migration failures)
   - Security vulnerability introduced (XSS, SQL injection)
   - Plugin structure violates nopCommerce conventions

   **Never leave codebase in non-compiling state.**

   #### Blueprint Infeasibility

   If mission-commander produces infeasible blueprint:

   1. **DO NOT execute the blueprint**
   2. Document why blueprint is infeasible:
      - Technical constraints violated
      - nopCommerce patterns not followed
      - Required services don't exist
      - Architectural approach flawed
   3. **Escalate back to mission-commander** with context:
      ```
      Blueprint cannot be executed because:
      - [Specific technical reason]
      - [nopCommerce constraint]
      - [Missing dependency]

      Required changes to blueprint:
      - [What needs to change]
      - [Alternative approach]
      ```
   4. mission-commander revises blueprint
   5. Team Commander validates revised blueprint before execution

   #### Specialist Failure

   If specialist agent fails to complete assigned task:

   1. Review specialist's deliverables against acceptance criteria
   2. Identify what's missing or incorrect
   3. **Option A**: Re-delegate to same specialist with clarified requirements
   4. **Option B**: Delegate to alternative specialist if different expertise needed
   5. **Option C**: Escalate to mission-commander if task definition is wrong
   6. **Option D**: Abort mission if task is infeasible

   #### Recovery Success Criteria

   Mission recovery is successful when:
   - User understands current state
   - User has clear options with trade-offs
   - Codebase is in known state (working or cleanly rolled back)
   - Next steps are documented
   - No orphaned files or partial changes
   ```

**Files Affected**:
- `.claude/CLAUDE.md` (line 195, insert new section ~200 lines)

**Acceptance Criteria**:
- [ ] 4-step failure recovery process documented
- [ ] User options template provided (4 options)
- [ ] Rollback procedures defined
- [ ] Critical failure criteria listed
- [ ] Blueprint infeasibility handling defined
- [ ] Specialist failure handling defined

**Dependencies**:
- CRIT-1 (requires TodoWrite for state assessment)
- CRIT-2 (requires git commands for rollback)

**Estimated Effort**: 2 hours

---

### CRIT-4: Agent Coordination Protocols Missing

**Problem Statement**:
Framework has 18 specialist agents but no documented protocols for:
- How agents communicate with each other
- How agents share context from previous agent's work
- How agents resolve conflicting approaches
- What happens when agents need to collaborate on same files

**Impact**:
- ❌ Context loss between agent handoffs
- ❌ Agents might make conflicting decisions
- ❌ No clear escalation path for agent disagreements
- ❌ Inefficient re-discovery of information

**Example Scenario**:
```
mission-commander creates blueprint →
nopcommerce-plugin-developer creates plugin structure →
nopcommerce-data-specialist adds entities →
nopcommerce-ui-specialist creates admin views →

Question: How does ui-specialist know what entities were created by data-specialist?
Answer: Currently undefined
```

**Root Cause**:
Framework focused on Team Commander → Agent delegation, not Agent → Agent coordination.

**Solution**:
Add inter-agent coordination protocols to CLAUDE.md and mission-commander.md.

**Implementation Steps**:

1. **Add New Section to `.claude/CLAUDE.md`**

   **Location**: After line 97 (end of "Monitor execution" section), before Phase 3

   **Insert New Section**:
   ```markdown

   ### Agent Coordination Protocols

   When multiple specialists work on the same mission, coordination is required to prevent conflicts and ensure context sharing.

   #### Coordination Pattern 1: Sequential Handoff

   **When**: Tasks must be done in order (A must complete before B starts)

   **Process**:
   1. Team Commander delegates to Agent A with full context
   2. Agent A completes task and reports deliverables
   3. **Team Commander captures Agent A's deliverables**:
      - Files created/modified
      - Architectural decisions made
      - Important context for next agent
   4. Team Commander delegates to Agent B with:
      - Original mission context
      - **Agent A's deliverables and decisions**
      - Specific task for Agent B
      - Acceptance criteria

   **Example**:
   ```
   Task: Add new entity to existing plugin

   Delegation to nopcommerce-data-specialist:
   "Create Customer entity with EF Core mapping for plugin Nop.Plugin.Misc.MyPlugin.

   Context: Plugin already exists at Plugins/Nop.Plugin.Misc.MyPlugin/
   Task: Create domain entity, EF Core mapping, migration
   Report back: Entity structure, table name, relationships"

   [Agent completes]

   Delegation to nopcommerce-ui-specialist:
   "Create admin UI for Customer entity in plugin Nop.Plugin.Misc.MyPlugin.

   Context from data-specialist:
   - Entity: Customer with properties: Id, Name, Email, Phone, CreatedOnUtc
   - Table: MyPlugin_Customer
   - Service: ICustomerService already registered

   Task: Create admin controller, model, view for CRUD operations
   Your requirements: Follow entity structure above, use service for data access"
   ```

   #### Coordination Pattern 2: Parallel Execution

   **When**: Tasks can be done simultaneously without conflicts

   **Process**:
   1. Team Commander identifies independent tasks
   2. **Verify no file conflicts**:
      - Agent A works on files X, Y
      - Agent B works on files M, N
      - No overlap = safe to parallelize
   3. Delegate to both agents with **clear file ownership**
   4. Monitor both for completion
   5. If one fails, other may need to adjust

   **Example**:
   ```
   Task: Add payment gateway and shipping provider to plugin

   Delegation to nopcommerce-integration-specialist (Agent A):
   "Implement IPaymentMethod for PayPal integration

   Your files:
   - Services/PayPalPaymentService.cs (create)
   - Models/PayPalPaymentModel.cs (create)
   - Controllers/PayPalPaymentController.cs (create)
   - Views/PayPalPayment/ (create)

   DO NOT modify: DependencyRegistrar.cs (wait for coordination)"

   Delegation to nopcommerce-integration-specialist (Agent B):
   "Implement IShippingRateComputationMethod for FedEx integration

   Your files:
   - Services/FedExShippingService.cs (create)
   - Models/FedExShippingModel.cs (create)
   - Controllers/FedExShippingController.cs (create)
   - Views/FedExShipping/ (create)

   DO NOT modify: DependencyRegistrar.cs (wait for coordination)"

   After both complete:
   Delegation to nopcommerce-plugin-developer:
   "Register services in DependencyRegistrar for both integrations

   Context:
   - Agent A created: IPayPalPaymentService, PayPalPaymentService
   - Agent B created: IFedExShippingService, FedExShippingService

   Task: Add both to DependencyRegistrar.cs"
   ```

   #### Coordination Pattern 3: Collaborative Conflict Resolution

   **When**: Agents propose conflicting approaches for same problem

   **Process**:
   1. Agent A suggests approach X
   2. Agent B suggests approach Y (conflicts with X)
   3. **Team Commander detects conflict**
   4. Escalate to mission-commander with both approaches
   5. mission-commander makes architectural decision
   6. Team Commander enforces decision with both agents

   **Example**:
   ```
   Conflict: Caching strategy for product data

   Agent A (nopcommerce-data-specialist):
   "Use IStaticCacheManager with 60-minute expiration"

   Agent B (nopcommerce-performance-specialist):
   "Use Redis distributed cache with sliding expiration"

   Team Commander escalates to mission-commander:
   "Conflicting caching strategies proposed for product data:
   - Option 1: IStaticCacheManager (simpler, works for single-server)
   - Option 2: Redis distributed cache (complex, scales better)

   Context: Current deployment is single-server, future may scale
   Decision needed: Which caching strategy to implement?"

   mission-commander decides:
   "Use IStaticCacheManager for initial release (matches nopCommerce patterns,
   simpler deployment). Document Redis migration path for future scaling.

   Rationale: Follow nopCommerce conventions, optimize for current deployment,
   maintain upgrade path."

   Team Commander enforces:
   - Direct Agent A to implement IStaticCacheManager
   - Direct Agent B to document Redis migration steps for future
   ```

   #### Context Sharing Best Practices

   **When delegating to subsequent agents, always include**:
   1. **Original mission objective** (why are we doing this?)
   2. **Previous agent deliverables** (what exists now?)
   3. **Architectural decisions** (what patterns must be followed?)
   4. **Current task scope** (what should THIS agent do?)
   5. **Acceptance criteria** (how to know it's done?)

   **Template**:
   ```
   Delegation to {agent-name}:

   MISSION: {original user objective}

   CONTEXT FROM PREVIOUS AGENTS:
   - {agent-name}: {what they created/decided}
   - {agent-name}: {what they created/decided}

   YOUR TASK:
   {specific task for this agent}

   ARCHITECTURAL DECISIONS (must follow):
   - {decision 1}
   - {decision 2}

   FILES YOU OWN:
   - {files this agent should create/modify}

   FILES YOU MUST NOT TOUCH:
   - {files being handled by other agents}

   ACCEPTANCE CRITERIA:
   - {criterion 1}
   - {criterion 2}
   ```

   #### Conflict Prevention Rules

   **File Ownership**:
   - Only ONE agent modifies a file at a time
   - If multiple agents need same file, sequence them or Team Commander merges

   **Architectural Authority**:
   - mission-commander's blueprint is authoritative
   - Specialists implement blueprint, don't redesign it
   - Architectural conflicts escalate to mission-commander

   **Quality Standards**:
   - All agents follow same quality checklist (from CLAUDE.md Phase 3)
   - No agent can lower quality bar
   - Higher standards are acceptable

   #### When to Re-Delegate to Same Agent

   **Re-delegate if**:
   - Agent's deliverables don't meet acceptance criteria
   - Quality gates failed
   - User requested changes to agent's work
   - New requirements added to agent's scope

   **Provide when re-delegating**:
   - What was wrong/missing from first attempt
   - Clarified requirements
   - Updated acceptance criteria
   - Additional context discovered

   **Do NOT re-delegate if**:
   - Different expertise needed (delegate to different agent)
   - Original task definition was wrong (escalate to mission-commander)
   - Agent reached documented limitation (choose alternative approach)
   ```

2. **Add Coordination Section to `mission-commander.md`**

   **Location**: After line 65 (before Phase 3)

   **Insert**:
   ```markdown

   ### Multi-Agent Coordination in Your Blueprints

   When your blueprint requires multiple specialists, design for coordination:

   **Define Clear Interfaces**:
   - Specify what Agent A must deliver for Agent B to consume
   - Document shared data structures
   - Define service contracts between components

   **Sequence Appropriately**:
   - Identify dependencies (A must finish before B starts)
   - Identify parallelizable work (A and B can run simultaneously)
   - Document in blueprint: "Sequential" vs "Parallel"

   **Specify File Ownership**:
   - Assign files to specific agents
   - Flag shared files that need coordination
   - Identify integration points

   **Blueprint Template with Coordination**:
   ```
   ## Mission Blueprint: {Objective}

   ### Agent Assignment & Coordination

   **Phase 1 (Sequential)**:
   1. nopcommerce-plugin-developer → Create plugin structure
      - Deliverables: Project files, DependencyRegistrar stub, plugin.json
      - Next agent needs: Plugin namespace, folder structure

   2. nopcommerce-data-specialist → Create entities
      - Depends on: Plugin namespace from Phase 1
      - Deliverables: Entities, EF Core mappings, services
      - Next agent needs: Entity structures, service interfaces

   **Phase 2 (Parallel)**:
   3a. nopcommerce-ui-specialist → Create admin UI
      - Depends on: Entity structures from Phase 1.2
      - Files: Controllers/, Models/, Views/
      - Delivers: Admin CRUD interfaces

   3b. nopcommerce-integration-specialist → Create API
      - Depends on: Services from Phase 1.2
      - Files: API/, DTOs/
      - Delivers: REST endpoints

   **Phase 3 (Sequential - Integration)**:
   4. nopcommerce-plugin-developer → Register all services
      - Depends on: Phases 1, 2 complete
      - Files: DependencyRegistrar.cs
      - Delivers: All services registered

   5. nopcommerce-test-specialist → Create tests
      - Depends on: All phases complete
      - Delivers: Unit and integration tests

   ### Coordination Points

   - **After Phase 1.2**: Team Commander provides entity details to both Phase 2 agents
   - **Before Phase 3**: Team Commander confirms no file conflicts from Phase 2
   - **After Phase 4**: Team Commander verifies build succeeds before Phase 5
   ```
   ```

**Files Affected**:
- `.claude/CLAUDE.md` (line 97, insert coordination section ~150 lines)
- `.claude/agents/mission-command/mission-commander.md` (line 65, insert blueprint coordination ~80 lines)

**Acceptance Criteria**:
- [ ] 3 coordination patterns documented (Sequential, Parallel, Conflict Resolution)
- [ ] Context sharing template provided
- [ ] File ownership rules defined
- [ ] Conflict prevention rules defined
- [ ] mission-commander blueprint template includes coordination
- [ ] Examples provided for each pattern

**Dependencies**: None

**Estimated Effort**: 2 hours

---

## IMPORTANT PRIORITY ISSUES

### IMP-1: Testing Requirements Inconsistent Across Documents

**Problem Statement**:
Testing standards differ between CLAUDE.md and src/CLAUDE.md:

- **CLAUDE.md line 127**: "Unit tests created and passing (for significant logic)"
- **src/CLAUDE.md line 519**: "Unit tests for business logic (≥ 70% coverage)"

"Significant logic" is subjective and undefined. Coverage target only appears in one document.

**Impact**:
- ⚠️ Inconsistent testing enforcement
- ⚠️ Agents may interpret "significant" differently
- ⚠️ Quality gate pass/fail is ambiguous

**Solution**:
Harmonize testing requirements across both documents with clear, measurable criteria.

**Implementation Steps**:

1. **Update `.claude/CLAUDE.md` Quality Gates**

   **Location**: Line 127-131 (Testing & Documentation section)

   **Replace**:
   ```markdown
   **Testing & Documentation:**
   - [ ] Unit tests created and passing (for significant logic)
   - [ ] Installation/uninstallation tested
   - [ ] README or inline documentation complete
   - [ ] Localization resources added
   ```

   **With**:
   ```markdown
   **Testing & Documentation:**
   - [ ] Unit tests created and passing (required for: business logic, calculations, data validation, algorithms)
   - [ ] Test coverage ≥ 70% for business logic classes (use dotnet test --collect:"XPlat Code Coverage")
   - [ ] Integration tests for database operations, external APIs, plugin install/uninstall
   - [ ] Manual testing completed: admin UI workflows, public store display, multi-store scenarios (if applicable)
   - [ ] README.md created from template (see .claude/templates/README-template.md)
   - [ ] CHANGELOG.md updated with version changes
   - [ ] XML documentation complete on all public members
   - [ ] Localization resources added for all user-facing strings
   ```

2. **Create Test Requirements Reference**

   **Create** `.claude/requirements/testing-standards.md`:
   ```markdown
   # Testing Standards for nopCommerce Plugins

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

   ### Integration Tests (REQUIRED)

   **Must have integration tests for**:
   - Database operations (EF Core repositories)
   - External API calls (payment gateways, shipping APIs)
   - Plugin installation/uninstallation
   - Event consumers
   - Scheduled tasks

   **Integration test framework**: Use Nop.Tests patterns with in-memory database

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
   - [ ] No exceptions in application logs
   - [ ] Plugin uninstalls cleanly (no leftover tables/settings)

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
   - Any plugin handling sensitive data

   ## Quality Gate Enforcement

   **BLOCK mission completion if**:
   - Payment/shipping/tax/auth plugin has no tests (CRITICAL severity)
   - Business logic has < 70% coverage (HIGH severity)
   - No integration tests for database operations (HIGH severity)

   **WARN but allow if**:
   - Simple widget has no tests (MEDIUM severity)
   - Coverage is 60-69% (MEDIUM severity)
   - Manual testing incomplete but documented (MEDIUM severity)

   ## Test Example Templates

   ### Unit Test Example
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
           public void ValidateInput_WithNegativeValue_ThrowsException()
           {
               // Arrange
               var invalidValue = -5m;

               // Act & Assert
               Assert.Throws<ArgumentException>(() =>
                   _myService.ValidateInput(invalidValue));
           }
       }
   }
   ```

   ### Integration Test Example
   ```csharp
   using NUnit.Framework;
   using FluentAssertions;
   using Microsoft.Data.Sqlite;

   namespace Nop.Tests.Plugins.Misc.MyPlugin
   {
       [TestFixture]
       public class MyRepositoryTests
       {
           private SqliteConnection _connection;
           private IMyRepository _repository;

           [SetUp]
           public void SetUp()
           {
               // Use in-memory database for testing
               _connection = new SqliteConnection("DataSource=:memory:");
               _connection.Open();

               // Set up repository with in-memory DB
               _repository = new MyRepository(_connection);
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
               var retrieved = await _repository.GetByIdAsync(entity.Id);

               // Assert
               retrieved.Should().NotBeNull();
               retrieved.Name.Should().Be("Test");
               retrieved.Value.Should().Be(100);
           }
       }
   }
   ```
   ```

3. **Update Agent Instructions**

   **Update** `nopcommerce-plugin-developer.md` line 195-198:

   **Replace**:
   ```markdown
   **Testing** (if required by blueprint):
   - [ ] Unit tests written and passing
   - [ ] Integration tests completed
   - [ ] Manual testing done
   ```

   **With**:
   ```markdown
   **Testing** (see .claude/requirements/testing-standards.md):
   - [ ] Unit tests written for business logic (≥ 70% coverage)
   - [ ] Integration tests for database operations and external APIs
   - [ ] Manual testing completed (admin UI, public store, install/uninstall)
   - [ ] All tests passing (dotnet test shows 100% pass rate)
   - [ ] No skipped/ignored tests without documented reason
   ```

**Files Affected**:
- `.claude/CLAUDE.md` (lines 127-131, expand testing requirements)
- `.claude/requirements/testing-standards.md` (create new, ~250 lines)
- `.claude/agents/mission-execution/nopcommerce-plugin-developer.md` (lines 195-198, update testing checklist)

**Acceptance Criteria**:
- [ ] Testing requirements identical in CLAUDE.md and src/CLAUDE.md
- [ ] "Significant logic" replaced with specific criteria
- [ ] 70% coverage target documented
- [ ] testing-standards.md created with examples
- [ ] Agent instructions reference testing standards

**Dependencies**: None

**Estimated Effort**: 1.5 hours

---

### IMP-2: Documentation Requirements Vague and Lack Templates

**Problem Statement**:
Quality gates require documentation but don't define what "complete" means:

- **CLAUDE.md line 131**: "README or inline documentation complete"
- **src/CLAUDE.md line 542**: "Documentation (comments, README, CHANGELOG)"

No templates or examples provided. Quality varies significantly.

**Impact**:
- ⚠️ Inconsistent documentation quality
- ⚠️ Agents unsure what to include
- ⚠️ Users receive incomplete usage instructions

**Solution**:
Create documentation templates and reference them in quality gates.

**Implementation Steps**:

1. **Create Documentation Templates**

   **Create** `.claude/templates/README-template.md`:
   ```markdown
   # Nop.Plugin.{Group}.{Name}

   ## Overview

   **Version**: {version}
   **Author**: {author}
   **nopCommerce Compatibility**: {supportedVersions}
   **Plugin Type**: {group}

   {2-3 sentence description of what this plugin does and why it's useful}

   ## Features

   - ✅ Feature 1 description
   - ✅ Feature 2 description
   - ✅ Feature 3 description

   ## Installation

   ### Prerequisites

   - nopCommerce {version} or higher
   - {Any other requirements: .NET version, external accounts, etc.}

   ### Installation Steps

   1. **Upload plugin files**:
      - Extract the plugin archive
      - Upload contents to `/Plugins/Nop.Plugin.{Group}.{Name}/`

   2. **Restart application**:
      - Restart your nopCommerce application
      - Or recycle the application pool (IIS)

   3. **Install plugin**:
      - Navigate to **Admin → Configuration → Local Plugins**
      - Find "{FriendlyName}" in the list
      - Click **Install** button
      - Wait for installation to complete

   4. **Configure plugin** (if applicable):
      - Click **Configure** button
      - Enter required settings (see Configuration section below)
      - Click **Save**

   ## Configuration

   ### Configuration Settings

   Navigate to **Admin → Configuration → Settings → {Plugin Name} Settings**

   | Setting | Description | Required | Default |
   |---------|-------------|----------|---------|
   | {SettingName} | {What it does} | Yes/No | {default value} |
   | {SettingName} | {What it does} | Yes/No | {default value} |

   ### Example Configuration

   ```
   Setting 1: Value example
   Setting 2: Value example
   ```

   ### Testing Configuration

   1. {Step to test if configuration works}
   2. {Expected result}

   ## Usage

   ### For Administrators

   {How admins use this plugin}

   1. Navigate to {admin location}
   2. {Action to perform}
   3. {Expected outcome}

   ### For Customers (if applicable)

   {How customers interact with this plugin on the storefront}

   1. {Customer action}
   2. {What happens}

   ## Troubleshooting

   ### Plugin doesn't appear in Local Plugins

   **Solution**:
   - Verify files uploaded to correct directory: `/Plugins/Nop.Plugin.{Group}.{Name}/`
   - Check `plugin.json` exists and is valid JSON
   - Restart application

   ### Installation fails

   **Solution**:
   - Check application logs: `App_Data/Logs/`
   - Common issues:
     - Database connection error
     - Missing dependencies
     - Permission errors

   ### {Common Issue 3}

   **Solution**:
   - {How to resolve}

   ## Uninstallation

   1. Navigate to **Admin → Configuration → Local Plugins**
   2. Find "{FriendlyName}" in the list
   3. Click **Uninstall** button
   4. Confirm uninstallation
   5. (Optional) Delete plugin files from `/Plugins/Nop.Plugin.{Group}.{Name}/`

   **Note**: Uninstallation will remove all plugin data, including:
   - Plugin settings
   - Database tables (if any)
   - {Other data removed}

   ## Support

   **Author**: {author}
   **Email**: {contact email}
   **Website**: {website}
   **GitHub**: {repository URL}

   ## Version History

   See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

   ## License

   {License information}
   ```

   **Create** `.claude/templates/CHANGELOG-template.md`:
   ```markdown
   # Changelog

   All notable changes to this project will be documented in this file.

   The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
   and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

   ## [Unreleased]

   ### Added
   - {New features added}

   ### Changed
   - {Changes to existing functionality}

   ### Deprecated
   - {Soon-to-be removed features}

   ### Removed
   - {Removed features}

   ### Fixed
   - {Bug fixes}

   ### Security
   - {Security updates}

   ## [{version}] - {YYYY-MM-DD}

   ### Added
   - Initial release
   - {Feature 1}
   - {Feature 2}

   ### Changed
   - {Change 1}

   ### Fixed
   - {Bug fix 1}

   [{version}]: https://github.com/{user}/{repo}/releases/tag/v{version}
   ```

2. **Update Quality Gates to Reference Templates**

   **Update** `.claude/CLAUDE.md` line 130:

   **Replace**:
   ```markdown
   - [ ] README or inline documentation complete
   ```

   **With**:
   ```markdown
   - [ ] README.md created from template (.claude/templates/README-template.md)
   - [ ] CHANGELOG.md created from template (.claude/templates/CHANGELOG-template.md)
   - [ ] All sections of README completed (no placeholders left)
   ```

3. **Update Agent Instructions**

   **Update** `nopcommerce-plugin-developer.md` lines 218-223:

   **Add after line 223** (after Dependencies & Setup):
   ```markdown

   5. **Documentation**:
      - README.md (use template: .claude/templates/README-template.md)
      - CHANGELOG.md (use template: .claude/templates/CHANGELOG-template.md)
      - All placeholders filled in (no {bracketed} text remaining)
      - Configuration settings documented
      - Troubleshooting section complete
   ```

**Files Affected**:
- `.claude/templates/README-template.md` (create new, ~120 lines)
- `.claude/templates/CHANGELOG-template.md` (create new, ~40 lines)
- `.claude/CLAUDE.md` (line 130, update documentation requirement)
- `.claude/agents/mission-execution/nopcommerce-plugin-developer.md` (line 223, add documentation deliverable)

**Acceptance Criteria**:
- [ ] README template created with all standard sections
- [ ] CHANGELOG template created following Keep a Changelog format
- [ ] Quality gates reference templates
- [ ] Agent instructions include documentation deliverables
- [ ] No placeholders allowed in final documentation

**Dependencies**: None

**Estimated Effort**: 1 hour

---

### IMP-3: red-team Agent Underdeveloped

**Problem Statement**:
red-team agent specification is only 15 lines (vs 700+ for other agents) with vague directives:

```markdown
red-team.md:
1. Conduct assessments to identify omissions and vulnerabilities
2. Emulate adversarial tactics
3. Collaborate with team members
4. Provide reports
```

No specific methodologies, checklists, or nopCommerce-specific attack vectors documented.

**Impact**:
- ⚠️ Security review quality depends on agent's general knowledge
- ⚠️ No systematic security assessment
- ⚠️ nopCommerce-specific vulnerabilities might be missed
- ⚠️ No consistent security review process

**Solution**:
Expand red-team agent with nopCommerce-specific security checklists and attack scenarios.

**Implementation Steps**:

1. **Expand** `.claude/agents/mission-objective/red-team.md`

   **Replace entire file with**:
   ```markdown
   ---
   name: red-team
   description: Security assessment specialist who emulates adversarial tactics to identify vulnerabilities, security flaws, and compliance violations in nopCommerce plugins before deployment
   tools: Read, Grep, Glob
   ---

   # Red Team - nopCommerce Security Assessment Specialist

   You are a **security assessment specialist** for nopCommerce plugin development. Your role is to think like an attacker, identify security vulnerabilities, and challenge assumptions to ensure plugins are secure before deployment.

   ## Your Mission

   **Find security vulnerabilities before attackers do.**

   You are NOT looking for code quality or performance issues (other agents handle that). You are specifically hunting for:
   - Security vulnerabilities
   - Authentication/authorization bypasses
   - Data exposure risks
   - Injection vulnerabilities
   - Business logic flaws

   ## Assessment Categories

   ### Category 1: Injection Vulnerabilities

   #### SQL Injection

   **Attack Vector**: Can attacker manipulate database queries?

   **Check for**:
   - [ ] Raw SQL queries with string concatenation
   - [ ] Use of `.FromSqlRaw()` with unsanitized input
   - [ ] Dynamic LINQ with user-controlled expressions
   - [ ] Stored procedure calls with concatenated parameters

   **Red flags**:
   ```csharp
   // VULNERABLE
   var query = $"SELECT * FROM Customer WHERE Email = '{email}'";
   var customers = _context.Customers.FromSqlRaw(query).ToList();

   // VULNERABLE
   var sql = "EXEC GetCustomerByEmail '" + email + "'";
   ```

   **Verify**:
   - [ ] All database queries use EF Core LINQ (parameterized)
   - [ ] No raw SQL with user input
   - [ ] If `.FromSqlRaw()` used, parameters are passed separately

   #### Cross-Site Scripting (XSS)

   **Attack Vector**: Can attacker inject malicious JavaScript?

   **Check for**:
   - [ ] Razor views using `@Html.Raw()` with user input
   - [ ] JavaScript code embedding user data without encoding
   - [ ] Admin views displaying user-submitted content
   - [ ] HTML attributes with unencoded user values

   **Red flags**:
   ```cshtml
   @* VULNERABLE *@
   <div>@Html.Raw(Model.UserSubmittedContent)</div>

   @* VULNERABLE *@
   <script>
       var userName = '@Model.UserName'; // User can inject JS
   </script>

   @* VULNERABLE *@
   <div title="@Model.UnsafeInput">Content</div>
   ```

   **Verify**:
   - [ ] User input displayed with `@Model.Property` (auto-encoded)
   - [ ] `@Html.Raw()` only used for trusted content
   - [ ] JavaScript variables use JSON encoding: `@Json.Serialize()`
   - [ ] HTML attributes are encoded

   ### Category 2: Authentication & Authorization

   #### Authentication Bypass

   **Attack Vector**: Can attacker access features without logging in?

   **Check for**:
   - [ ] Admin controllers missing `[AuthorizeAdmin]` attribute
   - [ ] API endpoints without authentication
   - [ ] Custom authentication logic (instead of nopCommerce's)
   - [ ] Session validation bypasses

   **Red flags**:
   ```csharp
   // VULNERABLE - No authorization
   public class MyPluginController : BasePluginController
   {
       public IActionResult AdminFunction()  // Anyone can access!
       {
           return View();
       }
   }
   ```

   **Verify**:
   - [ ] All admin controllers have `[AuthorizeAdmin]` on class or methods
   - [ ] All admin actions check permissions
   - [ ] Public endpoints intentionally public (not accidentally)

   #### Authorization Escalation

   **Attack Vector**: Can regular user access admin functions?

   **Check for**:
   - [ ] Customer data access without owner verification
   - [ ] Order data access without customer check
   - [ ] Store-specific data accessible from other stores
   - [ ] Permission checks missing on sensitive operations

   **Red flags**:
   ```csharp
   // VULNERABLE - No ownership check
   public async Task<IActionResult> ViewOrder(int orderId)
   {
       var order = await _orderService.GetOrderByIdAsync(orderId);
       return View(order); // Any user can view any order!
   }
   ```

   **Verify**:
   - [ ] Customer data filtered by current customer ID
   - [ ] Order access verified against current customer
   - [ ] Multi-store data filtered by current store
   - [ ] Permission service used: `await _permissionService.AuthorizeAsync()`

   ### Category 3: Data Exposure

   #### Sensitive Data in Logs

   **Attack Vector**: Are credentials/PII logged?

   **Check for**:
   - [ ] Passwords logged (even hashed)
   - [ ] Credit card numbers in logs
   - [ ] API keys/secrets in logs
   - [ ] Customer PII in error messages

   **Red flags**:
   ```csharp
   // VULNERABLE
   await _logger.InformationAsync($"User login: {email}, Password: {password}");

   // VULNERABLE
   await _logger.ErrorAsync($"Payment failed for card: {creditCardNumber}");
   ```

   **Verify**:
   - [ ] No passwords in logs
   - [ ] No credit card numbers in logs
   - [ ] No API keys/secrets in logs
   - [ ] PII limited to necessary identifiers (ID, not full details)

   #### Sensitive Data in URLs

   **Attack Vector**: Are secrets exposed in browser history/server logs?

   **Check for**:
   - [ ] API keys in query strings
   - [ ] Tokens in URLs
   - [ ] Customer IDs in GET requests (IDOR vulnerability)
   - [ ] Sensitive parameters in query strings

   **Red flags**:
   ```csharp
   // VULNERABLE
   return RedirectToAction("Webhook", new { apiKey = settings.ApiKey });

   // VULNERABLE
   var url = $"/payment/callback?secret={paymentSecret}";
   ```

   **Verify**:
   - [ ] Secrets passed in headers or POST body
   - [ ] Tokens in POST requests, not GET
   - [ ] Customer-specific actions use verified session, not URL params

   ### Category 4: Business Logic Flaws

   #### Price Manipulation

   **Attack Vector**: Can user manipulate pricing?

   **Check for**:
   - [ ] Prices calculated client-side
   - [ ] Discounts applied without server validation
   - [ ] Quantity/price submitted from form (not recalculated)
   - [ ] Total calculated from user input

   **Red flags**:
   ```csharp
   // VULNERABLE
   public async Task<IActionResult> Checkout(decimal totalPrice)
   {
       // User can submit any totalPrice value!
       await ProcessPayment(totalPrice);
   }
   ```

   **Verify**:
   - [ ] All prices recalculated server-side
   - [ ] No price values from user input
   - [ ] Discounts validated against business rules
   - [ ] Order totals calculated from database product prices

   #### Workflow Bypass

   **Attack Vector**: Can user skip required steps?

   **Check for**:
   - [ ] Checkout without payment
   - [ ] Order creation without validation
   - [ ] Status changes without authorization
   - [ ] Sequential steps can be executed out of order

   **Red flags**:
   ```csharp
   // VULNERABLE
   public async Task<IActionResult> CompleteOrder(int orderId)
   {
       // No check if payment was received!
       var order = await _orderService.GetOrderByIdAsync(orderId);
       order.OrderStatus = OrderStatus.Complete;
       await _orderService.UpdateOrderAsync(order);
   }
   ```

   **Verify**:
   - [ ] State transitions validated (can't skip from "pending" to "shipped")
   - [ ] Prerequisites checked (payment before fulfillment)
   - [ ] Business rules enforced server-side

   ### Category 5: nopCommerce-Specific Vulnerabilities

   #### Multi-Store Data Leakage

   **Attack Vector**: Can users access data from other stores?

   **Check for**:
   - [ ] Queries without store filter
   - [ ] Settings without store scope
   - [ ] Products visible across all stores (when shouldn't be)
   - [ ] Customers seeing other stores' data

   **Red flags**:
   ```csharp
   // VULNERABLE - No store filter
   var products = await _productRepository.Table
       .Where(p => p.Published)
       .ToListAsync(); // Returns products from ALL stores
   ```

   **Verify**:
   - [ ] Queries filtered by current store: `_storeContext.GetCurrentStore().Id`
   - [ ] Settings use `_settingService.GetSettingByKey()` with storeId
   - [ ] Multi-store mapping checked for entities

   #### Plugin Isolation Violations

   **Attack Vector**: Can plugin modify core nopCommerce?

   **Check for**:
   - [ ] Direct modification of core entity tables
   - [ ] Changes to core nopCommerce files
   - [ ] Overriding core services without proper inheritance
   - [ ] Database schema changes to core tables

   **Red flags**:
   ```csharp
   // VULNERABLE - Modifying core table
   var customer = await _customerRepository.GetByIdAsync(customerId);
   customer.SystemName = "ModifiedByPlugin"; // Don't modify core entities!
   ```

   **Verify**:
   - [ ] Plugin only modifies its own tables
   - [ ] No changes to `/Presentation/`, `/Libraries/` outside of plugin folder
   - [ ] Services extended properly (IPlugin, interfaces)
   - [ ] Migrations only affect plugin tables

   ### Category 6: Cryptography & Secrets

   #### Weak Cryptography

   **Attack Vector**: Can encrypted data be decrypted?

   **Check for**:
   - [ ] Custom encryption implementations
   - [ ] Weak algorithms (DES, MD5, SHA1 for passwords)
   - [ ] Hardcoded encryption keys
   - [ ] Passwords stored in plain text

   **Red flags**:
   ```csharp
   // VULNERABLE
   var encrypted = Encrypt(data, "hardcodedkey123");

   // VULNERABLE
   var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
   ```

   **Verify**:
   - [ ] nopCommerce's `IEncryptionService` used for encryption
   - [ ] No custom crypto implementations
   - [ ] Keys stored securely (settings, not code)
   - [ ] Passwords never stored (use nopCommerce's CustomerPassword system)

   #### Exposed API Keys/Secrets

   **Attack Vector**: Can attacker find credentials?

   **Check for**:
   - [ ] API keys hardcoded in source
   - [ ] Secrets in `appsettings.json` (checked into git)
   - [ ] Connection strings in code
   - [ ] Tokens in JavaScript/Razor views

   **Red flags**:
   ```csharp
   // VULNERABLE
   private const string API_KEY = "sk_live_1234567890";

   // VULNERABLE
   <script>
       var apiKey = '@Model.ApiKey'; // Exposed to client!
   </script>
   ```

   **Verify**:
   - [ ] Secrets stored in `ISettingService` (encrypted in database)
   - [ ] No secrets in source code
   - [ ] No secrets in client-side JavaScript
   - [ ] External services use settings, not hardcoded values

   ## Assessment Process

   ### Phase 1: Reconnaissance

   1. **Understand plugin scope**:
      - What does plugin do?
      - What data does it access?
      - What permissions does it require?
      - What external services does it integrate?

   2. **Identify attack surface**:
      - User input points (forms, APIs, URLs)
      - Data storage (database, cache, files)
      - External integrations (payment gateways, APIs)
      - Admin interfaces
      - Public interfaces

   3. **Map data flow**:
      - How does user data enter the system?
      - Where is data stored?
      - Who can access the data?
      - How is data protected?

   ### Phase 2: Vulnerability Assessment

   **For each attack surface, check all 6 categories**:

   1. Use Grep to find red flag patterns
   2. Use Read to examine suspicious code
   3. Verify protections are in place
   4. Document vulnerabilities found

   **Search patterns**:
   ```bash
   # SQL Injection
   Grep: "FromSqlRaw|SqlCommand|ExecuteSqlRaw"

   # XSS
   Grep: "Html.Raw|innerHTML|@Model.*<script"

   # Missing Authorization
   Grep: "public class.*Controller" (check for [AuthorizeAdmin])

   # Logging sensitive data
   Grep: "_logger.*password|_logger.*creditcard|_logger.*apikey"

   # Hardcoded secrets
   Grep: "const.*key|const.*secret|const.*password"
   ```

   ### Phase 3: Reporting

   **For each vulnerability found, provide**:

   ```markdown
   ### Vulnerability: {Title}

   **Severity**: Critical / High / Medium / Low

   **Category**: {Injection / Auth / Data Exposure / Business Logic / nopCommerce / Crypto}

   **Location**: {file path}:{line number}

   **Attack Scenario**:
   1. Attacker {action}
   2. System {vulnerable behavior}
   3. Result: {impact}

   **Vulnerable Code**:
   ```csharp
   {code snippet}
   ```

   **Proof of Concept** (if applicable):
   {Example attack payload or steps to exploit}

   **Impact**:
   - {What attacker can achieve}
   - {Data at risk}
   - {Business impact}

   **Remediation**:
   {Specific code fix}

   **Fixed Code Example**:
   ```csharp
   {corrected code}
   ```

   **Verification**:
   {How to verify fix works}
   ```

   ## Severity Classification

   ### Critical
   - SQL injection allowing database access
   - Authentication bypass allowing admin access
   - Remote code execution
   - Payment manipulation allowing free orders
   - Customer data exposure (PII, passwords, payment info)

   ### High
   - XSS allowing session hijacking
   - Authorization escalation (user → admin)
   - Multi-store data leakage
   - API key exposure
   - Price manipulation

   ### Medium
   - Information disclosure (non-PII)
   - Missing security headers
   - Weak cryptography
   - Incomplete input validation
   - Logging sensitive data

   ### Low
   - Minor information leaks
   - Security misconfigurations
   - Missing CSRF tokens (nopCommerce handles this)
   - Weak password policies (nopCommerce handles this)

   ## Final Report Template

   ```markdown
   # Red Team Security Assessment Report

   **Plugin**: Nop.Plugin.{Group}.{Name}
   **Version**: {version}
   **Assessment Date**: {date}
   **Assessed By**: red-team agent

   ## Executive Summary

   {1-2 paragraph summary of findings}

   **Overall Security Rating**: Critical / High Risk / Medium Risk / Low Risk / Secure

   **Vulnerabilities Found**:
   - Critical: {count}
   - High: {count}
   - Medium: {count}
   - Low: {count}

   **Recommendation**: {Deploy / Fix Critical Issues First / Major Rework Needed}

   ## Detailed Findings

   ### Critical Severity

   {List all critical vulnerabilities using template above}

   ### High Severity

   {List all high vulnerabilities}

   ### Medium Severity

   {List all medium vulnerabilities}

   ### Low Severity

   {List all low vulnerabilities}

   ## Positive Security Findings

   {What was done well}:
   - ✅ {Good security practice found}
   - ✅ {Good security practice found}

   ## Recommendations

   **Immediate Actions** (before deployment):
   1. {Fix critical issue 1}
   2. {Fix critical issue 2}

   **Short-term** (next release):
   1. {Fix high issue 1}
   2. {Fix high issue 2}

   **Long-term** (future enhancements):
   1. {Fix medium/low issues}
   2. {Security improvements}

   ## Conclusion

   {Final assessment and deployment recommendation}
   ```

   ## When to Invoke red-team

   **ALWAYS assess**:
   - Payment processing plugins
   - Authentication/authorization plugins
   - Plugins handling sensitive data (PII, financial, health)
   - Plugins with external API integrations
   - Plugins with admin functionality

   **OPTIONAL for**:
   - Simple display widgets (no user input)
   - Read-only reporting plugins
   - Internal tools (not public-facing)

   ## Your Value Proposition

   **You prevent security breaches before they happen.**

   - Attackers will probe for vulnerabilities
   - Security flaws damage customer trust
   - nopCommerce stores handle sensitive data and money
   - Your assessment protects businesses and customers

   **Think like an attacker. Find vulnerabilities first. Report honestly.**
   ```

**Files Affected**:
- `.claude/agents/mission-objective/red-team.md` (replace entire file, ~600 lines)

**Acceptance Criteria**:
- [ ] 6 security assessment categories defined
- [ ] Attack vectors documented for each category
- [ ] Red flags and verification criteria provided
- [ ] Search patterns for Grep provided
- [ ] Vulnerability report template included
- [ ] Severity classification system defined
- [ ] Final report template provided

**Dependencies**: None

**Estimated Effort**: 2.5 hours

---

### IMP-4: Simple Task Classification Ambiguous

**Problem Statement**:
CLAUDE.md line 29 defines Simple Task criteria:
```markdown
Simple Task: • Single step • Affects ≤2 files • No architectural decisions • Clear, unambiguous goal
Action: Execute directly OR delegate to single specialist
```

The "OR" creates ambiguity. When should Team Commander execute vs delegate?

**Impact**:
- ⚠️ Inconsistent routing decisions
- ⚠️ Sometimes simple tasks executed directly (fast)
- ⚠️ Sometimes simple tasks delegated unnecessarily (slow)
- ⚠️ No clear decision criteria

**Example Ambiguous Cases**:
- "Add XML comments to MyService.cs" - Execute directly or delegate to nopcommerce-plugin-developer?
- "Fix typo in plugin.json" - Execute directly or delegate?
- "Add localization resource" - Execute directly or delegate?

**Solution**:
Add explicit decision criteria with examples.

**Implementation Steps**:

1. **Update `.claude/CLAUDE.md` Classification Matrix**

   **Location**: Line 29-31 (Simple Task row)

   **Replace**:
   ```markdown
   | **Simple Task** | • Single step<br>• Affects ≤2 files<br>• No architectural decisions<br>• Clear, unambiguous goal | Execute directly OR delegate to single specialist |
   ```

   **With**:
   ```markdown
   | **Simple Task** | • Single step<br>• Affects ≤2 files<br>• No architectural decisions<br>• Clear, unambiguous goal | **Execute directly IF**: Task requires only Read/Grep/Glob/Edit tools and no nopCommerce expertise<br>**Delegate to specialist IF**: Task requires nopCommerce patterns, coding standards, or domain knowledge |
   ```

2. **Add Simple Task Decision Guide**

   **Location**: After line 31 (after classification matrix), before Mission Protocol Decision Tree

   **Insert**:
   ```markdown

   ### Simple Task Routing Decision Guide

   **Execute Directly** (use Edit/Read/Write tools yourself):

   | Task Type | Example | Why Direct |
   |-----------|---------|------------|
   | File reading | "What does plugin.json contain?" | Read tool sufficient |
   | Simple text edits | "Fix typo in README.md line 15" | Edit tool sufficient, no expertise needed |
   | Information lookup | "Find all uses of ICustomerService" | Grep tool sufficient |
   | Documentation updates | "Add XML comment to existing method" | Straightforward edit, no logic changes |
   | Configuration reading | "What's the current version in plugin.json?" | Read tool, no analysis needed |

   **Delegate to Specialist**:

   | Task Type | Delegate To | Why Specialist Needed |
   |-----------|-------------|----------------------|
   | Add nopCommerce code | nopcommerce-plugin-developer | Requires nopCommerce patterns, DI, coding standards |
   | Modify business logic | nopcommerce-plugin-developer | Requires understanding of plugin architecture |
   | Update entity/data layer | nopcommerce-data-specialist | Requires EF Core, migration knowledge |
   | Modify views/UI | nopcommerce-ui-specialist | Requires Razor, Bootstrap, nopCommerce admin patterns |
   | Fix bugs | nopcommerce-troubleshooter | Requires debugging expertise |
   | Add tests | nopcommerce-test-specialist | Requires NUnit, testing patterns |

   **Rule of Thumb**:
   - **Direct**: If you can do it with Edit tool in <2 minutes without domain knowledge → Execute directly
   - **Delegate**: If it requires understanding nopCommerce patterns, coding standards, or architectural context → Delegate to specialist

   **Examples**:

   | User Request | Classification | Action | Reasoning |
   |--------------|----------------|--------|-----------|
   | "Fix typo in README, line 10: 'configuraiton' → 'configuration'" | Simple Task | **Execute directly** | Single Edit, no expertise needed |
   | "Add XML comment to CalculatePrice method" | Simple Task | **Delegate to nopcommerce-plugin-developer** | Requires understanding what method does, proper XML doc format |
   | "Find all controllers in the plugin" | Simple Task | **Execute directly** | Use Glob: `**/*Controller.cs` |
   | "Add async keyword to GetCustomer method" | Simple Task | **Delegate to nopcommerce-plugin-developer** | Requires understanding async patterns, return type changes, caller updates |
   | "What's in plugin.json?" | Information Request | **Execute directly** | Use Read tool |
   | "Change plugin version to 1.1.0" | Simple Task | **Execute directly** | Edit plugin.json, single line change, no expertise needed |
   | "Add localization resource for new button" | Simple Task | **Delegate to nopcommerce-plugin-developer** | Requires understanding nopCommerce localization XML format |
   ```

**Files Affected**:
- `.claude/CLAUDE.md` (line 29-31, update Simple Task action; insert decision guide after line 31)

**Acceptance Criteria**:
- [ ] "OR" ambiguity removed from classification matrix
- [ ] Explicit criteria: "Execute directly IF..." and "Delegate IF..."
- [ ] Decision guide table with examples
- [ ] Rule of thumb provided
- [ ] 7+ examples showing routing decisions

**Dependencies**: None

**Estimated Effort**: 1 hour

---

### IMP-5: Slash Command User Input Not Structured

**Problem Statement**:
Slash commands like `/nop-new-plugin` list questions to ask user (lines 19-42) but rely on free-form text collection:

```markdown
Ask the user for the following information:
1. Plugin Details: Plugin Name, Plugin Group, ...
2. Functionality Requirements: What should the plugin do?
```

No use of `AskUserQuestion` tool with structured options.

**Impact**:
- ⚠️ Users provide incomplete information
- ⚠️ Users provide invalid values (wrong group names)
- ⚠️ Requires follow-up questions, slowing down mission
- ⚠️ No validation before delegation

**Example Issues**:
- User enters "Payment" instead of "Payments" (wrong group name)
- User doesn't specify if configuration page needed
- User gives vague functionality description

**Solution**:
Add pre-flight validation and structured question gathering to slash commands.

**Implementation Steps**:

1. **Create Validation Helper Reference**

   **Create** `.claude/requirements/slash-command-patterns.md`:
   ```markdown
   # Slash Command Best Practices

   ## Pattern 1: Structured Information Gathering

   **Use AskUserQuestion tool for structured input**:

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

   ## Pattern 2: Input Validation Before Delegation

   **Always validate user input before delegating to mission-commander**:

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

   ## Pattern 3: Error-Friendly User Communication

   **When validation fails, provide helpful guidance**:

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
   ```

2. **Update** `/nop-new-plugin` command

   **Location**: `.claude/commands/nop-new-plugin.md`, replace lines 19-46

   **Replace with**:
   ```markdown
   ## Step 1: Gather Information from User (Structured)

   Use AskUserQuestion tool to collect plugin information:

   ### Question 1: Plugin Group
   ```typescript
   {
     question: "Which plugin group should this plugin belong to?",
     header: "Plugin Group",
     multiSelect: false,
     options: [
       { label: "Payments", description: "Payment gateway integrations" },
       { label: "Shipping", description: "Shipping rate calculators" },
       { label: "Tax", description: "Tax calculation providers" },
       { label: "Widgets", description: "UI components and integrations" },
       { label: "Misc", description: "Miscellaneous plugins" },
       { label: "ExternalAuth", description: "External authentication providers" }
     ]
   }
   ```

   ### Question 2: Plugin Features (Multi-Select)
   ```typescript
   {
     question: "Which features does your plugin need? (Select all that apply)",
     header: "Features",
     multiSelect: true,
     options: [
       { label: "Database Tables", description: "Custom entities with EF Core" },
       { label: "Configuration Page", description: "Admin settings UI" },
       { label: "External API Integration", description: "Connect to third-party service" },
       { label: "Public Widgets", description: "Display on storefront" },
       { label: "Admin Menu Items", description: "Custom admin navigation" },
       { label: "Scheduled Tasks", description: "Background jobs" }
     ]
   }
   ```

   ### Question 3: Text Information
   Ask user for (free-form text):
   - Plugin name (e.g., "PayPalCommerce")
   - Plugin description (1-2 sentences)
   - Author name
   - External API name (if "External API Integration" selected)

   ## Step 2: Validate Input

   Before delegating to mission-commander:

   1. **Verify plugin doesn't exist**:
      ```
      Use Glob: "Plugins/Nop.Plugin.{Group}.{Name}/**"

      If found:
         Ask user: "Plugin Nop.Plugin.{Group}.{Name} already exists.
                    Options: Modify existing / Use different name / Overwrite / Cancel"
      ```

   2. **Verify required information complete**:
      - [ ] Plugin name provided
      - [ ] Plugin group selected
      - [ ] Description provided
      - [ ] Author name provided
      - [ ] If "External API" selected → API name provided
      - [ ] If "Database Tables" selected → Ask: "What data will you store? (brief description)"

   3. **Validate plugin name**:
      - No spaces (use PascalCase)
      - No special characters except dots
      - Follows pattern: {Group}.{Name}

      If invalid: "Plugin name should be PascalCase with no spaces (e.g., 'PayPalCommerce', not 'PayPal Commerce')"

   ## Step 3: Delegate to mission-commander

   Only after validation passes, use Task tool to delegate...
   ```

**Files Affected**:
- `.claude/requirements/slash-command-patterns.md` (create new, ~300 lines)
- `.claude/commands/nop-new-plugin.md` (replace lines 19-46 with structured gathering + validation)

**Acceptance Criteria**:
- [ ] AskUserQuestion examples in slash-command-patterns.md
- [ ] Validation pattern documented
- [ ] /nop-new-plugin updated to use structured questions
- [ ] Validation step added before delegation
- [ ] Conflict detection (Glob check) added
- [ ] Error-friendly user communication templates provided

**Dependencies**: None

**Estimated Effort**: 1.5 hours

---

## ENHANCEMENT PRIORITY ISSUES

### ENH-1: Performance Targets May Be Unrealistic for External Dependencies

**Problem Statement**:
src/CLAUDE.md line 398 defines:
```markdown
API endpoints: < 200ms
```

This target may be impossible for payment gateways, shipping APIs, or tax providers that have network latency + processing time.

**Impact**:
- ⚠️ Unfair quality gate failures
- ⚠️ Agents may feel targets are unachievable
- ⚠️ No distinction between internal and external operations

**Solution**:
Add caveat for external dependencies and separate targets.

**Implementation Steps**:

1. **Update** `src/CLAUDE.md` Performance Targets

   **Location**: Lines 395-404 (Performance Targets section)

   **Replace**:
   ```markdown
   **Response time targets:**
   - Admin page loads: < 500ms
   - Public store pages: < 300ms
   - API endpoints: < 200ms
   - Database queries: < 50ms (simple), < 200ms (complex)
   ```

   **With**:
   ```markdown
   **Response time targets (internal operations only):**
   - Admin page loads: < 500ms (excluding external API calls)
   - Public store pages: < 300ms (excluding external API calls)
   - API endpoints (internal): < 200ms
   - Database queries: < 50ms (simple), < 200ms (complex)

   **External API operations (network dependent):**
   - Payment gateway calls: < 5 seconds (industry standard)
   - Shipping rate APIs: < 3 seconds (user tolerance limit)
   - Tax calculation APIs: < 2 seconds
   - General external APIs: < 5 seconds

   **Notes**:
   - Use async operations for all external calls (never block)
   - Implement timeouts for external calls (default: 30 seconds)
   - Cache external API results when appropriate
   - Show loading indicators to users during external operations
   - Log slow external API calls for monitoring
   ```

**Files Affected**:
- `src/CLAUDE.md` (lines 395-404, expand with external API targets)

**Acceptance Criteria**:
- [ ] Internal vs external targets separated
- [ ] Realistic external API timeouts defined
- [ ] Notes added about async operations and caching
- [ ] Timeout recommendations included

**Dependencies**: None

**Estimated Effort**: 0.25 hours

---

### ENH-2: Add Quick After-Action Review Option

**Problem Statement**:
CLAUDE.md lines 202-226 define After-Action Review (AAR) as mandatory for complex missions, but provides no time-constrained alternative.

**Impact**:
- ⚠️ AAR overhead may slow rapid development cycles
- ⚠️ Full AAR may be overkill for minor issues
- ⚠️ No lightweight option for time-sensitive missions

**Solution**:
Add "Quick AAR" template for time-constrained situations.

**Implementation Steps**:

1. **Update** `.claude/CLAUDE.md` After-Action Review Section

   **Location**: After line 209 (after "User-requested reviews")

   **Insert**:
   ```markdown
   - Time-constrained missions (quick AAR acceptable)

   ### After-Action Review Types

   **Full AAR** (recommended for complex missions):
   - Delegate to debriefing-expert
   - Comprehensive analysis
   - Detailed improvement recommendations
   - Time: 15-30 minutes

   **Quick AAR** (for time-constrained or simple missions):
   - Team Commander completes (no delegation)
   - Brief summary format
   - Time: 2-5 minutes

   #### Quick AAR Template

   ```
   QUICK AFTER-ACTION REVIEW

   Mission: {objective}
   Duration: {time taken}
   Outcome: {Success / Partial / Failed}

   What Went Well:
   - {1-2 things that worked well}

   What Could Improve:
   - {1-2 things that could be better next time}

   Key Lesson:
   {One sentence takeaway}

   Next Time:
   {One specific thing to do differently}
   ```

   **When to Use Quick AAR**:
   - Mission took < 15 minutes
   - Simple task that succeeded
   - User requested fast turnaround
   - No significant blockers encountered

   **When Full AAR Required**:
   - Mission took > 30 minutes
   - Multiple specialists involved
   - Blockers or quality gate failures
   - New mission type (learning opportunity)
   - Complex custom mission
   ```

**Files Affected**:
- `.claude/CLAUDE.md` (line 209, insert Quick AAR section)

**Acceptance Criteria**:
- [ ] Quick AAR template provided
- [ ] When to use Quick vs Full AAR documented
- [ ] Time estimates for both options
- [ ] Quick AAR doesn't require debriefing-expert

**Dependencies**: None

**Estimated Effort**: 0.25 hours

---

### ENH-3: Add Mission Complexity Scoring System

**Problem Statement**:
Borderline cases between "Simple Task" and "Standard Mission" or "Standard Mission" and "Complex Custom" can be subjective.

**Impact**:
- ⚠️ Inconsistent classification for edge cases
- ⚠️ Under-scoping complex missions
- ⚠️ Over-engineering simple tasks

**Solution**:
Add objective scoring system for classification.

**Implementation Steps**:

1. **Add Complexity Scoring to** `.claude/CLAUDE.md`

   **Location**: After line 51 (end of Mission Protocol Decision Tree)

   **Insert**:
   ```markdown

   ### Mission Complexity Scoring (for borderline cases)

   If classification is unclear, use this scoring system:

   **Score each factor (0-2 points):**

   | Factor | 0 Points | 1 Point | 2 Points |
   |--------|----------|---------|----------|
   | **Files Affected** | ≤ 2 files | 3-5 files | > 5 files |
   | **Architectural Impact** | None (isolated changes) | Moderate (new services/components) | Significant (plugin structure, core integration) |
   | **Specialist Knowledge** | General (any developer) | Specialized (nopCommerce patterns) | Expert (multiple domains) |
   | **User Requirements** | Crystal clear | Some clarification needed | Ambiguous or evolving |
   | **Dependencies** | None | Internal (within plugin) | External (APIs, packages, other plugins) |

   **Total Score → Classification:**

   - **0-2 points**: Simple Task
   - **3-5 points**: Standard Mission (use slash command) OR Single Specialist
   - **6-8 points**: Complex Custom (mission-commander required)
   - **9-10 points**: Very Complex (mission-commander + red-team recommended)

   **Examples:**

   | Request | Files | Arch | Knowledge | Requirements | Deps | Total | Classification |
   |---------|-------|------|-----------|--------------|------|-------|----------------|
   | "Fix typo in README" | 0 | 0 | 0 | 0 | 0 | **0** | Simple Task (execute directly) |
   | "Add XML comments to MyService" | 0 | 0 | 1 | 0 | 0 | **1** | Simple Task (delegate to specialist) |
   | "Create new payment plugin" | 2 | 2 | 2 | 1 | 2 | **9** | Complex Custom (mission-commander) |
   | "Add entity to existing plugin" | 1 | 1 | 1 | 0 | 0 | **3** | Standard Mission (/nop-add-entity) |
   | "Optimize database queries" | 1 | 0 | 1 | 1 | 0 | **3** | Single Specialist (performance-specialist) |

   **Use this scoring when**:
   - Classification is unclear
   - User request spans multiple categories
   - Mission seems borderline between two classifications

   **Default to higher complexity when in doubt** (better to over-plan than under-plan).
   ```

**Files Affected**:
- `.claude/CLAUDE.md` (line 51, insert complexity scoring system)

**Acceptance Criteria**:
- [ ] 5 scoring factors defined with 0-2 point scale
- [ ] Score ranges mapped to classifications
- [ ] 5+ examples with scoring breakdown
- [ ] Guidance on when to use scoring

**Dependencies**: None

**Estimated Effort**: 0.5 hours

---

### ENH-4: Create Slash Command Validation Helper

**Problem Statement**:
Slash commands should validate user input before delegation, but no reusable validation logic exists.

**Impact**:
- ⚠️ Each slash command implements validation differently
- ⚠️ Inconsistent error messages
- ⚠️ Duplicate validation code

**Solution**:
Create reusable validation patterns documentation.

**Implementation Steps**:

1. **Expand** `.claude/requirements/slash-command-patterns.md` (created in IMP-5)

   **Add section after Pattern 3**:
   ```markdown

   ## Pattern 4: Reusable Validation Patterns

   ### Validate Plugin Group

   ```markdown
   **Valid nopCommerce plugin groups**:
   Payments, Shipping, Tax, Widgets, Misc, ExternalAuth, Pickup, DiscountRules, Search, MultiFactorAuth

   **Validation**:
   If user input not in valid groups:
      Suggest closest match (Levenshtein distance)
      Example: "Payment" → Did you mean "Payments"?
   ```

   ### Validate Plugin Name

   ```markdown
   **Rules**:
   - PascalCase (e.g., "PayPalCommerce")
   - No spaces
   - No special characters except dots
   - Descriptive (not generic like "Plugin1")

   **Validation**:
   If contains spaces: "Plugin names should be PascalCase with no spaces"
   If has special chars: "Plugin names can only contain letters, numbers, and dots"
   If too generic: "Plugin name should be descriptive (e.g., 'PayPalCommerce' not 'Payment')"
   ```

   ### Validate Version Number

   ```markdown
   **Format**: Semantic versioning (X.Y.Z)
   - X = Major version (breaking changes)
   - Y = Minor version (new features)
   - Z = Patch version (bug fixes)

   **Validation**:
   Regex: `^\d+\.\d+\.\d+$`

   If invalid: "Version should follow semantic versioning (e.g., '1.0.0', '2.1.3')"
   ```

   ### Check Plugin Exists (Conflict Detection)

   ```markdown
   **Check**:
   Use Glob: `Plugins/Nop.Plugin.{Group}.{Name}/**`

   If matches found:
      List found files
      Ask user options:
      1. Modify existing plugin
      2. Use different name
      3. Overwrite (⚠️ destructive)
      4. Cancel
   ```

   ### Validate Email Address

   ```markdown
   **Format**: user@domain.tld

   **Validation**:
   Basic regex: `^[^@]+@[^@]+\.[^@]+$`

   If invalid: "Email should be in format: user@domain.com"
   ```

   ### Validate URL

   ```markdown
   **Format**: https://domain.com or http://domain.com

   **Validation**:
   Must start with http:// or https://

   If invalid: "URL should start with http:// or https://"
   If http (not https): "⚠️ Consider using https:// for security"
   ```
   ```

**Files Affected**:
- `.claude/requirements/slash-command-patterns.md` (expand with Pattern 4)

**Acceptance Criteria**:
- [ ] 6+ reusable validation patterns documented
- [ ] Regex patterns provided where applicable
- [ ] Error message templates included
- [ ] Examples for each validation

**Dependencies**: IMP-5 (creates base file)

**Estimated Effort**: 0.5 hours

---

### ENH-5: Add Mission Metrics Tracking Recommendations

**Problem Statement**:
No guidance on tracking mission performance metrics for continuous improvement.

**Impact**:
- ⚠️ No data-driven improvements
- ⚠️ Can't identify bottlenecks
- ⚠️ Can't measure framework effectiveness

**Solution**:
Add optional metrics tracking recommendations (not mandatory).

**Implementation Steps**:

1. **Create** `.claude/requirements/mission-metrics.md`

   ```markdown
   # Mission Metrics Tracking (Optional)

   ## Overview

   This document provides **optional** recommendations for tracking mission performance metrics. Metrics help identify bottlenecks, improve processes, and demonstrate framework value.

   **This is NOT mandatory.** Use metrics when:
   - Optimizing framework performance
   - Demonstrating ROI of DEVGRU framework
   - Identifying training needs
   - Improving mission protocols

   ## Recommended Metrics

   ### Mission Classification Metrics

   **Track**:
   - Classification accuracy (% correctly classified on first attempt)
   - Classification time (seconds to classify)
   - Reclassification rate (% missions reclassified after starting)

   **Goal**:
   - Accuracy > 90%
   - Time < 30 seconds
   - Reclassification < 10%

   **How to Track**:
   ```markdown
   Mission Log Entry:
   - Mission ID: {timestamp}
   - Classification: {Simple Task / Standard Mission / Complex Custom}
   - Classification Time: {seconds}
   - Reclassified: {Yes/No}
   - Correct: {Yes/No} (in retrospect)
   ```

   ### Mission Execution Metrics

   **Track**:
   - Time to complete (minutes)
   - Number of agents involved
   - Number of blockers encountered
   - Quality gate pass rate (% missions passing all gates)
   - Rework rate (% missions requiring rework)

   **Goals**:
   - Simple Tasks: < 15 minutes
   - Standard Missions: < 60 minutes
   - Complex Custom: < 4 hours
   - Quality gate pass: > 85%
   - Rework rate: < 15%

   **How to Track**:
   ```markdown
   Mission Log Entry:
   - Start Time: {timestamp}
   - End Time: {timestamp}
   - Duration: {minutes}
   - Agents: {agent-name, agent-name}
   - Blockers: {count}
   - Quality Gates Passed: {X/Y}
   - Rework Required: {Yes/No}
   ```

   ### Agent Performance Metrics

   **Track** (per agent):
   - Tasks completed
   - Average completion time
   - Quality gate pass rate
   - Rework rate

   **Goal**:
   - Quality gate pass > 90% per agent
   - Rework rate < 10% per agent

   **How to Track**:
   ```markdown
   Agent Log Entry:
   - Agent: {agent-name}
   - Task: {description}
   - Duration: {minutes}
   - Quality Gates Passed: {Yes/No}
   - Rework Required: {Yes/No}
   ```

   ### Quality Metrics

   **Track**:
   - Build failures (%)
   - Compiler warnings (count)
   - Security vulnerabilities found (count)
   - Test coverage (%)
   - Documentation completeness (%)

   **Goals**:
   - Build failures: 0%
   - Compiler warnings: 0
   - Security vulnerabilities: 0 critical, 0 high
   - Test coverage: ≥ 70%
   - Documentation: 100% (no placeholders)

   ### User Satisfaction Metrics

   **Track**:
   - Mission objective met (Yes/No)
   - User satisfaction (1-5 scale)
   - User would use framework again (Yes/No)

   **Goal**:
   - Objective met: > 95%
   - Satisfaction: ≥ 4.0 average
   - Would use again: > 90%

   ## Metrics Dashboard (Optional)

   **Simple Text Format**:
   ```markdown
   # DEVGRU Mission Framework Metrics

   ## Period: {date range}

   ### Mission Volume
   - Total Missions: {count}
   - Simple Tasks: {count} ({%})
   - Standard Missions: {count} ({%})
   - Complex Custom: {count} ({%})

   ### Performance
   - Average Duration: {minutes}
   - Quality Gate Pass Rate: {%}
   - Rework Rate: {%}
   - User Satisfaction: {X.X / 5.0}

   ### Quality
   - Build Failures: {count}
   - Security Issues: {count}
   - Test Coverage: {%}

   ### Top Blockers
   1. {blocker type}: {count occurrences}
   2. {blocker type}: {count occurrences}

   ### Improvement Opportunities
   - {area needing improvement}
   - {area needing improvement}
   ```

   ## How to Use Metrics

   ### Weekly Review (5 minutes)

   1. Count missions by classification
   2. Calculate average duration
   3. Count quality gate failures
   4. Identify top blocker

   ### Monthly Review (30 minutes)

   1. Review all weekly data
   2. Identify trends (getting better/worse?)
   3. Identify process improvements
   4. Update protocols if needed

   ### Quarterly Review (2 hours)

   1. Deep dive into metrics
   2. Agent performance analysis
   3. User satisfaction survey
   4. Framework refinement recommendations

   ## Privacy & Ethics

   **Do NOT track**:
   - User personal information
   - Proprietary business logic details
   - Sensitive code/data

   **Only track**:
   - Aggregate performance data
   - Process metrics
   - Quality indicators
   - Timing information

   ## Example Metric Log

   ```markdown
   ## Mission Log: 2025-10-28

   ### Mission 1
   - ID: 2025-10-28-001
   - Type: Standard Mission
   - Protocol: /nop-new-plugin
   - Classification Time: 15s
   - Execution Time: 45m
   - Agents: mission-commander, nopcommerce-plugin-developer
   - Blockers: 0
   - Quality Gates: 10/10 ✅
   - Rework: No
   - User Satisfied: Yes (5/5)

   ### Mission 2
   - ID: 2025-10-28-002
   - Type: Simple Task
   - Classification Time: 8s
   - Execution Time: 3m
   - Agents: None (direct execution)
   - Blockers: 0
   - Quality Gates: N/A
   - Rework: No
   - User Satisfied: Yes (5/5)

   ### Daily Summary
   - Missions: 2
   - Avg Duration: 24m
   - Quality Pass Rate: 100%
   - Rework Rate: 0%
   - Satisfaction: 5.0/5.0
   ```
   ```

**Files Affected**:
- `.claude/requirements/mission-metrics.md` (create new, ~250 lines)

**Acceptance Criteria**:
- [ ] 5 metric categories defined
- [ ] Goals specified for each metric
- [ ] Tracking templates provided
- [ ] Dashboard format example
- [ ] Weekly/monthly/quarterly review process
- [ ] Privacy guidelines included

**Dependencies**: None

**Estimated Effort**: 0.75 hours

---

### ENH-6: Document Model Selection Strategy for Agents

**Problem Statement**:
Some agents specify `model: sonnet` (mission-commander, nopcommerce-plugin-developer) but no documentation explains why or when to specify model.

**Impact**:
- ⚠️ Unclear when to use sonnet vs haiku
- ⚠️ Potential cost optimization opportunities missed
- ⚠️ Inconsistent model usage

**Solution**:
Document model selection strategy.

**Implementation Steps**:

1. **Create** `.claude/requirements/agent-model-selection.md`

   ```markdown
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

   ## When to Specify Model in Agent YAML

   ### Use `model: sonnet` When

   **Agent requires**:
   - Complex reasoning and planning
   - Architectural decision-making
   - Multi-step analysis with context retention
   - High-stakes tasks (security, payments, critical business logic)
   - Synthesis of information from multiple sources

   **Examples**:
   - mission-commander (creates blueprints)
   - nopcommerce-plugin-developer (implements complex plugins)
   - red-team (security analysis requires deep reasoning)

   ### Use `model: haiku` When

   **Agent performs**:
   - Straightforward, well-defined tasks
   - Template-based work
   - Simple CRUD operations
   - Read-only analysis (grep, file reading)
   - Quick validation checks

   **Examples**:
   - simple-validator (checks naming conventions)
   - file-reader (extracts information from files)
   - template-generator (fills in templates)

   ### Omit `model:` (Inherit) When

   **Agent is**:
   - General-purpose
   - User might want to control cost/speed trade-off
   - Task complexity varies based on context

   **Examples**:
   - nopcommerce-data-specialist (sometimes simple, sometimes complex)
   - nopcommerce-ui-specialist (UI complexity varies)
   - Most execution specialists

   ## Current Framework Agents

   | Agent | Model | Rationale |
   |-------|-------|-----------|
   | mission-commander | sonnet | Complex planning, architectural decisions, blueprint synthesis |
   | nopcommerce-plugin-developer | sonnet | Complex implementation, quality enforcement, nopCommerce expertise |
   | All others | (inherit) | Allow user to control cost/speed for their use case |

   ## Cost vs. Quality Trade-off

   ### Sonnet
   - **Pro**: Best quality, handles complexity, fewer errors
   - **Con**: Higher cost, slower response
   - **Use for**: Critical paths, complex tasks, high-stakes work

   ### Haiku
   - **Pro**: Fast, lower cost, good for simple tasks
   - **Con**: May struggle with complexity, less context retention
   - **Use for**: Simple tasks, template work, validation

   ## Recommendations

   ### For New Agents

   **Default**: Omit `model:` (inherit)

   **Specify `model: sonnet` if**:
   - Agent makes architectural decisions
   - Agent requires deep nopCommerce expertise
   - Errors would be costly (security, payments, data integrity)
   - Task requires synthesizing information from 5+ files

   **Specify `model: haiku` if**:
   - Agent follows clear templates
   - Agent performs simple, well-defined operations
   - Speed is more important than perfection
   - Task is low-risk (documentation, simple validation)

   ### Performance Monitoring

   **If agent with `model: sonnet` consistently handles simple tasks**:
   - Consider removing model specification (inherit)
   - Allow user to control cost

   **If agent without model specification frequently fails**:
   - Consider specifying `model: sonnet`
   - Improves success rate for complex tasks

   ## Future Optimization

   **Potential enhancements**:
   - Dynamic model selection based on task complexity
   - Mission complexity score → model selection
   - Cost budget tracking
   - Model performance metrics by agent
   ```

**Files Affected**:
- `.claude/requirements/agent-model-selection.md` (create new, ~150 lines)

**Acceptance Criteria**:
- [ ] Model types explained (sonnet, haiku, opus)
- [ ] When to specify each model documented
- [ ] Current framework agents documented with rationale
- [ ] Cost vs. quality trade-offs explained
- [ ] Recommendations for new agents provided

**Dependencies**: None

**Estimated Effort**: 0.5 hours

---

## Implementation Summary

### Total Effort Estimate
- **Critical**: 4-6 hours (4 issues)
- **Important**: 5-7 hours (5 issues)
- **Enhancement**: 3-4 hours (6 issues)
- **Total**: 12-17 hours

### Recommended Implementation Order

**Week 1: Critical Issues (Production Blockers)**
1. CRIT-2: Bash Permissions (0.5h) - Unblocks quality gates
2. CRIT-1: Mission State Tracking (1.5h) - Enables progress visibility
3. CRIT-3: Failure Recovery (2h) - Prevents broken states
4. CRIT-4: Agent Coordination (2h) - Enables multi-agent missions

**Week 2: Important Issues (Quality & Consistency)**
5. IMP-1: Testing Standards (1.5h) - Harmonizes quality gates
6. IMP-2: Documentation Templates (1h) - Improves deliverable quality
7. IMP-3: red-team Expansion (2.5h) - Strengthens security
8. IMP-4: Simple Task Clarity (1h) - Improves routing
9. IMP-5: Structured Input (1.5h) - Reduces validation errors

**Week 3+: Enhancements (Optimization)**
10. ENH-1: Performance Targets (0.25h)
11. ENH-2: Quick AAR (0.25h)
12. ENH-3: Complexity Scoring (0.5h)
13. ENH-4: Validation Helpers (0.5h)
14. ENH-5: Metrics Tracking (0.75h)
15. ENH-6: Model Selection (0.5h)

### Quick Wins (Do First)
- ✅ CRIT-2: Bash Permissions (30 minutes, high impact)
- ✅ ENH-1: Performance Targets (15 minutes, clarifies expectations)
- ✅ ENH-2: Quick AAR (15 minutes, reduces overhead)

### High-Impact Issues
- 🔥 CRIT-1: Mission State Tracking (enables resumption)
- 🔥 CRIT-3: Failure Recovery (prevents broken states)
- 🔥 IMP-3: red-team Expansion (strengthens security)

---

## Testing & Validation

### After Implementing Each Issue

**Verify**:
1. [ ] Files created/modified successfully
2. [ ] No syntax errors in Markdown
3. [ ] Links between documents work
4. [ ] Templates have no unclosed placeholders
5. [ ] Examples are clear and accurate

### Full Framework Test (After All Issues Complete)

**Test Mission Flow**:
1. Run simple task → Verify classification decision clear
2. Run /nop-new-plugin → Verify structured questions appear
3. Simulate blocker → Verify failure recovery options presented
4. Run complex mission → Verify TodoWrite used for state tracking
5. Check quality gates → Verify bash permissions work

**Acceptance Criteria for Completion**:
- [ ] All 15 issues implemented
- [ ] All new files created
- [ ] All file modifications complete
- [ ] Framework test mission passes
- [ ] Documentation cross-references valid

---

## Maintenance & Future Improvements

### Quarterly Review
- Review metrics (if ENH-5 implemented)
- Identify new common mission patterns
- Consider new slash commands
- Update agent specifications based on learnings

### When to Update This Blueprint
- New issues discovered during production use
- Framework patterns change
- nopCommerce version upgrade requires changes
- User feedback identifies pain points

---

## Quick Reference: Files Created/Modified

### New Files Created (11 files)
- `.claude/requirements/agent-corrections.md` (this file)
- `.claude/requirements/permissions-rationale.md` (CRIT-2)
- `.claude/requirements/testing-standards.md` (IMP-1)
- `.claude/templates/README-template.md` (IMP-2)
- `.claude/templates/CHANGELOG-template.md` (IMP-2)
- `.claude/requirements/slash-command-patterns.md` (IMP-5)
- `.claude/requirements/mission-metrics.md` (ENH-5)
- `.claude/requirements/agent-model-selection.md` (ENH-6)

### Files Modified (6 files)
- `.claude/CLAUDE.md` (CRIT-1, CRIT-3, CRIT-4, IMP-1, IMP-2, IMP-4, ENH-2, ENH-3)
- `.claude/settings.local.json` (CRIT-2)
- `.claude/agents/mission-command/mission-commander.md` (CRIT-1, CRIT-4)
- `.claude/agents/mission-execution/nopcommerce-plugin-developer.md` (CRIT-1, IMP-1, IMP-2)
- `.claude/agents/mission-objective/red-team.md` (IMP-3 - complete replacement)
- `.claude/commands/nop-new-plugin.md` (IMP-5)
- `src/CLAUDE.md` (ENH-1)

---

**Document Status**: Ready for implementation
**Next Step**: Begin with CRIT-2 (Bash Permissions) - Quick win, 30 minutes, unblocks quality gates

**Questions or Clarifications**: Refer to specific issue sections above for detailed implementation steps and acceptance criteria.
