# product-platform-template


# Project Name

This project is a template for a product platform using [Blazor/.NET](https://dotnet.microsoft.com/) containerized with Docker and configured to use a PostgreSQL database. Environment variables are used to configure database access and Docker settings securely and flexibly.

## Environment Variables

The following environment variables are required to run this application. Create a `.env` file in the project’s root directory, and add the variables as shown below. Be sure to replace placeholder values as needed.

### Required Environment Variables

| Variable          | Description                                                             | Example Value                                      |
|-------------------|-------------------------------------------------------------------------|----------------------------------------------------|
| `POSTGRES_USER`   | Username for the PostgreSQL database. Used for database authentication. | `pptuser`                                          |
| `POSTGRES_PASSWORD` | Password for the PostgreSQL user. Used for database authentication.   | `yourpassword123`                                  |
| `POSTGRES_DB`     | Name of the PostgreSQL database.                                        | `ppt`                                              |
| `DATABASE_URL`    | Full connection string for PostgreSQL. Format: `Host=<host>;Port=<port>;Database=<database>;Username=<user>;Password=<password>` | `Host=localhost;Port=5432;Database=ppt;Username=pptuser;Password=yourpassword123` |
| `DOCKER_USERNAME` | Docker Hub username, used for tagging and pushing Docker images.        | `ptandersen`                                       |

### Example `.env` File

Here’s an example `.env` file setup:

```plaintext
POSTGRES_USER=pptuser
POSTGRES_PASSWORD=yourpassword123
POSTGRES_DB=ppt
DATABASE_URL=Host=localhost;Port=5432;Database=ppt;Username=pptuser;Password=yourpassword123
DOCKER_USERNAME=ptandersen
