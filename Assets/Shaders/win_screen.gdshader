shader_type canvas_item;

uniform vec4 color_top : source_color = vec4(0.05, 0.2, 0.05, 1.0);
uniform vec4 color_bottom : source_color = vec4(0.01, 0.05, 0.01, 1.0);

void fragment() {
    float gradient = UV.y;
    vec4 color = mix(color_top, color_bottom, gradient);
    
    // Thêm hiệu ứng lấp lánh nhẹ
    float sparkle = pow(sin(TIME * 2.0 + UV.x * 10.0 + UV.y * 10.0), 20.0) * 0.1;
    color.rgb += vec3(sparkle);
    
    COLOR = color;
}
