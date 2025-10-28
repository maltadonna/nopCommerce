# Full Init Analysis

Perform a thorough codebase analysis for creating or updating CLAUDE.md files, ensuring no configuration directories or important files are missed.

## Analysis Checklist

You MUST systematically search for ALL of the following patterns before creating or updating any CLAUDE.md file:

### 1. Hidden Configuration Directories
- Search: `**/.claude/**` - Claude Code agent configurations, commands, settings
- Search: `**/.cursor/**` - Cursor IDE rules and configurations  
- Search: `**/.github/**` - GitHub workflows, templates, Copilot instructions
- Search: `**/.vscode/**` - VS Code workspace settings and extensions
- Search: `**/.*` - All hidden files and directories/

### 2. Standard Configuration Files
- Search: `**/CLAUDE*.md` - Existing Claude documentation files
- Search: `**/.cursorrules` - Cursor IDE coding rules
- Search: `**/.claude/settings*.json` - Claude Code settings (both shared and local)
- Search: `**/.mcp.json` - Model Context Protocol configurations
- Search: `**/README*.md` - Project documentation
- Search: `**/package.json` - Node.js project configuration
- Search: `**/requirements.txt` - Python dependencies
- Search: `**/*.csproj` - .NET project files
- Search: `**/*.sln` - Visual Studio solution files
- Search: `**/Cargo.toml` - Rust project configuration
- Search: `**/go.mod` - Go module files
- Search: `**/pom.xml` - Maven project files
- Search: `**/build.gradle` - Gradle build files

### 3. Agent and Command Configurations
- Read: All files in `**/.claude/agents/**` - Available specialist agents
- Read: All files in `**/.claude/commands/**` - Custom slash commands

### 4. Testing and Build Configurations
- Search: `**/jest.config.*` - Jest testing configuration
- Search: `**/vitest.config.*` - Vitest testing configuration  
- Search: `**/pytest.ini` - Python testing configuration
- Search: `**/Dockerfile*` - Container configurations
- Search: `**/docker-compose*.yml` - Docker compose files
- Search: `**/.env*` - Environment configurations (document patterns, don't read contents)

### 5. Code Quality and Linting
- Search: `**/.eslintrc*` - ESLint configuration
- Search: `**/.prettierrc*` - Prettier configuration
- Search: `**/tsconfig*.json` - TypeScript configuration
- Search: `**/.editorconfig` - Editor configuration

## Required Actions

1. **Execute Systematic Search**: Run ALL search patterns above using Glob tool
2. **Read Key Files**: Read contents of discovered configuration files and documentation
3. **Analyze Agent Framework**: If `.claude/` directory exists, thoroughly analyze the mission framework and available agents
4. **Document Architecture**: Identify and document the "big picture" architecture that requires reading multiple files to understand
5. **Extract Commands**: Document build, test, lint, and development commands from discovered configuration files
6. **Create/Update CLAUDE.md**: Include all discovered patterns, configurations, and architectural insights

## Quality Standards

- Do NOT create CLAUDE.md until ALL search patterns have been executed
- Include specific commands found in actual configuration files (don't fabricate)
- Document the mission framework and available agents if `.claude/` directory exists
- Focus on architectural patterns that span multiple files
- Include user override commands and agent classification systems if present
- Mention important security configurations and file access patterns

## Output Requirements

Create a new additional /CLAUDE.md file to drive context that must include:
- Mission Directives from `.claude/Claude.md` as the priority first sections (if present)
- Project type and technology stack
- Actual build/test/lint commands from configuration files
- Available agents and their specializations (if applicable) 
- Code architecture patterns requiring multi-file understanding
- Development workflow and deployment information
- Testing strategies and frameworks in use
- This file should be used in each context window for all future commands