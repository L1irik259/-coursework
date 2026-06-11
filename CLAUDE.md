# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

A **Franchise Management System** consisting of two .NET applications sharing a single SQL Server Express database (`FranchiseDB` on `WINDOWS-S4Q07EB\SQLEXPRESS`):

- **FranchAdm** — WPF desktop admin app (.NET Framework 4.7.2) for managing franchise data, orders, users, and generating PDF receipts
- **FranchiseAgregator** — ASP.NET Core MVC web app (.NET 10.0 SDK, project targets .NET 6.0) for the public-facing franchise catalog, ordering, reviews, and news

## Local development (macOS / Linux)

The production database is SQL Server Express on Windows (`WINDOWS-S4Q07EB\SQLEXPRESS`). Locally, use Docker:

```bash
# 1. Start SQL Server (azure-sql-edge — native ARM64)
docker compose up -d

# 2. Wait ~15 s for the server to be ready, then apply migrations
cd FranchiseAgregator/FranchiseAgregator
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update

# 3. Run the web app
dotnet run
# → http://localhost:5224
```

`ASPNETCORE_ENVIRONMENT=Development` makes EF tools and `dotnet run` pick up
`appsettings.Development.json`, which overrides `DefaultConnection` to point at
`localhost:1433` (SA password: `FranchiseHub_Dev_2024!`).

```bash
# Stop and remove the container (data is preserved in the sqlserver_data volume)
docker compose down

# Wipe data too
docker compose down -v
```

## Build & Run

```bash
# Build the web application
cd FranchiseAgregator
dotnet build FranchiseAgregator.sln

# Run the web application
dotnet run --project FranchiseAgregator/FranchiseAgregator.csproj

# Build the desktop application (requires Windows + Visual Studio or MSBuild)
cd FranchAdm
msbuild FranchAdm.sln

# Apply EF Core migrations (web app)
cd FranchiseAgregator/FranchiseAgregator
dotnet ef database update
```

There are no automated tests in this repository.

## Architecture

### Shared Database
Both applications target the same SQL Server database. The schema covers: `Franchise`, `Franchiser`, `Category`, `FranType`, `FranStatus`, `Users`, `Role`, `Order`, `OrderStatus`, `Review`, `Feedback`, `News`, `Tag`, `FranchiseTag`, `PriceHistory`, `Region`, `Favorite`, `ContactMethod`.

### FranchAdm (Desktop)
- XAML pages live in `FranchAdm/FranchAdm/Pages/` and windows in `FranchAdm/FranchAdm/Windows/`
- Entity classes are **auto-generated** from an EDMX model (`FranchiseModel.edmx`) — do not hand-edit them
- `UserSession.cs` holds the authenticated user for the session
- `PdfReceiptService.cs` uses iTextSharp for PDF generation; `QRCoder` is used for QR code generation

### FranchiseAgregator (Web)
- Standard MVC layout: `Controllers/`, `Models/`, `Views/`, `wwwroot/`
- `ApplicationDbContext` (EF Core) is the single data access entry point; 13 migration files are in `Migrations/`
- Cookie-based auth with 24-hour expiration, configured in `Program.cs`
- `HomeController` — landing page and popular franchises
- `CatalogController` — franchise search with filtering, sorting, and pagination
- `FranchiseController` — franchise detail pages, tags, recommendations
- `OrderController` — order creation and management
- `AccountController` — login, register, logout
- `AdminController` — admin dashboard
- `FavoritesController`, `ReviewController`, `NewsController`, `FeedbackController` — self-explanatory
