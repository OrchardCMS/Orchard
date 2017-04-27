Feature: Users
    In order to prevent users module regressions
    As a site owner
    I want to create, search and modify user accounts

@management
Scenario: There is only one user by default
    Given I have installed Orchard
    When I go to "admin/users"
    Then I should see "Users"
        And I should see "<a[^>]*>admin</a>"

@management
Scenario: I can create a new user
    Given I have installed Orchard
    When I go to "admin/users"
    Then I should see "Users"
    When I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user1 |
        | Email | user1@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Publish"
        And I am redirected
    Then I should see "User created"
    When I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user2 |
        | Email | user2@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        | UserRoles.Roles[0].Granted | true |
        And I hit "Publish"
        And I am redirected
    Then I should see "User created"
        And I should see "Adding role Administrator to user user2"
    When I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user3 |
        | Email | user3@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        | UserRoles.Roles[0].Granted | true |
        | UserRoles.Roles[1].Granted | true |
        | UserRoles.Roles[2].Granted | true |
        | UserRoles.Roles[3].Granted | true |
        | UserRoles.Roles[4].Granted | true |
        And I hit "Publish"
        And I am redirected
    Then I should see "User created"
        And I should see "Adding role Administrator to user user3"
        And I should see "Adding role Editor to user user3"
        And I should see "Adding role Moderator to user user3"
        And I should see "Adding role Author to user user3"
        And I should see "Adding role Contributor to user user3"
    When I follow "Add a new user"
        And I hit "Publish"
    Then I should see "The UserName field is required."
    Then I should see "The Email field is required."
    Then I should see "The Password field is required."
    Then I should see "The ConfirmPassword field is required."
    When I go to "admin/users"
        And I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user4 |
        | Email | user4@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a123456! |
        And I hit "Publish"
    Then I should see "Password confirmation must match"

@management
Scenario: I can edit an existing user
    Given I have installed Orchard
    When I go to "admin/users"
        And I follow "Publish"
        And I fill in
        | name | value |
        | UserName | user1 |
        | Email | user1@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Save Draft"
        And I am redirected
    Then I should see "User created"
    When I fill in
        | name | value |
        | Options.Search | user1 |
        And I hit "Filter"
    Then I should see "<a[^>]*>user1</a>"
    When I follow "Edit"
        And I fill in
        | name | value |
        | UserName | user2 |
        | Email | user2@domain.com |
        And I hit "Save Draft"
        And I am redirected
    Then I should see "User information updated"
        And I should see "<a[^>]*>user2</a>"
        And I should see "user2@domain.com"

@management
Scenario: I should not be able to reuse an existing username or email
    Given I have installed Orchard
    When I go to "admin/users"
# create user1
        And I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user1 |
        | Email | user1@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Publish"
        And I am redirected
# create user2
        And I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user2 |
        | Email | user2@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Publish"
        And I am redirected
    Then I should see "<a[^>]*>user1</a>"
        And I should see "<a[^>]*>user2</a>"
# filtering on 'user1' to have only one Edit link to follow
    When I fill in
        | name | value |
        | Options.Search | user1 |
        And I hit "Filter"
    Then I should see "<a[^>]*>user1</a>"
    When I follow "Edit"
        And I fill in
        | name | value |
        | UserName | user2 |
        | Email | user1@domain.com |
        And I hit "Save Draft"
    Then I should see "User with that username and/or email already exists."
    When I fill in
        | name | value |
        | UserName | user1 |
        | Email | user2@domain.com |
        And I hit "Save Draft"
    Then I should see "User with that username and/or email already exists."

@management
@ignore
Scenario: I should be able to remove an existing user
    Given I have installed Orchard
    When I go to "admin/users"
# create user1
        And I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user1 |
        | Email | user1@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Publish"
        And I am redirected
    Then I should see "<a[^>]*>user1</a>"
