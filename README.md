# godot_idemessage_cs

Minimal lsp server for godot ide message for c#
provides NodePath Completion, ScenePath Completion, ResourcePath Completion, InputAction Completion etc.

## Requirements

- [.NET Runtime](https://dotnet.microsoft.com/download) 8.0 or later

## Setup with nvim 

```lua
vim.lsp.config.godot_node = {
    cmd = {
        "path/to/exe",
    },
    root_markers = { "project.godot" },
    filetypes = { "cs" }
}
vim.lsp.enable({ "godot_node" })
```

