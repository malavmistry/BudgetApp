# Agent Decision Rules

## Routing rules

- IF database schema changes THEN invoke Database Agent.
- IF architecture changes or a new abstraction affects multiple layers THEN invoke Architecture Agent.
- IF a new project or layer may be required THEN invoke Project Structure Agent.
- IF new folder or file organization is required THEN invoke Folder Organization Agent.
- IF implementation is required THEN invoke Coding Agent.
- IF public handler or JSON contract changes THEN invoke API Agent.
- IF behavior changes THEN invoke Testing Agent.
- IF work is ready for merge or closure THEN invoke Reviewer Agent.
- IF auth, secrets, sensitive data, or privileged behavior changes THEN invoke Security Agent.
- IF build, deployment, containers, or environment configuration changes THEN invoke DevOps Agent.
- IF docs must be updated THEN invoke Documentation Agent.
- IF debugging is the primary task THEN invoke Troubleshooting Agent.
- IF refactoring is requested without behavior change THEN invoke Refactoring Agent.

## Simple workflow examples

- Simple bug fix: Planner -> Coding -> Reviewer
- Database change: Planner -> Database -> Coding -> Testing -> Reviewer
- New feature: Planner -> Architecture as needed -> Database/API as needed -> Coding -> Testing -> Reviewer -> Documentation