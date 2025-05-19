# language: en

Feature: Board Management
  As a team leader or member
  I want to manage boards and tasks
  So that I can organize team work effectively

  Scenario: Create a board successfully
    Given I am a "Team Leader" user with ID 4 in team 1
    When I create a board named "Sprint Planning"
    Then the operation should succeed
    And the returned board ID should not be 0

  Scenario: Fail to create board as team member
    Given I am a "Team Member" user with ID 5 in team 1
    When I create a board named "Backlog"
    Then the operation should fail with status code 403

  Scenario: Update board successfully
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 2 exists for team 1
    When I update board 2 name to "Revised Plan"
    Then the operation should succeed

  Scenario: Fail to update board as team member
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 3 exists for team 1
    When I update board 3 name to "Another Plan"
    Then the operation should fail with status code 403

  Scenario: Fail to update non-existent board
    Given I am a "Team Leader" user with ID 4 in team 1
    When I update board 99 name to "Non Existent"
    Then the operation should fail with status code 404

  Scenario: Delete board successfully
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 4 exists for team 1
    When I delete board 4
    Then the operation should succeed

  Scenario: Fail to delete board as team member
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 5 exists for team 1
    When I delete board 5
    Then the operation should fail with status code 403

  Scenario: Fail to delete non-existent board
    Given I am a "Team Leader" user with ID 4 in team 1
    When I delete board 99
    Then the operation should fail with status code 404

  Scenario: Get board by ID successfully
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 6 exists for team 1
    When I get board with ID 6
    Then the operation should succeed
    And the returned board ID should be 6

  Scenario: Fail to get board by ID as outsider
    Given I am a "Team Member" user with ID 99 in team 99
    And a board with ID 7 exists for team 1
    When I get board with ID 7
    Then the operation should fail with status code 403

  Scenario: Fail to get non-existent board by ID
    Given I am a "Team Member" user with ID 5 in team 1
    When I get board with ID 99
    Then the operation should fail with status code 404

  Scenario: Get team boards successfully
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 8 exists for team 1
    And a board with ID 9 exists for team 1
    When I get all boards for team 1
    Then the operation should succeed
    And the returned board list should contain at least 2 boards

  Scenario: Fail to get team boards as outsider
    Given I am a "Team Member" user with ID 99 in team 99
    And a board with ID 10 exists for team 1
    When I get all boards for team 1
    Then the operation should fail with status code 403

  Scenario: Get board tasks successfully
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 11 exists for team 1
    And the board has tasks
    When I fetch tasks for board 11
    Then the task response should contain tasks

  Scenario: Fail to fetch tasks as outsider
    Given a board with ID 12 exists for team 1
    And I am a "Team Member" user with ID 99 in team 99
    When I fetch tasks for board 12
    Then the operation should fail with status code 403

  Scenario: Create task successfully
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 13 exists for team 1
    When I create a task named "Implement Feature A" for board 13
    Then the operation should succeed
    And the returned task ID should not be 0

  Scenario: Fail to create task as team member
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 14 exists for team 1
    When I create a task named "Write Unit Tests" for board 14
    Then the operation should fail with status code 403

  Scenario: Fail to create task for non-existent board
    Given I am a "Team Leader" user with ID 4 in team 1
    When I create a task named "Deploy" for board 99
    Then the operation should fail with status code 404

  Scenario: Get task by ID successfully
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 15 exists for team 1
    And a task with ID 1 exists on board 15
    When I get task with ID 1
    Then the operation should succeed
    And the returned task ID should be 1

  Scenario: Fail to get non-existent task by ID
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 16 exists for team 1
    When I get task with ID 99
    Then the operation should fail with status code 404

  Scenario: Update task successfully
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 17 exists for team 1
    And a task with ID 2 exists on board 17
    When I update task 2 title to "Implement Feature B"
    Then the operation should succeed

  Scenario: Fail to update task as team member
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 18 exists for team 1
    And a task with ID 3 exists on board 18
    When I update task 3 title to "Refactor Code"
    Then the operation should fail with status code 403

  Scenario: Fail to update non-existent task
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 19 exists for team 1
    When I update task 99 title to "Something"
    Then the operation should fail with status code 404

  Scenario: Delete task successfully
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 20 exists for team 1
    And a task with ID 4 exists on board 20
    When I delete task 4
    Then the operation should succeed

  Scenario: Fail to delete task as team member
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 21 exists for team 1
    And a task with ID 5 exists on board 21
    When I delete task 5
    Then the operation should fail with status code 403

  Scenario: Fail to delete non-existent task
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 22 exists for team 1
    When I delete task 99
    Then the operation should fail with status code 404

  Scenario: Assign task successfully
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 23 exists for team 1
    And a task with ID 6 exists on board 23
    And a user with ID 6 exists in team 1
    When I assign task 6 to user 6
    Then the operation should succeed

  Scenario: Fail to assign task as team member
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 24 exists for team 1
    And a task with ID 7 exists on board 24
    And a user with ID 6 exists in team 1
    When I assign task 7 to user 6
    Then the operation should fail with status code 403

  Scenario: Fail to assign non-existent task
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 25 exists for team 1
    And a user with ID 6 exists in team 1
    When I assign task 99 to user 6
    Then the operation should fail with status code 404

  Scenario: Fail to assign to non-existent user
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 26 exists for team 1
    And a task with ID 8 exists on board 26
    When I assign task 8 to user 99
    Then the operation should fail with status code 404

  Scenario: Mark task as complete successfully
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 27 exists for team 1
    And a task with ID 9 exists on board 27 assigned to user 5
    When I mark task 9 as complete
    Then the operation should succeed

  Scenario: Fail to mark task as complete if not assigned
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 28 exists for team 1
    And a task with ID 10 exists on board 28 not assigned to user 5
    When I mark task 10 as complete
    Then the operation should fail with status code 403

  Scenario: Fail to mark non-existent task as complete
    Given I am a "Team Member" user with ID 5 in team 1
    When I mark task 99 as complete
    Then the operation should fail with status code 404

  Scenario: Confirm task completion successfully
    Given I am a "Team Leader" user with ID 4 in team 1
    And a board with ID 29 exists for team 1
    And a task with ID 11 exists on board 29 marked as complete
    When I confirm task 11 completion
    Then the operation should succeed

  Scenario: Fail to confirm task completion as team member
    Given I am a "Team Member" user with ID 5 in team 1
    And a board with ID 30 exists for team 1
    And a task with ID 12 exists on board 30 marked as complete
    When I confirm task 12 completion
    Then the operation should fail with status code 403

  Scenario: Fail to confirm non-existent task completion
    Given I am a "Team Leader" user with ID 4 in team 1
    When I confirm task 99 completion
    Then the operation should fail with status code 404