# filtering on 'user1' to have only one Delete link to follow
    When I fill in
        | name | value |
        | Options.Search | user1 |
        And I hit "Filter"
    Then I should see "<a[^>]*>user1</a>"
    When I follow "Delete"
        And I am redirected
    Then I should see "User user1 deleted"
        And I should not see "<a[^>]*>user1</a>"

@filtering
Scenario: I should not be able to filter users by name
    Given I have installed Orchard
    When I go to "admin/users"
# create user1
        And I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user1 |
        | Email | user1@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Publish"
        And I am redirected
# create user2
        And I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user2 |
        | Email | user2@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Publish"
        And I am redirected
    Then I should see "<a[^>]*>user1</a>"
        And I should see "<a[^>]*>user2</a>"
    When I fill in
        | name | value |
        | Options.Search | user1 |
        And I hit "Filter"
    Then I should see "<a[^>]*>user1</a>"
        And I should not see "<a[^>]*>admin</a>"
        And I should not see "<a[^>]*>user2</a>"
    When I fill in
        | name | value |
        | Options.Search | user1@domain.com |
        And I hit "Filter"
    Then I should see "<a[^>]*>user1</a>"
        And I should not see "<a[^>]*>admin</a>"
        And I should not see "<a[^>]*>user2</a>"
    When I fill in
        | name | value |
        | Options.Search | @domain.com |
        And I hit "Filter"
    Then I should see "<a[^>]*>user1</a>"
        And I should see "<a[^>]*>user2</a>"
        And I should not see "<a[^>]*>admin</a>"

@filtering
Scenario: I should be able to filter users by status
    Given I have installed Orchard
    When I go to "admin/users"
# create user1
        And I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user1 |
        | Email | user1@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Publish"
        And I am redirected
# create user2
        And I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user2 |
        | Email | user2@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Publish"
        And I am redirected
    Then I should see "<a[^>]*>user1</a>"
        And I should see "<a[^>]*>user2</a>"
    When I fill in
        | name | value |
        | Options.Search | user1 |
        And I hit "Filter"
    Then I should see "<a[^>]*>user1</a>"
    When I hit "Disable"
        And I am redirected
    Then I should see "User user1 disabled"
    When I fill in
        | name | value |
        | Options.Search | |
        | Options.Filter | Pending |
        And I hit "Filter"
    Then I should see "<a[^>]*>user1</a>"
        And I should not see "<a[^>]*>user2</a>"
        And I should not see "<a[^>]*>admin</a>"
    When I fill in
        | name | value |
        | Options.Search | |
        | Options.Filter | EmailPending |
        And I hit "Filter"
    Then I should not see "<a[^>]*>user1</a>"
        And I should not see "<a[^>]*>user2</a>"
        And I should not see "<a[^>]*>admin</a>"
    When I fill in
        | name | value |
        | Options.Search | |
        | Options.Filter | Approved |
        And I hit "Filter"
    Then I should not see "<a[^>]*>user1</a>"
        And I should see "<a[^>]*>user2</a>"
        And I should see "<a[^>]*>admin</a>"
    When I fill in
        | name | value |
        | Options.Search | |
        | Options.Filter | All |
        And I hit "Filter"
    Then I should see "<a[^>]*>user1</a>"
        And I should see "<a[^>]*>user2</a>"
        And I should see "<a[^>]*>admin</a>"
@email
Scenario: I should not be able to add users with invalid email addresses
    Given I have installed Orchard
    When I go to "admin/users"
        And I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user1 |
        | Email | NotAnEmail |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Publish"
    Then I should see "You must specify a valid email address."
@email
Scenario: I should be able to add users with valid email addresses
    Given I have installed Orchard
    When I go to "admin/users"
        And I follow "Add a new user"
        And I fill in
        | name | value |
        | UserName | user1 |
        | Email | user1@domain.com |
        | Password | a12345! |
        | ConfirmPassword | a12345! |
        And I hit "Publish"
        And I am redirected
    Then I should see "User created"
