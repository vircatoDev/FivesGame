#!/usr/bin/env python3
"""Generates the tile relief: a rounded mask and a bevel overlay (light top-left rim, shadow bottom-right).

Run from the repository root after changing a constant:  python3 tools/make_tile_bevel.py
Needs Pillow (pip install pillow).
"""
from PIL import Image, ImageChops, ImageDraw, ImageFilter

OUT = "Assets/Content/Sprites/KidsPuzzle/"
SIZE = 256          # sprite size; tiles scale it down
RADIUS = 26         # corner radius
RIM = 7             # width of the lit and shaded rims
OFFSET = 5          # light direction: how far the rims lean to the top-left and the bottom-right
BLUR = 3            # softness of the rims
LIGHT = 0.75        # opacity of the white highlight
SHADOW = 0.55       # opacity of the warm shadow
OUTLINE = 0.35      # opacity of the thin outer edge
SHADOW_RGB = (60, 40, 20)
OUTLINE_RGB = (90, 60, 30)


def rounded(inset=0):
    mask = Image.new("L", (SIZE, SIZE), 0)
    ImageDraw.Draw(mask).rounded_rectangle((inset, inset, SIZE - 1 - inset, SIZE - 1 - inset), RADIUS, fill=255)
    return mask


def rim(shape, dx, dy):
    inner = Image.new("L", (SIZE, SIZE), 0)
    inner.paste(rounded(RIM), (dx, dy))
    return ImageChops.subtract(shape, inner).filter(ImageFilter.GaussianBlur(BLUR))


def layer(rgb, alpha, opacity):
    return Image.merge("RGBA", [Image.new("L", (SIZE, SIZE), v) for v in rgb] + [alpha.point(lambda a: int(a * opacity))])


shape = rounded()
edge = ImageChops.subtract(shape, rounded(2)).filter(ImageFilter.GaussianBlur(0.8))

bevel = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
bevel = Image.alpha_composite(bevel, layer(SHADOW_RGB, rim(shape, -OFFSET, -OFFSET), SHADOW))
bevel = Image.alpha_composite(bevel, layer((255, 255, 255), rim(shape, OFFSET, OFFSET), LIGHT))
bevel = Image.alpha_composite(bevel, layer(OUTLINE_RGB, edge, OUTLINE))
bevel.putalpha(ImageChops.multiply(bevel.getchannel("A"), shape))

bevel.save(OUT + "tile-bevel.png")
Image.merge("RGBA", [Image.new("L", (SIZE, SIZE), 255)] * 3 + [shape]).save(OUT + "tile-shape.png")
print("Wrote", OUT + "tile-bevel.png", "and", OUT + "tile-shape.png")
