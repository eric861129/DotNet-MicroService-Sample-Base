# Skills Index

This project vendors a local copy of selected Codex skills under `.agent/skills` so they can be reviewed and selected in the context of this microservice solution.

## Skill Location

- Local project skills: `.agent/skills`
- Installed global skills: `C:\Users\EricHuang黃祈豫\.codex\skills`

## Recommended Usage In This Repository

Use these first when working on the corresponding task types in this solution:

| Scenario | Recommended skill |
| --- | --- |
| Build or refactor .NET service code | `csharp-developer` |
| Design service boundaries, messaging, or resilience | `microservices-architect` |
| Design REST or GraphQL contracts | `api-designer` |
| Write ADRs or review system structure | `architecture-designer` |
| Generate or improve technical docs | `code-documenter` |
| Reverse-engineer existing code or undocumented behavior | `spec-miner` |
| Explore requirements before implementation | `brainstorming` |
| Create an implementation plan before coding | `writing-plans` |
| Execute a previously written implementation plan | `executing-plans` |
| Debug failures or regressions | `systematic-debugging` |
| Implement with tests first | `test-driven-development` |
| Verify before claiming completion | `verification-before-completion` |
| Request or process code review | `requesting-code-review`, `receiving-code-review` |

## Skill Catalog

### Architecture And API

| Skill | Primary use | Project path |
| --- | --- | --- |
| `api-designer` | REST or GraphQL API design, resource modeling, versioning, pagination, error contracts | `.agent/skills/api-designer` |
| `architecture-designer` | High-level system design, ADRs, scalability planning, architecture review | `.agent/skills/architecture-designer` |
| `microservices-architect` | Bounded contexts, service decomposition, DDD, sagas, CQRS, distributed tracing | `.agent/skills/microservices-architect` |

### .NET And Documentation

| Skill | Primary use | Project path |
| --- | --- | --- |
| `csharp-developer` | C#, .NET 8+, ASP.NET Core, EF Core, CQRS, Blazor, async service implementation | `.agent/skills/csharp-developer` |
| `code-documenter` | Technical docs, OpenAPI, docstrings, guides, documentation quality checks | `.agent/skills/code-documenter` |
| `spec-miner` | Reverse engineering, dependency mapping, extracting specs from existing code | `.agent/skills/spec-miner` |

### Workflow And Delivery

| Skill | Primary use | Project path |
| --- | --- | --- |
| `brainstorming` | Clarify intent, requirements, and design before implementation | `.agent/skills/brainstorming` |
| `dispatching-parallel-agents` | Split independent work into parallel tracks | `.agent/skills/dispatching-parallel-agents` |
| `executing-plans` | Carry out a written implementation plan with checkpoints | `.agent/skills/executing-plans` |
| `finishing-a-development-branch` | Choose merge, PR, or cleanup path after implementation is done | `.agent/skills/finishing-a-development-branch` |
| `receiving-code-review` | Evaluate review feedback rigorously before applying it | `.agent/skills/receiving-code-review` |
| `requesting-code-review` | Request focused review before merge or handoff | `.agent/skills/requesting-code-review` |
| `subagent-driven-development` | Execute independent plan tasks via separate subagents | `.agent/skills/subagent-driven-development` |
| `systematic-debugging` | Structured debugging before proposing fixes | `.agent/skills/systematic-debugging` |
| `test-driven-development` | Write tests first for features and bug fixes | `.agent/skills/test-driven-development` |
| `using-git-worktrees` | Isolate feature work in a dedicated git worktree | `.agent/skills/using-git-worktrees` |
| `using-superpowers` | Discover and apply skills consistently at session start | `.agent/skills/using-superpowers` |
| `verification-before-completion` | Run verification before claiming success | `.agent/skills/verification-before-completion` |
| `writing-plans` | Create implementation plans from requirements or specs | `.agent/skills/writing-plans` |
| `writing-skills` | Create, edit, or verify skill definitions | `.agent/skills/writing-skills` |

## Upstream Sources

These project-local copies were sourced from:

- `https://github.com/Jeffallan/claude-skills/tree/main/skills/csharp-developer`
- `https://github.com/Jeffallan/claude-skills/tree/main/skills/microservices-architect`
- `https://github.com/Jeffallan/claude-skills/tree/main/skills/code-documenter`
- `https://github.com/Jeffallan/claude-skills/tree/main/skills/api-designer`
- `https://github.com/Jeffallan/claude-skills/tree/main/skills/architecture-designer`
- `https://github.com/Jeffallan/claude-skills/tree/main/skills/spec-miner`
- `https://github.com/obra/superpowers/tree/main/skills`

## Maintenance Notes

- Treat `.agent/skills` as vendored content copied from upstream skill repositories.
- If upstream skill content changes, re-copy the affected skill directory and update this index if the intent or grouping changes.
- Review any skill-specific scripts or assets before modifying them locally.
