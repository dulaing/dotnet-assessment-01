# How To Use Domain-Driven Design In Clean Architecture

- **Video:** https://youtube.com/watch?v=1Lcr2c3MVF4
- **Duration:** 30:27
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 15/22

## Key Learnings

Live refactor of a "Gatherly" (gathering/event management) app from an **anemic domain model** (entities are plain data bags; all logic sits in command handlers) to a **rich domain model** (behavior and invariants live on the entities themselves). Entities: `Member`, `Gathering`, `Invitation`, `Attendee`.

## Concepts & Patterns

- **Constructors for required state**: replace handler-side property assignment with an entity constructor that takes all mandatory fields.
- **Static factory methods over "fat" constructors**: when creation involves branching calculation logic (e.g. gathering type determines whether to set `MaxNumberOfAttendees` vs. an invitation-expiry timestamp), put that logic in a static `Gathering.Create(...)` factory rather than the constructor itself — keeps the constructor simple.
- **Private constructors**: once a factory method is the sanctioned creation path, make the constructor `private` so callers can't bypass the invariants the factory enforces.
- **Private property setters everywhere**: prevents any external code from mutating entity state outside of intentional methods.
- **Aggregate ownership**: when handler code creates a child object (`Invitation`) and immediately adds it to a parent's collection (`Gathering.Invitations`), that's a signal the parent should own creation — move the logic into a `Gathering.SendInvitation(member)` method that both builds and appends the `Invitation`.
- **`internal` constructors**: once only the owning aggregate creates a child entity, mark that child's constructor `internal` so it's only callable from within the Domain project (not bypassable by application-layer code).
- **Collection encapsulation**: expose `IReadOnlyCollection<T>` properties backed by a `private readonly List<T>` field — prevents external `Add`/`Remove` calls while still allowing internal mutation from within the entity's own methods.
- **Domain validation inside behavior methods**: e.g. "creator can't invite themselves" and "can't invite to a past gathering" checks move from the handler into `Gathering.SendInvitation`, throwing exceptions with messages (deep dive on validation strategies deferred to a future video).
- **Status-transition methods**: `Invitation.Accept()` / `Invitation.Expire()` encapsulate state changes, marked `internal` since only `Gathering` calls them.

## Implementation Steps

1. `CreateGatheringCommandHandler`: move plain property assignment into a `Gathering(id, creator, type, scheduledAtUtc, name, location)` constructor, then move the type-specific max-attendees/expiry calculation into a static `Gathering.Create(...)` factory (accepting the extra `maxNumberOfAttendees` / `invitationsValidBeforeInHours` params). Make the constructor `private` and all setters `private` once the factory is the only entry point.
2. `SendInvitationCommandHandler`: give `Invitation` a constructor taking `(id, member, gathering)` that derives `MemberId`/`GatheringId` and sets `Status = Pending`, `CreatedOnUtc = now` internally. Move the "add to gathering's list" + validation logic into `Gathering.SendInvitation(member)`, which creates the invitation, appends it, and returns it. Make `Invitation`'s constructor `internal`.
3. Encapsulate `Gathering.Invitations` and `Gathering.Attendees` as `IReadOnlyCollection<T>` over private backing lists.
4. `AcceptInvitationCommandHandler`: move the expiry check (gathering full for fixed-attendee type, or expiry date passed) and accept/expire branching into `Gathering.AcceptInvitation(invitation)`. On expiry: call `invitation.Expire()`, return `null` (method returns nullable `Attendee`). On success: call `invitation.Accept()`, construct a new `Attendee` via an `internal` constructor that derives its fields from the `Invitation`, add it to the attendees list, increment the attendee count, return it.
5. Resulting handler is thin: fetch entities from repositories → call one rich domain method → `if (attendee is not null) repository.Add(attendee)` → always `SaveChangesAsync` (state changed either way) → only send the "invitation accepted" email `if (invitation.Status == InvitationStatus.Accepted)`.

## Gotchas & Tips

- Watch for handler code that creates an object and immediately hands it to a parent collection — that's the tell that creation logic belongs on the parent aggregate, not the handler.
- Deliberately unresolved in this video (flagged as food for thought / future topic): the handler calls `SaveChangesAsync` (DB write) and then calls an external email service — what happens if the DB write succeeds but the email send fails, and could/should the email call happen before the DB write instead? Sets up a later video (likely outbox pattern / domain events).
- Recommended reading: Eric Evans' *Domain-Driven Design* book.
