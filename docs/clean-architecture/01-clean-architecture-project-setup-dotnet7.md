# Clean Architecture Project Setup From Scratch With .NET 7

- **Video:** https://youtube.com/watch?v=fe4iuaoxGbA
- **Duration:** 12:02
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 1/22

## Concepts & Patterns

- Clean Architecture = 4 projects (layers): **Domain**, **Application**, **Infrastructure**, **Presentation**, tied together by a runnable **Web API** host project.
- Dependency rule: inner layers never reference outer layers. Domain references nothing. Application references Domain. Infrastructure/Presentation reference Application (and transitively Domain). The Web API references Presentation, Infrastructure, and Application directly (no need for a direct Domain reference — it comes transitively via Application).
- Each layer gets its own `DependencyInjection` static class exposing an `IServiceCollection` extension method (`AddApplication()`, `AddInfrastructure()`, `AddPresentation()`) so DI registration lives next to the code it wires up, not all crammed into `Program.cs`.

## Implementation Steps

1. Create a blank solution + a solution folder to separate source from tests.
2. **Domain** project (class library, .NET 7): left empty at first; would normally hold entities, business rules, factory interfaces, enums, value objects, custom exceptions.
3. **Application** project (class library): references Domain. Install:
   - `Mediator` (NuGet) — CQRS/use-case dispatch. New versions configure via an `MediatorServiceConfiguration` action, registering handlers `.RegisterServicesFromAssembly(assembly)`.
   - `FluentValidation.DependencyInjectionExtensions` — includes FluentValidation + DI wiring, register validators `.AddValidatorsFromAssembly(assembly)`.
   - Add `DependencyInjection.AddApplication()` extension method wiring both.
4. **Infrastructure** project (class library): references nothing extra yet (external services/DB go here later). Add `Microsoft.Extensions.DependencyInjection.Abstractions` to expose `IServiceCollection`, plus `AddInfrastructure()` method. Optionally split into separate **Infrastructure** + **Persistence** projects (personal preference).
5. **Presentation** project (class library): holds controllers/minimal API endpoints/Razor pages, kept as a *separate project* rather than inside the Web API (see gotcha below). Add `AddPresentation()` method.
6. **Web API** project (ASP.NET Core Web API, .NET 7): the startup/host project. References Presentation, Infrastructure, Application. In `Program.cs`, call `builder.Services.AddApplication().AddInfrastructure().AddPresentation()`.
7. Add logging: `Serilog.AspNetCore` package, configure via `host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration))`, driven from `appsettings.json`. Add `app.UseSerilogRequestLogging()` for HTTP request logging.

## Gotchas & Tips

- Keeping controllers in a separate **Presentation** class library (rather than directly in the Web API project) is a deliberate architecture-enforcement trick — covered in more depth in video 2.
- Clean Architecture is not a silver bullet; evaluate it against alternatives like plain N-layer or Vertical Slice Architecture based on project needs.
