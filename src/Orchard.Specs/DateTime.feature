Feature: DateTime Field
    In order to add Date content to my types
    As an administrator
    I want to create, edit and publish DateTime fields

Scenario: Creating and using Date fields
    
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
    
    # Adding a Date field
    When I go to "Admin/ContentTypes/Edit/Event"
        And I follow "Add Field"
        And I fill in
            | name          | value             |
            | DisplayName   | Date of the event |
            | Name          | EventDate         |
            | FieldTypeName | DateTimeField     |
        And I hit "Save"
        And I am redirected
    Then I should see "The \"Date of the event\" field has been added."

    # Invalid Date
    When I go to "Admin/Contents/Create/Event"
    Then I should see "Date of the event"
    When I fill in 
            | name                        | value      |
            | Event.EventDate.Editor.Date | 31/01/2012 |
            | Event.EventDate.Editor.Time | 12:00 AM   |
        And I hit "Save"
    Then I should see "Date of the event could not be parsed as a valid date and time"

    # Creating an Event content item
    When I go to "Admin/Contents/Create/Event"
    Then I should see "Date of the event"
    When I fill in 
            | name                        | value      |
            | Event.EventDate.Editor.Date | 01/31/2012 |
        And I fill in 
            | name                        | value    |
            | Event.EventDate.Editor.Time | 12:00 AM |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Event has been created."
    When I go to "Admin/Contents/List"
    Then I should see "Date of the event" 
        And I should see "1/31/2012 12:00"

    # The hint should be displayed
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                 | value                       |
            | Fields[EventDate].DateTimeFieldSettings.Hint | Enter the date of the event |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Enter the date of the event"
    
    # Display = DateOnly
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                    | value    |
            | Fields[EventDate].DateTimeFieldSettings.Display | DateOnly |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Event.EventDate.Editor.Date"
        And I should not see "Event.EventDate.Editor.Time"
    
    # Display = TimeOnly
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                    | value    |
            | Fields[EventDate].DateTimeFieldSettings.Display | TimeOnly |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Event.EventDate.Editor.Time"
        And I should not see "Event.EventDate.Editor.Date"

    # Required & Date and Time
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                     | value       |
            | Fields[EventDate].DateTimeFieldSettings.Display  | DateAndTime |
            | Fields[EventDate].DateTimeFieldSettings.Required | true        |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Event.EventDate.Editor.Date"
    When I fill in 
            | name                        | value      |
            | Event.EventDate.Editor.Date | 01/31/2012 |
            | Event.EventDate.Editor.Time | 12:00 AM   |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Event has been created."
    When I go to "Admin/Contents/Create/Event"
     And I fill in 
            | name                        | value      |
            | Event.EventDate.Editor.Date | 01/31/2012 |
        And I hit "Save"
    Then I should see "Date of the event is required."
    When I go to "Admin/Contents/Create/Event"
     And I fill in 
            | name                        | value    |
            | Event.EventDate.Editor.Time | 12:00 AM |
        And I hit "Save"
    Then I should see "Date of the event is required."

    # Required & Date only
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                     | value    |
            | Fields[EventDate].DateTimeFieldSettings.Display  | DateOnly |
            | Fields[EventDate].DateTimeFieldSettings.Required | true     |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Event.EventDate.Editor.Date"
    When  I hit "Save"
    Then I should see "Date of the event is required."

    # Required & Time only
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                     | value    |
            | Fields[EventDate].DateTimeFieldSettings.Display  | TimeOnly |
            | Fields[EventDate].DateTimeFieldSettings.Required | true     |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Event.EventDate.Editor.Date"
    When I hit "Save"
    Then I should see "Date of the event is required."

    # The default value should be proposed on creation
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                        | value       |
            | Fields[EventDate].DateTimeFieldSettings.Display     | DateAndTime |
            | Fields[EventDate].DateTimeFieldSettings.Editor.Date | 01/31/2016  |
            | Fields[EventDate].DateTimeFieldSettings.Editor.Time | 10:00 AM    |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Event.EventDate.Editor.Date"
    When I hit "Save"
        And I am redirected
    Then I should see "Your Event has been created."
    When I go to "Admin/Contents/List"
    Then I should see "Date of the event" 
        And I should see "1/31/2016 10:00"

Scenario: Creating and using date time fields in another culture

    # Creating an Event content type 
    Given I have installed Orchard
        And I have installed "Orchard.Fields"
        And I have the file "Content\orchard.core.po" in "Core\App_Data\Localization\fr-FR\orchard.core.po"
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
    
    # Adding a Date field
    When I go to "Admin/ContentTypes/Edit/Event"
        And I follow "Add Field"
        And I fill in
            | name          | value             |
            | DisplayName   | Date of the event |
            | Name          | EventDate         |
            | FieldTypeName | DateTimeField     |
        And I hit "Save"
        And I am redirected
    Then I should see "The \"Date of the event\" field has been added."

    # Date & Time are inputted based on current culture
    When I have "fr-FR" as the default culture
        And I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                     | value       |
            | Fields[EventDate].DateTimeFieldSettings.Display  | DateAndTime |
            | Fields[EventDate].DateTimeFieldSettings.Required | true        |
        And I hit "Save"
    When I go to "Admin/Contents/Create/Event"
        And I fill in 
            | name                        | value      |
            | Event.EventDate.Editor.Date | 01/31/2012 |
            | Event.EventDate.Editor.Time | 12:00 AM   |
        And I hit "Save"
    Then I should see "Date of the event could not be parsed as a valid date and time"
    When I go to "Admin/Contents/Create/Event"
        And I fill in 
            | name                        | value      |
            | Event.EventDate.Editor.Date | 31/01/2012 |
            | Event.EventDate.Editor.Time | 18:00      |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Event has been created."