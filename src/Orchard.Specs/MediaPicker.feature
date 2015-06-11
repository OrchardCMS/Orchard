Feature: Media Picker Field
    In order to add a media content to my types
	As an administrator
    I want to create, edit and publish media fields

Scenario: Creating and using media fields
	
	# Creating an Event content type 
    Given I have installed Orchard
		And I have installed "Orchard.Media"
		And I have installed "Orchard.MediaPicker"

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
	
	# Adding a media field
	When I go to "Admin/ContentTypes/Edit/Event"
		And I follow "Add Field"
		And I fill in
            | name          | value            |
            | DisplayName   | File             |
            | Name          | File             |
            | FieldTypeName | MediaPickerField |
		And I hit "Save"
		And I am redirected
	Then I should see "The \"File\" field has been added."

	# Creating an Event content item
	When I go to "Admin/Contents/Create/Event"
	Then I should see "File"
	When I fill in 
	        | name           | value |
	        | Event.File.Url |       |
		And I hit "Save"
		And I am redirected
	Then I should see "Your Event has been created."

	# The hint should be displayed
	When I go to "Admin/ContentTypes/Edit/Event"
		And I fill in 
	        | name                                    | value                |
	        | Fields[0].MediaPickerFieldSettings.Hint | Please select a file |
		And I hit "Save"
		And I go to "Admin/Contents/Create/Event"
	Then I should see "Please select a file"

	# The value should be required
	When I go to "Admin/ContentTypes/Edit/Event"
		And I fill in 
	        | name                                        | value |
	        | Fields[0].MediaPickerFieldSettings.Required | true  |
		And I hit "Save"
		And I go to "Admin/Contents/Create/Event"
		And I fill in 
	        | name           | value |
	        | Event.File.Url |       |
		And I hit "Save"
	Then I should see "The field File is mandatory."

	# The value should be bound
	When I go to "Admin/ContentTypes/Edit/Event"
		And I fill in 
	        | name                                                 | value |
	        | ext-Fields[0].MediaPickerFieldSettings               | true  |
	        | Fields[0].MediaPickerFieldSettings.AllowedExtensions | jpg   |
		And I hit "Save"
		And I go to "Admin/Contents/Create/Event"
	And I fill in 
	        | name           | value |
	        | Event.File.Url | ~/Media/Default/images/Image.png   |
		And I hit "Save"
	Then I should see "The field File must have one of these extensions: jpg"