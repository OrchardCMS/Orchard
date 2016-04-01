# Layouts
The Layouts module enables users to visually add layout and content elements to a canvas.

## Snippets
Snippets are elements who are dynamically discovered based on shapes that follow a certain naming convention.
The naming convention is that all shapes ending in "Snippet" are harvested as a snippet element. For example, MyLogoSnippet.cshtml will be registered as an element called "My Logo".

### Parameterized Snippets
In addition to snippet files, you can provide a snippet manifest file for a particular snippet using YAML.
The naming convention of the snippet manifest file is to use the same name as the shape template, but suffixed with .Params.txt. For example: "MyLogoSnippet.Params.txt".

This manifest file declares meta information about your snippet, such as a display name, description, toolbox icon, and most importantly, fields.

Fields declared in the manifest will appear as configurable fields for users when adding the snippet to the canvas.
The snippet shape template itself can then reference these fields and output the user-provided values.

The following is a sample snippet manifest file:

```yaml
DisplayName: My Logo
ToolboxIcon: \uf1c5
Fields:
  - Name: ImageUrl
    Type: Url
    DisplayName: Image URL
    Description: The URL to your logo.

  - Name: Payoff
    Type: Text
    DisplayName: Payoff
    Description: The payoff to display next to your logo.
```
