Feature: Board Management
  As a team leader or member
  I want to manage boards
  So that I can organize team tasks

  Scenario: Create a board successfully
    Given I am a "Team Leader" user with ID 4 in team 1
    When I create a board named "Sprint Planning"
    Then the operation should succeed
    And the returned board ID should not be 0

  Scenario: Fail to create board as team member
    Given I am a "Team Member" user with ID 5 in team 1
    When I create a board named "Backlog"
    Then the operation should fail with status code 403

  Scenario: Get board tasks successfully
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 1 exists for team 1
    And the board has tasks
    When I fetch tasks for board 1
    Then the task response should contain tasks

  Scenario: Fail to fetch tasks as outsider
    Given a board with ID 1 exists for team 1
    And I am a "Team Member" user with ID 99 in team 99
    When I fetch tasks for board 1
    Then the operation should fail with status code 403
