Functionality: User administration - Create user
  As an administrator
  I would like to be able to create a new user
  to give new people access to the system.

  Scenario: Successful user creation
    Given I am an administrator with the ID 1
    And I have the following user data:
        | Name       | Email                   |
        | Max Müller | max.mueller@example.com |
    When I try to create a new user with this data
    Then the operation should be successful
    And a new user with the specified data and a unique ID should have been created
    And I should receive a success message with the created user