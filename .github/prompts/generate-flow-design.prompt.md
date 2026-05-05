# Generate Power Automate Flow Design

Create a Power Automate cloud flow design for escalating shift exceptions.

Create or update:

- `docs/power-platform/power-automate-flows.md`

The flow should:

1. Trigger when a Shift Exception is created or updated in Dataverse
2. Check whether Priority is Critical or High
3. Check whether the shift starts within the next 24 hours
4. Notify the operations coordinator
5. Escalate to a trust contact if the exception remains unresolved
6. Write an audit record to Exception Action
7. Handle errors and retries

Include:
- Trigger
- Flow steps
- Conditions
- Dataverse actions
- Notification content
- Error handling
- Test cases