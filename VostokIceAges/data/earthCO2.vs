in vec3 uv_vertexAttrib;
in vec2 uv_texCoordAttrib0;
uniform mat4 uv_modelViewProjectionMatrix;

out vec2 TexCoord;

const mat3 RotMat = mat3(1,  0, 0,
						 0,  0, 1,
						 0, -1, 0);

void main()
{  
  gl_Position = uv_modelViewProjectionMatrix*vec4(1.01*RotMat*uv_vertexAttrib,1.0);
  TexCoord=vec2((-0.25 +1-uv_texCoordAttrib0.x),1.-uv_texCoordAttrib0.y);
  
}