# Library Management Minimal Web API Assessment

## Assessment Overview

The current intern batch has learned the fundamentals of building a .NET Web API, including REST API design, resource naming, request and response contracts, DTOs vs domain models, HTTP status codes, validation, error response structures, OpenAPI/Swagger, and Minimal APIs.

This assessment is designed to evaluate whether they can apply those concepts by building a practical Library Management System using a .NET Minimal Web API.

The goal is not only to make the API work, but to design it properly using clean API contracts, domain concepts, SOLID principles, EF Core, PostgreSQL, and the repository pattern.

## Assessment Title

**Library Management System - .NET Minimal Web API**

## Scenario

A small library wants a backend API to manage its books and members. Members can borrow available books and return them later. The system should track book availability and borrowing history.

The interns must build a Web API that supports book management, member management, borrowing, and returning books.

## Technology Requirements

The solution must use:

* .NET 9 or later
* Minimal APIs
* Entity Framework Core
* PostgreSQL
* Docker or Docker Compose for PostgreSQL
* Repository pattern
* DTOs for request and response contracts
* OpenAPI/Swagger
* Validation
* Consistent error response structure

## Functional Requirements

### 1\. Book Management

The API should support creating, reading, updating, and deleting books.

#### Endpoints

|Action|HTTP Method|Endpoint|
|-|-|-|
|Create a book|POST|`/api/books`|
|Get all books|GET|`/api/books`|
|Get book by id|GET|`/api/books/{id}`|
|Update book|PUT|`/api/books/{id}`|
|Delete book|DELETE|`/api/books/{id}`|

#### Book Data

A book should have the following data:

```csharp
Id
Title
Author
Isbn
PublishedYear
TotalCopies
AvailableCopies
```

#### Business Rules

* `Title` is required.
* `Author` is required.
* `Isbn` is required.
* `Isbn` should be unique.
* `PublishedYear` cannot be in the future.
* `TotalCopies` must be greater than `0`.
* `AvailableCopies` cannot be greater than `TotalCopies`.

## 2\. Member Management

The API should support registering, reading, updating, and deleting library members.

### Endpoints

|Action|HTTP Method|Endpoint|
|-|-|-|
|Register member|POST|`/api/members`|
|Get all members|GET|`/api/members`|
|Get member by id|GET|`/api/members/{id}`|
|Update member|PUT|`/api/members/{id}`|
|Delete member|DELETE|`/api/members/{id}`|

### Member Data

A member should have the following data:

```csharp
Id
FullName
Email
PhoneNumber
RegisteredDate
IsActive
```

### Business Rules

* `FullName` is required.
* `Email` is required.
* `Email` must be valid.
* `Email` should be unique.
* New members should be active by default.
* An inactive member cannot borrow books.

## 3\. Borrowing and Returning Books

The API should support borrowing books, returning books, and viewing borrowing history.

### Endpoints

|Action|HTTP Method|Endpoint|
|-|-|-|
|Borrow a book|POST|`/api/borrowings`|
|Get borrowing history|GET|`/api/borrowings`|
|Get borrowings by member|GET|`/api/members/{memberId}/borrowings`|
|Return a book|POST|`/api/borrowings/{id}/return`|

### Borrowing Data

A borrowing record should have the following data:

```csharp
Id
BookId
MemberId
BorrowedDate
DueDate
ReturnedDate
Status
```

### Borrowing Status

The borrowing status can be:

```csharp
Borrowed
Returned
Overdue
```

### Business Rules

* A book can be borrowed only if `AvailableCopies > 0`.
* A member can borrow only if the member is active.
* A member cannot have more than 3 active borrowed books.
* Borrowing a book should reduce `AvailableCopies` by `1`.
* Returning a book should increase `AvailableCopies` by `1`.
* A book cannot be returned twice.
* The due date should be 14 days from the borrowed date.

## API Design Expectations

Interns should apply proper REST API design principles.

### Resource Naming

Use meaningful resource names:

* Use nouns instead of verbs.
* Use plural resource names.
* Keep endpoint names consistent.

Good examples:

```http
GET /api/books
GET /api/books/{id}
POST /api/books
PUT /api/books/{id}
DELETE /api/books/{id}
POST /api/borrowings
POST /api/borrowings/{id}/return
```

Avoid examples like:

```http
GET /api/getBooks
POST /api/createBook
POST /api/returnBook
```

## Request and Response Contracts

The API should use clear request and response contracts.

Interns should not expose EF Core entities directly from API endpoints.

## HTTP Status Code Expectations

Interns should use correct HTTP status codes.

|Scenario|Expected Status Code|
|-|-|
|Resource created successfully|`201 Created`|
|Resource returned successfully|`200 OK`|
|Resource updated successfully|`200 OK` or `204 No Content`|
|Resource deleted successfully|`204 No Content`|
|Validation failure|`400 Bad Request`|
|Resource not found|`404 Not Found`|
|Duplicate ISBN or email|`409 Conflict`|
|Business rule violation|`400 Bad Request` or `409 Conflict`|

