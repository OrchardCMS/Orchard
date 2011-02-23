Feature: Content Types
    In order to add new types to my site
    As an adminitrator
    I want to create create content types

Scenario: I can create a new content type
    Given I have installed Orchard
    When I go to "Admin/ContentTypes"
    Then I should see "<a[^>]*>.*?Create new type</a>"
    When I go to "Admin/ContentTypes/Create"
        And I fill in
            | name | value |
            | DisplayName | Event |
            | Name | Event |
        And I hit "Create"
        And I go to "Admin/ContentTypes/"
    Then I should see "Event"

Scenario: I can't create a content type with an already existing name
    Given I have installed Orchard
    When I go to "Admin/ContentTypes/Create"
        And I fill in
            | name | value |
            | DisplayName | Event |
            | Name | Event |
        And I hit "Create"
        And I go to "Admin/ContentTypes/"
    Then I should see "Event"
    When I go to "Admin/ContentTypes/Create"
        And I fill in
            | name | value |
            | DisplayName | Event |
            | Name | Event-2 |
        And I hit "Create"
    Then I should see "<h1[^>]*>.*?New Content Type.*?</h1>"
        And I should see "validation-summary-errors"

Scenario: I can't create a content type with an already existing technical name
    Given I have installed Orchard
    When I go to "Admin/ContentTypes/Create"
        And I fill in
            | name | value |
            | DisplayName | Dinner |
            | Name | Dinner |
        And I hit "Create"
        And I go to "Admin/ContentTypes/"
    Then I should see "Dinner"    
    When I go to "Admin/ContentTypes/Create"
        And I fill in
            | name | value |
            | DisplayName | Dinner2 |
            | Name | Dinner |
        And I hit "Create"
    Then I should see "<h1[^>]*>.*?New Content Type.*?</h1>"
        And I should see "validation-summary-errors"

Scenario: I can't rename a content type with an already existing name
    Given I have installed Orchard
    When I go to "Admin/ContentTypes/Create"
        And I fill in
            | name | value |
            | DisplayName | Dinner |
            | Name | Dinner |
        And I hit "Create"
        And I go to "Admin/ContentTypes/"
    Then I should see "Dinner"    
    When I go to "Admin/ContentTypes/Create"
        And I fill in
            | name | value |
            | DisplayName | Event |
            | Name | Event |
        And I hit "Create"
        And I go to "Admin/ContentTypes/"
    Then I should see "Event"    
    When I go to "Admin/ContentTypes/Edit/Dinner"
        And I fill in
            | name | value |
            | DisplayName | Event |
        And I hit "Save"
    Then I should see "validation-summary-errors"