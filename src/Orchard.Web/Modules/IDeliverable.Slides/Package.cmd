..\..\..\..\..\IDeliverable\Packaging\ILMerge\ILMerge.exe /v4 /ndebug /out:bin\IDeliverable.Slides.dll bin\IDeliverable.Slides.dll bin\IDeliverable.Licensing.Orchard.dll bin\IDeliverable.Licensing.dll

del /q bin\IDeliverable.Licensing.dll
del /q bin\IDeliverable.Licensing.Orchard.dll

..\..\..\..\..\IDeliverable\Packaging\Confuser\Confuser.CLI.exe Confuser.crproj

for %%i in (bin\*.*) do if not "%%i"=="bin\IDeliverable.Slides.dll" del /q "%%i"

rmdir /s /q Properties

del /q Confuser.crproj
del /q Package.cmd