## Validation Requirements

Validation should be applied to all request models.

Interns may use one of the following:

* Manual validation
* Data annotations
* FluentValidation

Validation should cover:

* Required fields
* Email format
* Published year
* Total copies
* Duplicate ISBN
* Duplicate member email
* Borrowing limit
* Book availability

### Example Validation Error Response

```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": \[
    {
      "field": "email",
      "message": "Email is required"
    }
  ]
}
```

## Error Response Structure

The API should return a consistent error response structure.

### Example Error Response

```json
{
  "statusCode": 404,
  "message": "Book not found",
  "traceId": "00-abc..."
}
```

### Expected Error Cases

The API should handle the following error cases properly:

* Book not found
* Member not found
* Borrowing record not found
* ISBN already exists
* Email already exists
* Book is unavailable
* Member is inactive
* Member borrowing limit exceeded
* Book has already been returned

## Database Requirements

The application should use PostgreSQL as the database.

PostgreSQL should run inside a Docker container.

### Example Docker Compose File

```yaml
services:
  postgres:
    image: postgres:16
    container\_name: library-postgres
    environment:
      POSTGRES\_DB: librarydb
      POSTGRES\_USER: libraryuser
      POSTGRES\_PASSWORD: librarypassword
    ports:
      - "5432:5432"
```

## EF Core Requirements

Interns should:

* Configure EF Core with PostgreSQL.
* Create a `DbContext`.
* Create entity mappings where needed.
* Create EF Core migrations.
* Apply migrations to the PostgreSQL database.
* Use repositories instead of calling `DbContext` directly from API endpoints.

## Repository Pattern Requirements

The application should use repository interfaces and implementations.

### Suggested Repositories

```csharp
IBookRepository
IMemberRepository
IBorrowingRepository
```

### Expected Flow

```text
Endpoint -> Service -> Repository -> DbContext
```

API endpoints should be thin. Business logic should not be placed directly inside the endpoint handlers.

## Domain and SOLID Expectations

Interns should apply simple domain concepts and SOLID principles.

### Domain Expectations

The domain model should include behavior where appropriate.

For example:

```csharp
book.BorrowCopy();
borrowing.ReturnBook();
book.ReturnCopy();
```

Business rules should be handled in the domain or application service layer, not scattered across API endpoints.

### SOLID Expectations

The solution should demonstrate:

* Single Responsibility Principle
* Dependency Injection
* Interface-based repository abstractions
* Separation between API, application logic, domain, and infrastructure
* Clear responsibility boundaries between endpoints, services, repositories, and entities

## Suggested Project Structure

```text
Library.Api
  Endpoints
    BookEndpoints.cs
    MemberEndpoints.cs
    BorrowingEndpoints.cs

  Contracts
    Books
    Members
    Borrowings

  Domain
    Entities
    Enums

  Application
    Services
    Interfaces

  Infrastructure
    Data
    Repositories

  Program.cs
```

## OpenAPI and Swagger Requirements

Swagger should be enabled so that APIs can be tested and inspected easily.

The Swagger documentation should show:

* Available endpoints
* Request models
* Response models
* HTTP status codes where possible
* Clear endpoint names or tags

## Assessment Deliverables

Each intern should submit:

1. Source code in a Git repository.
2. A `README.md` file.
3. Docker or Docker Compose setup for PostgreSQL.
4. EF Core migration files.
5. Swagger/OpenAPI enabled.
6. Sample seed data or clear instructions to create sample data.
7. Unit tests or integration tests for important business rules.

## README Expectations

The `README.md` should include:

* Project overview
* Technologies used
* How to run PostgreSQL using Docker
* How to run EF Core migrations
* How to run the API
* How to access Swagger
* Example API requests
* Assumptions made during implementation

## 

## Testing Expectations

At minimum, interns should test the important borrowing rules.

Suggested tests:

* A book cannot be borrowed when no copies are available.
* An inactive member cannot borrow a book.
* A member cannot borrow more than 3 active books.
* Returning a book increases available copies.
* A book cannot be returned twice.

## 

## Bonus Challenges

The following tasks are optional:

* Add pagination to `GET /api/books`.
* Add search by book title or author.
* Add filtering by book availability.
* Add overdue detection.
* Add soft delete for books and members.
* Add global exception handling middleware.
* Add integration tests using Testcontainers for PostgreSQL.
* Add basic audit fields such as `CreatedAt`, `UpdatedAt`, and `DeletedAt`.

## Final Notes for Interns

Focus on building a clean and maintainable API, not just a working API.

The implementation should show that you understand:

* How to design RESTful endpoints.
* How to separate API contracts from domain models.
* How to validate input.
* How to return meaningful HTTP responses.
* How to use EF Core with PostgreSQL.
* How to apply repository pattern thoughtfully.
* How to keep business logic out of API endpoints.
* How to structure a .NET Minimal API project clearly.

