# Musiko

A lightweight, translucent MP3 player for Windows.

## Install on Windows

1. Double-click `Musiko.exe` to open the player in its own Microsoft Edge app window. The EXE includes the player page; if copied without `index.html`, it extracts the page into your local app data folder.
2. Alternatively, double-click `Install-Musiko.bat` to create Desktop and Start Menu shortcuts.


You can also double-click `Run-Musiko.bat` to launch without shortcuts. Microsoft Edge must be installed. To rebuild the EXE after changing the player, run `Build-Musiko-EXE.bat`.

Select **Add music** or drag audio files onto the window. Musiko reads files locally; it does not upload them.

## Player controls

- Play or pause with the center button or the Space key
- Skip tracks, shuffle, repeat, and adjust volume
- Click the progress bar to seek; use the left and right arrow keys to move by five seconds
- Remove a track with the × button shown when hovering over it
- Open the immersive full-screen player with the expand button; press Escape to close it
- MP3 title, artist, album, and embedded ID3 cover artwork are picked up automatically when available
- Open the lyrics preview from the player. It reads embedded MP3 lyrics or a matching `.lrc` file; when timed local lyrics are missing, playback automatically looks them up online
- Search by title, artist, or album; press `/` to jump to search
- Collapse the sidebar to an icon rail with the button next to the logo, or press Ctrl+B (Cmd+B on Mac); Musiko remembers your choice
- Save tracks with the heart button and create playlists with the + button in the sidebar
- Imported tracks, favorites, and playlists are stored in this browser on this PC

To add synced lyrics, select the audio track and a same-named `.lrc` file together (for example, `Song.mp3` and `Song.lrc`). Lyrics embedded in MP3 tags are picked up automatically.

Automatic synced-lyrics lookup uses the track title, artist, album, and duration with LRCLIB. These track details are sent to LRCLIB; matched lyrics are saved in Musiko's local library for later playback. An internet connection and usable artist/title tags are required.

The translucent frosted-glass effect is drawn over Musiko's own background. The Windows app window is a standalone Edge app window; Windows desktop content does not show through it.
