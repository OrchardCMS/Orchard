Feature: Lists
    In order to add new lists to my site
    As an administrator
    I want to create lists

Scenario: I can create a new list
    Given I have installed Orchard
        And I have installed "Orchard.Lists"
    When I go to "Admin/ContentTypes"
        And I go to "Admin/ContentTypes/Create"
        And I fill in
            | name        | value |
            | DisplayName | Event |
            | Name        | Event |
        And I hit "Create"
        And I am redirected
        And I fill in
            | name                          | value |
            | PartSelections[5].IsSelected  | True  |
        And I hit "Save"
        And I go to "Admin/ContentTypes/"
    Then I should see "Event"

    When I go to "Admin/Contents/Create/List"
        And I fill in
            | name                               | value  |
            | Title.Title                        | MyList |
            | Container.SelectedItemContentTypes | Event  |
        And I hit "Save"
        And I am redirected
    Then I should see "Your List has been created"
    When I go to "Admin/Lists"
    Then I should see "MyList"
    When I follow "Contained Items (0)"
    Then I should see "'MyList' has no content items"
