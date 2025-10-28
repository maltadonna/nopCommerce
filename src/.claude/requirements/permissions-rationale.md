# Bash Permissions Rationale

## Overview

This document explains the bash command permissions configured in `.claude/settings.local.json` for the DEVGRU Mission Command Framework. Permissions are carefully balanced to enable quality gate verification while preventing destructive operations.

---

## Allowed Commands

### Build & Test Commands (Required for Quality Gates)

**`dotnet build*`** - Verify compilation succeeds, no warnings
- **Why needed**: CLAUDE.md line 136 requires "Use Bash (dotnet build) to confirm compilation succeeds"
- **Quality gate**: Zero compiler warnings (mandatory)
- **Example**: `dotnet build NopCommerce.sln --configuration Release`
- **Risk**: None (read-only verification, creates output files but doesn't modify source)

**`dotnet test*`** - Run unit/integration tests
- **Why needed**: Quality gates require tests passing
- **Quality gate**: Unit tests created and passing (CLAUDE.md line 127)
- **Example**: `dotnet test --logger "console;verbosity=detailed"`
- **Risk**: None (executes tests, doesn't modify source)

**`dotnet clean*`** - Clean build artifacts
- **Why needed**: Clean builds to verify from scratch
- **Quality gate**: Ensures fresh build without cached artifacts
- **Example**: `dotnet clean NopCommerce.sln`
- **Risk**: Low (deletes bin/obj folders, but these are regenerated)

**`dotnet restore*`** - Restore NuGet packages
- **Why needed**: Package dependency verification
- **Quality gate**: Ensures all dependencies available
- **Example**: `dotnet restore NopCommerce.sln`
- **Risk**: Low (downloads packages, doesn't modify source)

---

### Version Control Commands (Read-Only, for Context)

**`git status*`** - Check working tree state
- **Why needed**: Failure recovery protocols (CRIT-3) require "Use git status to see what files were modified/created"
- **Use case**: Assess impact of partial changes, identify what was modified
- **Example**: `git status`, `git status --short`
- **Risk**: None (read-only)

**`git diff*`** - View code changes
- **Why needed**: Failure recovery requires "Use git diff to see specific changes"
- **Use case**: Review changes before rollback, assess modification scope
- **Example**: `git diff`, `git diff --staged`, `git diff HEAD~1`
- **Risk**: None (read-only)

**`git log*`** - View commit history
- **Why needed**: Understand recent changes, follow commit message style
- **Use case**: Review project history, check commit patterns
- **Example**: `git log --oneline -10`, `git log --graph`
- **Risk**: None (read-only)

**`git show*`** - View specific commits
- **Why needed**: Examine previous implementations, review changes
- **Use case**: See what changed in specific commit
- **Example**: `git show HEAD`, `git show abc123`
- **Risk**: None (read-only)

---

### Frontend Build Commands (Required for Plugin Assets)

**`npm install*`** - Install JavaScript dependencies
- **Why needed**: Frontend build process requires npm packages
- **Use case**: Install dependencies before running Gulp tasks
- **Example**: `npm install` (in Presentation/Nop.Web directory)
- **Risk**: Low (installs to node_modules, doesn't modify source)

**`npx gulp*`** - Build frontend assets
- **Why needed**: Plugins may have custom CSS/JS that need building
- **Use case**: Compile SCSS, minify JS, copy vendor files
- **Example**: `npx gulp`, `npx gulp copyDependencies`
- **Risk**: Low (builds to wwwroot/lib, doesn't modify source code)

---

### File Operations

**`dir:*`** - Directory listing (already allowed in original config)
- **Why needed**: File structure verification, locate files
- **Use case**: Verify plugin files exist, confirm structure
- **Example**: `dir Plugins/Nop.Plugin.Payments.PayPal`
- **Risk**: None (read-only)

---

## Denied Commands (Destructive Operations)

### Git Remote Operations

**`git push*`** - Push to remote repository
- **Why denied**: Prevents accidental pushes to shared repositories
- **Risk if allowed**: Could push work-in-progress or broken code to remote
- **Workaround**: User manually pushes after review
- **Exception**: None (intentionally blocked)

**`git reset --hard*`** - Hard reset (discards changes)
- **Why denied**: Destructive operation that loses uncommitted work
- **Risk if allowed**: Permanent data loss
- **Workaround**: Use `git restore .` for controlled rollback (not in deny list)
- **Exception**: None (too dangerous)

---

### File Deletion Commands

**`rm -rf*`** - Recursive force delete (Linux/Mac)
- **Why denied**: Can delete entire directories without confirmation
- **Risk if allowed**: Accidental deletion of source code, entire project
- **Workaround**: User manually deletes files, or agent lists files for user to delete
- **Exception**: None (too dangerous)

**`del /f*`** - Force delete (Windows)
- **Why denied**: Can delete files without confirmation
- **Risk if allowed**: Accidental file deletion
- **Workaround**: User manually deletes files
- **Exception**: None (too dangerous)

---

## Ask Before Running (Require User Confirmation)

### Git Write Operations

**`git commit*`** - Create commit
- **Why ask**: User should review changes before committing
- **Reason**: Commits are part of permanent history
- **User sees**: List of files being committed, commit message
- **User confirms**: Yes/No
- **Best practice**: Team Commander drafts commit message, user approves

**`git add*`** - Stage files for commit
- **Why ask**: User should confirm what's being staged
- **Reason**: Prevents staging wrong files (e.g., secrets, .env)
- **User sees**: Files being added to staging area
- **User confirms**: Yes/No
- **Best practice**: Team Commander identifies changed files, user approves staging

---

## Security Considerations

### Commands Are Safe Because

**All allowed commands are**:
1. **Read-only** (git status, git diff, git log, git show, dir) - Cannot modify anything
2. **Local build/test operations** (dotnet build, dotnet test, npm install, npx gulp) - Only modify generated files (bin/, obj/, node_modules/, wwwroot/lib/)
3. **No remote access** - Cannot push to GitHub, deploy to production, access external systems (except npm registry for packages)

**No commands can**:
- Modify source code files (*.cs, *.cshtml, *.json)
- Delete files permanently
- Push to remote repositories
- Expose credentials or secrets
- Execute arbitrary code outside of build/test context

### Defense in Depth

**Layer 1: Permission System**
- Only specific command patterns allowed
- Destructive commands explicitly denied
- Write operations require user confirmation

**Layer 2: Command Patterns**
- Wildcards used carefully (e.g., `dotnet build*` allows `--configuration` flag but not arbitrary commands)
- No shell metacharacters that could be exploited

**Layer 3: User Confirmation**
- Git write operations require user to see and approve
- User has final say on commits and staging

---

## Quality Gate Enablement

These permissions specifically enable the following quality gates from CLAUDE.md:

| Quality Gate | Required Permission | Line in CLAUDE.md |
|--------------|---------------------|-------------------|
| "Use Bash (dotnet build) to confirm compilation succeeds" | `dotnet build*` | 136 |
| "Zero compiler warnings" | `dotnet build*` | 107 |
| "Unit tests created and passing" | `dotnet test*` | 127 |
| "Use Read tool to verify file changes exist" | `dir:*` | 133 |
| "Use git status to see what files were modified" | `git status*` | CRIT-3 (failure recovery) |
| "Use git diff to see specific changes" | `git diff*` | CRIT-3 (failure recovery) |

**Without these permissions, quality gates are unenforceable.**

---

## Permission Evolution

### Original Permissions (Too Restrictive)
```json
{
  "allow": [
    "Bash(claude mcp add:*)",
    "WebFetch(domain:docs.nopcommerce.com)",
    "WebSearch"
  ]
}
```

**Problem**: Could not verify builds, run tests, or assess changes.

### Current Permissions (Balanced)
```json
{
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
```

**Result**: Quality gates enforceable, destructive operations prevented, write operations require user approval.

---

## Future Considerations

### Potential Additions

**If needed in the future**:
- `Bash(docker build*)` - For containerized plugin development
- `Bash(dotnet run*)` - For integration testing with running application
- `Bash(git checkout -b*)` - For creating feature branches (currently requires user to create manually)

**Evaluation criteria**:
1. Is it required for a quality gate?
2. Is it read-only or reversible?
3. Does it pose security risk?
4. Can user do it manually instead?

### Monitoring

**Track permission usage**:
- Which commands are used most frequently?
- Are there patterns of denied commands being attempted?
- Do users frequently override with manual operations?

**Adjust as needed based on**:
- Quality gate requirements evolving
- Framework maturity increasing
- Security posture changing

---

## Examples of Permission Usage

### Example 1: Quality Gate Verification

**Scenario**: Team Commander verifying plugin build succeeds

```bash
# Allowed - verify build
dotnet build Plugins/Nop.Plugin.Payments.PayPal/Nop.Plugin.Payments.PayPal.csproj

# Allowed - check for warnings in output
# (no warnings = quality gate passed)

# Allowed - run tests
dotnet test Tests/Nop.Tests/Nop.Tests.csproj

# Quality gate: PASSED ✅
```

### Example 2: Failure Recovery Assessment

**Scenario**: Mission failed mid-execution, need to assess damage

```bash
# Allowed - see what changed
git status

# Allowed - see specific changes
git diff

# Based on assessment, Team Commander offers user rollback option
# User chooses: git restore . (NOT git reset --hard, which is denied)
```

### Example 3: Commit Creation (User Approval Required)

**Scenario**: Plugin development complete, ready to commit

```bash
# Ask user - stage files
git add Plugins/Nop.Plugin.Payments.PayPal/*
# → User sees file list, confirms

# Ask user - create commit
git commit -m "Add PayPal payment plugin..."
# → User sees commit message, confirms

# Denied - cannot push automatically
git push origin feature/paypal
# → User must push manually (after reviewing changes)
```

---

## Conclusion

The permission configuration is designed to:
- ✅ Enable quality gate verification (build, test, code review)
- ✅ Support failure recovery (git status, git diff)
- ✅ Allow frontend builds (npm, gulp)
- ✅ Prevent destructive operations (git reset --hard, rm -rf, del /f, git push)
- ✅ Require user approval for write operations (git commit, git add)

**Security posture**: Conservative but functional
**Quality gates**: Fully enforceable
**Risk level**: Low (no destructive operations allowed)

**Last updated**: 2025-10-28 (CRIT-2 implementation)
