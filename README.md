# Concert_Blazor_API

Blazor + ASP .NET Core Web API project to create a live music on-demand website to book a concert by selecting the setlist and adding different services, like an e-commerce website.
Users can make song requests which are managed by Admin.

_TECHNOLOGIES USED_

- .NET 9

- FRONTEND
  - Blazor: .NET front-end web framework to create single-page applications (SPA)
  - Bootstrap: CSS Framework for developing responsive and mobile-first websites
  - C# libraries:
    - Quickgrid: Razor component for quickly and efficiently displaying data in tabular form
    - Serilog: Simple .NET logging with fully-structured events
	- Blazored.Toast: Toast notifications
	- Blazored.Modal: Modal messages

- BACKEND
  - ASP .NET Core Web API: .NET framework for building HTTP-based APIs (RESTful web services)
  - Entity Framework Core: Object Relational Mapping (ORM) framework
  - SQL Server: Relational DataBase Management System (RDBMS)
  - C# libraries:
    - Scalar: Open source API platform used as an API client and to generate user-friendly documentation
    - DotEnv.Core: Manage env. files
	- Automapper: Object-to-object mapping library
	- Serilog: Simple .NET logging with fully-structured events
	- AspNetCore.Identity: Manage authentication and authorization
	- JWT: Manage JsonWebToken
	
_CONCEPTS USED_

- GENERAL
  - N-Tier architecture:
    - DataAccess: 
	  - DataAccess: Class library project with interfaces
	  - DataAccess.API: .NET CORE Web API project
	  - DataAccess.InMemory: Class library project with an in-memory database used as a PoC (Proof Of Concept)
	- UIWasm: Blazor WebAssembly standalone project
	- Business: Class library project with business logic
  - REST API (Representational State Transfer)
  - Design patterns: Repository, Unit of work, Dependency injection
  - Environment variables
  - DTO (Domain Transfer Object)
  - Asynchronous programming
  - Authentication and authorization, refresh token-based
  - Json Web Token (JWT) for access token
  - Backend-for-frontend pattern (BFF): Used to manage Blazor authorization&authentication in the API
    - JWT token for external API calls
	- HTTP only cookie used for frontend calls

- FRONTEND
  - Exception handling in API calls
  - Logging Exceptions into BrowserConsole
  - Toast notifications
  - Modal messages
  - Cookies used to manage authentication & authorization with authentication state provider

- BACKEND
  - CRUD operations, with hard and soft deletes
  - Middlewares:
    - Global exception handing
    - Id route parameter validation
    - Model validation
  - Action filters
  - Logging into a file:
    - Exceptions
	- API requests
	- API responses
  - Advanced Encryption Standard (AES) to store refresh token value in database
  - AuditLog table to log all changes in db (added, modified, soft deleted, restored and deleted entities)

_TODO_

- FRONTEND
  - If API calls also for authorization, create HttpClientFactory
  - Add retry policy with Polly
  - Add logging to API requests and responses
  - Send logs to a backend API for centralized storage.

- BACKEND
  - Add Add filtering, sorting, pagination
  - Add Include properties to Get methods
  - Add tracked to Get methods
  - Repository
    - RestoreAsync: In response distinguish between Not existing entity and Entity already restored scenarios
	- Add RemoveRange method
	- If soft deleted already, you must be able to deleted without restoring it first
  - Add a running background service, like I did in Library Manager???
  - Authentication&Authorization
	- (Idea) Implement roleEnum to store all available roles
	- Set tokens to work depending on logged in device
	- Best practices:
	  - Register:
	    - Limit registration: Implement email verification before enabling the account
		- Log and monitor user registrations (include IP-Address and User-Agent)
	  - Login:
	    - Rate limiting to prevent brute-force attacks: Used to restrict the number of requests a user/IP can make
		- Lock account after multiple failed attempts
		- Log login attempts
 
_BUGS_

- FRONTEND
  - If API not running, error when fetching SongRequest
