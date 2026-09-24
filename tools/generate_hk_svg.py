import cv2
from PIL import Image
import numpy as np

# Load original HK logo white
im = Image.open('frontend/public/brand/hk-logo-white.png').convert('RGBA')
arr = np.array(im)[:, :86, 3] # alpha channel of mark

# Upscale 16x for pristine subpixel curve accuracy
scale = 16
big = cv2.resize(arr, (arr.shape[1] * scale, arr.shape[0] * scale), interpolation=cv2.INTER_CUBIC)
_, thresh = cv2.threshold(big, 127, 255, cv2.THRESH_BINARY)
contours, _ = cv2.findContours(thresh, cv2.RETR_TREE, cv2.CHAIN_APPROX_TC89_KCOS)

all_pts = np.vstack(contours)
bx, by, bw, bh = cv2.boundingRect(all_pts)

# In 512x512 canvas:
target_h = 310.0
target_scale = target_h / bh
target_w = bw * target_scale

# Center mark at (256, 256)
center_x = bx + bw / 2.0
center_y = by + bh / 2.0

shift_x = 256.0 - center_x * target_scale
shift_y = 256.0 - center_y * target_scale

paths = []
for c in contours:
    approx = cv2.approxPolyDP(c, 1.2, True).reshape(-1, 2)
    pts = approx * target_scale + np.array([shift_x, shift_y])
    d = f'M {pts[0][0]:.2f} {pts[0][1]:.2f} ' + ' '.join(f'L {p[0]:.2f} {p[1]:.2f}' for p in pts[1:]) + ' Z'
    paths.append(d)

svg_lines = [
    '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" width="100%" height="100%">',
    '  <defs>',
    '    <linearGradient id="hkRedGrad" x1="0%" y1="0%" x2="100%" y2="100%">',
    '      <stop offset="0%" stop-color="#E63946"/>',
    '      <stop offset="100%" stop-color="#D90429"/>',
    '    </linearGradient>',
    '  </defs>',
    '  <!-- Background Circle (Official KHK Brand Red) -->',
    '  <circle cx="256" cy="256" r="240" fill="url(#hkRedGrad)"/>',
    '  <!-- Official Hradec Králové Mark (White) -->',
    '  <g fill="#FFFFFF">'
]

for p in paths:
    svg_lines.append(f'    <path d="{p}"/>')

svg_lines.append('  </g>')
svg_lines.append('</svg>')

svg_content = '\n'.join(svg_lines)

with open('frontend/public/icon.svg', 'w', encoding='utf-8') as f:
    f.write(svg_content)
with open('frontend/public/favicon.svg', 'w', encoding='utf-8') as f:
    f.write(svg_content)
with open('frontend/app/icon.svg', 'w', encoding='utf-8') as f:
    f.write(svg_content)

print('Successfully generated SVG files!')
