Feature: Tags
    In order to add tags to my content
    As an author
    I want to create, publish and edit pages

Scenario: I can add a tag to a new Page
    Given I have installed Orchard
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Title.Title | Super Duper |
            | LayoutPart.LayoutEditor.Data | { "elements": [ { "typeName": "Orchard.Layouts.Elements.Text", "state": "Content=This+is+super."} ] } |
            | Tags.Tags | Foo, Bar |
        And I hit "Publish Now"
        And I go to "super-duper"
    Then I should see "<h1[^>]*>.*?Super Duper.*?</h1>"
        And I should see "Foo"
        And I should see "Bar"

Scenario: I can't add a tag with disallowed chars to a new Page
    Given I have installed Orchard
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Title.Title | Super Duper |
            | LayoutPart.LayoutEditor.Data | { "elements": [ { "typeName": "Orchard.Layouts.Elements.Text", "state": "Content=This+is+super."} ] } |
            | Tags.Tags | Foo, I <3 Orchard |
        And I hit "Publish Now"
        And I am redirected
    Then I should see "forbidden chars"
