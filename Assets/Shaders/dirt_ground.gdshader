shader_type canvas_item;

// Truyền vị trí local từ vertex sang fragment để tiling nhất quán
varying vec2 world_pos;

// Hash function tạo giá trị ngẫu nhiên từ tọa độ 2D
float hash2(vec2 p) {
    p = fract(p * vec2(127.1, 311.7));
    p += dot(p, p + 74.1);
    return fract(p.x * p.y);
}

void vertex() {
    world_pos = VERTEX;
}

void fragment() {
    // ── Tiling: 1 "đá" rộng khoảng 24px game units ──
    float cell_size = 24.0;
    vec2 uv = world_pos / cell_size;

    // ── Pixel Art: lượng tử hóa UV thành lưới pixel thô ──
    float px = 1.0 / 6.0;
    uv = floor(uv / px) * px;

    // ── Voronoi: tạo các ô giống mảnh đất/đá ──
    vec2 g = floor(uv);
    vec2 f = fract(uv);

    float min_d    = 10.0;
    float cell_val = 0.0;

    for (int y = -1; y <= 1; y++) {
        for (int x = -1; x <= 1; x++) {
            vec2 n   = vec2(float(x), float(y));
            // Mỗi ô có tâm ngẫu nhiên → tạo hình dạng tự nhiên
            vec2 rnd = vec2(hash2(g + n), hash2(g + n + vec2(7.3, 3.7)));
            rnd = 0.5 + 0.4 * sin(rnd * 6.28318);
            vec2 d   = n + rnd - f;
            float dist = length(d);
            if (dist < min_d) {
                min_d    = dist;
                cell_val = hash2(g + n + vec2(13.1, 19.7));
            }
        }
    }

    // ── Bảng màu nâu đất pixel art (khớp ảnh tham chiếu) ──
    vec3 base  = vec3(0.66, 0.43, 0.22);  // Nâu trung tính
    vec3 light = vec3(0.80, 0.57, 0.32);  // Nâu sáng (điểm sáng)
    vec3 dark  = vec3(0.50, 0.31, 0.14);  // Nâu tối (bóng)
    vec3 edge  = vec3(0.26, 0.14, 0.05);  // Đường viền đen nâu

    // Trộn màu theo cell_val để mỗi ô có màu hơi khác nhau
    vec3 col = mix(dark, mix(base, light, cell_val), cell_val * 0.7 + 0.3);

    // ── Đường viền giữa các ô đá ──
    float border = smoothstep(0.05, 0.16, min_d);
    col = mix(edge, col, border);

    COLOR = vec4(col, 1.0);
}
