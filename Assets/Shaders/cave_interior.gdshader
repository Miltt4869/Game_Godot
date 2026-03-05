shader_type canvas_item;

void fragment() {
    // UV: (0,0) = góc trái trên cửa hang, (1,1) = góc phải dưới
    float x = UV.x;
    float y = UV.y;

    // ── Độ sâu theo chiều ngang: tối hơn ở giữa/trong ──
    float center_dist = 1.0 - abs(x - 0.5) * 2.0; // 0=cạnh, 1=giữa
    // ── Độ sâu theo chiều dọc: ánh sáng từ miệng hang trên xuống ──
    float light_from_top = 1.0 - y;

    // Màu nền hang: nâu đen sâu
    vec3 void_color   = vec3(0.005, 0.003, 0.002);  // đen tuyệt đối sâu
    vec3 wall_color   = vec3(0.09,  0.07,  0.04);   // vách tối gần cửa
    vec3 entry_color  = vec3(0.18,  0.13,  0.07);   // rìa cửa (sáng nhất)

    // Mix gradient từ cửa vào → sâu trong hang
    float depth = center_dist * 0.4 + light_from_top * 0.6;
    vec3 col = mix(void_color, mix(wall_color, entry_color, light_from_top * 0.8), depth);

    // ── Ánh đuốc hắt ra từ trong (warm amber) ──
    // Tập trung ở chính giữa-dưới của hang
    float torch_x = 1.0 - abs(x - 0.5) * 2.8;
    float torch_y = max(0.0, y - 0.3);
    float torch   = pow(clamp(torch_x, 0.0, 1.0), 2.0) * pow(torch_y, 0.7) * 0.6;
    col += vec3(0.20, 0.10, 0.02) * torch;

    // ── Ánh sáng lạnh từ trời (xanh lam nhạt ở miệng) ──
    float sky_light = (1.0 - center_dist * 1.3) * light_from_top * 0.5;
    col += vec3(0.04, 0.06, 0.10) * clamp(sky_light, 0.0, 1.0);

    // ── Bóng vách hai bên hang ──
    float side_shadow = 1.0 - smoothstep(0.0, 0.18, min(x, 1.0 - x));
    col *= (1.0 - side_shadow * 0.5);

    COLOR = vec4(col, 1.0);
}
