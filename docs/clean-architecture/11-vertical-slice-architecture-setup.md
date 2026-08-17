# Vertical Slice Architecture Project Setup From Scratch

- **Video:** https://youtube.com/watch?v=msjnfdeDCmo
- **Duration:** 22:43
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 11/22

## Key Learnings

Clean Architecture organizes code **by layer** (domain, application, presentation, infrastructure as concentric circles, dependencies point inward). Vertical Slice Architecture keeps the same conceptual layers but organizes code **by feature**: everything needed for one feature (command, handler, endpoint, validator) lives together — often in a single file — instead of being scattered across layer folders. This maximizes cohesion within a feature and minimizes coupling between features.

## Concepts & Patterns

- **Folder structure**: `Features/<Entity>/<FeatureName>.cs` — e.g. `Features/Articles/CreateArticle.cs` containing a static class `CreateArticle` with nested types.
- **Single-file feature slice pattern** (`CreateArticle` example):
  ```csharp
  public static class CreateArticle
  {
      public sealed class Command : IRequest<Result<Guid>>
      {
          public string Title { get; init; } = string.Empty;
          public string Content { get; init; } = string.Empty;
          public List<string> Tags { get; init; } = new();
      }

      public sealed class Validator : AbstractValidator<Command>
      {
          public Validator()
          {
              RuleFor(c => c.Title).NotEmpty();
              RuleFor(c => c.Content).NotEmpty();
          }
      }

      internal sealed class Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
          : IRequestHandler<Command, Result<Guid>>
      {
          public async Task<Result<Guid>> Handle(Command request, CancellationToken ct)
          {
              var validation = await validator.ValidateAsync(request, ct);
              if (!validation.IsValid)
                  return Result.Failure<Guid>(new Error("CreateArticle.Validation", validation.ToString()));

              var article = new Article { Id = Guid.NewGuid(), Title = request.Title, /* ... */ CreatedOnUtc = DateTime.UtcNow };
              dbContext.Articles.Add(article);
              await dbContext.SaveChangesAsync(ct);
              return article.Id;
          }
      }
  }
  ```
- **Endpoint registration via Carter** (`ICarterModule`) instead of a plain extension method — avoids naming collisions between many `MapEndpoint`-style extension methods across features:
  ```csharp
  public class CreateArticleEndpoint : ICarterModule
  {
      public void AddRoutes(IEndpointRouteBuilder app) =>
          app.MapPost("api/articles", async (CreateArticleRequest request, ISender sender) =>
          {
              var command = request.Adapt<CreateArticle.Command>(); // Mapster
              var result = await sender.Send(command);
              return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result.Value);
          });
  }
  ```
  Registration: `builder.Services.AddCarter();` then `app.MapCarter();` (scans assembly for `ICarterModule` implementations).
- **Decoupling API contract from internal command**: define a separate `CreateArticleRequest` DTO (in a `Contracts` folder) rather than binding the endpoint directly to the `Command`, so the public API shape and internal command can evolve independently. Use **Mapster**'s `.Adapt<T>()` for simple one-to-one mapping instead of hand-written mapping code.
- **Query slice example** (`GetArticle`): `Query : IRequest<Result<ArticleResponse>>` with just an `ArticleId`; handler projects directly from `DbContext` into the `ArticleResponse` contract via `Select(...)`, returns `Result.Failure<ArticleResponse>(...)` when `FirstOrDefaultAsync` yields null. Endpoint uses `MapGet("api/articles/{id}", ...)`, returns `Results.NotFound(result.Error)` or `Results.Ok(result.Value)`.
- **NuGet packages used**: `Mediator` (CQRS), `FluentValidation` (+ DI extensions), `Carter` (minimal API module registration), `Mapster` (object mapping).

## Gotchas & Tips

- A full feature (command + handler + validator + endpoint) fits in ~60 lines in one file — contrast with Clean Architecture where the same feature spans domain/application/presentation/infrastructure folders.
- Don't define the `ICarterModule` endpoint class *nested inside* the feature's static class — Carter's assembly scan needs it as a standalone top-level class, and nesting causes conflicts/naming issues (author catches and fixes this mid-video).
- The presenter notes he already achieves much of this cohesion within Clean Architecture by keeping command/handler/query/validator together in the application layer's folder — vertical slices push that further by also pulling in the endpoint (presentation) and can pull in infrastructure concerns.
- Trade-off: works well per-feature, but watch for duplicated logic/coupling creeping in *between* slices — the architecture doesn't automatically prevent that, it just makes within-feature cohesion easy.
