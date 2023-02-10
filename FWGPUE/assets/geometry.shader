#version 330 core
layout(location = 0) in vec3 vPos;
layout(location = 1) in vec3 vCol;

uniform mat4 uProjection;
uniform mat4 uView;

out vec3 fCol;

void main() {
	gl_Position = uProjection * uView * vec4(vPos, 1.0);
	fCol = vCol;
}

^ vertex ^ / v fragment v
#version 330 core

in vec3 fCol;

out vec4 FragColor;

void main() {
	FragColor = vec4(fCol, 1.0);
}