uniform sampler2D stateTexture;
uniform float dT;
uniform float sunTemp;
out vec4 FragColor;



void main(){


	vec4 currentColor = texture(stateTexture, vec2(0.5));
	float currentTemp = currentColor.r;
	//initalization to Sun
	if (currentTemp <1.){
	  currentTemp = 5600.;
	}
	float newTemp = currentTemp + ((sunTemp >currentTemp) ?  min(sunTemp-currentTemp,dT) : max(sunTemp-currentTemp,-dT));
    newTemp = clamp(newTemp,1000,10000);
	FragColor = vec4(newTemp,currentColor.gba);
}