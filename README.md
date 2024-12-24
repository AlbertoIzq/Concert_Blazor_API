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

- BACKEND
  - ASP .NET Core Web API: .NET framework for building HTTP-based APIs (RESTful web services)
  - Entity Framework Core: Object Relational Mapping (ORM) framework
  - SQL Server: Relational DataBase Management System (RDBMS)
  - C# libraries:
    - Scalar: Open source API platform used as an API client and to generate user-friendly documentation
    - DotEnv.Core: Manage env. files
	- Automapper: Object-to-object mapping library
	- Serilog: Simple .NET logging with fully-structured events

_CONCEPTS USED_

- N-Tier architecture
- REST (Representational State Transfer)
- Environment variables
- DTO (Domain Transfer Object)
- Asynchronous programming
- Design patterns: Repository, Unit of work, Dependency injection
- Logging into a file
- CRUD operations, with hard and soft deletes
- Middlewares:
  - Global exception handing
  - Id route parameter validation
  - Model validation

_TODO_

- API
  - Add Add filtering, sorting, pagination
  - Add Include properties to Get methods
  - Add tracked to Get methods
  - Repository
    - RestoreAsync: In response distinguish between Not existing entity and Entity already restored scenarios
	- Add RemoveRange method

_BUGS_
