#version 330 core
layout(location = 0) in vec3 vPos;
layout(location = 1) in vec2 vUv;

// takes up 4 slots as vec4s
layout(location = 2) in mat4 vTransform_i;
// |              3
// |			  4
// '------------- 5 
layout(location = 6) in vec4 vUv_i;

uniform mat4 uProjection;
uniform mat4 uView;
uniform vec2 uAtlasSize;

uniform vec2 uCameraLoc;

out vec2 fUv;

float map(float value, float min1, float max1, float min2, float max2) {
	return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}
vec2 mapv2(vec2 o, vec2 s, vec2 e) {
	return vec2(map(o.x, 0.0, 1.0, s.x, e.x), map(o.y, 0.0, 1.0, s.y, e.y));
}

void main() {
	float distanceFromCenter = length(vPos.xy - uCameraLoc) / 100.0;
	vec2 pos = vPos.xy + (uCameraLoc - vPos.xy) * distanceFromCenter * vPos.z;

	gl_Position = uProjection * uView * vTransform_i * vec4(pos, vPos.z, 1.0);

	vec2 vStart = vUv_i.xy / uAtlasSize;
	vec2 vWidth = vUv_i.zw / uAtlasSize;
	fUv = mapv2(vUv, vStart, vStart + vWidth);
}

^ vertex ^ / v fragment v
#version 330 core

uniform sampler2D uTextureAtlas;

in vec2 fUv;

out vec4 FragColor;

void main() {
	FragColor = texture(uTextureAtlas, fUv);
}