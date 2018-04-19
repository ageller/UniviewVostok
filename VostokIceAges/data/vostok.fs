uniform float uv_fade;
uniform float uv_alpha;

uniform float simUseTime;
uniform float ntAve;
uniform float tAveRange;
uniform float dy;

uniform sampler3D pastGlacierTex;
uniform sampler3D futureGlacierTex;
uniform sampler2D CO2Tex;

//parameters
uniform vec2 CO2range;
uniform vec2 pastTimerange;
uniform vec2 futureTimerange;
uniform float nFramesPast;

in vec2 TexCoord;

out vec4 FragColor;

const float PI = 3.1415926535897932384626433;

vec4 getmC(sampler3D tex, vec3 coord){
	float smoothN = 5;
	float smoothL = 0.002;
	vec3 newCoord = coord;
	vec4 color = vec4(0);
	for (int i=0; i<smoothN; i++){
		for (int j=0; j<smoothN; j++){
			newCoord = coord;
			newCoord.x += (i/smoothN - 0.5)*smoothL/cos(2.*(coord.y - 0.5) * PI/180.);
			newCoord.y += (j/smoothN - 0.5)*smoothL;
			color += texture(tex, newCoord);

		}
	}
	color = color/smoothN/smoothN;

	return color;

}

float getIceY(sampler3D tex, vec3 coord, float wlim){
	vec4 color = texture(tex, coord);
	float w = max(max(abs(color.r - color.g), abs(color.r - color.b)),  abs(color.g - color.b)) / length(color.rgb);
	vec3 ucoord = coord;

	if (w <= wlim){
	//in ice now
		while (w <= wlim && ucoord.y < 1){
			ucoord.y += dy;
			color = texture(tex, ucoord);
 			w = max(max(abs(color.r - color.g), abs(color.r - color.b)),  abs(color.g - color.b)) / length(color.rgb);		
 		}
 	} else {
	//not in ice now
		while (w > wlim && ucoord.y > 0){
			ucoord.y -= dy;
			color = texture(tex, ucoord);
 			w = max(max(abs(color.r - color.g), abs(color.r - color.b)),  abs(color.g - color.b)) / length(color.rgb);		
 		}
 	}

 	return ucoord.y;	
}


void main()
{	
	vec4 color = vec4(0);
	
	if (simUseTime < 0){
		//get the CO2 level from the Vostok ice coore
		float time = clamp((simUseTime - pastTimerange[0])/(pastTimerange[1] - pastTimerange[0]), 0, 1);
		vec4 CO2val = texture(CO2Tex, vec2(time, 0.5));
		if (ntAve > 0){
			CO2val = vec4(0.);
			float offset = 0.;
			for (int i=0; i<2*ntAve; i++){
				offset = (i - ntAve)/ntAve*tAveRange;
				time = clamp((simUseTime + offset - pastTimerange[0])/(pastTimerange[1] - pastTimerange[0]), 0, 1);
				CO2val += texture(CO2Tex, vec2(time, 0.5));
			}
			CO2val /= (2*ntAve);
		}
		float c = CO2val.r*256. + 100 + CO2val.g;

		//find the position in the glacier texture for FragColor
		float zpos = clamp(1. - (c - CO2range[0])/(CO2range[1] - CO2range[0]),0.001,0.999);
		
		float zpos1 = floor(zpos * nFramesPast) / nFramesPast; 
		float zpos2 = ceil(zpos * nFramesPast) / nFramesPast; 
		vec4 color1 = texture(pastGlacierTex, vec3(TexCoord, zpos1));
		vec4 color2 = texture(pastGlacierTex, vec3(TexCoord, zpos2));
		//vec4 color1 = getmC(pastGlacierTex, vec3(TexCoord, zpos1));
		//vec4 color2 = getmC(pastGlacierTex, vec3(TexCoord, zpos2));
		float zmix = (zpos - zpos1)/(zpos2 - zpos1);
		color = color1;

		//see if we need to fix the transition from land to ice
		float yNow = TexCoord.y;
		float wlim1 = 0.05;
		float wlim2 = wlim1;
		float w10 = max(max(abs(color1.r - color1.g), abs(color1.r - color1.b)),  abs(color1.g - color1.b)) / length(color1.rgb);
		float w20 = max(max(abs(color2.r - color2.g), abs(color2.r - color2.b)),  abs(color2.g - color2.b)) / length(color2.rgb);

		//get the y locations of the ice
		//NOTE: I need to do this at each pixel so that I can get rid of the region right around the ice
		//any attempts I've made to only find the ice location when the interpolation is needed results in regions that don't get selected and therefore stripes on the final color
		vec3 uTexCoord1 = vec3(TexCoord, zpos1);
		vec3 uTexCoord2 = vec3(TexCoord, zpos2);
		uTexCoord1.y = getIceY(pastGlacierTex, uTexCoord1, wlim1);
		uTexCoord2.y = getIceY(pastGlacierTex, uTexCoord2, wlim2);
		
		float yoff = 0.02;

		//transition of ice between texture pixels
		if (w20 <= wlim2 && w10 > wlim1){
			float d21 = uTexCoord2.y - uTexCoord1.y;
			yNow = uTexCoord1.y + d21 * zmix;
			if (yNow >= TexCoord.y){
				color = color2;//vec3(0.9);
			}  
		} //else {
			// //problem region
			// if (abs(TexCoord.y - uTexCoord2.y) <= yoff){
			// 	color = vec4(0,1,0,1);
			// }
		//}

		//land
		if (w10 > wlim1 && w20 > wlim2 && abs(TexCoord.y - uTexCoord2.y) > yoff){
			color = mix(color1, color2, zmix);
		}
		//ice
		if (w10 < wlim1 && w20 < wlim2){
			color = mix(color1, color2, zmix);		
		}
		
	} else {
		float zpos = clamp(1. - (simUseTime - futureTimerange[0])/(futureTimerange[1] - futureTimerange[0]), 0.001, 0.999);;
		color = texture(futureGlacierTex, vec3(TexCoord, zpos));


	}

	
	color.a = uv_alpha*uv_fade;
	FragColor = color;
}