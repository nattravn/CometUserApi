# CometUserAPI

A comprehensive user management API built with ASP.NET Core 7/8, featuring authentication, authorization, user roles, and customer management.

## Features

- **User Management**: Complete user CRUD operations with registration and credential management
- **Authentication & Authorization**: JWT-based authentication with refresh token support
- **Role-Based Access Control**: User roles and permissions management
- **Customer Management**: Manage customer information and profiles
- **Product Management**: Product and product image management
- **Email Service**: Email notifications and OTP management
- **Password Management**: Secure password handling with OTP verification
- **Database Migration**: Entity Framework Core migrations for data persistence

## Technology Stack

- **Framework**: ASP.NET Core 7.0 / 8.0
- **Database**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Email**: SMTP-based email service
- **ORM**: Entity Framework Core with Code-First migrations

## Project Structure

### Controllers

- `AuthorizeController` - Authentication and authorization endpoints
- `UserController` - User management operations
- `UserRoleController` - User role and permissions management
- `CustomerController` - Customer management operations
- `ProductController` - Product management endpoints

### Services

- `UserService` - User business logic
- `CustomerService` - Customer operations
- `EmailService` - Email communication
- `UserRoleService` - Role and permission handling
- `RefreshHandler` - Token refresh logic

### Models

- `UserModel` - User data model
- `UserRegistration` - Registration details
- `CustomerModel` - Customer information
- `JwtSettings` - JWT configuration
- `EmailSettings` - Email service configuration

### Database Entities

- `TblUser` - User table
- `TblCustomer` - Customer table
- `TblProduct` - Product information
- `TblRole` - User roles
- `TblRolepermission` - Role permissions
- `TblOtpManager` - OTP management
- `TblRefreshtoken` - Refresh token storage

## Configuration

### appsettings.json

Configure the following settings:

- JWT settings (secret key, expiration)
- Email/SMTP settings
- Database connection strings
- API response settings

### Entity Framework

- Multiple database contexts for separation of concerns
- Code-First migrations in the `Migrations` folder
- Support for both SQL Server and other EF Core providers

## Getting Started

### Prerequisites

- .NET 7.0 or 8.0 SDK
- SQL Server or compatible database

### Build

```bash
dotnet build
```

### Run Migrations

```bash
dotnet ef database update
```

### Run

```bash
dotnet run
```

## API Endpoints

### Authentication

- `POST /api/authorize/login` - User login
- `POST /api/authorize/refresh` - Refresh access token
- `POST /api/authorize/register` - User registration

### Users

- `GET /api/user` - Get all users
- `GET /api/user/{id}` - Get user by ID
- `POST /api/user` - Create new user
- `PUT /api/user/{id}` - Update user
- `DELETE /api/user/{id}` - Delete user

### Customers

- `GET /api/customer` - Get all customers
- `POST /api/customer` - Create customer
- `PUT /api/customer/{id}` - Update customer

### Roles

- `GET /api/userrole` - Get user roles
- `POST /api/userrole` - Create role
- `PUT /api/userrole/{id}` - Update role

### Products

- `GET /api/product` - Get all products
- `POST /api/product` - Create product

## Security

- JWT bearer token authentication
- Basic authentication handler support
- Password encryption with OTP verification
- Role-based authorization on endpoints

## Logging

Application logs are stored in the `Logs/` directory:

- `Log.txt` - Main application log file

## Additional Resources

- Upload directory: `wwwroot/upload/` for product images and files
- Export directory: `wwwroot/export/` for exported data

## License

[Add your license information here]

## Contact

[Add contact information here]
