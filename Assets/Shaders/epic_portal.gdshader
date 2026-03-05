shader_type canvas_item;

uniform vec4 portal_color_1 : source_color = vec4(0.4, 0.0, 0.8, 1.0); // Purple core
uniform vec4 portal_color_2 : source_color = vec4(0.0, 0.5, 1.0, 1.0); // Blue outer
uniform float rotation_speed = 2.0;
uniform float swirl_amount = 8.0;

void fragment() {
    // Tọa độ UV chuẩn hóa từ tâm (-0.5 đến 0.5)
    vec2 p = UV - 0.5;
    float r = length(p);      // Khoảng cách từ tâm (bán kính)
    float angle = atan(p.y, p.x); // Góc
    
    // Gợn sóng ma thuật (Xoắn ốc)
    angle += r * swirl_amount - TIME * rotation_speed;
    
    // Tạo sóng liên tục kiểu tia laze
    float intensity = sin(angle * 6.0) * 0.5 + 0.5;
    
    // Fade mềm ở viền (không bị hình thoi)
    intensity *= smoothstep(0.5, 0.1, r); 
    
    // Hỗn hợp màu
    vec4 final_color = mix(portal_color_1, portal_color_2, intensity);
    
    // Lõi rực sáng ở chính giữa
    float core = smoothstep(0.25, 0.0, r);
    final_color += vec4(1.0, 1.0, 1.0, 1.0) * core; // Chói trắng
    
    // Bo tròn viền ngoài (cắt bỏ góc vuông của ColorRect)
    final_color.a *= smoothstep(0.5, 0.45, r);
    
    // Độ nhấp nháy năng lượng
    final_color.a *= 0.8 + 0.2 * sin(TIME * 5.0);
    
    COLOR = final_color;
}
