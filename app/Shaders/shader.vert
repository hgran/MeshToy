#version 330 core

layout(location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;

// ...However, they aren't needed for the vertex shader itself.
// Instead, we create an output variable so we can send that data to the fragment shader.

out vec2 texCoord;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    // Then, we further the input texture coordinate to the output one.
    // texCoord can now be used in the fragment shader.
    
    texCoord = aTexCoord;

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}