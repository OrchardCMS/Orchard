Feature: Lists
    In order to add new lists to my site
    As an administrator
    I want to create lists

Scenario: I can create a new list
    Given I have installed Orchard
		And I have installed "Orchard.Lists"
    When I go to "Admin/Contents/Create/List"
        And I fill in
            | name | value |
            | Title.Title | MyList |
        And I hit "Save"
        And I go to "Admin/Contents/List/List"
    Then I should see "MyList"

Scenario: I can add content items to a list
    Given I have installed Orchard
		And I have installed "Orchard.Lists"
        And I have a containable content type "MyType"
    When I go to "Admin/Contents/Create/List"
        And I fill in
            | name | value |
            | Title.Title | MyList |
        And I hit "Save"
        And I go to "Admin/Contents/List/List"
    Then I should see "MyList"
    When I follow "Contained Items"
    Then I should see "The 'MyList' List has no content items."
    When I follow "Create New Content" where href has "ReturnUrl"
    Then I should see "MyType"
    When I follow "MyType" where href has "ReturnUrl"
        And I fill in
            | name | value |
            | Title.Title | MyContentItem |
        And I hit "Save"
        And I am redirected
    Then I should see "Manage MyList"
        And I should see "MyContentItem"
