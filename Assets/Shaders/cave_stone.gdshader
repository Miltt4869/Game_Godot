shader_type canvas_item;

varying vec2 world_pos;

float hash2(vec2 p) {
    p = fract(p * vec2(127.1, 311.7));
    p += dot(p, p + 74.1);
    return fract(p.x * p.y);
}

void vertex() {
    world_pos = VERTEX;
}

void fragment() {
    // Tile đá lớn hơn directive (32px per stone)
    float cell_size = 32.0;
    vec2 uv = world_pos / cell_size;

    // Pixel art quantize (lưới pixel thô)
    float px = 1.0 / 5.0;
    uv = floor(uv / px) * px;

    // Voronoi tạo ô đá tự nhiên
    vec2 g = floor(uv);
    vec2 f = fract(uv);

    float min_d  = 10.0;
    float min_d2 = 10.0;
    float cell_val = 0.0;

    for (int y = -1; y <= 1; y++) {
        for (int x = -1; x <= 1; x++) {
            vec2 n   = vec2(float(x), float(y));
            vec2 rnd = vec2(hash2(g + n), hash2(g + n + vec2(7.3, 3.7)));
            rnd = 0.5 + 0.4 * sin(rnd * 6.28318);
            vec2 d   = n + rnd - f;
            float dist = length(d);
            if (dist < min_d) {
                min_d2   = min_d;
                min_d    = dist;
                cell_val = hash2(g + n + vec2(13.1, 19.7));
            } else if (dist < min_d2) {
                min_d2 = dist;
            }
        }
    }

    // ── Bảng màu đá xám-nâu (hang núi) ──
    vec3 base  = vec3(0.40, 0.35, 0.28);  // Đá xám nâu trung tính
    vec3 light = vec3(0.54, 0.48, 0.38);  // Mặt đá sáng (phản quang)
    vec3 dark  = vec3(0.24, 0.20, 0.15);  // Đá tối (bóng sâu)
    vec3 crack = vec3(0.10, 0.08, 0.05);  // Kẽ nứt / viền

    vec3 col = mix(dark, mix(base, light, cell_val), cell_val * 0.65 + 0.28);

    // Kẽ nứt dọc viền ô
    float border = smoothstep(0.04, 0.13, min_d);
    col = mix(crack, col, border);

    // Vết nứt phụ mỏng (chi tiết thứ cấp)
    float crack2 = min_d2 - min_d;
    col = mix(col * 0.6, col, smoothstep(0.0, 0.05, crack2));

    // Vân ẩm ướt nhẹ (hang đá thường ẩm)
    float moisture = hash2(floor(world_pos / 8.0) * vec2(3.3, 7.7));
    col = mix(col, col * vec3(0.8, 0.9, 1.0), moisture * 0.15);

    COLOR = vec4(col, 1.0);
}
