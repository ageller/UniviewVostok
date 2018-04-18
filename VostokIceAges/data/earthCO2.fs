uniform float uv_fade;
uniform float uv_alpha;

in vec2 TexCoord;

uniform sampler2D CO2Tex;

uniform float simUseTime;
uniform float ntAve;
uniform float tAveRange;
uniform bool showCO2;
uniform float CO2lim;

//parameters
uniform vec2 CO2range;
uniform vec2 pastTimerange;
uniform vec2 futureTimerange;

out vec4 FragColor;

void main()
{	
	float CO2color = 0.;

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
		
		if (showCO2){
			CO2color = (1. - zpos)*CO2lim;
		}	
	} else {
		float time = clamp((simUseTime - futureTimerange[0])/(futureTimerange[1] - futureTimerange[0]), 0, 1);
		float c = 0.;
		if (simUseTime <= 2100){
			c = 3.85 * simUseTime - 7341.;
		} else {
			c = 240.*exp(-0.002*(simUseTime - 2100.)) + 500.;
		}
		CO2color = clamp((c - 360)/(745 - 360), 0 , 1)*CO2lim;
	}
	
	FragColor = vec4(CO2color, 0., 0., CO2color);
	FragColor.a *= uv_fade * uv_alpha;

}