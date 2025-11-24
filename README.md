# 🎓 Academic Management System (AMS)

A comprehensive academic management system built with .NET that demonstrates Clean Architecture principles and Clean Code practices. The system manages students, professors, courses, departments, and academic relationships with advanced features like automatic index generation, enrollment tracking, and academic reporting.

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Technology Stack](#-technology-stack)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [Database Schema](#-database-schema)
- [Screenshots](#-screenshots)
- [Clean Code Practices](#-clean-code-practices)
- [Key Functionalities](#-key-functionalities)
- [License](#-license)

## ✨ Features

### Core Functionality
- **Student Management**: Complete CRUD operations for students with automatic university index generation
- **Professor Management**: Manage academic staff with titles and office assignments
- **Course Management**: Define courses with ECTS credits, prerequisites, and lecturer assignments
- **Department Management**: Organize courses and staff by departments
- **Enrollment System**: Many-to-many relationship handling between students and courses with grades and semester tracking
- **Master's Students**: Extended student entity with thesis topics and promoter assignments

### Advanced Features
- **Automatic Index Numbering**: Transactional sequence generation for student and professor indexes (e.g., S1001, P101)
- **Smart Index Decrement**: Automatic rollback of sequence counters when the highest-numbered entity is deleted
- **Prerequisites System**: Self-referencing many-to-many relationship for course prerequisites
- **Office Assignment**: One-to-one relationship between professors and offices
- **Academic Reports**:
  - Most popular professor (by total enrolled students)
  - Course GPA by department
  - Student with the hardest study plan (based on ECTS credits)
- **Data Generation**: Bogus-powered fake data generator for testing and development
- **Terminal GUI**: Interactive console interface using Terminal.Gui

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│           AMS.ConsoleUI (UI)            │
│     Terminal.Gui, Dependency Injection  │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│    AMS.Infrastructure (Infrastructure)  │
│  Entity Framework Core, SQL Server,     │
│  Configurations, Migrations, Seeder     │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│     AMS.Application (Application)       │
│   Services, DTOs, Business Logic,       │
│   CRUD Operations, Reports              │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│        AMS.Domain (Domain)              │
│   Entities, Value Objects, Enums,       │
│   Domain Interfaces                     │
└─────────────────────────────────────────┘
```

### Layer Responsibilities

- **Domain**: Core business entities and rules (Student, Professor, Course, etc.)
- **Application**: Business logic, services, and data transfer objects
- **Infrastructure**: Data access, Entity Framework Core context, and database configurations
- **UI**: User interface layer with Terminal.Gui for interactive console application

## 🛠️ Technology Stack

- **.NET 10.0**: Latest .NET framework
- **Entity Framework Core 10.0**: ORM for data access
- **SQL Server**: Database engine
- **Terminal.Gui 1.19.0**: Cross-platform terminal UI toolkit
- **Bogus 35.6.5**: Fake data generator
- **Microsoft.Extensions.Hosting**: Dependency injection and hosting
- **C# 13**: Latest C# language features

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server) (Express or higher)
- Your favorite IDE (Visual Studio, Rider, or VS Code)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/peterprospl12/academic-management-system.git
   cd academic-management-system
   ```

2. **Update the connection string**
   
   Edit `AMS.ConsoleUI/appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=AcademicManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   cd AMS.ConsoleUI
   dotnet run
   ```

   The application will automatically:
   - Apply database migrations
   - Seed initial data if the database is empty
   - Launch the interactive Terminal.Gui interface

## 📁 Project Structure

```
academic-management-system/
├── AMS.Domain/
│   ├── Entities/           # Domain entities (Student, Professor, Course, etc.)
│   ├── ValueObjects/       # Value objects (Address)
│   ├── Enums/             # Enumerations (AcademicTitle)
│   ├── Common/            # Base classes and interfaces
│   └── Interfaces/        # Domain interfaces
├── AMS.Application/
│   ├── Services/          # Business logic services
│   ├── DTOs/              # Data transfer objects
│   ├── Interfaces/        # Service interfaces
│   └── Common/            # Common application models
├── AMS.Infrastructure/
│   └── Persistence/
│       ├── ApplicationDbContext.cs
│       ├── Configurations/    # Entity configurations
│       ├── Migrations/        # EF Core migrations
│       └── Seeder/           # Data seeding with Bogus
└── AMS.ConsoleUI/
    ├── Views/             # Terminal.Gui views
    ├── Helpers/           # UI helper classes
    ├── Extensions/        # Extension methods
    └── Program.cs         # Application entry point
```

## 🗄️ Database Schema

### Key Entities

- **Student**: FirstName, LastName, UniversityIndex, YearOfStudy, Address (owned entity)
- **Professor**: FirstName, LastName, UniversityIndex, AcademicTitle, Address (owned entity)
- **MasterStudent**: Inherits from Student, adds ThesisTopic and Promoter relationship
- **Course**: Name, CourseCode, Ects, Department, Lecturer, Prerequisites (self-referencing)
- **Enrollment**: Many-to-many join entity between Student and Course with Grade and Semester
- **Department**: Name
- **Office**: One-to-one relationship with Professor
- **SequenceCounter**: Manages auto-incrementing indexes with Prefix and CurrentValue

### Relationships

- **Student ↔ Course**: Many-to-many through Enrollment
- **Course ↔ Course**: Many-to-many self-referencing for prerequisites
- **Professor ↔ Office**: One-to-one
- **MasterStudent → Professor**: Many-to-one (Promoter) with SetNull on delete
- **Course → Professor**: Many-to-one (Lecturer)
- **Course → Department**: Many-to-one

## 📸 Screenshots

### Main Menu
<img width="1078" height="562" alt="image" src="https://github.com/user-attachments/assets/eb98135b-30e1-42c0-bb9e-8d275467524e" />

### Courses Management
<img width="1593" height="569" alt="image" src="https://github.com/user-attachments/assets/fc128e2a-a85e-46e8-a057-8985984d3a2c" />

<img width="1588" height="566" alt="image" src="https://github.com/user-attachments/assets/4cc16e2d-d4ec-4871-9c4c-66d3df000fc6" />


### Assigning office (key: O in Professors List)
<img width="1588" height="568" alt="image" src="https://github.com/user-attachments/assets/accd61b9-ec1f-4b28-94bd-72dc6e8074a7" />


### Academic Reports
<img width="773" height="567" alt="image" src="https://github.com/user-attachments/assets/0c1eaebe-a2f9-4310-9790-a19ca62fc452" />


### Data Generator
<img width="618" height="568" alt="image" src="https://github.com/user-attachments/assets/5a3e00e2-dd1f-47b7-9a31-309711c9a481" />


## 🧹 Clean Code Practices

This project demonstrates various Clean Code principles:

### SOLID Principles
- **Single Responsibility**: Each service class has a single, well-defined purpose
- **Open/Closed**: Extensible through inheritance (e.g., MasterStudent extends Student)
- **Liskov Substitution**: Derived entities can replace base entities
- **Interface Segregation**: Small, focused interfaces for services
- **Dependency Inversion**: Depends on abstractions (IApplicationDbContext, service interfaces)

### Code Quality Features
- **Meaningful Names**: Clear, descriptive names for classes, methods, and variables
- **Small Functions**: Methods focused on single tasks
- **Async/Await**: Proper asynchronous programming throughout
- **LINQ Queries**: Declarative data access with optimizations:
  - `AsNoTracking()` for read-only queries
  - Projection with `Select()` to minimize data retrieval
  - `Include()/ThenInclude()` to avoid N+1 query problems
- **Result Pattern**: Consistent error handling with Result<T> type
- **Value Objects**: Immutable Address record for better domain modeling
- **Repository Pattern**: DbContext acts as repository with service layer abstraction
- **Dependency Injection**: Constructor injection throughout the application
- **Configuration Over Code**: Entity configurations separated from entities

### Entity Framework Optimizations
- Server-side query evaluation
- Eager loading for related data
- Unique indexes on frequently queried columns
- Transactional operations for data consistency
- Owned entities for value objects

## 🎯 Key Functionalities

### 1. Automatic Index Generation
Students and professors receive unique indexes automatically:
```csharp
// Transactional sequence generation
var counter = await GetOrCreateSequenceAsync(prefix);
counter.CurrentValue++;
var newIndex = $"{prefix}{counter.CurrentValue}";
```

### 2. Smart Index Management
When deleting the last entity in a sequence, the counter rolls back:
```csharp
// Prevents gaps at the end of sequences
if (entity.UniversityIndex == $"{prefix}{counter.CurrentValue}")
{
    counter.CurrentValue--;
}
```

### 3. Complex LINQ Queries
Optimized queries for academic reports:
```csharp
// Most popular professor
var topProf = await context.Professors
    .AsNoTracking()
    .Select(p => new { /* projection */ })
    .OrderByDescending(x => x.TotalStudents)
    .FirstOrDefaultAsync();
```

### 4. Terminal UI Navigation
Interactive console application with:
- List views with filtering
- Create/Update/Delete operations
- Report generation and display
- Data seeding control
