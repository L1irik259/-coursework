# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

A **Franchise Management System** with two .NET applications sharing one SQL Server database (`FranchiseDB`):

- **FranchAdm** — WPF desktop admin app (.NET Framework 4.7.2) for managing franchise data, orders, users, and generating PDF receipts
- **FranchiseAgregator** — ASP.NET Core MVC web app (.NET 10.0) for the public-facing franchise catalog, ordering, reviews, and news

## Local development (macOS / Linux)

The production database lives on `WINDOWS-S4Q07EB\SQLEXPRESS`. Locally, use the Docker Compose file at the repo root (azure-sql-edge — native ARM64):

```bash
# 1. Start SQL Server
docker compose up -d

# 2. Wait ~15 s, then create the schema via EF migrations
cd FranchiseAgregator/FranchiseAgregator
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update

# 3. Run the app  →  http://localhost:5224
dotnet run
```

`ASPNETCORE_ENVIRONMENT=Development` makes both EF tools and `dotnet run` load `appsettings.Development.json`, which overrides `DefaultConnection` to `localhost:1433` (SA password: `FranchiseHub_Dev_2024!`).

```bash
docker compose down      # stop, keep volume
docker compose down -v   # stop, wipe data
```

## Build

```bash
# Web app
cd FranchiseAgregator && dotnet build FranchiseAgregator.sln

# Desktop app (Windows only — requires MSBuild)
cd FranchAdm && msbuild FranchAdm.sln
```

There are no automated tests.

## Architecture

### Shared Database

Both apps target the same `FranchiseDB`. Core tables: `Franchise`, `Franchiser`, `Category`, `FranType`, `FranStatus`, `Users`, `Role`, `Order`, `OrderStatus`, `Review`, `Feedback`, `News`, `Tag`, `FranchiseTag`, `PriceHistory`, `Region`, `Favorite`, `ContactMethod`, `Files`.

### FranchAdm (Desktop)

- XAML pages in `FranchAdm/FranchAdm/Pages/`, windows in `Windows/`
- All entity classes are **auto-generated from `FranchiseModel.edmx`** — do not hand-edit them
- `UserSession.cs` holds the authenticated user for the session
- `PdfReceiptService.cs` / `QRCoder` handle PDF and QR generation

### FranchiseAgregator (Web)

- Standard MVC: `Controllers/`, `Models/`, `Views/`, `Services/`, `wwwroot/`
- `ApplicationDbContext` (EF Core) is the single DB entry point; 14 migrations in `Migrations/`
- `Services/PdfOrderService.cs` generates order PDFs (QuestPDF + QRCoder); registered as a scoped service. `QuestPDF.Settings.License = LicenseType.Community` is set in `Program.cs`
- Cookie auth, 24 h expiration, configured in `Program.cs`

#### Key architectural rules

**`DbFile` dual-mode** — the `Files` table stores either an external link or a generated binary:
- External link: `FileUri` set, `FileContent` null
- Generated PDF: `FileContent` (binary), `FileName` set, `FileUri` null

**PDF auto-attach** — `OrderController.ChangeStatus` calls `AttachPdfToOrder` whenever the new status name equals `"Выполнен"` (case-insensitive). The method loads the full order graph, calls `PdfOrderService.GenerateOrderPdf`, and upserts the `Files` record. Clients download via `GET /Order/DownloadPdf/{orderId}`.

**Namespace** — all C# code uses `FranchiseAgregator` (single g). Razor views previously had a typo (`FranchiseAggregator`, double g) that has been corrected; keep the single-g spelling consistent.

**`ApplicationDbContext.OnConfiguring`** contains a hardcoded Windows fallback connection string used only when EF tools run without a configured host (e.g. bare `dotnet ef` without setting `ASPNETCORE_ENVIRONMENT`). The runtime always uses `appsettings*.json`.
