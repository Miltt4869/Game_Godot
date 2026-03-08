shader_type canvas_item;

uniform vec4 chroma_key : source_color = vec4(0.0, 1.0, 0.0, 1.0);
uniform float tolerance : hint_range(0.0, 1.0) = 0.4;
uniform float transition : hint_range(0.0, 1.0) = 0.08;
uniform float spill_reduction : hint_range(0.0, 1.0) = 0.5;

void fragment() {
    vec4 color = texture(TEXTURE, UV);
    
    // Distance to chroma key color
    float d = distance(color.rgb, chroma_key.rgb);
    
    // More aggressive green mask
    float green_diff = color.g - (color.r + color.b) * 0.5;
    float green_mask = smoothstep(0.01, 0.1, green_diff);
    
    // Alpha calculation
    float alpha = smoothstep(tolerance, tolerance + transition, d);
    alpha = min(alpha, 1.0 - green_mask);
    
    // Spill reduction (remove green tint from edges)
    vec3 non_green = color.rgb;
    non_green.g = min(non_green.g, (color.r + color.b) * 0.8);
    vec3 final_color = mix(color.rgb, non_green, green_mask * spill_reduction);
    
    COLOR = vec4(final_color, color.a * alpha);
}
