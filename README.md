
# Sideloading Unlocker

![Preview](MetroUnlocker.png "Sideloading Unlocker")

## Unlock signed sideloading on:

- Windows 8.0 and up


## Unlock full sideloading on:

- Windows 8.1 Pro and up

Development sideloading might take a while to activate. If it doesn't happen fast enough, use Product Policy Editor to disable SPPSVC, set the system clock to 2026, try sideloading until it activates, revert the clock and enable SPPSVC again.

Known bug:

If you have 0 bytes free on your disk, it might fail to write the new tokens, leaving the tokens file empty and when SPPSVC starts, it will recreate the file but everything will be deactivated including Windows itself. 

MetroUnlocker always tries to make a backup of your tokens before modifying them. Just make sure you have at least 30MB free before trying to activate sideloading. If something goes wrong, just restore the tokens.dat from the backup to `C:\Windows\System32\spp\store\2.0\`. In worst case you can just reactivate what you want activated again with TSForge.

Feel free to reach out to me or create an issue if you need help.

Huge thanks to [TSForge](https://github.com/massgravel/TSforge) for making this possible by reverse engineering SPPSVC!