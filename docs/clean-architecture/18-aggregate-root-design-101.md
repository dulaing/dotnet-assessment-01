# Aggregate Root Design 101 | DDD, Clean Architecture, .NET 6

- **Video:** https://youtube.com/watch?v=0D3EB2jvQ44
- **Duration:** 5:59
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 18/22

## Key Learnings

- An **aggregate** is a cluster of entities/value objects treated as a single consistency (transactional) boundary. Example domain: a "gathering management system" with two aggregates:
  - `Member` aggregate — just the `Member` entity itself (its own root).
  - `Gathering` aggregate — root `Gathering` entity plus child entities `Attendee` and `Invitation`.
- Core rule: **you may only ever read/modify/persist an aggregate as a whole**, in one operation. Think of it as mapping to a SQL transaction boundary — the aggregate must always be left in a consistent state.
- Consequence: you should **not** expose repository methods that fetch a child entity directly (e.g. an `InvitationRepository.GetById`) if `Invitation` belongs to the `Gathering` aggregate. Instead, fetch the aggregate root and navigate to the child via its collection.

## Concepts & Patterns

- `AggregateRoot` is a thin abstract class in the `Primitives` folder that **inherits from `Entity`** — i.e., an aggregate root *is* an entity, just one that additionally acts as a consistency boundary for the objects beneath it.

```csharp
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id) { }
}
```

- Mark the aggregate root entity by having it inherit `AggregateRoot` instead of `Entity` (e.g. `public sealed class Gathering : AggregateRoot`).

## Implementation Steps

1. Change `Gathering : Entity` → `Gathering : AggregateRoot`.
2. Remove any repository method that fetches a child entity in isolation (e.g. `IInvitationRepository.GetById`) — this breaks the "load whole aggregate" rule.
3. Fix call sites (e.g. `AcceptInvitationCommandHandler`):
   - Add `GatheringId` to the command (`AcceptInvitationCommand`) since you now need it to load the root.
   - Fetch the `Gathering` aggregate root first via `IGatheringRepository`.
   - Locate the child `Invitation` by querying the root's in-memory collection: `gathering.Invitations.FirstOrDefault(i => i.Id == command.InvitationId)`.
   - Validate (`invitation is null` or wrong status) → return early.
   - Proceed with the rest of the logic unchanged, but now the whole aggregate is loaded/modified/persisted together.

## Gotchas & Tips

- This pattern deliberately makes some queries less direct (extra lookup work, more data loaded) in exchange for guaranteed consistency — a trade-off, not a free win.
- Teased in this video: a follow-up will show how **domain events** can pull side-effect logic (e.g. sending an "invitation accepted" email) out of the command handler without changing behavior — see video 21 (Domain Events).
