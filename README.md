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

---

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
 ---

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

---

## Development Approach

The development of this API followed a **Test-Driven Development (TDD)** approach, ensuring that functionality was built incrementally with automated tests at each layer.

### Step-by-Step Approach

#### 1️ Client Layer
- We started with the **AvailabilityServiceClient**, writing unit tests using **Flurl.HttpTest** to mock API responses.
- The goal was to deserialize the response into an object that could be efficiently used for calculations in the application service.
- During manual testing, we discovered that the **FacilityId** field was required for successful slot booking, even though this was **not** initially documented in the external API contract.
- We adapted the implementation to ensure that **FacilityId** was included both in the **Get Availability** request and in the **Take Slot** logic.

#### 2️ Application Service & Business Logic
- We built **SlotService**, responsible for orchestrating business rules such as filtering available time slots and booking slots.  
  - This service could be refactored in the future if new features are introduced.  
  - A suggested improvement would be to split it into **AvailabilityService** and **BookingService** for better separation of concerns.  
- The goal was to generate a list of slots based on the **start and end working hours** and then filter this initial list to **remove lunch breaks and busy slots**.  
- Several scenarios were covered with unit tests:
  - **Combinations of lunch breaks and busy slot filtering.**  
  - **Busy slots overlapping working hours.**  
  - **Invalid slot durations (negative or zero).**  
  - **Busy slots outside working hours.**  


#### 3️ MediatR & Minimal APIs
- We used **MediatR** to decouple application logic from the API endpoints.
- The API was implemented using **Minimal APIs**.
- This approach makes the service **lightweight and highly maintainable**.

#### 4️ Authentication & Security
- The external service requires **Basic Authentication**, so we:
  - Configured **Swagger UI** to accept credentials.
  - Forwarded the **Authorization header** using the `BasicAuthenticationHandler` implementation.

#### 5️ Exception Handling & Logging
- We introduced **AppException** to standardize error handling with HTTP status codes.  
  - Creating multiple custom exception types was considered unnecessary for this exercise, as it would introduce unnecessary complexity.  
- Logging was implemented using **.NET's built-in ILogger**, providing structured logs directly to the **console**.  
- This approach ensures that logs are visible in the console and makes the API **ready for containerized deployments** in AWS/Azure.  


---

### Design Decisions and Challenges

- **RawDays Dictionary for JSON Parsing**  
The external service only returns the days that have available slots, which posed a challenge for structured deserialization. This was resolved by introducing the RawDays property, which acts as an intermediate mapping layer to convert the raw JSON response into a structured dictionary.

- **CQRS Potential**  
By splitting GET (availability) and POST (booking) into separate service methods in SlotService, we laid the foundation for a future CQRS architecture.
This separation would allow us to scale read (availability queries) and write (booking operations) independently, optimizing performance and scalability.

- **Lightweight API Design**      
    - **Minimal APIs** instead of controllers for a simpler, more efficient REST interface.
    - **MediatR** for clean separation of concerns between commands and handlers.

- **TDD approach** to ensure minimal working code with full test coverage.
   - This structured approach ensured that **every layer was developed incrementally**, thoroughly tested, and prepared for **future scalability and deployment**.

---

## How to Run the Project

1. **Clone the Repository**  
   ```sh
   git clone <repo-url>
   cd docplanner-api
   ```

2. **Ensure .NET 9+ is Installed**  
   You can check your installed version with:
   ```sh
   dotnet --version
   ```

3. **Run the API**  
 
 To run the API locally you can just open the solution with Visual Studio or Rider and run it or you can, follow these steps:

   ```sh
   dotnet run --project src/Docplanner.API --launch-profile http
   ```

4. **Access Swagger UI**  
   Open `http://localhost:5093/swagger/index.html` in your browser.

---

## Swagger & Testing the API

This API uses **Swagger** for interactive API documentation and testing.

### **How to Authenticate in Swagger**
1. Open `http://localhost:5093/swagger/index.html`.
2. Click **Authorize** (top-right button).
3. Enter **Basic Auth credentials**:
   - **Username:** `<username>`
   - **Password:** `<password>`
4. Click **Authorize** and close the modal.
5. You can now test endpoints directly from Swagger.

### **Example Requests in Swagger**
- **Get Available Slots:**  
  Call `/api/availability/{date}` using `yyyyMMdd` format.  
  Example: `/api/availability/20240318`.
- **Book a Slot:**  
  Call `/api/bookings` with a **valid JSON payload**.

---

## Missing Tests & Future Improvements

Due to time constraints, the following **tests and features were not implemented**:

### **1. Integration Tests**
Integration tests could be implemented using ASP.NET Core's WebApplicationFactory to ensure API consistency and correct dependency resolution in a controlled test environment.
1. Testing API Endpoints and Business Logic Together
   - Unlike unit tests that isolate single methods, integration tests call the real API endpoints (e.g., /api/availability/{date} or /api/bookings) to ensure the entire flow works correctly.
2. Validating Request & Response Formats
   - Ensures that incoming requests match the expected payload structure and data types.
   - Confirms that the API returns properly formatted JSON responses with expected status codes.
   - Prevents regressions when modifying the DTOs or service contracts.
3. Verifying Security and Authentication
   - Ensures that the API correctly enforces Basic Authentication.
   - Tests that endpoints return 401 Unauthorized when requests lack valid credentials.

### **2. BDD Tests (Behavior-Driven Development)**
To configure BDD tests, we would use TestContainers and Specflow. 
- The goal is to have an end-to-end (E2E) test with the application running alongside all its dependencies, but with stubbed responses for external services.
- BDD tests provide a business-readable format to describe expected API behaviors, ensuring alignment between technical implementation and business requirements.

### **3. Middleware for exceptions handling**
- Implementing an exception handling middleware would further centralize error responses, ensuring consistency across all API layers and reducing the need for repetitive error-handling logic.
