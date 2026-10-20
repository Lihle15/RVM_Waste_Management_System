# ♻️ RVM Waste Management System (RVMWMS)

**A technology-driven recycling and waste management solution**

RVM Waste Management System (RVMWMS) is a web-based application developed as a National Diploma final-year ICT project at Durban University of Technology (DUT). The system encourages responsible recycling by connecting users with Reverse Vending Machines (RVMs), rewarding recycling activity, and supporting more efficient waste collection and management.

The project combines waste management, user engagement, community reporting and education, administrative monitoring, and e-commerce in one application.

## 📌 Table of Contents

- [Overview](#-overview)
- [Problem Statement](#-problem-statement)
- [Project Objectives](#-project-objectives)
- [Key Features](#-key-features)
- [Technology Stack](#-technology-stack)
- [User Roles](#-user-roles)
- [System Architecture](#-system-architecture)
- [Database](#-database)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [Environmental Impact](#-environmental-impact)
- [Future Improvements](#-future-improvements)
- [Contributors](#-contributors)

## 🌍 Overview

RVMWMS makes it easier for users to locate recycling machines, take part in recycling activities, report illegal dumping, learn about recycling, and engage with a rewards-based recycling ecosystem.

Administrators can monitor machine capacity, manage products, orders and feedback, and support timely waste collection.

The system was built with local usability in mind, including isiZulu language support on selected interface pages to improve accessibility for users who are more comfortable in isiZulu than English.

## ❗ Problem Statement

Poor waste disposal, limited recycling participation, and inefficient waste collection can contribute to environmental pollution and service delivery challenges. Traditional waste management processes also make it difficult to monitor collection needs and encourage consistent recycling behaviour.

RVMWMS addresses these challenges through a digital platform that supports recycling participation, machine location discovery, collection monitoring, community reporting, and recycling incentives.

## 🎯 Project Objectives

- Encourage responsible waste disposal and recycling.
- Help registered users locate available Reverse Vending Machines.
- Support monitoring of machine capacity and collection requirements.
- Provide administrators with dashboards for managing waste management activities.
- Encourage recycling through a rewards system.
- Let community members report illegal dumping and access recycling education.
- Provide an e-commerce component for relevant products and recycling materials.
- Improve accessibility through selected isiZulu interface translations.

## ✨ Key Features

### 1. 📍 Reverse Vending Machine Locator

- Allows registered users to locate registered RVMs.
- Displays machine locations using map pins.
- Helps users identify where they can participate in recycling activities.

### 2. 👤 User Registration, Authentication and Profiles

- User registration and login.
- Role-based access to system features.
- A profile area where users can manage their own details.

### 3. 📊 Administrative Dashboard

- Administrative view of system activities.
- Management of registered machines, products, orders and related records.
- Helps administrators oversee waste management operations.

### 4. 🚛 Machine Capacity and Collection Alerts

- Monitoring of RVM capacity.
- Collection alerts when machines reach configured capacity thresholds.
- Helps operators identify machines that need collection.

### 5. 🎁 Recycling Rewards

- A rewards component that encourages recycling participation.
- Links recycling activity to user incentives.

### 6. 🛒 E-commerce: Products, Cart and Orders

- Product catalogue for RVM-related products and recycling materials.
- Shopping cart and order management.
- Checkout payments through the **PayFast sandbox** (test environment only; no real payments are processed).

### 7. 🗑️ Illegal Dumping Reports

- Users can submit dumping reports so problem areas can be flagged to administrators.

### 8. 📚 Recycling Education

- An education section with recycling information to support awareness and behaviour change.

### 9. 💬 Feedback

- Users can submit feedback about the system and their experience, which administrators can review.

### 10. 🌐 isiZulu Language Support

- Language switching between English and isiZulu on selected pages, including the home page and shared layout.
- Improves the accessibility of the prototype for isiZulu-speaking users.

## 🛠️ Technology Stack

| Technology | Purpose |
|---|---|
| C# | Application logic and backend development |
| ASP.NET Core MVC | Web application framework and MVC architecture |
| MySQL | Relational database management |
| PayFast (sandbox) | Test payment gateway for the cart and checkout flow |
| HTML5 | Web page structure |
| CSS3 | Styling and interface presentation |
| JavaScript | Client-side interactions |
| Visual Studio | Development environment |

**Application type:** Web application  
**Architecture:** Model–View–Controller (MVC)  
**Database:** MySQL

> Note: RVMWMS is a web application with a MySQL database. It should not be described as a cloud-based application.

## 👥 User Roles

The application uses role-based access control with the following roles:

| Role | Purpose |
|---|---|
| **Admin** | Administrative access and system management |
| **Customer** | Customer-facing functionality, including the RVM locator, shopping, rewards, dumping reports and feedback |
| **Operator** | Operational functionality associated with waste collection and machine monitoring |

Access to individual pages and actions depends on the permissions implemented for each role.

## 🏗️ System Architecture

RVMWMS follows the Model–View–Controller pattern provided by ASP.NET Core MVC.

- **Model:** Application data and the associated business logic.
- **View:** The user interface, built with Razor views, HTML, CSS and JavaScript.
- **Controller:** Processes requests, coordinates application logic, and returns the appropriate responses.
- **Database:** MySQL stores the application's persistent data.

### Controllers

| Controller | Responsibility |
|---|---|
| `HomeController` | Landing page and general navigation |
| `AdminController` | Administrative dashboard and management |
| `RVMsController` | RVM records, locations and capacity |
| `ProductsController` | Product catalogue |
| `CartController` | Shopping cart and PayFast sandbox checkout |
| `OrdersController` | Order tracking and management |
| `RewardsController` | Recycling rewards |
| `DumpingReportController` | Illegal dumping reports |
| `EducationController` | Recycling education content |
| `FeedbackController` | User feedback |
| `ProfileController` | User profile management |
| `LanguageController` | English / isiZulu language switching |

### High-level request flow

```text
             User
              |
              v
       ASP.NET Core MVC View
              |
              v
          Controller
              |
              v
       Application Logic
              |
              v
          MySQL Database
              |
              v
      Response to the User
```

This structure separates the user interface, request handling, and data management, making the application easier to maintain and extend.

## 🗄️ Database

The application uses MySQL for persistent data storage.

The database is named:

`RVMWasteDB`

It supports user accounts, role-based access, machine information, products, orders, rewards, dumping reports, feedback, and other records associated with implemented features.

The exact tables, relationships, and constraints should be documented alongside the final database schema.

## 🚀 Getting Started

Follow these steps to set up the project locally. Adjust them to match the repository's current solution and database configuration.

### Prerequisites

- [Visual Studio](https://visualstudio.microsoft.com/) with the ASP.NET and web development workload
- The [.NET SDK](https://dotnet.microsoft.com/download) version targeted by the project
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- [MySQL Workbench](https://dev.mysql.com/downloads/workbench/) (optional)
- Git

### 1. Clone the repository

```bash
git clone https://github.com/Lihle15/RVM_Waste_Management_System.git
cd RVM_Waste_Management_System
```

### 2. Open the project

Open the solution file (`.sln`) in Visual Studio and restore NuGet packages if prompted.

### 3. Configure MySQL

Start your MySQL server and create the database:

```sql
CREATE DATABASE RVMWasteDB;
```

If the project includes a database export or SQL script, import it to create the required tables and relationships.

### 4. Configure the database connection

Update the connection string in `appsettings.json` (or your user secrets) to match your local MySQL server address, port, username and password. Do not commit credentials or other secrets to source control.

### 5. Configure PayFast (sandbox)

The cart uses PayFast's **sandbox** environment. Add your own sandbox merchant details through `appsettings.json` or user secrets. Never commit real merchant credentials, and never use live keys in this project.

### 6. Build and run

In Visual Studio:

1. Set the web application project as the startup project.
2. Build the solution.
3. Run the application using the configured development profile.

Or from the command line:

```bash
dotnet restore
dotnet run
```

The application should open in your default browser when startup succeeds.

**Important:** Verify the project file, connection string format and schema import procedure against the current repository before treating these instructions as final.

## 📁 Project Structure

The application follows the ASP.NET Core MVC structure:

```text
RVMWMApp/
├── Controllers/       # Admin, Cart, Products, RVMs, DumpingReport,
│                      # Education, Feedback, Home, Language, Orders,
│                      # Profile, Rewards
├── Models/
├── Views/
├── wwwroot/           # Static files (CSS, JS, images)
├── appsettings.json
├── Program.cs
└── README.md
```

The exact structure may vary slightly depending on the files included in the repository.

## 🌱 Environmental Impact

RVMWMS explores how software can support environmental sustainability and waste management. Its intended benefits include:

- Encouraging greater participation in recycling.
- Making recycling machine locations easier to discover.
- Supporting more timely collection through capacity monitoring.
- Promoting recycling through incentives and education.
- Giving communities a way to flag illegal dumping.

These are the project's intended benefits; measurable environmental outcomes would require implementation data and real-world evaluation.

## 🔮 Future Improvements

- Live RVM capacity monitoring through connected hardware or sensor integrations.
- Improved analytics for collection planning and recycling trends.
- Expanded multilingual support.
- Enhanced accessibility and mobile responsiveness.
- More detailed reporting for administrators and operators.
- Additional reward tracking and redemption functionality.
- Moving from the PayFast sandbox to live payments, with appropriate security review.
- Integration with external mapping services where appropriate.
- Automated testing, security hardening, and deployment documentation.

These are possible future enhancements, not claims about existing functionality.

## 🎓 Project Context

RVMWMS was developed as part of my diploma final-year ICT project at Durban University of Technology. It demonstrates software development principles applied to a practical environmental and service delivery challenge, bringing together web development, database integration, role-based access, payments, and user-centred design.

---

**RVMWMS — Using technology to support responsible recycling and waste management.** ♻️
