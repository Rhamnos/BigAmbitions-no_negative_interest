No Negative Interest - Big Ambitions mod
=========================================
Removes the weekly negative interest fee the bank charges on large balances.
Original game files are NOT modified. Uninstall by deleting the BepInEx folder
and winhttp.dll from your Big Ambitions directory.

INSTALL
-------
Linux:
  1. Run:  bash install.sh
  2. In Steam, right-click Big Ambitions → Properties → Launch Options, paste:
         WINEDLLOVERRIDES="winhttp=n,b" %command%

Windows:
  1. Right-click install.ps1 → Run with PowerShell
     (if blocked, run:  powershell -ExecutionPolicy Bypass -File install.ps1)
  2. No Steam launch option needed on Windows.

VERIFY
------
After launching the game, open:
  Big Ambitions/BepInEx/LogOutput.log

You should see:
  [Info :No Negative Interest] All patches applied — bank will never charge negative interest.

If it says "skipped because the game was updated", the mod needs an update for the
new game version — the old RVAs shifted and it refused to patch unknown code.

UNINSTALL
---------
Delete from your Big Ambitions folder:
  - BepInEx/
  - winhttp.dll
  - doorstop_config.ini
  - dotnet/
  - .doorstop_version
Then clear the Steam launch option.
