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
	vec4 color = texture(glacierTex, vec3(TexCoord, zpos));
	color.a = uv_alpha*uv_fade;

	float CO2color = 0.;
	if (showCO2){
		CO2color = (1. - zpos)*CO2lim;
	}
	color.r += CO2color;

	FragColor = color;
	//FragColor = vec4(zpos, 0, 0, 1);
}