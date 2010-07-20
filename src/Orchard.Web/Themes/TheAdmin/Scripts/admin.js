$(document).ready(function(){
$("#navigation li span").click(function() {
$(this).next().next().slideToggle(400);
return false;
 });
}); 
