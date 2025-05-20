# Login.feature
# language: en

Feature: User Login and Account Creation

  As a user of the Organization Board application
  I want to be able to log in and create new accounts
  So that I can access and manage my organization's boards and tasks

  # --- Existing Login Scenarios ---

  Scenario: Successful User Login with Valid Credentials
    Given a user with email "test@example.com" and password "hashedPassword" exists in the database
    And the RSA service will decrypt "encryptedPassword" to "decryptedPassword"
    And the BCrypt service will verify "decryptedPassword" against "hashedPassword" as true
    When the user attempts to log in with email "test@example.com" and password "encryptedPassword"
    Then the login should be successful and the user details are returned

  Scenario: Login Fails with Invalid Email
    Given no user with email "wrong@example.com" exists in the database
    When the user attempts to log in with email "wrong@example.com" and password "anyPassword"
    Then an UnauthorizedAccessException should be thrown

  Scenario: Login Fails with Invalid Password
    Given a user with email "test@example.com" and password "hashedPassword" exists in the database
    And the RSA service will decrypt "wrongEncryptedPassword" to "decryptedWrongPassword"
    And the BCrypt service will verify "decryptedWrongPassword" against "hashedPassword" as false
    When the user attempts to log in with email "test@example.com" and password "wrongEncryptedPassword"
    Then an UnauthorizedAccessException should be thrown

  Scenario: Login Fails due to RSA Decryption Error
    Given a user with email "test@example.com" and password "hashedPassword" exists in the database
    And the RSA service throws an exception when decrypting "encryptedPassword"
    When the user attempts to log in with email "test@example.com" and password "encryptedPassword"
    Then an ApplicationException with message "Something went wrong while logging in." should be thrown

  Scenario: Login Fails due to BCrypt Verification Error
    Given a user with email "test@example.com" and password "hashedPassword" exists in the database
    And the RSA service will decrypt "encryptedPassword" to "decryptedPassword"
    And the BCrypt service throws an exception when verifying "decryptedPassword" against "hashedPassword"
    When the user attempts to log in with email "test@example.com" and password "encryptedPassword"
    Then an ApplicationException with message "Something went wrong while logging in." should be thrown

  # --- New Login Scenarios (BVT/ECT) ---

  Scenario: Login Fails with Email at Minimum Invalid Length (BVT)
    Given a user with email "valid@example.com" and password "hashedPassword" exists in the database
    And the RSA service will decrypt "encryptedPassword" to "decryptedPassword"
    When the user attempts to log in with email "a@a.c" and password "encryptedPassword"
    Then an UnauthorizedAccessException with message "Attempted to perform an unauthorized operation." should be thrown

  Scenario: Login Fails with Email Containing Invalid Format (ECT)
    Given a user with email "test@example.com" and password "hashedPassword" exists in the database
    And the RSA service will decrypt "encryptedPassword" to "decryptedPassword"
    When the user attempts to log in with email "invalid-email" and password "encryptedPassword"
    Then a ValidationException with message "Invalid email or password" should be thrown

  Scenario: Login Fails with Missing Email (Required attribute)
    Given a user with email "test@example.com" and password "hashedPassword" exists in the database
    And the RSA service will decrypt "encryptedPassword" to "decryptedPassword"
    When the user attempts to log in with email "" and password "encryptedPassword"
    Then a ValidationException with message "The Email field is required." should be thrown 

  Scenario: Login Fails with Missing Password (Required attribute)
    Given a user with email "test@example.com" and password "hashedPassword" exists in the database
    And the RSA service will decrypt "encryptedPassword" to "decryptedPassword"
    When the user attempts to log in with email "test@example.com" and password ""
    Then a ValidationException with message "Invalid email or password" should be thrown

  # --- Existing Account Creation Scenarios ---

  Scenario: Successful Account and Organization Creation
    Given the "Admin" role exists in the database
    And the BCrypt service will hash "newPassword123!" to "hashedNewPassword123!"
    When a new account is created with email "newuser@example.com", password "newPassword123!", and organization "NewOrg"
    Then a new organization "NewOrg" should be saved to the database
    And a new user "newuser@example.com" with "hashedNewPassword123!" and "Admin" role for "NewOrg" should be saved to the database

  Scenario: Account and Organization Creation Fails due to Database Error during User Save
    Given the "Admin" role exists in the database
    And the BCrypt service will hash "P@ssw0rd1" to "hashedNewPassword"
    And the organization "NewOrg" is successfully saved
    And a database error occurs when saving the user "newuser@example.com"
    When a new account is created with email "newuser@example.com", password "P@ssw0rd1", and organization "NewOrg"
    Then an ApplicationException with message "Something went wrong while logging in." should be thrown

  # --- New Account Creation Scenarios (BVT/ECT) ---

  Scenario: Account Creation Fails with Missing Email (Required attribute)
    Given the "Admin" role exists in the database
    And the BCrypt service will hash "password123!" to "hashedPassword123!"
    When a new account is created with email "", password "password123!", and organization "ValidOrg"
    Then a ValidationException with message "The Email field is required." should be thrown

  Scenario: Account Creation Fails with Email Containing Invalid Format (ECT)
    Given the "Admin" role exists in the database
    And the BCrypt service will hash "password123!" to "hashedPassword123!"
    When a new account is created with email "invalid-email-format", password "password123!", and organization "ValidOrg"
    Then a ValidationException with message "Invalid email format." should be thrown

  Scenario: Account Creation Fails with Password Lacking Required Complexity (ECT)
    Given the "Admin" role exists in the database
    And the BCrypt service will hash "simplepassword123" to "hashedSimplePassword"
    When a new account is created with email "newuser@example.com", password "simplepassword123", and organization "ValidOrg"
    Then a ValidationException with message "Password must be at least 8 characters long, contain at least one uppercase letter, one number, and one special character." should be thrown

  Scenario: Account Creation Fails with Missing Password (Required attribute)
    Given the "Admin" role exists in the database
    And the BCrypt service will hash "" to ""
    When a new account is created with email "newuser@example.com", password "", and organization "ValidOrg"
    Then a ValidationException with message "The Password field is required." should be thrown 

  Scenario: Account Creation Fails with Missing Organization Name (Required attribute)
    Given the "Admin" role exists in the database
    And the BCrypt service will hash "password123!" to "hashedPassword123!"
    When a new account is created with email "newuser@example.com", password "Password123!", and organization ""
    Then a ValidationException with message "Organization name is required." should be thrown