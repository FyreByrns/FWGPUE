#version 330 core
layout(location = 0) in vec3 vPos;

layout(location = 1) in mat4 vTransform_i;

uniform mat4 uProjection;
uniform mat4 uView;

void main() {
	gl_Position = uProjection * uView * vTransform_i * vec4(vPos, 1.0);
}

^ vertex ^ / v fragment v
#version 330 core

uniform sampler2D uTextureAtlas;

out vec4 FragColor;

void main() {
	//FragColor = vec4(1.0);
	FragColor = gl_FragCoord;
}