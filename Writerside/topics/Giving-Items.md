# Giving Items

<primary-label ref="runtime"/>

<link-summary>
Giving Items in MCCompiled allows for additional attributes like enchantments, item names, lore, and more.
</link-summary>

The `give` command in MCCompiled is almost syntactically the same as Minecraft's default `give` command:
- `give <selector> <item> [count] [data]`

However, the command contains numerous extra features which make it immensely more useful than the Minecraft command.

## Attributes
After the regular arguments of the give command, you can specify as many attributes as you would like. Attributes
change the item that will be given in different ways. An example showing how this may look is shown below:
```%lang%
give @s diamond_pickaxe keep candestroy: "diamond_block"
```

### Including Attributes on New Lines
When using lots of attributes, `give` commands can get really long. You can specify attributes on new lines to keep
everything shorter and better formatted. In the following example, a sweet sword is created using multiple of the
attributes on this page:
```%lang%
give @s netherite_sword 1 keep
    enchant: sharpness 5
    enchant: unbreaking 3
    name: "Hyper Sword"
    lore: "A legendary sword."
```

## Vanilla Attributes
Let's begin with the attributes which are supported in vanilla Minecraft, using JSON components.

Keep on Death
: The item will be kept in the player's inventory, even after they die.
- `keep`

Lock in Inventory
: The item will be locked in the player's inventory. They will be unable to drop it, craft with it, etc. but it will not
be constrained to any specific slot.
- `lockinventory`

Lock in Slot
: The item will be locked in the player's inventory, same as above, but it will be unable to be moved from the slot it's
placed in, too.
- `lockslot`

Can Place on Block
: Specifies a block which the given item can be placed on in adventure mode (2).
- `canplaceon: <block>`

Can Destroy Block
: Specifies a block which the given item can break in adventure mode (2).
- `candestroy: <block>`

## Extended Attributes
When an extended attribute is used, the give command will be internally changed over to a `structure load` command,
and a structure will be generated containing the item. The advantage of doing this is that you can access to a whole
world of attributes that aren’t possible in the current Minecraft command system.

> The only downside to using extended attributes is that when making a change to the item, the world needs to be
> reloaded. Additionally, the `./structures/compiler` directory may become crowded with old items that are no longer used.
> {style="warning"}

Enchant Item
: Adds the given enchantment to the item. It has no constraints on enchantment level. This attribute can be repeated to
add more enchantments to the same item.
- `enchant: <enchantment> <level>`

Item Name
: Sets the display name of the item. Doesn’t support
[format-strings](Text-Commands.md#format-strings) or [localization](Localization.md).
- `name: <text>`

Item Lore
: Adds a line of lore to the item; extra text that is displayed when hovering over/selecting it. This attribute can be
repeated to add more lines of lore to the item. Doesn’t support [format-strings](Text-Commands.md#format-strings) or
[localization](Localization.md).
- `lore: <text>`

### Book-Specific Attributes
If the item you're giving is a `written_book`, you can use the following attributes:

Book Title
: Sets the title of the book, which is also the item's name when held. Doesn’t support
[format-strings](Text-Commands.md#format-strings) or [localization](Localization.md).
- `title: <text>`

Book Author
: Sets the author of the book. Doesn’t support
[format-strings](Text-Commands.md#format-strings) or [localization](Localization.md).
- `author: <text>`

Add Page
: Adds a page with the given text to the book. This attribute can be repeated to add more pages to the book. Doesn’t
support [format-strings](Text-Commands.md#format-strings) or [localization](Localization.md).
- `page: <text>`

### Leather-Specific Attributes
If the item is a leather item (armor, horse armor, etc.), you can use the `dye` attribute:

Dye Leather
: Sets the color of the leather item to the given red, green, and blue color. These numbers range from 0 to 255.
- `dye: 255 0 120`