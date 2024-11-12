# product-platform-template

This project is a template for a product platform using [Blazor/.NET](https://dotnet.microsoft.com/) containerized with Docker and configured to use a PostgreSQL database. Environment variables are used to configure database access and Docker settings securely and flexibly.

## Environment Variables

The following environment variables are required to run this application. Create a `.env` file in the project’s root directory, and add the variables as shown below. Be sure to replace placeholder values as needed.

### Required Environment Variables

| Variable                     | Description                                                                                               | Example Value                                                |
|------------------------------|-----------------------------------------------------------------------------------------------------------|--------------------------------------------------------------|
| `POSTGRES_USER`              | Username for the PostgreSQL database. Used for database authentication.                                   | `pptuser`                                                    |
| `POSTGRES_PASSWORD`          | Password for the PostgreSQL user. Used for database authentication.                                       | `yourpassword123`                                            |
| `POSTGRES_DB`                | Name of the PostgreSQL database.                                                                          | `ppt`                                                        |
| `DATABASE_URL`               | Full connection string for PostgreSQL.                                                                    |                                                              |
|                              | Format: `Host=<host>;Port=<port>;Database=<database>;Username=<user>;Password=<password>`                 | `Host=localhost;Port=5432;Database=ppt;Username=pptuser;Password=yourpassword123` |
| `CONTAINER_DATABASE_URL`     | PostgreSQL connection string for use within the Docker container.                                         | `Host=postgres;Port=5432;Database=ppt;Username=pptuser;Password=yourpassword123` |
| `NON_CONTAINER_DATABASE_URL` | PostgreSQL connection string for local development (outside Docker).                                      | `Host=localhost;Port=5432;Database=ppt;Username=pptuser;Password=yourpassword123` |
| `DOCKER_USERNAME`            | Docker Hub username, used for tagging and pushing Docker images.                                          | `johndoe`                                                    |
| `DOTNET_RUNNING_IN_CONTAINER`| Indicates if the application is running inside a Docker container. Set to `true` or `false`.              | `false`                                                      |
| `CERTIFICATE_PATH`           | File path to the SSL certificate (in `.pfx` format) for secure connections.                               | `/path/to/your/certificate.pfx`                              |
| `CERTIFICATE_PASSWORD`       | Password for the SSL certificate.                                                                         | `dummy_certificate_password`                                 |
| `DOMAIN_NAME`                | Primary domain name for the application.                                                                  | `example.com`                                                |
| `WWW_DOMAIN_NAME`            | Subdomain for the application, typically `www` prefix.                                                    | `www.example.com`                                            |
| `EMAIL`                      | Contact email address used for application notifications.                                                 | `contact@example.com`                                        |
| `STATIC_FILES_PATH`          | File path to static files served by the application.                                                      | `/srv/static_files`                                          |
| `STATIC_FILE_BASE_URL`       | Base URL where static files are hosted.                                                                   | `https://staticfiles.example.com/`                           |

### Example `.env` File

Here’s an example `.env` file setup:

```plaintext
POSTGRES_USER=pptuser
POSTGRES_PASSWORD=yourpassword123
POSTGRES_DB=ppt
DATABASE_URL=Host=localhost;Port=5432;Database=ppt;Username=pptuser;Password=yourpassword123
CONTAINER_DATABASE_URL=Host=postgres;Port=5432;Database=ppt;Username=pptuser;Password=yourpassword123
NON_CONTAINER_DATABASE_URL=Host=localhost;Port=5432;Database=ppt;Username=pptuser;Password=yourpassword123
DOCKER_USERNAME=johndoe
DOTNET_RUNNING_IN_CONTAINER=false
CERTIFICATE_PATH=/path/to/your/certificate.pfx
CERTIFICATE_PASSWORD=dummy_certificate_password
DOMAIN_NAME=example.com
WWW_DOMAIN_NAME=www.example.com
EMAIL=contact@example.com
STATIC_FILES_PATH=/srv/static_files
STATIC_FILE_BASE_URL=https://staticfiles.example.com/
