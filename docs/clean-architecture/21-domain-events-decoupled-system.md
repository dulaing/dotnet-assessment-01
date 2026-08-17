# Using Domain Events To Build A Decoupled System That Scales

- **Video:** https://youtube.com/watch?v=AHzWJ_SMqLo
- **Duration:** 14:02
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 21/22

## Key Learnings

- **Problem**: a command handler (`CreateOrderCommandHandler`) both saves to the database *and* publishes an integration event to a message bus (Rebus/RabbitMQ) in the same method. This couples two external systems non-atomically — either operation can fail independently, risking an inconsistent state.
- **Fix**: use **domain events** to decouple "persist to DB" from "notify the outside world." Domain events are raised inside the domain layer, published in-process, and their handlers do the side-effect work (e.g. sending the integration event).
- **Domain event vs. integration event**: a domain event is scoped to your bounded context/domain; an integration event crosses bounded-context boundaries (e.g. onto a message bus for other services).

## Concepts & Patterns

- `DomainEvent` base type (record) in `Primitives`, with a `Guid Id` (useful later to correlate a domain event with the integration event it produces):
```csharp
public abstract record DomainEvent(Guid Id);
```
- Concrete domain events named in **past tense** (they represent facts that already happened): `OrderCreatedDomainEvent`, `LineItemRemovedDomainEvent` — each a `record` inheriting `DomainEvent`, carrying strongly-typed IDs relevant to the event (e.g. `OrderId`, `LineItemId`).
- `Entity` base class gets:
  - a private `List<DomainEvent>` field,
  - a `protected void Raise(DomainEvent domainEvent)` method entities call internally,
  - a public `IReadOnlyCollection<DomainEvent> DomainEvents` getter to read them back out.
- Domain entities raise events at the point the fact occurs, e.g. inside `Order.Create(...)` factory: `Raise(new OrderCreatedDomainEvent(Guid.NewGuid(), Id))`; inside `RemoveLineItem(...)`: `Raise(new LineItemRemovedDomainEvent(...))`. This is easily unit-testable — create the entity, call the method, assert the expected domain event exists.
- **Handling**: `DomainEvent` implements `INotification` (from the lightweight `Mediator.Contracts` NuGet package added to the Domain project), so Application-layer handlers implement `INotificationHandler<TDomainEvent>` (e.g. `OrderCreatedDomainEventHandler`) — this is where the Rebus `IBus.Publish(...)` call for the integration event moves to, decoupling `CreateOrderCommandHandler` down to just "create the order + save."
- **Publishing mechanics**: override `SaveChangesAsync` in the `DbContext`:
```csharp
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var domainEvents = ChangeTracker.Entries<Entity>()
        .Select(e => e.Entity)
        .Where(e => e.DomainEvents.Any())
        .SelectMany(e => e.DomainEvents)
        .ToList();

    var result = await base.SaveChangesAsync(ct);

    foreach (var domainEvent in domainEvents)
        await _publisher.Publish(domainEvent, ct);   // IPublisher from Mediator

    return result;
}
```

## Gotchas & Tips

- **Publish-before-vs-after-SaveChanges trade-off**: publishing *before* `SaveChanges` means handlers act on events that aren't actually persisted yet (not true "facts"), and any handler failure rolls back the whole transaction unnecessarily. Publishing *after* `SaveChanges` (Milan's choice) means events represent actually-persisted facts, but a failing handler still throws and rolls back a request whose business logic already succeeded and was saved — an unwanted side effect.
- **Best solution flagged but not built in this video**: process domain events *together* with the entity changes as outbox messages in the same DB transaction (fully atomic, single system), then publish from the outbox in the background — i.e. the **Outbox pattern**, revisited with a more robust implementation in a follow-up video (see also video 20, which shows outbox conversion happening inside the Unit of Work's `SaveChangesAsync`).
