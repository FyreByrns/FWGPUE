#version 330 core
layout(location = 0) in vec2 vPos;
layout(location = 1) in vec2 vUv;

out vec2 fUv;

void main() {
	gl_Position = vec4(vPos.x, vPos.y, 0.0, 1.0);
	fUv = vUv;
}

^ vertex ^ / v fragment v
#version 330 core

out vec4 FragColor;
in vec2 fUv;
uniform sampler2D tex;

void main() {
	FragColor = texture(tex, fUv);
}