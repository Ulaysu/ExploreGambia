# ExploreGambia API

ExploreGambia is a comprehensive API for managing tours, bookings, payments, and attractions in Gambia. This project is built using ASP.NET Core and Entity Framework Core, targeting .NET 8.0.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [API Endpoints](#api-endpoints)
- [Data Models](#data-models)
- [Dependencies](#dependencies)
- [Seeding Data](#seeding-data)
- [Contributing](#contributing)
- [License](#license)

## Overview

ExploreGambia API provides a robust backend for managing tours, bookings, payments, and attractions. It includes endpoints for CRUD operations and integrates with a SQL Server database using Entity Framework Core.

## Features

- Manage Tours
- Manage Bookings
- Manage Payments
- Manage Attractions
- Manage Tour Guides
- Data Seeding for initial setup
- AutoMapper for DTO mapping
- Swagger for API documentation

## Installation

### Prerequisites

- .NET 8.0 SDK
- SQL Server
- A tool to manage SQL Server (SQL Server Management Studio or Azure Data Studio)

### Steps

1. **Clone the Repository**

   ```bash
   git clone <repository-url>
   cd ExploreGambia
   ```

2. **Set Up Database Connection String**

   - Open `appsettings.json`
   - Update the connection string under `"ConnectionStrings"`
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=ExploreGambiaDB;User Id=your_user;Password=your_password;TrustServerCertificate=True;"
     }
     ```

3. **Restore Dependencies**

   ```bash
   dotnet restore
   ```

4. **Apply Migrations & Seed the Database**

   ```bash
   dotnet ef database update
   ```

5. **Run the Application**

   ```bash
   dotnet run
   ```

6. **Access the API**

   - Base URL: `https://localhost:7297`
   - Swagger Documentation: [`https://localhost:44331/swagger`](https://localhost:44331/swagger)

## Usage

### Running the Application

To run the application, use the following command:

```bash
dotnet run
```

The API will be available at `https://localhost:7297`.

### Swagger

Swagger is enabled for API documentation. You can access it at: [`https://localhost:44331/swagger`](https://localhost:44331/swagger)

## API Endpoints

### Tours

- `GET /api/tours` - Get all tours
- `GET /api/tours/{id}` - Get a tour by ID
- `POST /api/tours` - Create a new tour
- `PUT /api/tours/{id}` - Update a tour
- `DELETE /api/tours/{id}` - Delete a tour

### Bookings

- `GET /api/bookings` - Get all bookings
- `GET /api/bookings/{id}` - Get a booking by ID
- `POST /api/bookings` - Create a new booking
- `PUT /api/bookings/{id}` - Update a booking
- `DELETE /api/bookings/{id}` - Delete a booking

### Payments

- `GET /api/payments` - Get all payments
- `GET /api/payments/{id}` - Get a payment by ID
- `POST /api/payments` - Create a new payment
- `PUT /api/payments/{id}` - Update a payment
- `DELETE /api/payments/{id}` - Delete a payment

### Tour Guides

- `GET /api/tourguides` - Get all tour guides
- `GET /api/tourguides/{id}` - Get a tour guide by ID
- `POST /api/tourguides` - Create a new tour guide
- `PUT /api/tourguides/{id}` - Update a tour guide
- `DELETE /api/tourguides/{id}` - Delete a tour guide

## Data Models

### Tour

Represents a tour package with details such as title, description, price, and duration

```csharp
public class Tour {
public Guid TourId { get; set; }
public string Title { get; set; }
public string Description { get; set; }  
public string Location { get; set; } 

[Precision(18, 2)]
public decimal Price { get; set; }  
public int MaxParticipants { get; set; } 
public DateTime StartDate { get; set; } 
public DateTime EndDate { get; set; } 
public string ImageUrl { get; set; } = string.Empty;
public bool IsAvailable { get; set; } = true; 

// Foreign Key Relationship
public Guid TourGuideId { get; set; } 
}
```

### Booking

Represents a customer's tour booking with reference to the booked tour .

```csharp
public class Booking {
     public Guid BookingId { get; set; } 
     public Guid TourId { get; set; } 
     public DateTime BookingDate { get; set; } = DateTime.UtcNow;
     public int NumberOfPeople { get; set; }
  
     [Precision(18, 2)]
     public decimal TotalAmount { get; set; }
     public BookingStatus Status { get; set; } = BookingStatus.Pending; 

       
}
```

### Payment

Tracks payments made for bookings, including the amount and status.

```csharp
public class Payment {
        public Guid PaymentId { get; set; }         
        public Guid BookingId { get; set; }         
        public string PaymentMethod { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }       
        public DateTime PaymentDate { get; set; } 
        public bool IsSuccessful { get; set; }     
        public Booking Booking { get; set; }       

}
```

### Attraction

Represents a tourist attraction with location and description.

```csharp
public class Attraction {
   public Guid AttractionId { get; set; } 
   public string Name { get; set; } = string.Empty;
   public string Description { get; set; } = string.Empty;
   public string Location { get; set; } = string.Empty;
   public string ImageUrl { get; set; } = string.Empty; 

   // Navigation Property for Many-to-Many Relationship
   public List<TourAttraction> TourAttractions { get; set; } = new List<TourAttraction>();
     
}
```

### Tour Guide

Represents a tour guide with personal details and expertise.

```csharp
public class TourGuide {
    public Guid TourGuideId { get; set; } // Primary Key
  
    public string FullName { get; set; } = string.Empty;
    
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Bio { get; set; } = string.Empty; // Short description of guide
    
    public bool IsAvailable { get; set; } = true; // Can they accept tours?
    
    // Navigation property
    public List<Tour> Tours { get; set; } = new List<Tour>();

}
```

## Dependencies

- .NET 8.0
- Entity Framework Core
- AutoMapper
- Swagger

## Seeding Data

To seed initial data into the database, the `DataSeeder` class is used. This class is called during the application startup to ensure the database is populated with initial data.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License.

