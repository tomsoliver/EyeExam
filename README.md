# Eye-Exam
Because we're testing your ability to C# :wink:.

# Problem Description 
We've provided the `EYEEXAMAPI` project.  This project is a lightweight data provider you should treat as another microservice in our stack. For the sake of speed and ease, just focus on consuming this api hosted locally.

This API has two routes:
1. Schedule GET
     Returns a collection of raw Schedule of Notice of Lease Data.  This is data provided in a lossy format.
1. Results GET
     The Expected Output of *your* API we expect to see. 

Your job is to write a *new* C# .NET API to consume this API that can take the raw Schedule data and parse it into the same format as the data provided on the /results/ route (don't worry about the order of the results, only the accuracy of the parsing). Your solution should implement some sort of storage or caching so that repeat calls don't incur repeat processing. 

No modification to the provided code should be necessary `EYEEXAMAPI`.

# Run Instructions

## Dependencies
The API was built with .NET 6 Minimal API. As such you'll need the .NET 6 SDK and relevant dev tools.   

## To run with VSStudio / Ryder
1. Open the EyeExamApi.csproj
1. Hit Debug. 
1. Api will open and navigate to swagger. 

## To run with VSCode 
1. Open your terminal.
2. Navigate to `src/EyeExamApi` 
3. Run `dotnet run`
4. Navigate to `https://localhost:7203/swagger/index.html` 
5. You may need to run `dotnet dev-certs https --trust` to accept the dotnet dev ssl cert

## To run with Docker
1. Run `docker build . -f ./src/EyeExamApi/Dockerfile -t eye-exam`
2. Run `docker run -p 80:80 -p 443:443 eye-exam`
3. Navigate to `https://localhost:7203/swagger/index.html` 
4. Make a request with `curl -v --insecure -XGET -H 'Authorization: basic dGVzdHk6bWNUZXN0RmFjZQ==' 'http://localhost:80/results'`

## Auth
The API uses Basic auth and the credentials can be find the in the appSettings json.

# What we're looking for

## Core Competencies
* Show off how you like to code _well_ 
     * Consistent standards
     * SOLID Principles
     * SoC
     * Code readability
     * Well documented code.
* Simple to run or well documented startup.
* Accurate Result Set.
* Test Coverage.

## Additional points
Extra points are available for
* Algorithm extensibility for future cases
* External visiblity and live diagnostics (think: logging, telemetry etc)
* Consideration of hosting + deployment (Docker, Serverless etc) 
* Write up of next steps to get the solution to production
* _anything else you think is relevant, get creative_ 

## Time
We expect this to take no more than two hours to cover the core competencies above, if you decide to spend more time to show off or hit some bonus points you're more than welcome to!

If you find yourself running out of time prior to completing the entire problem, we will also review partial submissions paired with a write up of what your next steps would have been.

# Submission
Submit your result via your medium of choice (zip archive / git repo) to the recruiter or contact at Orbital Witness, and we'll be in touch with the results! 

# Notes
## Project Organisation
Generally I organise my projects as follows:
- API: Responsible for HTTP infrastructure, observability, auth, etc.
- Domain: Contains all business logic for a given system. This exposes out relevant interfaces for repositories to implement. I usually prefer organising code by domain concept rather than infrastructural concept (like schedules rather than interfaces vs implementations)
- Repository(s): The domain exposes repository interfaces that the repository layer implements. It pushes observability metadata which gets recorded as tagged metrics or added to the request context and logged by the API.

## Considerations to go live
- More Test coverage
     - Acceptance tests (I usually do them in C#)
     - Ask about the raw schedule service to understand message format so it can be unit tested
     - Split existing test into smaller units
- Pagination of results, consider using IAsyncEnumerable for raw schedule service
- Security
     - Secure API with strong authentication if possible (maybe digest or OAuth2)
     - Secure API keys in something like AWS Secrets manager
- Add health check and ping (useful for ECS service management and load balance target groups)
- Diagnostics:
     - Add diagnostic header support, like UserAgent and X-Request-Id
     - API & Repository Metrics with relevant dimensions
     - Repository call metadata (like calls to Raw Schedule Service if it's another API) added to diagnostic context to be logged by middleware
     - Debug logs disabled when deploying
     - Log user identity
