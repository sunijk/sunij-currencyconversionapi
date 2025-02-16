# Currency Converter API

## Overview
The Currency Converter API is a secure & scalable ASP.NET Core that provides currency conversion services.
 It integrates with the Frankfurter currency API to fetch real-time exchange rates and supports functionalities such as:
- Get the latest exchange rates for a specific base currency.
- Converting amounts between different currencies.
- Retrieving historical exchange rates with pagination.
- Secure authentication using JWT.
- Caching mechanisms to optimize API calls and reduce latency.
- Resilience features including retry policies and circuit breakers.
- Comprehensive logging and monitoring using Serilog and OpenTelemetry.

## Tools & Technologies Used

### Backend
- **ASP.NET Core 8** - For building the RESTful API.
- **HttpClientFactory** - For managing HTTP requests to external APIs.
- **Polly** - To implement retry policies and circuit breakers for resilience.
- **Entity Framework Core** (optional) - For future database storage support.
- **Serilog** - For structured logging and monitoring.
- **OpenTelemetry** - For distributed tracing and observability.

### Security
- **JWT Authentication** - Secure access control with role-based authorization.

### Deployment & Scalability
- **Docker** - For containerization and easy deployment.
- **CI/CD Pipelines** - Automates builds, testing, and deployments.

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- Docker (for containerization, optional)
- Seq or ELK stack (for logging, optional)

### Installation Steps
1. Clone the repository:
   ```sh
   git clone https://github.com/your-repo/currency-converter-api.git
   cd currency-converter-api
   ```
2. Restore dependencies:
   ```sh
   dotnet restore
   ```
3. Configure the appsettings.json file:
   - Update `JWT` settings
   - Set `Frankfurter API` URL (if different from default)
   
4. Run the application:
   ```sh
   dotnet run
   ```
5. (Optional) Run in Docker:
   ```sh
   docker build -t currency-converter-api .
   docker run -p 5000:5000 currency-converter-api
   ```

## Authenticating the API in Swagger
1. Start the API and open Swagger UI (usually at `https://localhost:5001/swagger`).
2. Click on the Authorize button (padlock icon) in the top-right corner.
3. Enter your JWT token* in the following format:
   ```sh
   Bearer YOUR_ACCESS_TOKEN
   ```
4. To get create new token , please use "/api/auth/login" with Username = "admin" & Password = "password".
5. Click Authorize, then close the modal.
6. Now you can access the secured endpoints with authentication enabled.

## Assumptions Made
- The **Frankfurter API** is available and operational.
- Restricted currencies (`TRY, PLN, THB, MXN`) cannot be used in conversions.
- JWT authentication is required for all endpoints.
- The API is deployed in environments supporting Serilog logging.

## Possible Future Enhancements
- **Multi-provider support**: Allow integration with multiple exchange rate providers.
- **Database storage**: Persist historical exchange rates for offline access.
- **Enhanced rate-limiting**: Implement dynamic throttling based on user roles.
- **Kubernetes deployment**: Enable seamless scaling and orchestration.
- **OpenTelemetry integration**: Improve monitoring with distributed tracing.


For any issues or contributions, feel free to raise a pull request or open an issue in the repository!

