# Login.feature
# language: en

Feature: User Login and Account Creation

  As a user of the Organization Board application
  I want to be able to log in and create new accounts
  So that I can access and manage my organization's boards and tasks

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

  Scenario: Successful Account and Organization Creation
    Given the "Admin" role exists in the database
    And the BCrypt service will hash "newPassword" to "hashedNewPassword"
    When a new account is created with email "newuser@example.com", password "newPassword", and organization "NewOrg"
    Then a new organization "NewOrg" should be saved to the database
    And a new user "newuser@example.com" with "hashedNewPassword" and "Admin" role for "NewOrg" should be saved to the database

  Scenario: Account and Organization Creation Fails if Admin Role Not Found
    Given the "Admin" role does not exist in the database
    When a new account is created with email "newuser@example.com", password "newPassword", and organization "NewOrg"
    Then an ApplicationException with message "Something went wrong while logging in." should be thrown

  Scenario: Account and Organization Creation Fails due to Database Error during Organization Save
    Given the "Admin" role exists in the database
    And the BCrypt service will hash "newPassword" to "hashedNewPassword"
    And a database error occurs when saving the organization "NewOrg"
    When a new account is created with email "newuser@example.com", password "newPassword", and organization "NewOrg"
    Then an ApplicationException with message "Something went wrong while logging in." should be thrown

  Scenario: Account and Organization Creation Fails due to Database Error during User Save
    Given the "Admin" role exists in the database
    And the BCrypt service will hash "newPassword" to "hashedNewPassword"
    And the organization "NewOrg" is successfully saved
    And a database error occurs when saving the user "newuser@example.com"
    When a new account is created with email "newuser@example.com", password "newPassword", and organization "NewOrg"
    Then an ApplicationException with message "Something went wrong while logging in." should be thrown