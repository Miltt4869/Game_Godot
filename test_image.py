from PIL import Image
import sys
img = Image.open('Assets/Sprites/Player/Tháchanh_ đưng yen.png').convert('RGBA')
print(f"Size: {img.width}x{img.height}")
print(f"Top-left: {img.getpixel((0,0))}")
pixels = list(img.getdata())

# Check how many are visible if we remove green
visible = 0
green_count = 0
for r,g,b,a in pixels:
    if g > 200 and r < 80 and b < 80:
        green_count += 1
    elif a > 25:
        visible += 1
        
print(f"Green count: {green_count}")
print(f"Visible count: {visible}")
