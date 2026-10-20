# Musiko

A lightweight, translucent MP3 player for Windows.

## Install on Windows

1. Keep the Musiko files together in a folder on your computer.
2. Double-click `Install-Musiko.bat` to create Musiko shortcuts on your Desktop and in the Start Menu.
3. Open Musiko from either shortcut. It launches in its own Microsoft Edge app window.

You can also double-click `Run-Musiko.bat` to start it without creating shortcuts. Microsoft Edge must be installed.

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
- Save tracks with the heart button and create playlists with the + button in the sidebar
- Imported tracks, favorites, and playlists are stored in this browser on this PC

To add synced lyrics, select the audio track and a same-named `.lrc` file together (for example, `Song.mp3` and `Song.lrc`). Lyrics embedded in MP3 tags are picked up automatically.

Automatic synced-lyrics lookup uses the track title, artist, album, and duration with LRCLIB. These track details are sent to LRCLIB; matched lyrics are saved in Musiko's local library for later playback. An internet connection and usable artist/title tags are required.

The translucent frosted-glass effect is drawn over Musiko's own background. The Windows app window is a standalone Edge app window; Windows desktop content does not show through it.
