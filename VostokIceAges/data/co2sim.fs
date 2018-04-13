uniform float uv_fade;
uniform float uv_alpha;
uniform sampler3D co2sim;
uniform bool play;
uniform float framerate;
flat in float DistanceFade;
uniform float uv_simulationtimeSeconds;
uniform int numFrames;
in vec2 TexCoord;
//out vec4 color;

void main()
{	
  float zpos = 0.5;
  if (play) {
		zpos = fract(uv_simulationtimeSeconds*framerate/(1.0*numFrames));
  }	
  vec4 color = texture(co2sim,vec3(TexCoord,1.-zpos));
  color.a=uv_alpha*uv_fade;
  gl_FragColor=color;
}