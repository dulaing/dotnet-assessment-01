# Transactional Outbox Pattern | Clean Architecture, .NET 6

- **Video:** https://youtube.com/watch?v=XALvnX7MPeo
- **Duration:** 20:42
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 8/22

## Key Learnings

The Outbox pattern guarantees that persisting an entity **and** publishing its domain events happen **atomically** — both succeed or both fail, because the outbox message is written in the *same database transaction* as the entity change. Domain events are then published asynchronously later by a background job, decoupling the (possibly slow/unreliable) side effects (email, external calls) from the request transaction.

## Concepts & Patterns

- **OutboxMessage entity**: `Id`, `Type` (domain event type name), `Content` (JSON), `OccurredOnUtc`, `ProcessedOnUtc` (nullable — null = not yet processed).
- **EF Core `SaveChangesInterceptor`** — a class `ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor` overrides `SavingChangesAsync`. Steps:
  1. Get `DbContext` from `eventData`; bail if null.
  2. `changeTracker.Entries<AggregateRoot>()` → select the entity instances.
  3. `SelectMany(agg => agg.GetDomainEvents())` — requires adding `GetDomainEvents()` and `ClearDomainEvents()` methods to the `AggregateRoot` base class (alongside the existing `Raise`).
  4. Map each domain event → `OutboxMessage` (`Id = Guid.NewGuid()`, `OccurredOnUtc = DateTime.UtcNow`, `Type = domainEvent.GetType().Name`, `Content = JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })`). `TypeNameHandling.All` is required so the concrete event type can be deserialized later.
  5. `dbContext.Set<OutboxMessage>().AddRange(outboxMessages)`.
- **Wiring the interceptor**: register it as a singleton, then when configuring the `DbContext` use the `UseSqlServer(...).AddInterceptors(interceptor)` overload (resolve the interceptor via `IServiceProvider` in the `AddDbContext` factory-style registration).
- **Background processing job** — uses the **Quartz.NET** library (`Quartz` + `Quartz.Extensions.Hosting` NuGet packages):
  ```csharp
  [DisallowConcurrentExecution]
  public class ProcessOutboxMessagesJob(ApplicationDbContext dbContext, IPublisher publisher) : IJob
  {
      public async Task Execute(IJobExecutionContext context)
      {
          var messages = await dbContext.Set<OutboxMessage>()
              .Where(m => m.ProcessedOnUtc == null)
              .Take(20)
              .ToListAsync(context.CancellationToken);

          foreach (var message in messages)
          {
              var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(message.Content, jsonSettings);
              if (domainEvent is null) continue; // TODO: log/handle properly

              await publisher.Publish(domainEvent, context.CancellationToken);
              message.ProcessedOnUtc = DateTime.UtcNow;
          }
          await dbContext.SaveChangesAsync();
      }
  }
  ```
- **Quartz registration** in `Program.cs`: `services.AddQuartz(config => { var jobKey = new JobKey(nameof(ProcessOutboxMessagesJob)); config.AddJob<ProcessOutboxMessagesJob>(jobKey).AddTrigger(t => t.ForJob(jobKey).WithSimpleSchedule(s => s.WithIntervalInSeconds(10).RepeatForever())); config.UseMicrosoftDependencyInjectionJobFactory(); }); services.AddQuartzHostedService();`
- Quartz jobs run in a custom **scoped** service scope — so scoped services (like a `DbContext`) can be injected directly into the job constructor.

## Gotchas & Tips

- Batch processing (e.g. `Take(20)`) avoids loading unbounded rows each run.
- Missing piece called out explicitly: **no try/catch around publishing** in the demo — production code needs exception handling per message so one bad message doesn't block the batch or crash the job (left as "homework").
- Domain events can only be raised by `AggregateRoot` instances — the interceptor filters `changeTracker.Entries<AggregateRoot>()` specifically.
- `[DisallowConcurrentExecution]` ensures only one instance of the job runs at a time.
