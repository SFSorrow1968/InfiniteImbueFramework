# Imbues Quirks

## Entry 1

- **Issue**: Some imbues do not apply even though spell logic appears valid.
- **Context**: Items can fail application when collider group imbue type or custom spell restrictions block the path.
- **Solution/Workaround**: Validate collider group imbue type and custom spell restrictions on the item before deeper code changes.

## Entry 2

- **Issue**: Imbues may look missing right after item load.
- **Context**: Item modules attach on load and behavior may intentionally wait a few frames.
- **Solution/Workaround**: Account for the initial frame delay before treating this as a true application failure.
