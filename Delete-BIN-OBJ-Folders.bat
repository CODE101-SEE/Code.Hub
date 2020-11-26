@ECHO off
cls

ECHO Deleting all BIN OBJ, PKG folders...
ECHO.

FOR /d /r . %%d in (bin,obj,pkg) DO (
	IF EXIST "%%d" (		 	 
		ECHO %%d | FIND /I "\node_modules\" > Nul && ( 
			ECHO.Skipping: %%d
		) || (
			ECHO.Deleting: %%d
			rd /s/q "%%d"
		)
	)
)

ECHO.
ECHO.BIN,OBJ,PKG folders have been successfully deleted. Press any key to exit.
pause > nul