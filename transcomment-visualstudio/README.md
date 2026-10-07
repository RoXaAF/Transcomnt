# TransComment for Visual Studio

TransComment is a Visual Studio 2022 extension.

## Commands

- `Ctrl+Shift+T, R`: transform all existing comments in the active file by adding `transcomment:`.
- `Ctrl+Alt+T`: insert a `transcomment:` comment at the cursor or before selected text.
- Tools > Hide/Show Comments
- Tools > Add TransComment
- Tools > Transform All Comments

## Important

The hide/show command is visual only. It does not delete comments and it does not rewrite your file.
The transform command does rewrite the active file, adding the `transcomment:` marker to comments that do not already have it.

## Build

Open `TransComment.VisualStudio.csproj` in Visual Studio 2022 with the **Visual Studio extension development** workload installed, then build the project.

The generated `.vsix` can be installed by double-clicking it.

## Notes

This project targets Visual Studio 2022 (`17.x`). Visual Studio has stronger extension requirements than VS Code, so the VSIX must be built with the Visual Studio SDK installed.
