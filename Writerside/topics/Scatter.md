# Scatter

<primary-label ref="runtime"/>

<link-summary>
The scatter command is like the vanilla fill command... but with randomization!
</link-summary>

The `fill` command fills an area with blocks, but there's no control of any randomness. The `scatter` command does this.
- `scatter <x1, y1, z1> <x2, y2, z2> <block> [block states] <int: percentage> [string: seed]`

## Enabling the Feature
The `scatter` command requires the [`structures`](Optional-Features.md#structures) feature to be enabled, since it
generates a structure file which might sometimes be unwanted. To enable the feature, add the following to the top of your
file:
```%lang%
feature structures
```

## Technicalities
The scatter command generates a structure which is then loaded into the world with the `integrity` parameter. This poses
a few issues that are not a part of the vanilla `fill` command.

### Dynamic Sizing
As the structure is generated at compile-time, it doesn't support having a dynamic fill region size. The size must be
known at compile-time, either by having *all* coordinates relative, or by having *all* coordinates static.

### Block Data
It's much harder for the compiler to embed data into the structure file, as it doesn't have the same information that
Minecraft has internally. We're looking to revisit this in the future, but for now, there is no support for block data.
