$(document).ready(function(){
$("#navigation li span").click(function() {
$(this).next().next().slideToggle(600);
return false;
 });
}); 
