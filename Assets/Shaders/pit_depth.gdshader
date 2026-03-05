shader_type canvas_item;

// Hàm hash ngẫu nhiên để tạo texture vách đá thô
float hash(vec2 p) {
    p = fract(p * vec2(127.1, 311.7));
    p += dot(p, p + 74.1);
    return fract(p.x * p.y);
}

void fragment() {
    // UV.y = 0 ở miệng hố (trên), 1 ở đáy hố (dưới)
    float depth = UV.y;

    // ── Màu gradient theo độ sâu ──
    vec3 mouth  = vec3(0.22, 0.14, 0.07);   // Miệng hố: nâu đất tối
    vec3 mid    = vec3(0.08, 0.05, 0.03);   // Giữa hố: nâu sẫm
    vec3 bottom = vec3(0.01, 0.005, 0.002); // Đáy: gần đen tuyệt đối

    // Gradient sâu dần (phi tuyến — đáy hố tối nhanh hơn)
    vec3 col = mix(mouth, mid, smoothstep(0.0, 0.5, depth));
    col = mix(col, bottom, smoothstep(0.35, 1.0, depth));

    // ── Texture vách đá pixel art (dọc 2 bên) ──
    float wall_fade = 1.0 - smoothstep(0.0, 0.35, min(UV.x, 1.0 - UV.x));
    // Vân đá thô trên vách
    float px = 1.0 / 12.0;
    vec2 uv_px = floor(UV / px) * px;
    float rock_noise = hash(uv_px * vec2(8.0, 6.0));
    float rock_dark  = hash(uv_px * vec2(5.3, 9.1));
    vec3 wall_col = mix(mouth * 0.9, mouth * 1.15, rock_noise);
    wall_col = mix(wall_col, mouth * 0.6, step(0.72, rock_dark) * 0.6);
    // Pha vách vào 2 bên hố
    col = mix(col, wall_col, wall_fade * (1.0 - depth * 0.7));

    // ── Ánh sáng hắt lên nhẹ ở đáy (ambient underground glow) ──
    float glow_depth = max(0.0, depth - 0.8) / 0.2;
    col += vec3(0.04, 0.02, 0.01) * glow_depth * glow_depth;

    // ── Bóng tối ở miệng hố (cạnh trên tối hơn) ──
    float top_shadow = 1.0 - smoothstep(0.0, 0.12, depth);
    col *= (1.0 - top_shadow * 0.4);

    COLOR = vec4(col, 1.0);
}
