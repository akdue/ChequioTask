# ChequioTask

ASP.NET Core MVC cheque management app built with **.NET 9**, **Identity (with roles)**, **EF Core + SQL Server**, and a clean UI using **Bootswatch – Flatly**.

---

## ✨ Features
- **Authentication & Roles**: ASP.NET Core Identity with `Admin` and `User` roles.
- **Cheque CRUD**: Create / Read / Update / Delete **Cheques** with validation.
- **Status Lifecycle**: `Draft`, `Issued`, `Cleared`, `Bounced`, `Voided`.
- **Search & Filters**: query by number / payee, status filter, issue/due date range.
- **Print-Friendly Page**: dedicated `/Cheques/Print/{id}` view (A5 landscape, compact CSS) to avoid multi-page prints.
- **CreatedAtUtc**: server-side timestamp + optional DB default (`GETUTCDATE()`).
- **Polished UI**: Flatly theme (local assets via LibMan) and improved forms/tables.
- **Pages**: Home (hero), About, Identity (Login/Register/Forgot/Reset), Cheques.

---

## 🧱 Tech Stack
- **.NET**: .NET 9, ASP.NET Core MVC
- **Auth**: ASP.NET Core Identity (Razor Pages UI) with Roles
- **Data**: EF Core, SQL Server / LocalDB
- **UI**: Bootstrap 5 + Bootswatch (Flatly), client-side validation

---

## 📁 Project Structure (high level)
```
ChequioTask/
├── Controllers/
│   └── ChequesController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   └── Cheque.cs
├── Views/
│   ├── Shared/_Layout.cshtml
│   └── Cheques/ (Index, Details, Create, Edit, Delete, Print)
├── Areas/Identity/Pages/Account/
│   ├── Login.cshtml
│   ├── Register.cshtml
│   ├── ForgotPassword.cshtml
│   └── ResetPassword.cshtml
├── wwwroot/
│   ├── css/print-cheque.css
│   └── lib/ (bootswatch/flatly, bootstrap bundle)
├── appsettings.json
└── Program.cs
```

---

## 🚀 Getting Started

### Prerequisites
- **Visual Studio 2022** (latest) or `dotnet` SDK **9.x**
- **SQL Server** or **LocalDB**

### 1) Configure the connection string
`appsettings.json` (example for LocalDB):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ChequioTaskDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False"
}
```

### 2) Apply EF Core migrations
**Option A — Visual Studio / Package Manager Console**
```
Update-Database
```

**Option B — dotnet-ef CLI**
```bash
# (one time) install tools if needed
 dotnet tool install --global dotnet-ef
# from the project folder
 dotnet ef database update
```

### 3) Run the app
- **Visual Studio**: F5 / Ctrl+F5
- **CLI**: `dotnet run`

> **Email confirmation:** by default the template can be set to require confirmed accounts. For local development you can temporarily disable it in `Program.cs` (`RequireConfirmedAccount = false`).

---

## 🔐 Roles & Access
- **Admin**: full CRUD on cheques, can access Create/Edit/Delete.
- **User**: can sign in and view cheques (Index/Details) only.

| Area / Action                    | User | Admin |
|----------------------------------|:----:|:-----:|
| Cheques → Index / Details        |  ✅  |  ✅   |
| Cheques → Create / Edit / Delete |  ❌  |  ✅   |
| Cheques → Print                  |  ✅  |  ✅   |

### Creating Roles & Assigning Users (manual, for local dev)
If you do **not** seed roles, you can add them manually (example using SQL Server):
```sql
-- Create roles (Ids are strings; use NEWID() for ConcurrencyStamp)
INSERT INTO AspNetRoles (Id, [Name], NormalizedName, ConcurrencyStamp)
VALUES ('admin', 'Admin', 'ADMIN', CONVERT(nvarchar(450), NEWID()));

INSERT INTO AspNetRoles (Id, [Name], NormalizedName, ConcurrencyStamp)
VALUES ('user', 'User', 'USER', CONVERT(nvarchar(450), NEWID()));

-- Assign Admin to a user: replace @UserId with the target AspNetUsers.Id
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, 'admin');
```
> Depending on your Identity configuration, `Id` can be any unique string (GUIDs are common). Ensure you match your actual schema.

---

## 🧾 Cheque Entity
**Model:** `Cheque`

| Property       | Type             | Notes                                        |
|----------------|------------------|----------------------------------------------|
| `Id`           | `int`            | PK                                           |
| `Number`       | `string` (≤30)   | **Unique** (index)                           |
| `PayeeName`    | `string` (≤120)  |                                              |
| `Amount`       | `decimal(18,2)`  | ≥ 0.01                                       |
| `Currency`     | `string` (3)     | e.g. `JOD`, `USD`                            |
| `IssueDate`    | `DateTime`       | Date only in UI                              |
| `DueDate`      | `DateTime`       | Date only in UI                              |
| `Status`       | `ChequeStatus`   | Draft/Issued/Cleared/Bounced/Voided          |
| `Notes`        | `string` (≤500)  | optional                                     |
| `CreatedAtUtc` | `DateTime`       | server-stamped; DB default `GETUTCDATE()`    |

---

## 🔎 Search & Filters
`GET /Cheques` supports:
- `q` – matches **Number** or **PayeeName**
- `status` – numeric enum value (`ChequeStatus`)
- `from` – minimum **IssueDate** (yyyy-MM-dd)
- `to` – maximum **DueDate** (yyyy-MM-dd)

**Examples**
```
/Cheques?q=1002
/Cheques?status=1
/Cheques?from=2025-01-01&to=2025-12-31
```

---

## 🖨 Print
- Route: `GET /Cheques/Print/{id}`
- View: `Views/Cheques/Print.cshtml`
- Styles: `wwwroot/css/print-cheque.css`
- Defaults: **A5 landscape**, compact margins, prevents multi-page splits.

> To switch to A4: edit `@page { size: A4; }` and adjust margins in `print-cheque.css`.

---

## 🎨 Theming (Flatly)
- Local assets via **LibMan**:
  - `wwwroot/lib/bootswatch/flatly/bootstrap.min.css`
  - `wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js`
- Linked in `_Layout.cshtml` (theme first, then `site.css`).

---

## 🛠 Troubleshooting
- **SqlException: Login failed / DB not found**
  - Verify the connection string and run `Update-Database`.
- **404 after Edit → Save**
  - Ensure the Edit form includes `asp-route-id="@Model.Id"` and the POST action signature is `(int id, Cheque cheque)`.
- **Enum status not selectable**
  - Use `<select asp-for="Status" asp-items="Html.GetEnumSelectList<ChequeStatus>()">` in Create/Edit.
- **`CreatedAtUtc` not set**
  - Server-side set on Create; optionally configure DB default with `HasDefaultValueSql("GETUTCDATE()")`.
- **`Request` not found in view**
  - Use `ViewContext.HttpContext.Request` to read query params in Razor views.
- **CS8600 warnings (nullable)**
  - Treat query values as nullable and coalesce (e.g., `?.ToString() ?? string.Empty`).

---

## 🔭 Roadmap (ideas)
- Pagination & sorting on cheques list
- Export (CSV/PDF)
- Attachments (scanned cheques)
- Audit log / history
- Basic dashboard widgets

---

## 🤝 Contributing
PRs welcome. Please open an issue first to discuss major changes.

---

## 📄 License
Choose a license (e.g., **MIT**) and add it as `LICENSE` in the repo root.

