# Localization

The game is translated into English, Russian, French, Italian, German and Spanish with
[Unity Localization](https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/index.html).

## Where texts live

`Localization/Strings.xlsx` is the source of every text. It has one sheet with the columns
`Key | Comment | en | ru | fr | it | de | es`. Unity keeps a copy in the `UI` string table
(`Assets/Localization/Tables`), made by the importer; do not edit that table by hand.

To change texts:

1. Edit `Localization/Strings.xlsx` in Excel, Numbers or LibreOffice and save it as .xlsx.
2. In Unity choose **FivesGame > Localization > Import from Excel**.
3. Commit the spreadsheet together with the changed table assets.

The importer (`Fives.Editor.Localization.ExcelStringImporter`) reads the .xlsx directly and
has no third-party dependencies. It overwrites every value and removes keys that are no longer
in the sheet. `{0}`, `{1}` are `string.Format` placeholders filled by the code.

## Keys

- Static labels in prefabs use `LocalizeStringEvent` with a key, for example `common.play`.
- The shared header button switches between Settings and Back: each `HeaderCommonBtnSkins` entry holds
  a `TextKey`, applied with `LocalizeStringEvent.SetEntry`.
- Texts built in code use `ITexts.Get(key, args)`; their keys are in `TextKeys`.
- Theme and puzzle texts follow their stable ids: `theme.<id>`, `puzzle.<id>` and
  `puzzle.<id>.about`. `ThemeConfig.ThemeName` and `PuzzleData.Name` only migrate old saves.

## Language choice

`LanguageService` stores the chosen code with the player data. Without a choice it follows the
device language when supported, otherwise English. `LocalizedTexts` applies it to
`LocalizationSettings.SelectedLocale`. All locales are preloaded, so switching is immediate,
and `GameStartup` starts the game only after localization has initialized (WebGL cannot wait synchronously).

## Font

All texts must use characters present in `Assets/Content/Fonts/Lilita One SDF.asset`.
Its source font has no `Ё/ё`, so Russian texts use `Е/е`; use `-` instead of dashes and a
straight apostrophe. A test fails when the sheet contains a character the font lacks.

## Checks

`LocalizationTests` (EditMode) verify that every key has text in every language, the sheet covers
all keys used by code and content, prefab labels point to existing keys, the imported table
matches the sheet, the reader handles Excel's shared strings, and switching the locale changes the text.

French, Italian, German and Spanish are first drafts marked in the Comment column; they need a
check by a native speaker.
