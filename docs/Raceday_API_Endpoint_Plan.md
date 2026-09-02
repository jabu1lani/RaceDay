## Authentication Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| POST | /api/auth/register | Register a new user account with role assignment (Participant or Organiser) | None | { "email": "string", "password": "string", "firstName": "string", "lastName": "string", "phoneNumber": "string", "role": "string" } | 201 Created: User registered successfully<br>400 Bad Request: Validation errors<br>409 Conflict: Email already exists |
| POST | /api/auth/login | Authenticate a registered user and return a JWT access token for session management | None | { "email": "string", "password": "string" } | 200 OK: JWT token with user details<br>401 Unauthorized: Invalid credentials |
| POST | /api/auth/logout | Invalidate the current user's JWT token to end their session | Any authenticated user | None | 200 OK: Logged out successfully<br>401 Unauthorized: No valid token |

## User Profile Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| GET | /api/users/profile | Retrieve the complete profile information for the currently authenticated user | Any authenticated user | None | 200 OK: User profile object<br>401 Unauthorized: No valid token |
| PUT | /api/users/profile | Update the profile details for the currently authenticated user | Any authenticated user | { "firstName": "string", "lastName": "string", "phoneNumber": "string" } | 200 OK: Updated user profile<br>400 Bad Request: Validation errors<br>401 Unauthorized: No valid token |
| PUT | /api/users/change-password | Change the password for the currently authenticated user | Any authenticated user | { "currentPassword": "string", "newPassword": "string" } | 200 OK: Password updated successfully<br>400 Bad Request: Invalid current password |
