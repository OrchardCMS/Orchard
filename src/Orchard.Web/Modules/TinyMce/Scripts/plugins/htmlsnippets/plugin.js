
tinymce.PluginManager.add('htmlsnippets', function(editor, url) {
	var htmlSnippetAction = function() {
	  // Open window
	  var modalWindow = editor.windowManager.open({
		title: 'Insert/edit HTML snippets',
		body: [
		  {type: 'textbox', value: editor.selection.getContent(), multiline: true, name: 'html_snippet', label: 'HTML Snippet', minHeight:350}
		],
		height: 400,
		width: 600,
		onsubmit: function(e) {
		  // Insert content when the window form is submitted
		  editor.insertContent(e.data.html_snippet);
		}
	  });
	}

  // Add a button that opens a window
  editor.addButton('htmlsnippetsbutton', {
      image: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABmJLR0QAEABHAPK/xSSIAAAACXBIWXMAAC4jAAAuIwF4pT92AAAAvElEQVRIx+2VsRHCMAxFH3AwAR075CjSpGOfLEU6nAKmoIICJohHYANoVPjAji0nNLn8OxeSvny2vmTDDCUOQOfYb1mptpU9grDKDX12N20Nvq88OH+ZkLQGDFA7vho4SWwQdkADPIHS8ZfiOwonC1s5+R2oPPFKYka46hpegCtQ9PD3wA0452qwiXBWwlnklqhNKFGrKVFI5EdA5EYrsm8OQm1qPG0anaPRB216eI3wXNvYh2P/+eHM+MEHpr9Js1lOQvUAAAAASUVORK5CYII=',
	tooltip: 'Html snippet',
    onclick: htmlSnippetAction
  });

  // Adds a menu item to the tools menu
    editor.addMenuItem('HTMLsnippetmenuitem ', {
    text: 'Html snippet',
      image: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABmJLR0QAEABHAPK/xSSIAAAACXBIWXMAAC4jAAAuIwF4pT92AAAAvElEQVRIx+2VsRHCMAxFH3AwAR075CjSpGOfLEU6nAKmoIICJohHYANoVPjAji0nNLn8OxeSvny2vmTDDCUOQOfYb1mptpU9grDKDX12N20Nvq88OH+ZkLQGDFA7vho4SWwQdkADPIHS8ZfiOwonC1s5+R2oPPFKYka46hpegCtQ9PD3wA0452qwiXBWwlnklqhNKFGrKVFI5EdA5EYrsm8OQm1qPG0anaPRB216eI3wXNvYh2P/+eHM+MEHpr9Js1lOQvUAAAAASUVORK5CYII=',
    context: 'insert',
    onclick: htmlSnippetAction
  });

  return {
    getMetadata: function () {
      return  {
        name: "Html snippets plugin"
        //url: "http://exampleplugindocsurl.com"
      };
    }
  };
});
