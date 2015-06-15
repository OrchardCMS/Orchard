..\..\..\..\..\IDeliverable\Packaging\ILMerge\ILMerge.exe /v4 /ndebug /out:bin\IDeliverable.Slides.dll bin\IDeliverable.Slides.dll bin\IDeliverable.Licensing.Orchard.dll bin\IDeliverable.Licensing.dll

del bin\IDeliverable.Licensing.dll
del bin\IDeliverable.Licensing.Orchard.dll

..\..\..\..\..\IDeliverable\Packaging\Confuser\Confuser.CLI.exe Confuser.crproj