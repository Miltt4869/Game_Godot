from PIL import Image
img = Image.open(r"Assets\Sprites\Enemies\Snack _move.png").convert("RGBA")
pixels = img.load()
w, h = img.size
# Emulate RemoveBackground
for y in range(h):
    for x in range(w):
        r, g, b, a = pixels[x, y]
        pr = r / 255.0
        pg = g / 255.0
        pb = b / 255.0
        # Magenta screen
        if pr > 0.3 and pb > 0.3 and pr > pg * 1.1 and pb > pg * 1.1:
            pixels[x, y] = (0,0,0,0)

# Emulate Slice
cw = w // 4
ch = h // 1
for i in range(4):
    frame = img.crop((i*cw, 0, (i+1)*cw, ch))
    fx, fy = frame.size
    fpixels = frame.load()
    # Emulate clear
    for y in range(200):
        for x in range(cw):
            fpixels[x, y] = (0,0,0,0)
    
    # Save frame
    frame.save(f"frame_{i}.png")
