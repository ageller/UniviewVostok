uniform float uv_fade;
uniform float uv_alpha;

uniform float simUseTime;
uniform float ntAve;
uniform float tAveRange;
uniform sampler3D glacierTex;
uniform sampler2D co2Tex;

uniform vec2 co2range;
uniform vec2 timerange;

in vec2 TexCoord;

out vec4 FragColor;

void main()
{	

	//get the CO2 level from the Vostok ice coore
	float time = (simUseTime - timerange[0])/(timerange[1] - timerange[0]);
	vec4 co2val = texture(co2Tex, vec2(time, 0.5));
	if (ntAve > 0){
		co2val = vec4(0.);
		float offset = 0.;
		for (int i=0; i<2*ntAve; i++){
			offset = (i - ntAve)/ntAve*tAveRange;
			time = (simUseTime + offset - timerange[0])/(timerange[1] - timerange[0]);
			co2val += texture(co2Tex, vec2(time, 0.5));
		}
		co2val /= (2*ntAve);
	}
	float c = co2val.r*256. + 100 + co2val.g;

	//find the position in the glacier texture for FragColor
	float zpos = (c - co2range[0])/(co2range[1] - co2range[0]);
	vec4 color = texture(glacierTex, vec3(TexCoord, zpos));
	color.a = uv_alpha*uv_fade;

	FragColor = color;
}