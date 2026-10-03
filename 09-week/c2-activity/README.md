# Books API — REST API con Spring Boot

**CORHUILA · Ingeniería de Sistemas · Desarrollo FullStack 2026-B · Corte 2 (Semana 9)**
**Autor:** Victor Alfonso Tovar Ramirez ([@Naudi13](https://github.com/ariel5253))

API REST en **Spring Boot 3 / Java 17** con **arquitectura en capas** (`controller → service → repository → entity`), persistencia con **Spring Data JPA** (H2 en memoria), documentación con **Swagger (springdoc-openapi)** y pruebas con **Postman** y **JUnit/MockMvc**.

## Estructura del proyecto

```
09-week/
└── books-api/
    ├── pom.xml
    ├── README.md
    ├── postman/
    │   └── books-api.postman_collection.json
    └── src/
        ├── main/
        │   ├── java/com/corhuila/booksapi/
        │   │   ├── BooksApiApplication.java
        │   │   ├── config/        OpenApiConfig.java        (Swagger)
        │   │   ├── controller/    BookController.java       (capa web)
        │   │   ├── dto/           BookRequest, BookResponse (contratos + validación)
        │   │   ├── entity/        Book.java                 (modelo JPA)
        │   │   ├── exception/     GlobalExceptionHandler, ApiError, ...
        │   │   ├── repository/    BookRepository.java       (acceso a datos)
        │   │   └── service/       BookService, BookServiceImpl (lógica de negocio)
        │   └── resources/application.properties
        └── test/java/com/corhuila/booksapi/controller/BookControllerTest.java
```

## Requisitos

- JDK 17 o superior
- Maven 3.9+ (o importar el proyecto en IntelliJ IDEA / VS Code / Eclipse)
- Postman (para la colección)

## Cómo ejecutar

```bash
cd 09-week/books-api
mvn spring-boot:run
```

La aplicación queda en `http://localhost:8080`.

| Recurso | URL |
|---|---|
| Swagger UI | http://localhost:8080/swagger-ui.html |
| OpenAPI JSON | http://localhost:8080/api-docs |
| Consola H2 | http://localhost:8080/h2-console (JDBC URL: `jdbc:h2:mem:booksdb`, user: `sa`, sin contraseña) |

Ejecutar las pruebas automáticas:

```bash
mvn test
```

## Endpoints

Base path: `/api/v1/books`

| Método | Endpoint | Descripción | Respuestas |
|---|---|---|---|
| GET | `/api/v1/books` | Lista todos los libros | 200 |
| GET | `/api/v1/books/{id}` | Obtiene un libro por id | 200, 404 |
| POST | `/api/v1/books` | Crea un libro | 201, 400, 409 |
| PUT | `/api/v1/books/{id}` | Actualiza un libro | 200, 400, 404, 409 |
| DELETE | `/api/v1/books/{id}` | Elimina un libro | 204, 404 |

Ejemplo de cuerpo (POST / PUT):

```json
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "publishedYear": 2008,
  "price": 45.90
}
```

Ejemplo de error 400 (validación):

```json
{
  "timestamp": "2026-10-03T15:00:00Z",
  "status": 400,
  "error": "Bad Request",
  "message": "Validation failed",
  "path": "/api/v1/books",
  "details": ["title: title is required", "price: price must be >= 0"]
}
```

## Pruebas con Postman

1. Abre Postman → **Import** → selecciona `postman/books-api.postman_collection.json`.
2. Con la API corriendo, abre la colección y pulsa **Run** (Collection Runner).
3. Las 8 peticiones están ordenadas e incluyen tests; los casos **5 (404)**, **6 (400)** y **8 (404)** validan los errores.

## API reference

This API manages a catalog of books through a standard REST interface under the `/api/v1/books` base path. The `GET /api/v1/books` endpoint returns the complete list of books stored in the database, and it answers with `200 OK` even when the list is empty. The `GET /api/v1/books/{id}` endpoint returns a single book and responds with `404 Not Found` if no book exists with that id. The `POST /api/v1/books` endpoint creates a new book from a JSON body, returns `201 Created` with a `Location` header, and responds with `400 Bad Request` when the payload fails validation or `409 Conflict` when the ISBN already exists. The `PUT /api/v1/books/{id}` endpoint replaces all the fields of an existing book and returns the updated resource, or `404 Not Found` if the id does not exist. The `DELETE /api/v1/books/{id}` endpoint removes a book permanently and returns `204 No Content`, or `404 Not Found` when the book does not exist. Every error response uses the same JSON structure with the timestamp, status, message, request path, and a list of validation details.
