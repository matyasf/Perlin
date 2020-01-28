#version 450

layout (set = 0, binding = 0) uniform OrthographicProjection
{
    mat4 Projection;
};

layout(location = 0) in vec2 Pos;
layout(location = 1) in vec2 Size;
layout(location = 2) in float Alpha;
layout(location = 3) in float Rotation;
layout(location = 4) in vec4 TextureSubRegion; // rectangle normalized to 0..1

layout(location = 0) out vec2 fsin_TexCoords;
layout(location = 1) out float fsin_Alpha;

const vec2 Quads[4]= vec2[4]( // pivot is top left by default
    vec2(0, 1), // x, y
    vec2(1, 1),
    vec2(0, 0),
    vec2(1, 0)
);
vec2 rotate(vec2 pos, float rot) {
    float s = sin(rot);
    float c = cos(rot);
    mat2 m = mat2(c, -s, s, c); // 2x2 floating point matrix
    return m * pos;
}

void main() {
    vec2 quadPos = Quads[gl_VertexIndex]; 
    quadPos.x = (quadPos.x * Size.x);
    quadPos.y = (quadPos.y * Size.y);
    if (Rotation != 0) {
        quadPos = rotate(quadPos, -Rotation);   
    }
    quadPos.x = quadPos.x + Pos.x;
    quadPos.y = quadPos.y + Pos.y;
    
    // sub-texture support
    vec2 texturePos = Quads[gl_VertexIndex];
    texturePos.x = TextureSubRegion.x + TextureSubRegion.z * texturePos.x;
    texturePos.y = TextureSubRegion.y + TextureSubRegion.w * texturePos.y;
    
    gl_Position = Projection * vec4(quadPos, 0, 1);
    fsin_TexCoords = texturePos;
    fsin_Alpha = Alpha;
}