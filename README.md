# Amazon Clone - ITI Graduation Project

![Project Banner](https://i.postimg.cc/YSJmdHnK/amazon-logo.png)

## Table of Contents
- [Project Overview](#project-overview)
- [Tech Stack](#tech-stack)
- [Features](#features)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Contributors](#contributors)

## Project Overview

This **Amazon Clone** project is our graduation project at ITI, built to emulate an e-commerce platform with features like product listings, user authentication, a dynamic shopping cart, secure payments, and admin management. The application uses **ASP.NET Core** and **Angular** to ensure high performance, security, and a smooth user experience.

## Tech Stack

### Backend
- **ASP.NET Core 8 Web API**: Core application services
- **Entity Framework Core (EF Core)**: Object-Relational Mapping (ORM) for data access
- **N-Tier Architecture**: Structured layering for codebase organization
- **Generic Repository Pattern** and **Specification Design Pattern**: Efficient data handling and querying
- **Redis**: In-memory caching for faster data access
- **Microsoft Identity** and **JWT Authorization**: User management and secure authentication

### Admin Dashboard
- **ASP.NET MVC**: For a robust, easy-to-use admin interface

### Frontend
- **Angular 18**: For a dynamic, responsive user interface

### Payment
- **Stripe**: For secure payment integration
- **Webhook**: For handling payment events

## Features
- **User Authentication**: Register, login, and profile management
- **Product Catalog**: Browse products with filtering and search
- **Shopping Cart**: Add to cart, update quantities, and remove items
- **Order Management**: Place orders, view order history, and track order status
- **Admin Dashboard**: Manage products, users, and orders
- **Payment Processing**: Secure payments through Stripe

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (version 8.0 or later)
- [Node.js](https://nodejs.org/) (version 14 or later)
- [Angular CLI](https://angular.io/cli)
- [Redis](https://redis.io/download)
- [Stripe Account](https://stripe.com/) for payment integration

### Installation
1. **Clone the Repository**:
   ```bash
   
   git clone https://github.com/AbdEl-RahmanSaleh/Amazon.git
   cd Amazon
   
2. **Backend Setup**:
  - Navigate to the backend project folder.
  - Update your appsettings.json file with your database, Redis, and Stripe keys.
  - Run migrations to set up the database:
     ```bash
     
     dotnet ef database update
     
  - Start the API:
     ```bash
     
     dotnet run

3. **Frontend Setup**:
  - Navigate to the Angular app folder.
  - Install dependencies:
     ```bash
     
     npm install

  - Run the Angular development server:
     ```bash
     
     ng serve

4. **Admin Dashboard**:
  - Navigate to the admin project folder.
  - Update configurations as needed in the appsettings.json.
  - Run the ASP.NET MVC admin project:
    ````bash

    dotnet run

## Configuration
### Ensure to update the following keys in your appsettings.json files:
  - ConnectionStrings for your database
  - Redis Configuration
  - Stripe API Keys (found in your Stripe dashboard)

## Contributors
  - AbdElRahman Saleh [LinkedIn](https://www.linkedin.com/in/abdel-rahman-mohammed-saleh/)
  - Abdeen ElRefaey [LinkedIn](https://www.linkedin.com/in/abdeen-elrefaey-79779523a/)
  - Ali Awad [LinkedIn](https://www.linkedin.com/in/a-awad/)
  - Mohamed Abdel-Moneim [LinkedIn](https://www.linkedin.com/in/mohamed-mahmoud-abdel-moneim-024488231/)
  - Yousef Mohamed [LinkedIn](https://www.linkedin.com/in/yousef-mohamed-15509a18b/)
    
