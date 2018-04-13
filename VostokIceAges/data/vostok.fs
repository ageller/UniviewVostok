uniform float uv_fade;
uniform float uv_alpha;

uniform float simUseTime;
uniform float ntAve;
uniform float tAveRange;
uniform float CO2lim;
uniform bool showCO2;

uniform sampler3D glacierTex;
uniform sampler2D CO2Tex;

//parameters
uniform vec2 CO2range;
uniform vec2 timerange;
uniform float nFrames;

in vec2 TexCoord;

out vec4 FragColor;

void main()
{	

	//get the CO2 level from the Vostok ice coore
	float time = (simUseTime - timerange[0])/(timerange[1] - timerange[0]);
	vec4 CO2val = texture(CO2Tex, vec2(time, 0.5));
	if (ntAve > 0){
		CO2val = vec4(0.);
		float offset = 0.;
		for (int i=0; i<2*ntAve; i++){
			offset = (i - ntAve)/ntAve*tAveRange;
			time = (simUseTime + offset - timerange[0])/(timerange[1] - timerange[0]);
			CO2val += texture(CO2Tex, vec2(time, 0.5));
		}
		CO2val /= (2*ntAve);
	}
	float c = CO2val.r*256. + 100 + CO2val.g;

	//find the position in the glacier texture for FragColor
	float zpos = (c - CO2range[0])/(CO2range[1] - CO2range[0]);
	float zpos1 = floor(zpos * nFrames) / nFrames; 
	float zpos2 = ceil(zpos * nFrames) / nFrames; 
	// vec4 color = texture(glacierTex, vec3(TexCoord, zpos1));
	vec4 color1 = texture(glacierTex, vec3(TexCoord, zpos1));
	vec4 color2 = texture(glacierTex, vec3(TexCoord, zpos2));
	float zmix = (zpos - zpos1)/(zpos2 - zpos1);
	vec4 color = color1;//mix(color1, color2, zmix);

	//see if we need to fix the transition from land to ice
	float wlim = 1.5;
	float dy = 0.001;
	float w1 = color1.r + color1.g + color1.b;
	float w2 = color2.r + color2.g + color2.b;
	if (w2 >= wlim && w1 < wlim){
		//where the ice begins in zpos1
		vec2 uTexCoord1 = TexCoord;
		while (w1 < wlim && uTexCoord1.y > 0){
			uTexCoord1.y -= dy;
			color1 = texture(glacierTex, vec3(uTexCoord1, zpos1));
			w1 = color1.r + color1.g + color1.b;
		}
		//where the ice ends in zpos2
		vec2 uTexCoord2 = TexCoord;
		while (w2 >= wlim && uTexCoord2.y < 1){
			uTexCoord2.y += dy;
			color2 = texture(glacierTex, vec3(uTexCoord2, zpos2));
			w2 = color2.r + color2.g + color2.b;
		}
		float d21 = uTexCoord2.y - uTexCoord1.y;
		float yNow = uTexCoord1.y + d21 * zmix;
		if (yNow >= TexCoord.y){
			color.rgb = vec3(0.9);
		} 
	}

	//land
	if (w2 < wlim && w1 < wlim){
		color = mix(color1, color2, zmix);

	}



	//ivec3 iTexCoord = ivec3(round(TexCoord.x)*2048, round(TexCoord.y)*1024, floor(zpos)*32);
	//vec4 color = texelFetch(glacierTex, iTexCoord);
	
	color.a = uv_alpha*uv_fade;

	float CO2color = 0.;
	if (showCO2){
		CO2color = (1. - zpos)*CO2lim;
	}
	color.r += CO2color;

	FragColor = color;
	//FragColor = vec4(zpos, 0, 0, 1);
}