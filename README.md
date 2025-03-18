# Appointment & Booking API

## Overview
This API provides endpoints to retrieve available time slots for medical appointments and to book them. The development follows a **Test-Driven Development (TDD) approach**, ensuring high test coverage, maintainability, and minimal working code at every layer.

## Project Structure

This project follows Microsoft's official guidelines for structuring .NET applications. The code is organized into distinct layers following **Clean Architecture**, ensuring scalability, maintainability, and separation of concerns.

### Layers:

- **API Layer (`Docplanner.API`)**: Exposes the HTTP endpoints using **Minimal APIs** and handles request routing and authentication.
- **Application Layer (`Docplanner.Application`)**: Contains the business logic and orchestrates use cases using **MediatR** for **CQRS-style** request handling.
- **Infrastructure Layer (`Docplanner.Infrastructure`)**: Handles external dependencies such as the **availability service client**, **authentication**, and **exception handling**.
- **Domain Layer (`Docplanner.Domain`)**: Defines the **core business models** and **DTOs**, ensuring a clear contract between different layers.

This structure allows the application to scale efficiently, making it easier to introduce changes while maintaining **loose coupling** between components.

## Security

The external availability service we consume uses **Basic Authentication**. To ensure secure and seamless authentication, we implemented the following approach:

### Authentication Flow

1. **Swagger Integration**: 
   - Swagger UI is configured to require **Basic Auth credentials**.
   - Users can enter their username and password in the **Authorize** section.
   - The credentials are sent with every API request.

2. **Forwarding Credentials**: 
   - The `Authorization` header from the incoming request is **retrieved from the HTTP context**.
   - The same credentials are then **forwarded** to the external API.

3. **Client Authentication Handling**:
   - The `AvailabilityServiceClient` reads the credentials from the request context.
   - If credentials are missing, the request is rejected with a **401 Unauthorized** response.
   - Otherwise, the credentials are used to authenticate against the external API.
4. Click **Authorize** and then close the modal.
5. You can now make authenticated requests.

> **Note:** Credentials must be included in every request for the API to work properly.

### Retrieving Credentials in the Client

In our **`AvailabilityServiceClient`**, we retrieve the authentication header from the HTTP context and forward it to the external API:

```csharp
var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

if (string.IsNullOrEmpty(authHeader))
{
 throw new AppException("Authorization header is missing or invalid.", 401);
}

var response = await url
 .WithHeader("Authorization", authHeader)
 .GetStringAsync();
 ```

## RESTful API Design

The API follows **RESTful principles**, ensuring clear separation of concerns and intuitive resource modeling.

### Resource Naming & Endpoints

We initially considered modeling **"/api/slots"** for both availability retrieval and booking.  
However, after reviewing REST best practices, we opted for:

| Method | Endpoint              | Purpose |
|--------|-----------------------|---------|
| **GET**  | `/api/availability/{date}`  | Retrieve available slots for a given **week** (starting from Monday). |
| **POST** | `/api/bookings` | Book a specific time slot. |

#### **Why This Design?**
1. **Clear separation of concerns**  
   - `/api/availability/` → Describes **available resources** (querying slots).  
   - `/api/bookings/` → Represents **actions taken by a user** (making reservations).
   
2. **More RESTful than `POST /api/slots/take`**  
   - Instead of modeling an action (`take`), we represent the logical resource (`bookings`).
   - This aligns with how REST models entities rather than actions.

3. **Future Extensibility**  
   - If additional booking-related actions (e.g., **cancel a booking**) are needed, `/api/bookings/{id}` can be extended without breaking existing behavior.

### HTTP Status Codes & Responses
The API adheres to **HTTP standards** for response handling:
- **`200 OK`** → Successful retrieval of available slots.
- **`200 Created`** → Successfully booked a slot. Not using 201 because we don't have an SlotId to return
- **`400 Bad Request`** → Invalid input (e.g., wrong date format).
- **`404 Not Found`** → No available slots found for the given week.
- **`500 Internal Server Error`** → Unexpected errors.

This RESTful approach ensures **scalability, clarity, and consistency**, allowing seamless future enhancements without breaking existing consumers.


## Development Approach

The development of this API followed a **Test-Driven Development (TDD)** approach, ensuring that functionality was built incrementally with automated tests at each layer.

### Step-by-Step Approach

