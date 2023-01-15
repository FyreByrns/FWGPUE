#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vUv;
layout (location = 2) in vec4 vOffset;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec2 fUv;

// https://gist.github.com/companje/29408948f1e8be54dd5733a74ca49bb9 //
float map(float value, float min1, float max1, float min2, float max2) {
    return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

void main()
{
    gl_Position = uProjection * uView * uModel * (vec4(vPos, 1.0) + vOffset);
    fUv = vUv;
}
^ vertex ^ / v fragment v
#version 330 core
in vec2 fUv;

uniform sampler2D uTexture0;

out vec4 FragColor;

void main()
{
    FragColor = texture(uTexture0, fUv);
}
