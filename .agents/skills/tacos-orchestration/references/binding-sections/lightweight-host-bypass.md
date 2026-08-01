# Lightweight-host bypass

When the runtime does not support Task or subagent delegation, MUST-delegate steps (gather, summarize, spec review, apply review, e2e-scenarios, jira regenerate) **MAY** run inline in the parent session. The turn summary **MUST** note inline execution. Do not fail the workflow solely because delegation is unavailable.
