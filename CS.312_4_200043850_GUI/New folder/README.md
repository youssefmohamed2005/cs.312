# Student Management System (Windows Forms)

A GUI-focused university project built with C# Windows Forms and .NET 8, with a refreshed dashboard-style interface, consistent spacing, cards, navigation, and styled data grids.

## Run
1. Install Visual Studio 2022 with the **.NET desktop development** workload.
2. Open `StudentManagementSystem.csproj`.
3. Set the project as Startup Project and press **F5**.

## Demo login
- Username: `admin`
- Password: `admin123`

## Included screens
- Login with required-field validation and visual feedback.
- Admin dashboard with navigation, summary cards, and a custom GDI+ chart.
- Student CRUD screen with search/filter, department selection, and enrollment date.
- Reports screen with department distribution, CSV export, and async progress simulation.

## UI notes
- Design tokens live in `UI/Theme.cs`; reusable owner-drawn controls (cards, buttons, inputs, nav, toasts) in `UI/Controls.cs`.
- Icons use the built-in *Segoe MDL2 Assets* font (Windows 10/11).

## Notes
- Data is currently stored in memory and resets when the application closes.
- Departments and Subjects are navigation placeholders.
- Replace `StudentRepository` with an EF Core repository and SQL Server database for persistence.
- The login is demo-only; connect it to a secure authentication service before production use.
