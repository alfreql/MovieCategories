# Movie Categories

## Story:

**Title:** Manage Movie Categories

**As a:**  
User

**I want to:**  
Easily manage categories for movies in the database, including creating new categories, viewing existing ones, updating them, and deleting them when they are no longer needed.

## User Story Details:

### 1. Create a New Category
**Scenario:** Adding a new category to the movie database.  
**Given:** I am an authenticated User.  
**When:** I submit a request to create a new category with a unique name and description.  
**Then:** The category is created and stored in the database with a unique identifier, name, and description.

### 2. View Existing Categories
**Scenario:** Viewing a list of all existing categories.  
**Given:** I am an authenticated User.  
**When:** I request a list of all movie categories.  
**Then:** I receive a list of categories, each with its unique identifier, name, and description.

### 3. View a Category
**Scenario:** Viewing an specific category.  
**Given:** I am an authenticated User.  
**When:** I request an specific categoty by its unique identifier.  
**Then:** I receive the category, with its unique identifier, name, and description.

### 4. Update an Existing Category
**Scenario:** Updating the details of an existing category.  
**Given:** I am an authenticated User.  
**When:** I submit a request to update the name or description of an existing category.  
**Then:** The category details are updated in the database.

### 5. Delete a Category
**Scenario:** Deleting an existing category from the movie database.  
**Given:** I am an authenticated User.  
**When:** I submit a request to delete a category by its unique identifier.  
**Then:** The category is removed from the database.

### 5. User Authentication
**Scenario:** Secure access to the Movie's Categorie API endpoints.  
**Given:** I am a user.  
**When:** I try to access the API endpoints.  
**Then:** I must authenticate to perform actions like creating, updating, or deleting categories.


## Acceptance Criteria:

### Create Category API:
- **Endpoint:** `POST /api/MoviesCategories`
- **Request Body:** `{ "Category": "Action", "Description": "Action Movies" }`
- **Response:** `200 OK` with the new category id.

### Read Categories API:
- **Endpoint:** `GET /api/MoviesCategories`
- **Response:** `200 OK` with a list of categories.

### Read Category API:
- **Endpoint:** `GET /api/MoviesCategories/{id}`
- **Response:** `200 OK` with the category.

### Update Category API:
- **Endpoint:** `PUT /api/MoviesCategories/{id}`
- **Request Body:** `{ "Category": "Thriller", "Description": "Movies Thriller" }`
- **Response:** `200 OK`.

### Delete Category API:
- **Endpoint:** `DELETE /api/MoviesCategories/{id}`
- **Response:** `200 OK`.

### User Authentication API:
- **Endpoint:** `POST /token`
- **Request Body:** `{ "Email": "email@rmail.com", "Password": "password" }`
- **Response:** `200 OK` with an authentication token.

### Authorization:
Ensure endpoints for creating, viewing, updating, and deleting categories are restricted to authenticated users only.


## Run the Application:
1. Open Command Prompt and navigate to the directory `scripts`.
2. Run the following command: `docker-compose up --build`

### API Swagger:
- Identity API: http://localhost:5001/swagger/index.html
- MoviesCategories API: http://localhost:5002/swagger/index.html