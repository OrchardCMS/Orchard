<?php

$ds          = DIRECTORY_SEPARATOR;  
 
$storeFolder = 'uploads';
$enable_upload = true;   
 
if ( !empty($_FILES) && $enable_upload ) {
     
    $tempFile = $_FILES['file']['tmp_name'];                 
      
    $targetPath = dirname( __FILE__ ) . $ds. $storeFolder . $ds;  
     
    $targetFile =  $targetPath. $_FILES['file']['name']; 
 
    move_uploaded_file($tempFile,$targetFile); 
     
}

?>