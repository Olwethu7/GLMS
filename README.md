# GLMS - Global Logistics Management System

## What is this?

This is my final project for the PoE assignment - a logistics management system for TechMove Logistics. They needed to move away from their messy spreadsheets and email system, so I built this web platform to handle contracts, service requests, and international payments.

## What can it do?

### Contracts
- Store all client contracts in one place
- Upload PDF agreements
- Track contract status (Draft, Active, Expired, On Hold)
- Search and filter by date range or status (using LINQ)

### Service Requests
- Create service requests linked to contracts
- **Important business rule:** You can ONLY create requests for active contracts (I added validation for this)
- Each request gets a unique tracking number
- Update request status as work progresses

### Currency Conversion
- When you enter an amount in USD, it automatically converts to ZAR
- Uses a live exchange rate API (ExchangeRate-API)
- The exchange rate is displayed and updates in real-time

### File Upload
- Upload signed contracts as PDF files
- Only PDFs are allowed (no .exe or other files)
- Max file size is 10MB

## How I built it

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core for database stuff
- SQL Server (LocalDB for development)
- Bootstrap 5 for the UI
- xUnit for testing (I have 6 passing tests)

## How to run it

1. Clone the repo
2. Open GLMS.sln in Visual Studio 2022
3. Run `Update-Database` in Package Manager Console
4. Press F5 to run the project

## Testing

I wrote unit tests for:
- Currency conversion math
- File validation (making sure only PDFs work)
- Contract status logic

Open Test Explorer in Visual Studio to see all tests passing.

## What I learned

This was a big project but I got to practice:
- Working with APIs (currency exchange)
- File handling in ASP.NET Core
- Business rule validation
- Unit testing
- Git and GitHub

## Screenshots

*(Add your screenshots here)*

## Author

Olwethu - IIE Student

## Link

Check out the code: https://github.com/Olwethu7/GLMS