#### 1️ Client Layer First
- We started with the **AvailabilityServiceClient**, writing unit tests using **Flurl.HttpTest** to mock API responses.
- During testing, we discovered that the **FacilityId** field was required for successful slot booking, which was **not** initially documented in the external API contract.
- We adapted the implementation to ensure FacilityId was always included.

#### 2️ Application Service & Business Logic
- We built **SlotService**, responsible for orchestrating business rules like filtering available time slots.
- Several scenarios were covered with unit tests:
  - **Lunch breaks filtering.**
  - **Busy slots overlapping working hours.**
  - **Invalid slot durations (negative or zero).**
  - **Busy slots outside working hours.**

#### 3️ MediatR & Minimal APIs
- We used **MediatR** to decouple application logic from the API endpoints.
- The API was implemented using **Minimal APIs**, keeping controllers unnecessary.
- This approach makes the service **lightweight and highly maintainable**.

#### 4️ Authentication & Security
- The external service required **Basic Authentication**, so we:
  - Configured **Swagger UI** to accept credentials.
  - Passed the **Authorization header** through requests using **IHttpContextAccessor**.

#### 5️ Exception Handling & Logging
- We introduced **AppException** to standardize error handling with HTTP status codes.
- Logging was added using **.NET's built-in ILogger**, outputting structured logs to **console**.
- This approach prepares the API for **containerized deployments in AWS/Azure**.

---

### Key Design Decisions

- **RawDays Dictionary for JSON Parsing**  
We initially parsed raw JSON days dynamically into **DayOfWeek**, ensuring flexibility in processing different API response formats.  
This design choice was necessary as the API could return different day names dynamically.

- **CQRS Potential**  
By splitting **GET (availability)** and **POST (booking)** into different service methods in **SlotService**, we laid the foundation for a **future CQRS architecture**.  
This separation would allow scaling **availability queries** independently from **booking operations**, facilitating potential microservices migration.

- **Lightweight API Design**      
    - **Minimal APIs** instead of controllers for a simpler, more efficient REST interface.
    - **MediatR** for clean separation of concerns between commands and handlers.
    - **TDD approach** to ensure minimal working code with full test coverage.

This structured approach ensured that **every layer was developed incrementally**, thoroughly tested, and prepared for **future scalability and deployment**.

## How to Run the Project

To run the API locally, follow these steps:

1. **Clone the Repository**  
   ```sh
   git clone https://github.com/your-repo/docplanner-api.git
   cd docplanner-api
   ```

2. **Ensure .NET 8+ is Installed**  
   You can check your installed version with:
   ```sh
   dotnet --version
   ```

3. **Run the API**  
   ```sh
   dotnet run --project src/Docplanner.API
   ```

4. **Access Swagger UI**  
   Open `https://localhost:5001/swagger` in your browser.

---

## Swagger & Testing the API

This API uses **Swagger** for interactive API documentation and testing.

### **How to Authenticate in Swagger**
1. Open `https://localhost:5001/swagger`.
2. Click **Authorize** (top-right button).
3. Enter **Basic Auth credentials**:
   - **Username:** `techuser`
   - **Password:** `secretpassWord`
4. Click **Authorize** and close the modal.
5. You can now test endpoints directly from Swagger.

### **Example Requests in Swagger**
- **Get Available Slots:**  
  Call `/api/availability/{date}` using `yyyyMMdd` format.  
  Example: `/api/availability/20240318`.
- **Book a Slot:**  
  Call `/api/bookings` with a **valid JSON payload**.

## Missing Tests & Future Improvements

Due to time constraints, the following **tests were not implemented**:

### **1. Integration Tests**
- We did not add **TestContainers** for end-to-end API tests.
- Future improvement: Use **Docker containers** for **real database & API mocking**.

### **2. BDD Tests (Behavior-Driven Development)**
- No **SpecFlow tests** were added.
- Would help validate the entire booking flow from a **user perspective**.

### **3. Edge Cases for Security**
- No additional security tests beyond **Basic Auth handling**.

## Logging & Exception Handling

### **Logging Strategy**
- Used **ILogger** to output structured logs.
- Logs are written to the **console**, making them compatible with **Azure & AWS cloud environments**.

### **Centralized Exception Handling**
- Introduced `AppException` for **standardized error responses**.
- Errors are **mapped to HTTP status codes** automatically.
- Alternative: A **middleware** could be used for **global exception handling**.



