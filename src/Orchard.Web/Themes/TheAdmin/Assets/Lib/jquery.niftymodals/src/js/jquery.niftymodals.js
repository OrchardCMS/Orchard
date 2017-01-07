;(function($) {
  'use strict';

  var defaults = {
    closeSelector: '.md-close',
    contentSelector: '.md-content',
    classAddAfterOpen: 'md-show',
    classScrollbarMeasure: 'md-scrollbar-measure',
    classModalOpen: 'md-open',
    data: false,
    buttons: false,
    beforeOpen: false,
    afterOpen: false,
    beforeClose: false,
    afterClose: false
  };

  $.fn.niftyModal = function(method) {

    var config = {};
    var modal = {};
    var body = $('body');
    var bodyIsOverflowing, scrollbarWidth, originalBodyPad;

    var helpers = {

      removeModal: function( m ) {
        var mod = $( m );
        mod.removeClass( config.classAddAfterOpen );
        mod.css({'perspective':'1300px'});
        //Remove body open modal class
        body.removeClass(config.classModalOpen);
        //Reset body scrollbar padding
        helpers.resetScrollbar();
        mod.trigger('hide');
      },    
      showModal: function( m ) {
        var mod = $(m);
        var close = $(config.closeSelector, m);

        //beforeOpen event
        if( typeof config.beforeOpen === 'function' ){
          if( config.beforeOpen( modal ) === false){
            return false;
          }
        }

        //Calculate scrollbar width
        helpers.checkScrollbar();
        helpers.setScrollbar();

        //Make the modal visible
        mod.addClass( config.classAddAfterOpen );

        //Add body open modal class
        body.addClass( config.classModalOpen );

        //Close on click outside the modal
        $( mod ).on('click', function ( e ) {
          
          var _mContent = $(config.contentSelector, mod);
          var close = $(config.closeSelector, mod);

          if ( !$( e.target ).closest( _mContent ).length && body.hasClass( config.classModalOpen ) ) {

            //Before close event
            if( typeof config.beforeClose === 'function' ){
              if( config.beforeClose(modal, e) === false ){
                return false;
              }
            }
            
            helpers.removeModal(mod);
            close.off('click');

            //After close event
            if( typeof config.afterClose === 'function' ){
              config.afterClose(modal, e);
            }
          }
        });

        //After open event
        if( typeof config.afterOpen === 'function' ){
          config.afterOpen( modal );
        }

        setTimeout( function() {
          mod.css({'perspective':'none'});
        }, 500 ); 
        
        //Close Event
        close.on( 'click', function( ev ) {
          var btn = $(this);
          modal.closeEl = close.get( 0 );

          //Before close event
          if( typeof config.beforeClose === 'function' ){
            if( config.beforeClose(modal, ev) === false ){
              return false;
            }
          }  

          //Buttons callback
          if( config.buttons && $.isArray( config.buttons ) ){
            var cancel = true;
            
            $.each(config.buttons, function( i, v){
              if( btn.hasClass( v.class ) && typeof v.callback === 'function' ){
                cancel = v.callback( btn.get( 0 ), modal, ev);
              }
            });

            if( cancel === false && typeof cancel !== undefined ){
              return false;
            }
          }  
        
          helpers.removeModal( m );
          close.off('click');

          //After close event
          if( typeof config.afterClose === 'function' ){
            config.afterClose( modal, ev);
          }

          ev.stopPropagation();
        });
        
        mod.trigger('show');
      },
      measureScrollbar: function() {
        var scrollDiv = document.createElement('div');
        scrollDiv.className = config.classScrollbarMeasure;
        body.append(scrollDiv);
        var scrollbarWidth = scrollDiv.offsetWidth - scrollDiv.clientWidth;
        body[0].removeChild(scrollDiv);
        return scrollbarWidth;
      },
      checkScrollbar: function() {
        var fullWindowWidth = window.innerWidth;
        if (!fullWindowWidth) { // workaround for missing window.innerWidth in IE8
          var documentElementRect = document.documentElement.getBoundingClientRect();
          fullWindowWidth = documentElementRect.right - Math.abs(documentElementRect.left);
        }
        bodyIsOverflowing = document.body.clientWidth < fullWindowWidth;
        scrollbarWidth = helpers.measureScrollbar();
      },
      setScrollbar: function() {
        var bodyPad = parseInt((body.css('padding-right') || 0), 10);
        originalBodyPad = document.body.style.paddingRight || '';
        if (bodyIsOverflowing){
          body.css('padding-right', bodyPad + scrollbarWidth);
        }
      },
      resetScrollbar: function() {
        body.css('padding-right', originalBodyPad);
      }

    };

    var methods = {

      init : function( options ) {

        return this.each(function() {
          config = $.extend({}, defaults, options);

          modal.modalEl = this;

          if( config.data !== false ){
            modal.data = options.data;
          }

          //Show modal
          helpers.showModal( this );
        });

      },
      toggle: function(options) {
        return this.each(function() {
          config = $.extend({}, defaults, options);
          var modal = $(this);
          if(modal.hasClass(config.classAddAfterOpen)){
            helpers.removeModal(modal);
          }else{
            helpers.showModal(modal);
          }
        });
      },
      show: function(options) {
        config = $.extend({}, defaults, options);
        return this.each(function() {

          var mod = $( this );

          //Show the modal
          helpers.showModal( mod );
          
        });
      },
      hide: function(options) {
        config = $.extend({}, defaults, options);
        return this.each(function() {
          helpers.removeModal($(this));  
        });            
      },
      setDefaults: function( options ) {
        defaults = $.extend({}, defaults, options);
      },
      getDefaults: function( ) {
        return defaults;
      }
    };

    if (methods[method]) {
        return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
    } else if (typeof method === 'object' || !method) {
        return methods.init.apply(this, arguments);
    } else {
        $.error( 'Method "' +  method + '" does not exist in niftyModal plugin!');
    }

  };

})(jQuery);

/**
 * Self execute to support previous versions with 'md-trigger' class & data-modal attribute
 */

$('.md-trigger').on('click',function(){
  var modal = $(this).data('modal');
  $("#" + modal).niftyModal();
});