# Lint workflow

Run deterministic CLIs — not an LLM file-by-file checklist.

## Procedure

### 1. Semantic wiki lint

```bash
npm run kb:lint
```

Reports orphan pages, broken `[[wikilinks]]`, and index gaps. Exit code 1 when any issue is found.

### 2. Prettier format check

```bash
npm run format:check
```

Reports markdown formatting violations under `wiki/**/*.md` without modifying files.

### 3. Report results

Present both CLI outputs to the user. If semantic lint fails, still run Prettier check so the user sees all issues in one pass.

Optional format fix (formatting only — does not fix semantic issues):

```bash
npm run format:write
```

## Done when

- `npm run kb:lint` executed and output reported
- `npm run format:check` executed and output reported
- User informed of any failures and suggested fixes
