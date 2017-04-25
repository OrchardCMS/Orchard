Feature: Link Field
    In order to add Link content to my types
    As an administrator
    I want to create, edit and publish Link fields

Scenario: Creating and using Link fields

    # Creating an Event content type
    Given I have installed Orchard
        And I have installed "Orchard.Fields"
    When I go to "Admin/ContentTypes"
    Then I should see "<a[^>]*>.*?Create new type</a>"
    When I go to "Admin/ContentTypes/Create"
        And I fill in
            | name        | value |
            | DisplayName | Event |
            | Name        | Event |
        And I hit "Create"
        And I go to "Admin/ContentTypes/"
    Then I should see "Event"

    # Adding a Link field
    When I go to "Admin/ContentTypes/Edit/Event"
        And I follow "Add Field"
        And I fill in
            | name          | value     |
            | DisplayName   | Site Url  |
            | Name          | SiteUrl   |
            | FieldTypeName | LinkField |
        And I hit "Save"
        And I am redirected
    Then I should see "The \"Site Url\" field has been added."

    # Creating an Event content item
    When I go to "Admin/Contents/Create/Event"
    Then I should see "Site Url"
    When I fill in
            | name                | value                         |
            | Event.SiteUrl.Value | http://www.orchardproject.net |
        And I fill in
            | name               | value   |
            | Event.SiteUrl.Text | Orchard |
        And I hit "Save Draft"
        And I am redirected
    Then I should see "The Event has been created as a draft."
    When I go to "Admin/Contents/List"
    Then I should see "Site Url:"
        And I should see "<a href=\"http://www.orchardproject.net\">Orchard</a>"

    # The hint should be displayed
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                             | value                         |
            | Fields[0].LinkFieldSettings.Hint | Enter the url of the web site |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Enter the url of the web site"

    # The value should be required
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                 | value |
            | Fields[0].LinkFieldSettings.Required | true |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
        And I fill in
            | name                | value |
            | Event.SiteUrl.Value |       |
        And I hit "Save Draft"
    Then I should see "Url is required for Site Url."

    # The default value should be proposed on creation
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                     | value                         |
            | Fields[0].LinkFieldSettings.DefaultValue | http://www.orchardproject.net |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "value=\"http://www.orchardproject.net\""

    # The required attribute should be used
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                     | value |
            | Fields[0].LinkFieldSettings.Required     | true  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "required=\"required\""

    # The required attribute should not be used
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                     | value |
            | Fields[0].LinkFieldSettings.Required     | false |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should not see "required=\"required\""