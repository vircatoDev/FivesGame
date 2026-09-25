#!/usr/bin/env python3
"""Generates the tile sprites: a rounded mask, a bevel overlay (light top-left rim, shadow bottom-right)
and the hint marks (a glowing frame, a route dot and an arrow).

Run from the repository root after changing a constant:  python3 tools/make_tile_sprites.py
Needs Pillow (pip install pillow).
"""
from PIL import Image, ImageChops, ImageDraw, ImageFilter

OUT = "Assets/Content/Sprites/UI/Common/"   # packed into the UI-Common atlas
MASKS = "Assets/Content/Sprites/UI/Masks/"  # SoftMask sources stay out of the atlases
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
HINT_RGB = (255, 196, 48)   # hint frame and route colour
HINT_EDGE_RGB = (232, 128, 24)


def rounded(inset=0, size=SIZE, radius=RADIUS):
    mask = Image.new("L", (size, size), 0)
    ImageDraw.Draw(mask).rounded_rectangle((inset, inset, size - 1 - inset, size - 1 - inset), radius, fill=255)
    return mask


def rim(shape, dx, dy):
    inner = Image.new("L", (SIZE, SIZE), 0)
    inner.paste(rounded(RIM), (dx, dy))
    return ImageChops.subtract(shape, inner).filter(ImageFilter.GaussianBlur(BLUR))


def layer(rgb, alpha, opacity=1.0):
    return Image.merge("RGBA", [Image.new("L", alpha.size, v) for v in rgb] + [alpha.point(lambda a: int(a * opacity))])


def hint_frame():
    """A glowing rounded frame a little larger than a tile: soft glow, coloured band, white inner line."""
    size, pad = SIZE + 32, 16
    outer = rounded(pad - 2, size, RADIUS + 12)
    band = ImageChops.subtract(outer, rounded(pad + 10, size, RADIUS + 2))
    inner = ImageChops.subtract(rounded(pad + 10, size, RADIUS + 2), rounded(pad + 14, size, RADIUS))
    frame = layer(HINT_RGB, band.filter(ImageFilter.GaussianBlur(9)), 0.7)
    frame = Image.alpha_composite(frame, layer(HINT_RGB, band.filter(ImageFilter.GaussianBlur(1))))
    return Image.alpha_composite(frame, layer((255, 255, 255), inner.filter(ImageFilter.GaussianBlur(0.8)), 0.9))


def hint_dot():
    size = 64
    disc = Image.new("L", (size, size), 0)
    ImageDraw.Draw(disc).ellipse((6, 6, size - 7, size - 7), fill=255)
    core = Image.new("L", (size, size), 0)
    ImageDraw.Draw(core).ellipse((14, 14, size - 15, size - 15), fill=255)
    dot = layer(HINT_EDGE_RGB, disc.filter(ImageFilter.GaussianBlur(1)))
    return Image.alpha_composite(dot, layer(HINT_RGB, core.filter(ImageFilter.GaussianBlur(1))))


def hint_arrow():
    """A rounded arrowhead pointing right; the game rotates it along the last step."""
    width, height = 128, 112
    head = Image.new("L", (width, height), 0)
    ImageDraw.Draw(head).polygon([(18, 10), (116, 56), (18, 102), (40, 56)], fill=255)
    outline = head.filter(ImageFilter.MaxFilter(9)).filter(ImageFilter.GaussianBlur(1))
    arrow = layer(HINT_EDGE_RGB, outline)
    return Image.alpha_composite(arrow, layer(HINT_RGB, head.filter(ImageFilter.GaussianBlur(1))))


shape = rounded()
edge = ImageChops.subtract(shape, rounded(2)).filter(ImageFilter.GaussianBlur(0.8))

bevel = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
bevel = Image.alpha_composite(bevel, layer(SHADOW_RGB, rim(shape, -OFFSET, -OFFSET), SHADOW))
bevel = Image.alpha_composite(bevel, layer((255, 255, 255), rim(shape, OFFSET, OFFSET), LIGHT))
bevel = Image.alpha_composite(bevel, layer(OUTLINE_RGB, edge, OUTLINE))
bevel.putalpha(ImageChops.multiply(bevel.getchannel("A"), shape))

bevel.save(OUT + "tile-bevel.png")
Image.merge("RGBA", [Image.new("L", (SIZE, SIZE), 255)] * 3 + [shape]).save(MASKS + "tile-shape.png")
hint_frame().save(OUT + "hint-frame.png")
hint_dot().save(OUT + "hint-dot.png")
hint_arrow().save(OUT + "hint-arrow.png")
print("Wrote tile-bevel, hint-frame, hint-dot and hint-arrow to", OUT, "and tile-shape to", MASKS)
