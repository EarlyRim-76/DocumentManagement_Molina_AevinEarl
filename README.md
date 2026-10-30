# Document Management System

## 1. Application Overview

The Document Management System is an ASP.NET Core Web API application designed to provide a centralized solution for managing digital documents. It allows users to upload, retrieve, download, and delete documents through RESTful API endpoints.

The application manages document metadata separately from the actual physical files. Document information, such as the original filename, stored filename, content type, file size, and upload timestamp, is maintained in a database, while the actual files are stored in a designated file storage directory.

This approach provides an organized way to manage document records while maintaining separation between database operations and physical file storage.

## 2. Core Features

The application provides the following functionalities:

- **Document Upload:** Accepts document uploads through multipart/form-data requests. Validates uploaded files, including file size and supported file extensions.
- **Document Retrieval:** Retrieves document information using its unique document ID.
- **Document Listing:** Retrieves all available document records from the database.
- **Document Download:** Allows users to download previously uploaded documents while preserving their original filenames and content types.
- **Document Deletion:** Removes document records and their associated physical files from storage.

## 3. Technology Stack

| Technology | Purpose |
|---|---|
| C# | Primary programming language |
| ASP.NET Core Web API | Backend API framework |
| Entity Framework Core | Database access and ORM |
| SQL Server | Document metadata storage |
| Local File System | Physical document storage |
| xUnit | Automated unit testing |
| Moq | Mocking dependencies during testing |

## 4. Application Architecture

The application follows a layered architecture to separate responsibilities and improve maintainability.

### Controller Layer
Handles incoming HTTP requests, validates request parameters, invokes the appropriate service methods, and returns HTTP responses.

### Service Layer
Contains the application's business logic, including document creation, retrieval, downloading, and deletion operations.

### Data Access Layer
Uses Entity Framework Core to communicate with the database and manage document metadata.

### File Storage Layer
Handles physical file operations, including saving, retrieving, and deleting uploaded documents. It also performs storage path validation to help prevent unauthorized file access.

## 5. Document Information

Each uploaded document maintains the following metadata:

| Property | Description |
|---|---|
| DocumentId | Unique identifier for the document |
| OriginalFileName | Original name of the uploaded file |
| StoredFileName | Unique filename used in physical storage |
| ContentType | MIME type of the document |
| FileSize | Size of the uploaded document in bytes |
| UploadedAt | Date and time the document was uploaded |

## 6. API Endpoints

The application exposes RESTful endpoints for document management.

| HTTP Method | Endpoint | Description |
|---|---|---|
| POST | `/api/document` | Upload a document |
| GET | `/api/document` | Retrieve all documents |
| GET | `/api/document/{documentId}` | Retrieve a specific document |
| GET | `/api/document/{documentId}/download` | Download a document |
| DELETE | `/api/document/{documentId}` | Delete a document |

*Endpoint routes should be adjusted if your actual controller uses a different route prefix.*

## 7. Validation and Error Handling

The application implements validation and error handling to ensure reliable document management.

Examples include:

- Rejecting empty or invalid file uploads.
- Enforcing the configured maximum file size.
- Rejecting unsupported file extensions.
- Returning appropriate HTTP status codes when documents cannot be found.
- Validating storage paths to prevent path traversal.
- Handling unexpected database and file storage exceptions.

## 8. Automated Testing

The application includes automated tests using xUnit and Moq to verify core API functionality.

The test suite covers:

- Successful document uploads.
- Successful document retrieval.
- Retrieving nonexistent documents.
- Uploads exceeding the configured file size limit.
- Document deletion.
- Successful document downloads.
- Handling missing physical files.
- Retrieving all documents.
- Handling unexpected exceptions.

Unit tests isolate controller behavior by mocking service dependencies, while service and storage tests can independently verify business logic and physical file operations.

## 9. Project Objective

The primary objective of the Document Management System is to demonstrate the development of a structured, maintainable, and reliable document management API using ASP.NET Core.

## 10. AI Assited
- AI helps to create automation testing using XUnit
- AI assist the devs to generate test automations
- AI helps a developer to debug Issue
- AI helps as the new stack overflow a guide specially asking questions related to some code.

# 11. Instructions for Running the Application

## Prerequisites

Before running the application, make sure you have installed:

- Git
- .NET SDK version specified in `global.json`
- SQL Server or SQL Server LocalDB
- Visual Studio or Visual Studio Code (optional)

# Installation and Setup

### 1. Clone the repository

```bash
git clone https://github.com/EarlyRim-76/DocumentManagement_Molina_AevinEarl.git
```

### 2. Navigate to the project directory

```bash
cd DocumentManagement_Molina_AevinEarl
```

### 3. Configure your local database

Open `appsettings.json` and update the database connection string.

Replace the SQL Server instance name with your local SQL Server or LocalDB instance and specify your preferred database name.

Make sure your SQL Server instance is running before proceeding.

### 4. Restore project dependencies

```bash
dotnet restore
```

This automatically downloads the required NuGet packages defined in the project files.

### 5. Apply database migrations

If EF Core migrations are included in the repository, execute:

```bash
dotnet ef database update
```

### 6. Build the application

```bash
dotnet build
```

### 7. Run automated tests

```bash
dotnet test
```

### 8. Run the API

```bash
dotnet run --project DocumentManagement_Molina_AevinEarl
```

The application should now be running using the configured ASP.NET Core launch settings.

## Notes

- Ensure that your database connection string is configured correctly.
- Do not commit personal database credentials or sensitive connection strings to the repository.
- NuGet dependencies are restored automatically through `dotnet restore`.
- If the Entity Framework Core CLI is not installed, install it using:

```bash
dotnet tool install --global dotnet-ef
```


The application emphasizes separation of concerns, secure file handling, proper validation, consistent HTTP responses, and automated testing.

