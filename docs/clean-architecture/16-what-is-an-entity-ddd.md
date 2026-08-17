# What Is An Entity? | Domain-Driven Design, Clean Architecture, .NET 6

- **Video:** https://youtube.com/watch?v=00tCda35Bvk
- **Duration:** 10:14
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 16/22

## Key Learnings

Introduces the DDD concept of an **Entity** and builds a reusable base `Entity` class that all domain entities inherit from, centralizing identity-based equality so individual entities don't reimplement it.

## Concepts & Patterns

- **DDD definition (Eric Evans)**: "many objects are not fundamentally defined by their attributes but rather by their continuity and identity" — *continuity* means tracking the object across the application's lifetime, *identity* means uniquely identifying it. An object primarily defined by its identity is an **Entity** (as opposed to a Value Object, defined by its attributes).
- **`init`-only, privately-settable Id**: use `init` instead of `set` so the ID can only be assigned once at construction; making the `init` accessor `private` further restricts assignment to inside the class itself (only via the protected base constructor).
- **Identity-based equality**: two entities are equal if they're the same runtime type *and* have the same `Id` — not structural/attribute equality.

## Implementation Steps

1. Create a `Primitives` folder in the Domain project for cross-cutting base types.
2. Define `public abstract class Entity` (abstract so it can never be instantiated directly — only via a subclass).
3. Add `Guid Id { get; private init; }` plus a `protected Entity(Guid id) => Id = id;` constructor.
4. Override `Equals(object?)`: return false if the argument is null, if its runtime type differs from `GetType()` (not `is Entity`, to correctly reject subclass mismatches), or if it's not an `Entity`; otherwise compare `Id`.
5. Override `GetHashCode()`: return `Id.GetHashCode()` (optionally multiplied by a prime such as 41 for better distribution in hash-based collections).
6. Implement `IEquatable<Entity>` with an `Equals(Entity? other)` overload doing the same null/type/id checks.
7. Add `==` and `!=` static operators built on top of the `Equals` method, handling null on either side.
8. Update existing entities (`Gathering`, `Member`, `Invitation`) to inherit `Entity`, mark them `sealed` (not designed for further inheritance), pass `id` to the base via `: base(id)`, and delete their own local `Id` property (it was hiding the base member).

```csharp
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; private init; }

    protected Entity(Guid id) => Id = id;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj.GetType() != GetType()) return false;
        if (obj is not Entity entity) return false;
        return entity.Id == Id;
    }

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (other.GetType() != GetType()) return false;
        return other.Id == Id;
    }

    public override int GetHashCode() => Id.GetHashCode() * 41;

    public static bool operator ==(Entity? a, Entity? b) =>
        a is not null && b is not null && a.Equals(b);

    public static bool operator !=(Entity? a, Entity? b) => !(a == b);
}
```

## Gotchas & Tips

- Compare `GetType()`, not just `is Entity` — a naive `is`-based check would consider two entities of *different* subtypes with the same `Id` value equal, which is wrong.
- This base class is intentionally minimal (just identity + equality); add further shared behavior later as needed rather than over-engineering upfront.
- Next video in the series: domain validation strategies (two approaches to be compared).
