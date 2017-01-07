# jQuery NiftyModals
This is a jQuery plugin version of the [Codrops ModalWindowEffects](https://github.com/codrops/ModalWindowEffects).

## Getting Started

*  Install with bower: `bower install foxythemes-niftymodals`
*  Or download the [latest release](https://github.com/foxythemes/jquery-niftymodals/releases/latest) zip file

### Add script and style
Include css & js file in your html head:
```html
<link rel="stylesheet" type="text/css" href="[your path]/dist/jquery.niftymodals.css">
<script src="[your path]/dist/jquery.niftymodals.js"></script>
```

### Modal Markup
Modals have a required markup in order to apply the necessary styles:
```html
<div class="md-container md-effect-1" id="your-modal-id">
	<div class="md-content">
		<!-- your content -->
	</div>
</div>
<div class="md-overlay"></div>
```
You'll need to create the modal overlay just after the modal element: `<div class="md-overlay"></div>`

### Calling modal by tag attribute
You can use this option by adding the `md-trigger` class to an element, for example a button or link. You also need to specify the modal id using the `data-modal="modal-id"` data attribute like this:
```html
<a class="md-trigger" data-modal="modal-id">Show Modal</a>
```

### Show modal using JavaScript
This option only requires the basic modal markup and the you can show the modal like this:
```javascript
$('#button-id').on('click',function(){
  $('#modal-id').niftyModal();
});
```

To use this method you'll be able to set some additional options and apply them to the modal:
```javascript
$('#modal-id').niftyModal({
    overlaySelector: '.md-overlay',//Modal overlay class
    closeSelector: '.md-close',//Modal close element class
    classAddAfterOpen: 'md-show',//Body control class
    //This object will be available in the modal events
    data: {
      some_data: ''
    },
    //This option allow to attach a callback to a button with the class 'md-close'
    buttons: [
      {
        class: 'btn-ok',
        callback: function ( btn, modal, event ) {
          //You can cancel the modal hide event by returning false
          alert("You need to check your info!");
          return false;
        }
      },
      {
        class: 'btn-cancel',
        callback: function ( btn, modal, event ) {
          //You can access to the mocal data here
          var modal_data = modal.data.some_data;
        }
      }
    ],
    beforeOpen: function( modal ){
      //You can cancel the modal show event by returning false
    },
    afterOpen: function( modal ){
      //Executed after show event
    },
    beforeClose: function( modal ){
      //You can cancel the hide event by returning false
    },
    afterClose: function( modal ){
      //Executed after hide event
    }
});
```

### Setting defaults
You can set the default options like this:
```javascript
$.fn.niftyModal('setDefaults',{
	overlaySelector: '.modal-overlay',
	closeSelector: '.modal-close',
	classAddAfterOpen: 'modal-show',
});
```

### Integrate with bootstrap modals style
In order to integrate with bs modals style you'll need to load the css bootstrap adapter instead of the default one:
```html
<link rel="stylesheet" type="text/css" href="[your path]/dist/jquery.niftymodals.bootstrap.css">
```
This file contains only the essential css, this way you can use the bs classes like `modal-header`, `modal-content` & `modal-footer`.

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

# License
http://www.codrops.com

Integrate or build upon it for free in your personal or commercial projects. Please contact us first if you want to publish or sell ports (for example WordPress or Joomla plugins). Don't republish, redistribute or sell it "as-is". 

Read more here: http://tympanus.net/codrops/licensing/


