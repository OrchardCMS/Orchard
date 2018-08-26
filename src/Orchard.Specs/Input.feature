Feature: Input Field
    In order to add an input to my types
    As an administrator
    I want to create, edit and publish input fields

Scenario: Creating and using Input fields
    
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
    
    # Adding a Input field
    When I go to "Admin/ContentTypes/Edit/Event"
        And I follow "Add Field"
        And I fill in
            | name          | value      |
            | DisplayName   | Contact    |
            | Name          | Contact    |
            | FieldTypeName | InputField |
        And I hit "Save"
        And I am redirected
    Then I should see "The \"Contact\" field has been added."

    # The hint should be displayed
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                              | value                           |
            | Fields[Contact].InputFieldSettings.Hint | Enter the contact email address |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Enter the contact email address"
    
    # The pattern should be effective
    #When I go to "Admin/ContentTypes/Edit/Event"
    #	And I fill in 
    #        | name                                 | value       |
    #        | Fields[Contact].InputFieldSettings.Pattern | [^@]*@[^@]* |
    #	And I hit "Save"
    #	And I go to "Admin/Contents/Create/Event"
    #Then I should see "pattern=\"[^@]*@[^@]*\""
    
    # The input type should be effective
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                              | value   |
            | Fields[Contact].InputFieldSettings.Type | Email   |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "type=\"email\""
    
    # The title should be displayed
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                               | value                  |
            | Fields[Contact].InputFieldSettings.Title | Enter an email address |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "title=\"Enter an email address\""
    
    # The auto focus should be effective
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                   | value |
            | Fields[Contact].InputFieldSettings.AutoFocus | true  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "autofocus=\"autofocus\""
    
    # The auto complete should be effective
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                      | value |
            | Fields[Contact].InputFieldSettings.AutoComplete | true  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "autocomplete=\"on\""
    
    # The watermark should be displayed
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                     | value            |
            | Fields[Contact].InputFieldSettings.Placeholder | email@domain.com |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "placeholder=\"email@domain.com\""
    
    # The maxlength should be effective
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                   | value |
            | Fields[Contact].InputFieldSettings.MaxLength | 100   |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "maxlength=\"100\""
    
    # The value should be required
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                  | value |
            | Fields[Contact].InputFieldSettings.Required | true  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
        And I fill in 
            | name                | value |
            | Event.Contact.Value |       |
        And I hit "Save"
    Then I should see "The field Contact is mandatory."
    
    # Creating an Event content item
    When I go to "Admin/Contents/Create/Event"
    Then I should see "Contact"
    When I fill in 
            | name                | value                      |
            | Event.Contact.Value | contact@orchardproject.net |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Event has been created."
    When I go to "Admin/Contents/List"
    Then I should see "Contact:" 
        And I should see "contact@orchardproject.net"

    # The default value should be proposed on creation
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                      | value                       |
            | Fields[Contact].InputFieldSettings.DefaultValue | contact@orchardproject.net |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "value=\"contact@orchardproject.net\""

    # The required attribute should be used
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                      | value |
            | Fields[Contact].InputFieldSettings.Required     | true  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "required=\"required\""

    # The required attribute should not be used
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                      | value |
            | Fields[Contact].InputFieldSettings.Required     | false |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should not see "required=\"required\""