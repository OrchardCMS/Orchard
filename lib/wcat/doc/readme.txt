Getting Started with WCAT
======================================

Documentation
    It is suggested that you read the documentation for WCAT in order to understand
    the environment and configuration needed.  Documentation can be found under the
    doc folder in the directory that you installed WCAT to.

Installing WCAT
    Prerequisite: all machines that will run WCAT must have the administrator account
    enabled and must all share a common password.

    1.  Log into the WCAT Controller as administrator.
    2.  Install wcat.msi on the machine to be used as a WCAT Controller.
    3.  If any WCAT extension DLLs will be used, copy them to the WCAT installation
        directory. (typically c:\Program Files\WCAT)
    4.  Open a command prompt; navigate to the WCAT installation directory.
    5.  Run ‘cscript //H:Cscript’
    6.  Run ‘wcat.wsf –terminate –update –clients {comma separated list of WCAT client
        machines}’ where the “clients” parameter accepts a comma separated list (no
        spaces) of the machines you plan on using as WCAT Client machines.  Note: If
        the WCAT Controller machine will also be used as a WCAT Client, include
        ‘localhost’ OR the name of the WCAT Controller machine in the list of clients.

        NOTE: If WCAT has never been installed on the WCAT Client machines before,
        this will cause the machines to reboot.

Running WCAT
    1.  Log into the WCAT Controller as administrator.
    2.  Install wcat.msi on the machine to be used as a WCAT Controller.
    3.  Open a command prompt, navigate to the WCAT installation directory (typically
        c:\Program Files\WCAT)
    4.  Run ‘wcat.wsf –terminate –run –clients {comma separated client list} –t
        {scenario file} –f {settings file} –s {name of the Web Server} –singleip -x
    5.  Output will be generated in the current directory, ‘log.xml’.  To change this,
        use the ‘-o’ flag.  For more help on options to pass to wcat.wsf type
        ‘wcctl.exe -?’
