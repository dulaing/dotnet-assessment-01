# RBAC — Library Management API

| Endpoint                               | Admin | Member                      | Reasoning                                                                      |
| -------------------------------------- | ----- | --------------------------- | ------------------------------------------------------------------------------ |
| POST/PUT/DELETE /api/books             | ✅    | ❌                          | Catalog writes need accountability; members shouldn't alter shared inventory   |
| GET /api/books, /api/books/{id}        | ✅    | ✅                          | Catalog is public read data                                                    |
| POST/PUT/DELETE /api/members           | ✅    | ❌ (except PUT own profile) | Prevents self-editing of status/fines; own profile edit allowed for basic info |
| GET /api/members                       | ✅    | ❌                          | Full member list exposes PII of other users                                    |
| GET /api/members/{id}                  | ✅    | ✅ (own id only)            | Ownership check blocks horizontal access to others' records                    |
| POST /api/borrowings                   | ✅    | ✅ (own memberId only)      | Member can only borrow under their own ID, not impersonate others              |
| GET /api/borrowings                    | ✅    | ❌                          | All-borrowings view leaks every member's activity                              |
| GET /api/members/{memberId}/borrowings | ✅    | ✅ (own id only)            | Member views own history only                                                  |
| POST /api/borrowings/{id}/return       | ✅    | ✅ (own borrowing only)     | Prevents returning/modifying another member's borrowing record                 |
