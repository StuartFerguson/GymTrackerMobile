# Versioned JSON Export and Safe Import

## Goal

Provide a portable, versioned JSON backup of all local Gym Tracker state and a
safe import workflow that validates before mutation, supports replacement and
merge, and preserves recoverability.

## Scope and architecture

The persistence project owns an explicit backup document contract, serializer,
validator, and transactional service. The document uses DTOs rather than EF
entities so untrusted JSON is never attached to the live `DbContext` during
validation. Schema version `1` is the only supported version in this increment.

The backup document contains metadata plus exercises, workout templates and
template exercises, workout sessions and nested exercises/sets, activity
records, recommendations and outcomes, user settings, and backup metadata.
Every exported entity retains its stable ID; dates are serialized as UTC
ISO-8601 values and enums as strings.

## Validation

Import first parses JSON and validates the complete detached graph. Errors
contain a field path and message. The validator rejects malformed JSON, missing
required collections/metadata, unsupported schema versions, empty or duplicate
IDs, invalid enum values, non-UTC or unparseable dates, out-of-range scalar
values, duplicate keys/order values, and references to missing parent records.
Validation-only operations do not save, delete, or otherwise change database
state.

## Import semantics

Replacement requires explicit confirmation at the UI boundary. The service
writes a timestamped JSON recovery copy of the current state before applying the
validated document. Application occurs inside one database transaction after
clearing tracked state and removing existing rows in dependency order; all rows
from the document are inserted with their original IDs. A failure rolls back
the transaction and leaves the pre-import database intact.

Merge also requires explicit confirmation. It inserts missing entities and
updates entities with matching stable IDs, while retaining existing entities
absent from the imported document. Relationships must be valid against the
combined graph; conflicts that would violate identity or uniqueness constraints
are reported before mutation. Merge is transactional and does not replace the
recovery copy unless it succeeds.

## UI flow

Backup & Settings exposes export, import validation, replacement import, and
merge import actions. File selection and save operations are cancelable. Cancel
returns without confirmation or database mutation. Validation errors are
displayed with field paths; replacement and merge display confirmation before
mutation; operation failures display a concise error and do not report success.

## Testing

Persistence tests cover complete round-trip fidelity, schema and field
validation, validation isolation, transactional replacement, merge behavior,
rollback, and recovery file creation. UI tests cover cancel, confirmation,
success, and error paths through an injected file/backup boundary. Existing
reset and repository tests remain passing.
