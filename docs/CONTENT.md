# Remote content

Puzzle pictures are Addressables bundles, one group per theme. The theme groups are **remote**: they are built
into `ServerData/<platform>` and loaded from GitHub Pages, next to the WebGL site:

```
https://vircatodev.github.io/FivesGame/content/Android/
https://vircatodev.github.io/FivesGame/content/WebGL/
```

The theme previews stay **local**, inside the player, so the loading screen and the menus work without a network.
The catalog is remote too: a published catalog change reaches installed players without an app update.

## At runtime

- On start Addressables checks the remote catalog (5 s timeout). Offline, the cached or built-in catalog is used.
- The loading screen downloads every theme bundle that is not on the device yet, so the menus never wait for one.
- A game started offline still runs with the themes already downloaded. When a menu opens one that is missing,
  `ThemeDownloadGate` brings the loading screen back with the download progress; a failed download offers **Retry** or
  **Back**.
- Downloaded bundles stay in the Unity cache (IndexedDB in a browser), so a theme downloads once.
- In the editor, **FivesGame > Content** picks where play mode takes the pictures from (a choice kept in this machine's
  `Library`, not in git):
  - **Play Mode: Download From Server** plays the content built for the active platform and downloads the theme
    bundles from GitHub Pages, as a build does;
  - **Play Mode: Asset Database** reads the project's assets directly, with no network;
  - **Build Content** rebuilds the bundles for the active platform, needed after the pictures change;
  - **Clear Downloaded Content** forgets the downloaded bundles and the cached catalog, so the next play downloads them
    again.

## Releasing

The game and the content are published separately and never overwrite each other: the game sits at the root of the
site, the content under `content/`.

A player build (`tools/unity.sh apk`, `tools/unity.sh webgl`) also builds the content for its platform and saves the
content state in `Assets/AddressableAssetsData/<platform>/` (ignored by git, kept on the release machine).

```bash
tools/unity.sh webgl             # the game in Builds/WebGL, its content in ServerData/WebGL
tools/unity.sh publish           # the game only
tools/unity.sh publish-content   # the content only
```

A player needs its content online: publish the content of every player build that is released.

The catalog is named after the app version (`catalog_0.1.0.bin`): each released version reads its own catalog. Raise
the version in Player Settings for every release, or the new catalog replaces the one installed players read.

To build and upload content without a player, for the first upload or to try real downloads in the editor:

```bash
tools/unity.sh content-build webgl
tools/unity.sh publish-content
```

Then choose **FivesGame > Content > Play Mode: Download From Server**: the editor loads the theme bundles from
GitHub Pages as a build does.

## Updating content without an app update

Change pictures in the theme groups, then build an update against the released player's content state:

```bash
tools/unity.sh content android
tools/unity.sh publish-content
```

Publishing only adds files under `content/`: players of older builds keep loading their own catalogs and bundles.
Local groups (the previews) cannot change in a content update; they need a new player build.

## Why GitHub Pages

Unity Cloud Content Delivery needs a payment card. Addressables only needs a URL, so the remote load path points to
GitHub Pages: free, HTTPS, and on the same origin as the WebGL build, so browsers need no CORS setup.
