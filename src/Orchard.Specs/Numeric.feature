Feature: Numeric Field
    In order to add numeric content to my types
    As an administrator
    I want to create, edit and publish numeric fields

Scenario: Creating and using numeric fields

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

    # Adding a numeric field
    When I go to "Admin/ContentTypes/Edit/Event"
        And I follow "Add Field"
        And I fill in
            | name          | value        |
            | DisplayName   | Guests       |
            | Name          | Guests       |
            | FieldTypeName | NumericField |
        And I hit "Save"
        And I am redirected
    Then I should see "The \"Guests\" field has been added."

    # Creating an Event content item
    When I go to "Admin/Contents/Create/Event"
    Then I should see "Guests"
    When I fill in
            | name               | value |
            | Event.Guests.Value | 3     |
        And I hit "Save Draft"
        And I am redirected
    Then I should see "The Event has been created as a draft."
    When I go to "Admin/Contents/List"
    Then I should see "Guests:"
        And I should see "3"

    # The hint should be displayed
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                | value                 |
            | Fields[0].NumericFieldSettings.Hint | Please enter a number |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Please enter a number"

    # The value should be required
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                    | value |
            | Fields[0].NumericFieldSettings.Required | true  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
        And I fill in
            | name               | value |
            | Event.Guests.Value |       |
        And I hit "Save Draft"
    Then I should see "The Guests field is required."

    # The value should be bound
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                   | value |
            | Fields[0].NumericFieldSettings.Minimum | -10   |
            | Fields[0].NumericFieldSettings.Maximum | 100   |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "min=\"-10\""
        And I should see "max=\"100\""
    When I fill in
            | name               | value |
            | Event.Guests.Value | -20   |
        And I hit "Save Draft"
    Then I should see "The value must be greater than -10"
    When I go to "Admin/Contents/Create/Event"
        And I fill in
            | name               | value |
            | Event.Guests.Value | 101   |
        And I hit "Save Draft"
    Then I should see "The value must be less than 100"

    # Settings should be validated
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                   | value |
            | Fields[0].NumericFieldSettings.Minimum | a     |
            | Fields[0].NumericFieldSettings.Maximum | b     |
        And I hit "Save Draft"
    Then I should see "The value &#39;a&#39; is not valid for Minimum."
        And I should see "The value &#39;b&#39; is not valid for Maximum."

    # The value should be validated
    When I go to "Admin/Contents/Create/Event"
        And I fill in
            | name               | value |
            | Event.Guests.Value |   a   |
        And I hit "Save Draft"
    Then I should see "Guests is an invalid number"

    # The default value should be proposed on creation
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                        | value |
            | Fields[0].NumericFieldSettings.DefaultValue | 1234  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "value=\"1234\""

    # The required attribute should be used
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                        | value |
            | Fields[0].NumericFieldSettings.Required     | true  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "required=\"required\""

    # The required attribute should not be used
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in
            | name                                        | value |
            | Fields[0].NumericFieldSettings.Required     | false |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should not see "required=\"required\""