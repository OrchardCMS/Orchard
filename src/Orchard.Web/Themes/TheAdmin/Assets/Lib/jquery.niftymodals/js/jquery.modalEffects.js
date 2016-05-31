//Button Trigger
(function( $ ){
  var $this = new Object();    
  var methods = {
     init : function( options ) {      
        $this =  $.extend({}, this, methods); 
        $this.searching = false;
        $this.o = new Object();
  
        var defaultOptions = {
           overlaySelector: '.md-overlay',
           closeSelector: '.md-close',
           classAddAfterOpen: 'md-show',
           modalAttr: 'data-modal',
           perspectiveClass: 'md-perspective',
           perspectiveSetClass: 'md-setperspective',
           afterOpen: function(button, modal) {
            //do your stuff
           },
           afterClose: function(button, modal) {
            //do your suff
           }
        };
        
        $this.o = $.extend({}, defaultOptions, options);
        $this.n = new Object();
       
       
        var overlay = $($this.o.overlaySelector);
        $(this).click(function() {
        	var modal = $('#' + $(this).attr($this.o.modalAttr)),
        		close = $($this.o.closeSelector, modal);
          var el = $(this);       	
      		$(modal).addClass($this.o.classAddAfterOpen);
      		/* overlay.removeEventListener( 'click', removeModalHandler );
      		overlay.addEventListener( 'click', removeModalHandler ); */
          $(overlay).on('click', function () {
             removeModalHandler();
             $this.afterClose(el, modal);
             $(overlay).off('click');
          });
      		if( $(el).hasClass($this.o.perspectiveSetClass) ) {
      			setTimeout( function() {
      				$(document.documentElement).addClass($this.o.perspectiveClass);
      			}, 25 );
      		}
          
          $this.afterOpen(el, modal);
        	setTimeout( function() {
            modal.css({'perspective':'none'});
            
            //3D Blur Bug Fix
            if(modal.height() % 2 != 0){modal.css({'height':modal.height() + 1});}
            
          }, 500 ); 
          
        	function removeModal( hasPerspective ) {
        		$(modal).removeClass($this.o.classAddAfterOpen);
             modal.css({'perspective':'1300px'});
        		if( hasPerspective ) {
        			$(document.documentElement).removeClass($this.o.perspectiveClass);
        		}
        	}
        
        	function removeModalHandler() {
        		removeModal($(el).hasClass($this.o.perspectiveSetClass)); 
        	}
        
        	$(close).on( 'click', function( ev ) {
        		ev.stopPropagation();
        		removeModalHandler();
            $this.afterClose(el, modal);
        	});
        
        });
       
     },
     afterOpen: function (button, modal) {
        $this.o.afterOpen(button, modal);
     },
     afterClose: function (button, modal) {
        $this.o.afterClose(button, modal);  
     }
  };

  $.fn.modalEffects = function( method ) {
    if ( methods[method] ) {
      return methods[method].apply( this, Array.prototype.slice.call( arguments, 1 ));
    } else if ( typeof method === 'object' || ! method ) {
      return methods.init.apply( this, arguments );
    } else {
      $.error( 'Method ' +  method + ' does not exist on jQuery.modalEffects' );
    }    
  
  };
	function is_touch_device(){
		return !!("ontouchstart" in window) ? 1 : 0;
	}
})( jQuery );

//Nifty Modal
;(function($) {
 
    $.fn.niftyModal = function(method) {
 
        var defaults = {
           overlaySelector: '.md-overlay',
           closeSelector: '.md-close',
           classAddAfterOpen: 'md-show',
           modalAttr: 'data-modal',
           perspectiveClass: 'md-perspective',
           perspectiveSetClass: 'md-setperspective',
           afterOpen: function(modal) {
            //do your stuff
           },
           afterClose: function(modal) {
            //do your suff
           }
        }
 
        var config = {}
 
        var methods = {
 
          init : function(options) {
              return this.each(function() {
                  config = $.extend({}, defaults, options);
                  var modal = $(this);
                  
                  //Show modal
                  helpers.showModal(modal);
                  
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
              helpers.showModal($(this));
            });
          },

          hide: function(options) {
            config = $.extend({}, defaults, options);
            return this.each(function() {
              helpers.removeModal($(this));  
            });            
          }
        }
 
        var helpers = {
 
          removeModal: function(mod) {
            mod.removeClass(config.classAddAfterOpen);
            mod.css({'perspective':'1300px'});
            mod.trigger('hide');
          },
          
          showModal: function(mod){
            var overlay = $(config.overlaySelector);
            var close = $(config.closeSelector, mod);
            mod.addClass(config.classAddAfterOpen);
            
            //Overlay Click Event
            overlay.on('click', function (e) {
              var after = config.afterClose(mod, e);
              if( after === undefined || after != false){
                 helpers.removeModal(mod);
                 overlay.off('click');
              }
            });
            
            //Fire after open event
            config.afterOpen(mod);
            setTimeout( function() {
              mod.css({'perspective':'none'});
              
              //3D Blur Bug Fix
              if(mod.height() % 2 != 0){mod.css({'height':modal.height() + 1});}

            }, 500 ); 
            
            //Close Event
            close.on( 'click', function( ev ) {
              var after = config.afterClose(mod, ev);
              if( after === undefined || after != false){
                 helpers.removeModal(mod);
                 overlay.off('click');
              }
              ev.stopPropagation();
            });
            
            mod.trigger('show');
          }
 
        }
 
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        } else {
            $.error( 'Method "' +  method + '" does not exist in niftyModal plugin!');
        }
 
    }
 
})(jQuery);

$(document).ready(function(){
  $(".md-trigger").modalEffects();
});