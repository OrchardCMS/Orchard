﻿Feature: Enumeration Field
    In order to add a list of elements to my types
    As an administrator
    I want to create, edit and publish Enumeration fields

Scenario: Creating and using Enumeration fields
    
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
    
    # Adding a Enumeration field
    When I go to "Admin/ContentTypes/Edit/Event"
        And I follow "Add Field"
        And I fill in
            | name          | value            |
            | DisplayName   | Location         |
            | Name          | Location         |
            | FieldTypeName | EnumerationField |
        And I hit "Save"
        And I am redirected
    Then I should see "The \"Location\" field has been added."

    # Specifying Options
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                       | value   |
            | Fields[0].EnumerationFieldSettings.Options | Seattle |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "<option>Seattle</option>"

    # Creating an Event content item
    When I go to "Admin/Contents/Create/Event"
    Then I should see "Location"
    When I fill in 
            | name                 | value   |
            | Event.Location.Value | Seattle |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Event has been created."
    When I go to "Admin/Contents/List"
    Then I should see "Location:" 
        And I should see "Seattle"

    # The hint should be displayed
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                    | value                    |
            | Fields[0].EnumerationFieldSettings.Hint | Please select a location |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Please select a location"

    # The List Mode Dropdown
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                        | value    |
            | Fields[0].EnumerationFieldSettings.ListMode | Dropdown |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "select id=\"Event_Location_Value\" name=\"Event.Location.Value\""
    
    # The List Mode Radiobutton
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                        | value       |
            | Fields[0].EnumerationFieldSettings.ListMode | Radiobutton |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "input id=\"Event_Location_Value\" name=\"Event.Location.Value\" type=\"radio\""
    
    # The List Mode Listbox
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                        | value   |
            | Fields[0].EnumerationFieldSettings.ListMode | Listbox |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "select id=\"Event_Location_SelectedValues\" multiple=\"multiple\" name=\"Event.Location.SelectedValues\""
    
    # The List Mode Checkbox
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                        | value    |
            | Fields[0].EnumerationFieldSettings.ListMode | Checkbox |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "input type=\"checkbox\" name=\"Event.Location.SelectedValues\""
    
    # The value should be required
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                        | value |
            | Fields[0].EnumerationFieldSettings.Required | true  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
        And I hit "Save"
    Then I should see "The field Location is mandatory."
    
    # The default value should be used on creation
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                            | value    |
            | Fields[0].EnumerationFieldSettings.Options      | Seattle  |
            | Fields[0].EnumerationFieldSettings.ListMode     | Dropdown |
            | Fields[0].EnumerationFieldSettings.DefaultValue | Seattle  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "selected=\"selected">Seattle"
    
    # If not required and no value, the default value should be used
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                            | value          |
            | Fields[0].EnumerationFieldSettings.Required     | false          |
            | Fields[0].EnumerationFieldSettings.Options      | Boston         |
            | Fields[0].EnumerationFieldSettings.ListMode     | Checkbox       |
            | Fields[0].EnumerationFieldSettings.DefaultValue | foo;Boston;bar |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Location"
    When I fill in 
            | name                          | value |
            | Event.Location.SelectedValues |       |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Event has been created."
    When I go to "Admin/Contents/List"
    Then I should see "Location:" 
        And I should see "Boston"
    
    # If required and no value, the default value should be used
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                            | value           |
            | Fields[0].EnumerationFieldSettings.Required     | true            |
            | Fields[0].EnumerationFieldSettings.Options      | Phoenix         |
            | Fields[0].EnumerationFieldSettings.ListMode     | Checkbox        |
            | Fields[0].EnumerationFieldSettings.DefaultValue | foo;Phoenix;bar |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Location"
    When I fill in 
            | name                          | value |
            | Event.Location.SelectedValues |       |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Event has been created."
    When I go to "Admin/Contents/List"
    Then I should see "Location:" 
        And I should see "Phoenix"

    # If required and the default value is not valid, the value should be required
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                            | value    |
            | Fields[0].EnumerationFieldSettings.Required     | true     |
            | Fields[0].EnumerationFieldSettings.Options      | Phoenix  |
            | Fields[0].EnumerationFieldSettings.ListMode     | Checkbox |
            | Fields[0].EnumerationFieldSettings.DefaultValue | foo;bar  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Location"
    When I fill in 
            | name                          | value |
            | Event.Location.SelectedValues |       |
        And I hit "Save"
    Then I should see "The field Location is mandatory."
	
    # If required and no default value, the list box should have the required attribute
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                            | value   |
            | Fields[0].EnumerationFieldSettings.Required     | true    |
            | Fields[0].EnumerationFieldSettings.ListMode     | Listbox |
            | Fields[0].EnumerationFieldSettings.DefaultValue |         |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "required=\"required\""
	
    # If required and a default value is set, the list box should not have the required attribute
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                            | value   |
            | Fields[0].EnumerationFieldSettings.Required     | true    |
            | Fields[0].EnumerationFieldSettings.ListMode     | Listbox |
            | Fields[0].EnumerationFieldSettings.DefaultValue | Phoenix |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should not see "required=\"required\""
