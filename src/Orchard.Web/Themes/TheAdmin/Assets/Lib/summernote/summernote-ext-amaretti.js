(function (factory) {
  /* global define */
  if (typeof define === 'function' && define.amd) {
    // AMD. Register as an anonymous module.
    define(['jquery'], factory);
  } else {
    // Browser globals: jQuery
    factory(window.jQuery);
  }
}(function ($) {
  // template
  var tmpl = $.summernote.renderer.getTemplate();

  /**
   * @class plugin.amaretti 
   * 
   * Amaretti Plugin  
   */
  $.summernote.addPlugin({

    name: 'amaretti',

    init: function( layoutInfo ){
      var $editor = layoutInfo.editor();
      var $toolbar = layoutInfo.toolbar();

      //Remove the .btn-sm class from toolbar
      $toolbar.find(".btn-sm").removeClass("btn-sm");

    }

  });
}));