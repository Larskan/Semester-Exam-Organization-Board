Feature: BoardService user-team verification

  Scenario: User is a team member of a board
    Given a mocked database context
    And the following users exist:
      | UserID | TeamID | RoleID |
      | 1      | 10     | 1      |
    And the following boards exist:
      | BoardID | TeamID |
      | 100     | 10     |
    And a user ID of 1
    And a board ID of 100
    When I check if the user is a team member of the board
    Then the result should be true

  Scenario: User is not a team member of a board
    Given a mocked database context
    And the following users exist:
      | UserID | TeamID | RoleID |
      | 2      | 20     | 1      |
    And the following boards exist:
      | BoardID | TeamID |
      | 101     | 10     |
    And a user ID of 2
    And a board ID of 101
    When I check if the user is a team member of the board
    Then the result should be false