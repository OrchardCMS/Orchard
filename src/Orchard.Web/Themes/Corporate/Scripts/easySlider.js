/*
 * 	Easy Slider - jQuery plugin
 *	written by Alen Grakalic	
 *	http://cssglobe.com/post/3783/jquery-plugin-easy-image-or-content-slider
 *
 *	Copyright (c) 2009 Alen Grakalic (http://cssglobe.com)
 *	Dual licensed under the MIT (MIT-LICENSE.txt)
 *	and GPL (GPL-LICENSE.txt) licenses.
 *
 *	Built for jQuery library
 *	http://jquery.com
 *
 */
 
/*
 *	markup example for $("#images").easySlider();
 *	
 * 	<div id="images">
 *		<ul>
 *			<li><img src="images/01.jpg" alt="" /></li>
 *			<li><img src="images/02.jpg" alt="" /></li>
 *			<li><img src="images/03.jpg" alt="" /></li>
 *			<li><img src="images/04.jpg" alt="" /></li>
 *			<li><img src="images/05.jpg" alt="" /></li>
 *		</ul>
 *	</div>
 *
 */

(function($) {

	$.fn.easySlider = function(options){
	  
		// default configuration properties
		var defaults = {
			prevId: 		'prevBtn',
			prevText: 		'Previous',
			nextId: 		'nextBtn',	
			nextText: 		'Next',
			orientation:	'', //  'vertical' is optional;
			speed: 			800			
		}; 
		
		var options = $.extend(defaults, options);  
		
		return this.each(function() {  
			obj = $(this); 				
			var s = $("li", obj).length;
			var w = obj.width(); 
			var h = obj.height(); 
			var ts = s-1;
			var t = 0;
			var vertical = (options.orientation == 'vertical');
			$("ul", obj).css('width',s*w);			
			if(!vertical) $("li", obj).css('float','left');
			$(obj).after('<span id="'+ options.prevId +'"><a href=\"javascript:void(0);\">'+ options.prevText +'</a></span> <span id="'+ options.nextId +'"><a href=\"javascript:void(0);\">'+ options.nextText +'</a></span>');		
			//$("a","#"+options.prevId).hide();
			//$("a","#"+options.nextId).hide();
			$("a","#"+options.nextId).click(function(){		
				animate("next");
				//if (t>=ts) $(this).fadeOut();
				$("a","#"+options.prevId).fadeIn();
			});
			$("a","#"+options.prevId).click(function(){		
				animate("prev");
				//if (t<=0) $(this).fadeOut();
				$("a","#"+options.nextId).fadeIn();
			});	
			function animate(dir){
				if(dir == "next"){
					t = (t>=ts) ? ts : t+1;	
				} else {
					t = (t<=0) ? 0 : t-1;
				};								
				if(!vertical) {
					p = (t*w*-1);
					$("ul",obj).animate(
						{ marginLeft: p }, 
						options.speed
					);				
				} else {
					p = (t*h*-1);
					$("ul",obj).animate(
						{ marginTop: p }, 
						options.speed
					);					
				}
			};
			if(s>1) $("a","#"+options.nextId).fadeIn();	
		});
	  
	};

})(jQuery);