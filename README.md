# Semester-Exam Organization-Board
Exam for Secure Software Development

# Setup

## Migrate ef framework
```bash
dotnet ef migrations add FixTables
```
## Compose docker
```bash
docker-compose up
```

# Testing the features using Swagger
Once docker has been composed, go to:
```bash
localhost:8080/swagger
```
## Steps
1. Use the **api/Login/public-key** and save the entire key.
2. Use the **api/Login/EncryptPasswordBummyForWebsideResponsibility** - Type password and your public key, Execute and save your encrypted password
3. Use the **api/Login/Login** - Type in your Mail and your encrypted password and save the token it gives you.
4. Use the token to enter the Authorize button top right, make sure to write Bearer infront of the token.
5. You are now an Admin and can do anyting. If creating users: RoleId = 1(Admin), 2(Team Lead), 3(Team Member)


