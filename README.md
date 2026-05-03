# godot_idemessage_cs

Minimal lsp server for godot ide message for c#
provides NodePath Completion, ScenePath Completion, ResourcePath Completion, InputAction Completion etc.

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


## NOTE:

To be honest, this lsp server are too large but I don't know how to make it smaller (I tryed `PublishTrimmed` but it cause error).
If you have any idea, please let me know.

