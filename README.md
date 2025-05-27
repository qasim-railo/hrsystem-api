# HRSystem API

This is a modular **ASP.NET Core Web API** project designed to manage HR functions like Employee Records, Company & Department structures, Asset Assignment, Payroll, Leave Management, Gratuity Reports, and more.

---

## 🚀 Features

- Modular structure using DTOs and Service Interfaces
- RESTful API endpoints
- JWT-based Authentication (Frontend Integration ready)
- SQL Server Database
- Document Upload and Download
- Shift Schedules, Attendance, and Overtime Calculation
- Monthly Payroll with Payslip generation
- Final Settlement and Gratuity reports

---

## 🛠️ Technologies Used

- ASP.NET Core 7/8
- Entity Framework Core
- SQL Server
- AutoMapper
- JWT Authentication
- Swagger / OpenAPI

---

## 📦 Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/en-us/download)
- SQL Server (LocalDB or full version)
- Visual Studio / VS Code

### Clone the Repository

```bash
git clone https://github.com/YOUR-USERNAME/hrsystem-api.git
cd hrsystem-api

Restore Dependencies

dotnet restore


Update Database
Update your connection string in appsettings.json and run:

dotnet ef database update

Run the API

dotnet run
Visit https://localhost:5001/swagger to view API documentation.

🔐 Authentication
JWT-based token authentication

Login endpoint: /api/auth/login

Bearer token required for protected routes