#include <iostream>
#include <thread>
#include <glad/glad.h>
#include <windows.h>
#include <math.h>
#include <fstream>

#include <glm.hpp>
#include <gtc/matrix_transform.hpp>

#include <imgui.h>
#include <imgui_impl_glfw.h>
#include <imgui_impl_opengl3.h>
//Assimp configuration
#include <assimp/config.h>

#include <GLFW/glfw3.h>
#include "stb_image.h"
#include "Texture2D.h"
#include "Shader.h"
#include "Model.h"
#include "Camera.h"
#include "FBOLayer.h"
#include "GBuffer.h"
#include "LuaEnv.h"
#include "Water_Material.h"
//LUA API
extern "C" {
#include "Lua542/lua.h"
#include "Lua542/lauxlib.h"
#include "Lua542/lualib.h"
}
#if _WIN64
#pragma comment(lib, "Lua542/liblua54.a")
#endif
void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void processInput(GLFWwindow* window);
void GetDesktopResolution(int& horizontal, int& vertical);
//---VAOs---
unsigned int SetCubeVAO();
unsigned int SetGridVAO(float beginning, float size, float sizeX, float sizeY, int* sizeOfVao);
unsigned int PlaneVAO();
unsigned int DrawTriangle();
unsigned int renderQuad();
unsigned int DrawFBOQuad();
//---Skybox----
unsigned int skyBoxVAO();
unsigned int loadCubeMap(std::vector<std::string> faces);
//-------
unsigned int SetUBO();
// settings
const unsigned int SCR_WIDTH = 1330;
const unsigned int SCR_HEIGHT = 800;
//---Model importing---
vector<Model> modelLists;
vector<glm::vec3> modelPos;
Model* selectedModel;
//---Light importing---
vector<glm::vec3> lightPos;

float deltaTime = 0.0;
float lastFraming = 0.0;

char guiBuf[512];
char inpBuf[128];
bool renderGUI = true;
bool isMouseVisible = false;
int window_width = 0;
int window_height = 0;
//Lua config
vector<string> luaFiles;
char luaText[512];
char shaderText[2048];
bool isLuaExec = false;
int lua_GetTime(lua_State* D) {
	lua_pushnumber(D, glfwGetTime());
	return 1;
}

//Configure camera calls
Camera cameraEngine(glm::vec3(1.0, 1.0, 3.0));
void scroll_callback(GLFWwindow* window, double xpos, double ypos) {
	cameraEngine.ProcessMouseScroll(ypos);
}
float lastX = 400;
float lastY = 300;
bool isFirstTimeMove = true;
#pragma region GLFW/NON-OPENGL
void mouse_callback(GLFWwindow* window, double xpos, double ypos)
{
	if (!isMouseVisible) {
		if (isFirstTimeMove)
		{
			lastX = xpos;
			lastY = ypos;
			isFirstTimeMove = false;
		}
		float xOffSet = xpos - lastX;
		float yOffSet = lastY - ypos;

		lastX = xpos;
		lastY = ypos;

		cameraEngine.ProcessMouseMovement(xOffSet, yOffSet);
	}
}
float get_resolution(GLFWwindow* window) {
	int window_width;
	int window_height;
	glfwGetWindowSize(window, &window_width, &window_height);

	return (float)window_width / (float)window_height;
}
vector<string> splitStr(string s, string delimiter) {
	size_t pos_start = 0, pos_end, delim_len = delimiter.length();
	string token;
	vector<string> res;

	while ((pos_end = s.find(delimiter, pos_start)) != string::npos) {
		token = s.substr(pos_start, pos_end - pos_start);
		pos_start = pos_end + delim_len;
		res.push_back(token);
	}

	res.push_back(s.substr(pos_start));
	return res;
}
void drop_callback(GLFWwindow* window, int count, const char** paths)
{
	int i;
	for (i = 0; i < count; i++) {
		string fileImport(paths[i]);
		vector<string> fIm = splitStr(fileImport, ".");
		if (fIm.at(fIm.size() - 1) == "lua") {
			luaFiles.push_back(paths[i]);
		} else {
			Model new_mdl(paths[i]);
			modelLists.push_back(new_mdl);
			modelPos.push_back(cameraEngine.Position);
			string buf("\nSuccessfully loaded model: ");
			buf.append(paths[i]);
			buf.append("\n>");
			strncat_s(guiBuf, buf.c_str(), strlen(buf.c_str()));
		}
	}
	//std::cout << paths[i] << std::endl;
}
static inline void trim(std::string& s) {
	//ltrim
	s.erase(s.begin(), std::find_if(s.begin(), s.end(), [](unsigned char ch) {
		return !std::isspace(ch);
		}));
}
#pragma endregion
using namespace std;
int main()
{
#pragma region LUA TEST
	lua_State* L = luaL_newstate();
	luaL_openlibs(L);
#pragma endregion
	thread t2([&]() {
		glfwInit();
		glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
		glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
		glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
		//Borderless
		//glfwWindowHint(GLFW_DECORATED, GLFW_FALSE);
		//Multi-sample anti-aliasing (4x)
		//glfwWindowHint(GLFW_SAMPLES, 4);

#ifdef __APPLE__
		glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#endif

		// glfw window creation
		GLFWwindow* window = glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "Multiplex Engine - NEW PROJECT", NULL, NULL);
		if (window == NULL)
		{
			cout << "Failed to create GLFW window" << endl;
			glfwTerminate();
			return -1;
		}
		glfwMakeContextCurrent(window);
		glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);
		//Mouse
		glfwSetCursorPosCallback(window, mouse_callback);
		glfwSetScrollCallback(window, scroll_callback);
		//Check for controller
		if (glfwJoystickPresent(GLFW_JOYSTICK_1))
			cout << "CONTROLLER IS CONNECTED!" << endl;
		//glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
		//Drag drop
		glfwSetDropCallback(window, drop_callback);

		//glad: load all OpenGL function pointers
		if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
		{
			std::cout << "Failed to initialize GLAD" << std::endl;
			return -1;
		}
#pragma region CUBE
		Texture2D brickTexture("MultiplexAssets/IMAGES/bricks.jpg", true, false);
		Texture2D brickNormals("MultiplexAssets/IMAGES/brickNormal.png", true, false);
		unsigned int VAOCube = SetCubeVAO();
		Shader CubeShader("MultiplexAssets/SHADER/cube.vs",
			"MultiplexAssets/SHADER/cube.fs");
		CubeShader.setInt("textureA", 0);
		glActiveTexture(GL_TEXTURE0);
		brickTexture.Use();
#pragma endregion
		const char* welcome = "\tWelcome to Multiplex engine! Type help for list of commands.\n>";
		strncpy_s(guiBuf, welcome, strlen(welcome));

		unsigned int quadRender = renderQuad();
		Shader quadShader("MultiplexAssets/SHADER/Quad/quad.vs",
			"MultiplexAssets/SHADER/Quad/quad.fs");

		int sizeGrid;
		unsigned int GridVAO = SetGridVAO(-5, 1, 5, 5, &sizeGrid);
		Shader GridShader("MultiplexAssets/SHADER/Grid/grid.vs",
			"MultiplexAssets/SHADER/Grid/grid.fs");

		//Debug console
		cout << "WELCOME TO THE MULTIPLEX ENGINE DEBUG CONSOLE" << endl;
#pragma region SKYBOX
		Shader cubemapShader("MultiplexAssets/SHADER/Skybox/cubemapVert.vs",
			"MultiplexAssets/SHADER/Skybox/cubemapFrag.fs");
		vector<std::string> faces
		{
		   "MultiplexAssets/IMAGES/skybox/right.jpg",
		   "MultiplexAssets/IMAGES/skybox/left.jpg",
		   "MultiplexAssets/IMAGES/skybox/bottom.jpg",
		   "MultiplexAssets/IMAGES/skybox/top.jpg",
		   "MultiplexAssets/IMAGES/skybox/front.jpg",
		   "MultiplexAssets/IMAGES/skybox/back.jpg"
		};
		unsigned int cubemapTextures = loadCubeMap(faces);
		unsigned int skybox = skyBoxVAO();
#pragma endregion

		//Model importing
#pragma region MODELS IMPORTS
		Shader mdl_shd("MultiplexAssets/SHADER/Model/model.vs",
			"MultiplexAssets/SHADER/Model/model.fs");
		//This shader is for when the model has no textures provided
		//It creates a procedural generated texture checkerboard pattern like Source engine
		Shader missing_tex("MultiplexAssets/SHADER/Model/missingtex.vs",
			"MultiplexAssets/SHADER/Model/missingtex.fs");
#pragma endregion
#pragma region WATER_MATERIAL
		Shader waterMat("MultiplexAssets/SHADER/Materials/water.vs",
			"MultiplexAssets/SHADER/Materials/water.fs");
		Texture2D waterTexture("MultiplexAssets/IMAGES/StockMaterial/water.png", true, false);
		Texture2D dUdVTexture("MultiplexAssets/IMAGES/StockMaterial/dudvmap.jpg", true, false);
		unsigned int waterPlane = PlaneVAO();

		Water_Material watMats;
		watMats.setWaterPlaneSize(5.f);
#pragma endregion
#pragma region LightSources
		Shader lightSources("MultiplexAssets/SHADER/Quad/quad.vs",
			"MultiplexAssets/SHADER/Quad/lightSource.fs");
		Texture2D lightSourceTxt("MultiplexAssets/IMAGES/lightsource.png", true, false);
#pragma endregion
		//IMGUI CONFIG
#pragma region IMGUI_CONFIG
		IMGUI_CHECKVERSION();
		ImGui::CreateContext();
		ImGuiIO& io = ImGui::GetIO(); (void)io;
		io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;     // Enable Keyboard Controls
		//io.ConfigFlags |= ImGuiConfigFlags_NavEnableGamepad;      // Enable Gamepad Controls

		// Setup Dear ImGui style
		ImGui::StyleColorsDark();

		// Setup Platform/Renderer backends
		ImGui_ImplGlfw_InitForOpenGL(window, true);
		ImGui_ImplOpenGL3_Init("#version 330");
#pragma endregion

#pragma region FRAMEBUFFER ENVIRONMENT
		Shader FBO("MultiplexAssets/SHADER/FRAMEBUFFERS/fbo.vs",
			"MultiplexAssets/SHADER/FRAMEBUFFERS/fbo.fs");
		FBO.use();
		FBO.setInt("envFbo", 0);
		unsigned int fbo;
		glGenFramebuffers(1, &fbo);
		glBindFramebuffer(GL_FRAMEBUFFER, fbo);
		//Texture binding
		unsigned int textureColorbuffer;
		glGenTextures(1, &textureColorbuffer);
		glBindTexture(GL_TEXTURE_2D, textureColorbuffer);
		glTexImage2D(GL_TEXTURE_2D, 0, GL_SRGB, 800, 600, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL); //'SRGB' is used for Gamma Correction
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
		glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, textureColorbuffer, 0);

		//Render buffer object for depth and stencil attachment buffers
		unsigned int rbo;
		glGenRenderbuffers(1, &rbo);
		glBindRenderbuffer(GL_RENDERBUFFER, rbo);
		glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, 800, 600); // use a single renderbuffer object for both a depth AND stencil buffer.
		glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, rbo); // now actually attach it

		if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
			cout << "ERROR::FRAMEBUFFER:: Framebuffer is not complete!" << endl;
		//Go back to standard framebuffer
		glBindFramebuffer(GL_FRAMEBUFFER, 0);
		unsigned int FBOQuad = DrawFBOQuad();

		//For post-processing shader
		FBOLayer PP;
		unsigned int fboPP = PP.initFBO(&FBO);
		unsigned int rboPP = PP.initRBO();
#pragma endregion
#pragma region GBUFFER ENVIRONMENT
		GBuffer gbuf;
		gbuf.Gen_GNormal();
		gbuf.Gen_GAlbedo();
		gbuf.Gen_GPosition();
		//Produce RBO
		GLuint layers[3] = {gbuf.gPosition,gbuf.gNormal,gbuf.gAlbedo};
		glDrawBuffers(3, layers);
		gbuf.Gen_RBO();
		//Deferred Shading texture shader
		Shader gBufferLightingPass("MultiplexAssets/SHADER/FRAMEBUFFERS/DeferredRender/deferred.vs",
			"MultiplexAssets/SHADER/FRAMEBUFFERS/DeferredRender/deferred.fs");
		Shader gBufferGeomtryPass("MultiplexAssets/SHADER/FRAMEBUFFERS/DeferredRender/gbuffer.vs",
			"MultiplexAssets/SHADER/FRAMEBUFFERS/DeferredRender/gbuffer.fs");

		//Set Deferred Lighting pass
		gBufferLightingPass.use();
		gBufferLightingPass.setInt("gPosition", 0);
		gBufferLightingPass.setInt("gNormal", 1);
		gBufferLightingPass.setInt("gAlbedoSpec", 2);
#pragma endregion
		glEnable(GL_DEPTH_TEST);
		//Anti-aliasing
		//glEnable(GL_MULTISAMPLE);

		// render loop
		while (!glfwWindowShouldClose(window))
		{
			float currentTime = glfwGetTime();
			deltaTime = currentTime - lastFraming;
			lastFraming = currentTime;

			//Environment framebuffer
			glBindFramebuffer(GL_FRAMEBUFFER, fbo);
			//Begin GUI Render
			ImGui_ImplOpenGL3_NewFrame();
			ImGui_ImplGlfw_NewFrame();
			ImGui::NewFrame();

			// input
			processInput(window);
			glEnable(GL_DEPTH_TEST);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

			glm::mat4 model = glm::mat4(1.0f);
			glm::mat4 view = cameraEngine.GetViewMatrix();
			glm::mat4 projection = glm::perspective(glm::radians(cameraEngine.Zoom), get_resolution(window),
				0.1f, 200.0f);

#pragma region SKYBOX RENDER
			//Skybox Render
			glDepthMask(GL_FALSE);
			cubemapShader.use();
			cubemapShader.setMat4("projection", projection);
			view = glm::mat4(glm::mat3(cameraEngine.GetViewMatrix()));
			cubemapShader.setMat4("view", view);
			glBindVertexArray(skybox);
			glBindTexture(GL_TEXTURE_CUBE_MAP, cubemapTextures);
			glDrawArrays(GL_TRIANGLES, 0, 36);
			glDepthMask(GL_TRUE);
			view = cameraEngine.GetViewMatrix();
#pragma endregion

			//render
			glClearColor(0.161, 0.161, 0.161, 1.0f);

			//Grid
			glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
			glLineWidth(8.5);
			GridShader.use();
			model = glm::mat4(1.0);
			model = glm::translate(model, glm::vec3(0, -1, 0));
			GridShader.setMat4("model", model);
			GridShader.setMat4("projection", projection);
			GridShader.setMat4("view", view);
			GridShader.setFloat("timer", glfwGetTime());
			glBindVertexArray(GridVAO);
			glDrawArrays(GL_TRIANGLES, 0, sizeGrid);
			glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);

			//Quad
			quadShader.use();
			model = glm::mat4(1.0);
			model = glm::translate(model, glm::vec3(1.5, 2., 0));
			quadShader.setMat4("model", model);
			quadShader.setMat4("projection", projection);
			quadShader.setMat4("view", view);
			quadShader.setVec3("viewPos", cameraEngine.Position);
			quadShader.setFloat3("lightPos", 2.0f, 2.0f, 3.0f);
			quadShader.setInt("brick", 0);
			glActiveTexture(GL_TEXTURE0);
			brickTexture.Use();
			quadShader.setInt("brickMap", 1);
			glActiveTexture(GL_TEXTURE1);
			brickNormals.Use();
			glBindVertexArray(quadRender);
			glDrawArrays(GL_TRIANGLES, 0, 6);

			//Draw Cube
			model = glm::translate(model, glm::vec3(-2, -1, 0));
			model = glm::rotate(model, (float)glfwGetTime(), glm::vec3(0, 1, 0));
			CubeShader.use();
			CubeShader.setMat4("model", model);
			CubeShader.setMat4("projection", projection);
			CubeShader.setMat4("view", view);
			CubeShader.setFloat3("viewPos", cameraEngine.Position.x, cameraEngine.Position.y,
				cameraEngine.Position.z);
			glBindVertexArray(VAOCube);
			glDrawArrays(GL_TRIANGLES, 0, 36);

			//Model import
			mdl_shd.use();
			mdl_shd.setMat4("projection", projection);
			mdl_shd.setMat4("view", view);
			mdl_shd.setVec3("viewPos", cameraEngine.Position);
			mdl_shd.setFloat("multiplex_time", glfwGetTime()); //time var for model shader
			//Missing texture
			missing_tex.use();
			missing_tex.setMat4("projection", projection);
			missing_tex.setMat4("view", view);
			missing_tex.setVec3("viewPos", cameraEngine.Position);
			if (modelLists.size() > 0) {
				for (int i = 0; i < modelLists.size(); i++) {
					model = glm::mat4(1.);
					model = glm::translate(model, modelPos.at(i));
					if (modelLists.at(i).textures_loaded.size() == 0) {
						missing_tex.use();
						missing_tex.setMat4("model",model);
						modelLists.at(i).Draw(missing_tex);
					} else {
						mdl_shd.use();
						mdl_shd.setMat4("model", model);
						modelLists.at(i).Draw(mdl_shd);
					}
				}
			}

			//Water render
#pragma region WATER RENDER
			watMats.DrawWaterPlane(&waterMat,waterTexture, dUdVTexture, cameraEngine.Position,
				cubemapTextures,model,projection,view);
#pragma endregion
			//Light Source
			glEnable(GL_BLEND);
			lightSources.use();
			lightSources.setMat4("projection", projection);
			lightSources.setMat4("view", view);
			lightSources.setInt("lightSource", 0);
			glActiveTexture(GL_TEXTURE0);
			lightSourceTxt.Use();
            glBindVertexArray(quadRender);
			for (int i = 0; i < lightPos.size(); i++){
				model = glm::mat4(1.);
				model = glm::translate(model, lightPos.at(i));
				model = glm::scale(model, glm::vec3(.5, .3, 0.));
				lightSources.setMat4("model", model);
				glDrawArrays(GL_TRIANGLES, 0, 6);
			}
			glDisable(GL_BLEND);

			//Render GUI
			glfwSetInputMode(window, GLFW_CURSOR, (isMouseVisible) ?
				GLFW_CURSOR_NORMAL : GLFW_CURSOR_DISABLED);
			glfwGetWindowSize(window, &window_width, &window_height);

#pragma region FBO_MAIN_REAL_TIME_CONFIG
			int Wind_Width,Wind_Height;
			glfwGetWindowSize(window, &Wind_Width, &Wind_Height);
			glBindTexture(GL_TEXTURE_2D, textureColorbuffer);
			glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, Wind_Width, Wind_Height, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL);
			glBindRenderbuffer(GL_RENDERBUFFER, rbo);
			glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, Wind_Width, Wind_Height);

			glViewport(0, 0, Wind_Width, Wind_Height);
			glBindFramebuffer(GL_FRAMEBUFFER, 0);
			glDisable(GL_DEPTH_TEST);
			glClearColor(0.0f, .0f, .0f, 1.0f);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

#pragma region Post Processing Buffer
			PP.begin(fboPP);
			glActiveTexture(GL_TEXTURE0);
			FBO.use();
			glBindVertexArray(FBOQuad);
			glBindTexture(GL_TEXTURE_2D, textureColorbuffer);
			//GET POST PROCESSING TEXTURE
			glDrawArrays(GL_TRIANGLES, 0, 6);
			PP.BindTexture(rboPP, Wind_Width, Wind_Height);
			PP.initMainFBO(Wind_Width, Wind_Height);
#pragma endregion

#pragma endregion

#pragma region GUI_RENDER
			ImGui::Begin(" ", &renderGUI, ImGuiWindowFlags_MenuBar|ImGuiWindowFlags_NoTitleBar
			| ImGuiWindowFlags_NoMove);
			ImGui::SetWindowPos(ImVec2(0,0));
			ImGui::SetWindowSize(ImVec2(window_width/1.3,window_height/1.4));
			float offSetY = ImGui::GetWindowHeight();
			float offSetX = ImGui::GetWindowWidth();
			if (ImGui::BeginMenuBar()){
				if (ImGui::BeginMenu("File")){
					if (ImGui::BeginMenu("Open", "Ctrl+O")) {
						if (ImGui::MenuItem("Project Environment")) {}
						if (ImGui::MenuItem("3D Model")) {}
						ImGui::EndMenu();
					}
					if (ImGui::MenuItem("Save", "Ctrl+S")) { /* Do stuff */ }
					if (ImGui::MenuItem("Close", "Ctrl+W")) glfwSetWindowShouldClose(window, true);
					ImGui::EndMenu();
				}
				if (ImGui::BeginMenu("Edit"))
					ImGui::EndMenu();
				if (ImGui::BeginMenu("Settings")) {
					if (ImGui::MenuItem("Hide mouse/Camera viewport","HOME"))
						isMouseVisible = !isMouseVisible;
					ImGui::EndMenu();
				}
				if (ImGui::BeginMenu("Object")){
					if (ImGui::BeginMenu("Wavefront Model")){ImGui::EndMenu();}
					ImGui::Separator();
					if (ImGui::BeginMenu("Light")) {
						if (ImGui::MenuItem("Environment Light"))
							lightPos.push_back(cameraEngine.Position);
						if (ImGui::MenuItem("Point Light")) {}
						ImGui::EndMenu();
					}
					ImGui::Separator();
					if (ImGui::BeginMenu("Material")) {
						if (ImGui::MenuItem("Flat Water"))
							watMats.positions.push_back(cameraEngine.Position);
						if (ImGui::MenuItem("Fire")) {}
						ImGui::EndMenu();
					}
					ImGui::EndMenu();
				}
				if (ImGui::BeginMenu("Post-processing")) {
					ImGui::EndMenu();
				}
				ImGui::EndMenuBar();
			}
			if (ImGui::BeginTabBar("", ImGuiTabBarFlags_AutoSelectNewTabs)) {
				if (ImGui::BeginTabItem("Game (FBO)")) {
					ImGui::Image((void*)PP.textureBuffer, ImGui::GetWindowSize(),
						ImVec2(0,1),ImVec2(1,0));
					ImGui::EndTabItem();
				}
				ImGui::EndTabBar();
			}
			if (ImGui::IsKeyPressed(io.KeyMap[ImGuiKey_Home]))
				isMouseVisible = !isMouseVisible;
			ImGui::End();
			//Console and Shader tab
#pragma region Console and Shader Window
			ImGui::Begin("Utilities", NULL, ImGuiWindowFlags_NoCollapse|
				ImGuiWindowFlags_NoMove);
			ImGui::SetWindowPos(ImVec2(0,offSetY));
			ImGui::SetWindowSize(ImVec2(window_width,window_height-offSetY));
			float offSetY_1 = ImGui::GetWindowHeight();
			if (ImGui::BeginTabBar("", ImGuiTabBarFlags_AutoSelectNewTabs)) {
				if (ImGui::BeginTabItem("Console")) {
					ImGui::Text("Multiplex Engine console");
					ImGui::Separator();
					ImGui::InputTextMultiline("Input Console", guiBuf, IM_ARRAYSIZE(guiBuf),
						ImVec2(-1, ImGui::GetWindowContentRegionMax().y-100),
						ImGuiInputTextFlags_ReadOnly);
					ImGui::PushItemWidth(ImGui::GetWindowContentRegionMax().x - 10);
					ImGui::InputText(" ", inpBuf, IM_ARRAYSIZE(inpBuf));
					ImGui::PopItemWidth();
					ImGui::EndTabItem();
				}
				if (ImGui::BeginTabItem("Textures")) {
					ImGui::EndTabItem();
				}
				if (ImGui::BeginTabItem("Shaders")) {
					ImGui::InputTextMultiline(" ", shaderText, IM_ARRAYSIZE(shaderText),
						ImVec2(-1, ImGui::GetWindowContentRegionMax().y - 70));
					if (ImGui::Button("Execute"))
					{
						mdl_shd.CompileFragmentFromString(selectedModel->fragmentShaderDirectory.c_str(),
							shaderText);
					}
					ImGui::EndTabItem();
				}
				if (ImGui::BeginTabItem("Lua Env.")) {
					ImGui::InputTextMultiline(" ", luaText, IM_ARRAYSIZE(luaText),
						ImVec2(-1, ImGui::GetWindowContentRegionMax().y - 100));
					if (ImGui::Button("Execute"))
						isLuaExec = true;
					ImGui::SameLine();
					if (ImGui::Button("Load file")){}
					ImGui::SameLine();
					if (ImGui::Button("Save file")){}
					ImGui::Text(
						(to_string(cameraEngine.Position.x)+" "+to_string(cameraEngine.Position.y)+" "+
							to_string(cameraEngine.Position.z)).c_str());
					ImGui::EndTabItem();
				}
				ImGui::EndTabBar();
			}
#pragma region COMMAND LIST
			if (ImGui::IsKeyPressed(io.KeyMap[ImGuiKey_Enter])) {
				string inpParams(inpBuf);
				trim(inpParams);
				inpBuf[0] = '\0';
				vector<string> argums = splitStr(inpParams, " ");
				if (argums.at(0) == "help") {
					const char* help = R"(
--- List of commands ---
-- help - shows a list of commands
-- fullscreen - fullscreen window
-- lua [-e [script..] | -v]* -- Lua scripting environment
>
)";
					strncat_s(guiBuf, help, strlen(help));
				}
				else if (argums.at(0) == "lua") {
					if (argums.size() > 1) {
						if (argums.at(1) == "-v") {
							string MV_DESC = "\nMULTIPLEX ENGINE (MV3D)\nLUA VER:\n";
							MV_DESC.append((to_string(lua_version(L)) + "\n>").c_str());
							strncat_s(guiBuf, MV_DESC.c_str(), strlen(MV_DESC.c_str()));
						}
						else if (argums.at(1) == "-e") {
							string lua_exec;
							/*for (int i = 2; i < argums.size() - 1; i++)
								lua_exec.append(argums.at(i) + " ");
							lua_exec.append(argums.at(argums.size() - 1));
							if (luaL_dostring(L, lua_exec.c_str()) == 0) {
								lua_getglobal(L, "CameraEnginePosX");
								if (lua_isnumber(L, -1))
									cameraEngine.Position.x = lua_tonumber(L, -1);
							}*/
							isLuaExec = true;
						}
					}
					else {
						string MV_LUA_ENV = R"(
Lua environment enabled. To exit the enviornment type 'lua_exit'
>> 
)";
						strncat_s(guiBuf, MV_LUA_ENV.c_str(), strlen(MV_LUA_ENV.c_str()));
					}
				}
				else if (argums.at(0) == "fullscreen") {
					int width_A, height_A;
					GetDesktopResolution(width_A, height_A);
					glfwSetWindowPos(window, 0, 0);
					glfwSetWindowSize(window, width_A, height_A);
					const char* help = "\nChanged to fullscreen mode\n>";
					strncat_s(guiBuf, help, strlen(help));
				}
				else if (argums.at(0) == "water_obj") {
					watMats.positions.push_back(cameraEngine.Position);
					const char* help = "\nSuccessfully created water object\n>";
					strncat_s(guiBuf, help, strlen(help));
				}
			}
#pragma endregion
			ImGui::End();
#pragma endregion
#pragma region Model list
			ImGui::Begin("Resources", NULL, ImGuiWindowFlags_NoMove|ImGuiWindowFlags_NoCollapse);
			ImGui::SetWindowPos(ImVec2(offSetX, 0));
			ImGui::SetWindowSize(ImVec2(window_width-offSetX,(window_height-offSetY_1)/2));
			if (ImGui::BeginTabBar("", ImGuiTabBarFlags_AutoSelectNewTabs)) {
				if (ImGui::BeginTabItem("Hierarchy")) {
					ImGui::Text("List of models");
					ImGui::Separator();
					for (int i = 0; i < modelLists.size(); i++) {
						if (ImGui::TreeNode(modelLists.at(i).directory.c_str())) {
							string positionalIndex = to_string(modelPos.at(i).x);
							positionalIndex.append(", " + to_string(modelPos.at(i).y));
							positionalIndex.append(", " + to_string(modelPos.at(i).z));
							ImGui::Text(positionalIndex.c_str());
							string shaderUse = "FShader: ";
							shaderUse.append(modelLists.at(i).fragmentShaderDirectory);
							ImGui::Text(shaderUse.c_str());
							//Load selected model directory
							selectedModel = &modelLists.at(i);
							//Load selected model fragment shader
							ifstream readSh(selectedModel->fragmentShaderDirectory.c_str());
							stringstream readStream;
							readStream << readSh.rdbuf();
							strcpy_s(shaderText, readStream.str().c_str());
							readSh.close();

							ImGui::TreePop();
						}
					}
					ImGui::Text("List of lights");
					ImGui::Separator();
					for (int i = 0; i < lightPos.size(); i++) {
						if (ImGui::TreeNode("Env. Light")) {
							string positionalIndex = to_string(lightPos.at(i).x);
							positionalIndex.append(", " + to_string(lightPos.at(i).y));
							positionalIndex.append(", " + to_string(lightPos.at(i).z));
							ImGui::Text(positionalIndex.c_str());
							ImGui::TreePop();
						}
					}
					ImGui::EndTabItem();
				}
				if (ImGui::BeginTabItem("Lua")) {
					for (int i = 0; i < luaFiles.size(); i++) {
						if (ImGui::TreeNode(luaFiles.at(i).c_str()))
							ImGui::Text("Lua Script");
					}
					ImGui::EndTabItem();
				}
				ImGui::EndTabBar();
			}
			ImGui::End();
#pragma endregion
#pragma region Inspector
			ImGui::Begin("Inspector", NULL, ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoCollapse);
			ImGui::SetWindowPos(ImVec2(offSetX,(window_height - offSetY_1) / 2));
			ImGui::SetWindowSize(ImVec2(window_width - offSetX, (window_height - offSetY_1) / 2));
			ImGui::Text("Selected Model");
			if (selectedModel != nullptr) {
				ImGui::Text(selectedModel->directory.c_str());
			}
			if (ImGui::CollapsingHeader("Transform")) {
				ImGui::Text("X:");
				ImGui::Text("Y:");
				ImGui::Text("Z:");
			}
			if (ImGui::CollapsingHeader("Materials")) {
				//ImGui::Text("Diffuse Map");
				if (selectedModel != nullptr) {
					vector<Texture> texSel = selectedModel->textures_loaded;
					for (int i = 0; i < texSel.size(); i++){
						string texDet = texSel.at(i).type.c_str();
						texDet.append("\n");
						texDet.append(texSel.at(i).path.c_str());
						ImGui::Text(texDet.c_str());
						ImGui::Image((void*)texSel.at(i).id, ImVec2(128,128));
						ImGui::Separator();
					}
				}
			}
			ImGui::End();
#pragma endregion
			//End IMGUI Buffer
			ImGui::Render();
			ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
#pragma endregion
			isLuaExec = false;
			glBindVertexArray(0);
			glfwSwapBuffers(window);
			glfwPollEvents();
		}
		});
		//Lua scripting thread
		thread hg([]{
			LuaEnv* luaenv = new LuaEnv();
			while (true) {
				if (isLuaExec) {
					lua_register(luaenv->D, "GetTime", lua_GetTime);
					luaenv->SetCameraTable(&cameraEngine, luaText);
				}
			}
			delete luaenv;
			});
		hg.join();
	//Main render loop thread
	t2.join();
	delete selectedModel;
	// glfw: terminate, clearing all previously allocated GLFW resources.
	glfwTerminate();
	return 0;
}

void processInput(GLFWwindow* window)
{
	if (!isMouseVisible) {
		if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
			glfwSetWindowShouldClose(window, true);
		//Movement
		float speed = glfwGetKey(window, GLFW_KEY_LEFT_SHIFT) == GLFW_PRESS ? 8. * deltaTime
			: 2. * deltaTime;
		if (glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS)
			cameraEngine.ProcessKeyboard(FORWARD, speed);
		if (glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS)
			cameraEngine.ProcessKeyboard(BACKWARD, speed);
		if (glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS)
			cameraEngine.ProcessKeyboard(LEFT, speed);
		if (glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS)
			cameraEngine.ProcessKeyboard(RIGHT, speed);
		if (glfwGetKey(window, GLFW_KEY_SPACE) == GLFW_PRESS)
			cameraEngine.ProcessKeyboard(UP, speed);
		if (glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) == GLFW_PRESS)
			cameraEngine.ProcessKeyboard(DOWN, speed);

		//Controller support
		if (glfwJoystickPresent(GLFW_JOYSTICK_1))
		{
			//Controller look sensitivity
			float sensitivity = 10.5f;
			int axesCount;
			const float* axes = glfwGetJoystickAxes(GLFW_JOYSTICK_1, &axesCount);
			//Left Stick X axis
			if (axes[0] <= 1 && axes[0] >= -1)
				cameraEngine.ProcessKeyboard(RIGHT, speed * axes[0]);
			//Left Stick Y axis
			if (axes[1] <= 1 && axes[1] >= -1)
				cameraEngine.ProcessKeyboard(BACKWARD, speed * axes[1]);
			//Right Stick X and Y axis (mouse look)
			cameraEngine.ProcessMouseMovement(axes[2] * sensitivity, -axes[3] * sensitivity);
		}
	}
}
void framebuffer_size_callback(GLFWwindow* window, int width, int height)
{
	// make sure the viewport matches the new window dimensions; note that width and 
	// height will be significantly larger than specified on retina displays.
	glViewport(0, 0, width, height);
}
void GetDesktopResolution(int& horizontal, int& vertical)
{
	RECT desktop;
	// Get a handle to the desktop window
	const HWND hDesktop = GetDesktopWindow();
	// Get the size of screen to the variable desktop
	GetWindowRect(hDesktop, &desktop);
	// The top left corner will have coordinates (0,0)
	// and the bottom right corner will have coordinates
	// (horizontal, vertical)
	horizontal = desktop.right;
	vertical = desktop.bottom;
}
//UBO Setup
unsigned int SetUBO(Shader uboShader)
{
	unsigned int uniformBlockRed = glGetUniformBlockIndex(uboShader.ID, "Matricies");
	glUniformBlockBinding(uboShader.ID, uniformBlockRed, 0);
	unsigned int uboMatricies;
	glGenBuffers(1, &uboMatricies);
	glBindBuffer(GL_UNIFORM_BUFFER, uboMatricies);
	glBufferData(GL_UNIFORM_BUFFER, 2 * sizeof(glm::mat4), NULL, GL_STATIC_DRAW);
	glBindBuffer(GL_UNIFORM_BUFFER, 0);
	glBindBufferRange(GL_UNIFORM_BUFFER, 0, uboMatricies, 0, 2 * sizeof(glm::mat4));
	return uboMatricies;
}
//VAO SETUP
#pragma region VAOs
unsigned int SetCubeVAO() {
	float cubeArr[] = {
		// positions          // normals           // texture coords
	   -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f,  0.0f,
		0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f,  0.0f,
		0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f,  1.0f,
		0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f,  1.0f,
	   -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f,  1.0f,
	   -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f,  0.0f,

	   -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f,  0.0f,
		0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f,  0.0f,
		0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f,  1.0f,
		0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f,  1.0f,
	   -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f,  1.0f,
	   -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f,  0.0f,

	   -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f,  0.0f,
	   -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  1.0f,  1.0f,
	   -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
	   -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
	   -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  0.0f,  0.0f,
	   -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f,  0.0f,

		0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f,  0.0f,
		0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  1.0f,  1.0f,
		0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
		0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f,  1.0f,
		0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  0.0f,  0.0f,
		0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f,  0.0f,

	   -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f,  1.0f,
		0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  1.0f,  1.0f,
		0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f,  0.0f,
		0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f,  0.0f,
	   -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  0.0f,  0.0f,
	   -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f,  1.0f,

	   -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f,  1.0f,
		0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  1.0f,  1.0f,
		0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f,  0.0f,
		0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f,  0.0f,
	   -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f,  0.0f,
	   -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f,  1.0f
	};
	unsigned int VAOCube, VBOCube;
	glGenVertexArrays(1, &VAOCube);
	glGenBuffers(1, &VBOCube);

	glBindVertexArray(VAOCube);

	glBindBuffer(GL_ARRAY_BUFFER, VBOCube);
	glBufferData(GL_ARRAY_BUFFER, sizeof(cubeArr), cubeArr, GL_STATIC_DRAW);

	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)0);
	glEnableVertexAttribArray(0);
	// texture coord attribute
	glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)(3 * sizeof(float)));
	glEnableVertexAttribArray(1);

	glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)(6 * sizeof(float)));
	glEnableVertexAttribArray(2);
	return VAOCube;
}
unsigned int renderQuad()
{
	unsigned int quadVAO, quadVBO;
	// positions
	glm::vec3 pos1(-1.0f, 1.0f, 0.0f);
	glm::vec3 pos2(-1.0f, -1.0f, 0.0f);
	glm::vec3 pos3(1.0f, -1.0f, 0.0f);
	glm::vec3 pos4(1.0f, 1.0f, 0.0f);
	// texture coordinates
	glm::vec2 uv1(0.0f, 1.0f);
	glm::vec2 uv2(0.0f, 0.0f);
	glm::vec2 uv3(1.0f, 0.0f);
	glm::vec2 uv4(1.0f, 1.0f);
	// normal vector
	glm::vec3 nm(0.0f, 0.0f, 1.0f);

	// calculate tangent/bitangent vectors of both triangles
	glm::vec3 tangent1, bitangent1;
	glm::vec3 tangent2, bitangent2;

	// triangle 1
	glm::vec3 edge1 = pos2 - pos1;
	glm::vec3 edge2 = pos3 - pos1;
	glm::vec2 deltaUV1 = uv2 - uv1;
	glm::vec2 deltaUV2 = uv3 - uv1;

	float f = 1.0f / (deltaUV1.x * deltaUV2.y - deltaUV2.x * deltaUV1.y);

	tangent1.x = f * (deltaUV2.y * edge1.x - deltaUV1.y * edge2.x);
	tangent1.y = f * (deltaUV2.y * edge1.y - deltaUV1.y * edge2.y);
	tangent1.z = f * (deltaUV2.y * edge1.z - deltaUV1.y * edge2.z);

	bitangent1.x = f * (-deltaUV2.x * edge1.x + deltaUV1.x * edge2.x);
	bitangent1.y = f * (-deltaUV2.x * edge1.y + deltaUV1.x * edge2.y);
	bitangent1.z = f * (-deltaUV2.x * edge1.z + deltaUV1.x * edge2.z);

	// triangle 2
	edge1 = pos3 - pos1;
	edge2 = pos4 - pos1;
	deltaUV1 = uv3 - uv1;
	deltaUV2 = uv4 - uv1;

	f = 1.0f / (deltaUV1.x * deltaUV2.y - deltaUV2.x * deltaUV1.y);

	tangent2.x = f * (deltaUV2.y * edge1.x - deltaUV1.y * edge2.x);
	tangent2.y = f * (deltaUV2.y * edge1.y - deltaUV1.y * edge2.y);
	tangent2.z = f * (deltaUV2.y * edge1.z - deltaUV1.y * edge2.z);

	bitangent2.x = f * (-deltaUV2.x * edge1.x + deltaUV1.x * edge2.x);
	bitangent2.y = f * (-deltaUV2.x * edge1.y + deltaUV1.x * edge2.y);
	bitangent2.z = f * (-deltaUV2.x * edge1.z + deltaUV1.x * edge2.z);

	float quadVertices[] = {
		// positions            // normal         // texcoords  // tangent                          // bitangent
		pos1.x, pos1.y, pos1.z, nm.x, nm.y, nm.z, uv1.x, uv1.y, tangent1.x, tangent1.y, tangent1.z, bitangent1.x, bitangent1.y, bitangent1.z,
		pos2.x, pos2.y, pos2.z, nm.x, nm.y, nm.z, uv2.x, uv2.y, tangent1.x, tangent1.y, tangent1.z, bitangent1.x, bitangent1.y, bitangent1.z,
		pos3.x, pos3.y, pos3.z, nm.x, nm.y, nm.z, uv3.x, uv3.y, tangent1.x, tangent1.y, tangent1.z, bitangent1.x, bitangent1.y, bitangent1.z,

		pos1.x, pos1.y, pos1.z, nm.x, nm.y, nm.z, uv1.x, uv1.y, tangent2.x, tangent2.y, tangent2.z, bitangent2.x, bitangent2.y, bitangent2.z,
		pos3.x, pos3.y, pos3.z, nm.x, nm.y, nm.z, uv3.x, uv3.y, tangent2.x, tangent2.y, tangent2.z, bitangent2.x, bitangent2.y, bitangent2.z,
		pos4.x, pos4.y, pos4.z, nm.x, nm.y, nm.z, uv4.x, uv4.y, tangent2.x, tangent2.y, tangent2.z, bitangent2.x, bitangent2.y, bitangent2.z
	};
	// configure plane VAO
	glGenVertexArrays(1, &quadVAO);
	glGenBuffers(1, &quadVBO);
	glBindVertexArray(quadVAO);
	glBindBuffer(GL_ARRAY_BUFFER, quadVBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(quadVertices), &quadVertices, GL_STATIC_DRAW);
	glEnableVertexAttribArray(0);
	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 14 * sizeof(float), (void*)0);
	glEnableVertexAttribArray(1);
	glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 14 * sizeof(float), (void*)(3 * sizeof(float)));
	glEnableVertexAttribArray(2);
	glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, 14 * sizeof(float), (void*)(6 * sizeof(float)));
	glEnableVertexAttribArray(3);
	glVertexAttribPointer(3, 3, GL_FLOAT, GL_FALSE, 14 * sizeof(float), (void*)(8 * sizeof(float)));
	glEnableVertexAttribArray(4);
	glVertexAttribPointer(4, 3, GL_FLOAT, GL_FALSE, 14 * sizeof(float), (void*)(11 * sizeof(float)));
	return quadVAO;
}
unsigned int SetGridVAO(float beginning, float size,
	float sizeX, float sizeY, int* sizeOfVao) {
	std::vector<glm::vec3> vaoA;
	for (float x = beginning; x < sizeX; x++) {
		for (float y = beginning; y < sizeY; y++) {
			glm::vec3 p0 = glm::vec3(y * size, 0, x * size); //pos1
			glm::vec3 p1 = glm::vec3((y + 1) * size, 0, x * size); //pos2
			glm::vec3 p2 = glm::vec3((y + 1) * size, 0, (x + 1) * size); //pos3

			glm::vec3 p0_1 = glm::vec3(y * size, 0, x * size);
			glm::vec3 p1_1 = glm::vec3((y + 1) * size, 0, (x + 1) * size);
			glm::vec3 p2_1 = glm::vec3(y * size, 0, (x + 1) * size); //pos4

			vaoA.push_back(p0);
			vaoA.push_back(p1);
			vaoA.push_back(p2);
			vaoA.push_back(p0_1);
			vaoA.push_back(p1_1);
			vaoA.push_back(p2_1);
		}
	}
	unsigned int gridVAO, gridVBO;
	glGenVertexArrays(1, &gridVAO);
	glGenBuffers(1, &gridVBO);

	glBindVertexArray(gridVAO);
	glBindBuffer(GL_ARRAY_BUFFER, gridVBO);

	glBufferData(GL_ARRAY_BUFFER, sizeof(glm::vec3) * vaoA.size(), &vaoA[0].x, GL_STATIC_DRAW);
	glEnableVertexAttribArray(0);

	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), (void*)0);
	glEnableVertexAttribArray(1);
	*sizeOfVao = vaoA.size();
	return gridVAO;
}
unsigned int DrawFBOQuad() {
	float quadVertices[] = {
		// positions   // texCoords
		-1.0f,  1.0f,  0.0f, 1.0f,
		-1.0f, -1.0f,  0.0f, 0.0f,
		 1.0f, -1.0f,  1.0f, 0.0f,

		-1.0f,  1.0f,  0.0f, 1.0f,
		 1.0f, -1.0f,  1.0f, 0.0f,
		 1.0f,  1.0f,  1.0f, 1.0f
	};
	unsigned int quadVAO, quadVBO;
	glGenVertexArrays(1, &quadVAO);
	glGenBuffers(1, &quadVBO);
	glBindVertexArray(quadVAO);
	glBindBuffer(GL_ARRAY_BUFFER, quadVBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(quadVertices), &quadVertices, GL_STATIC_DRAW);
	glEnableVertexAttribArray(0);
	glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, 4 * sizeof(float), (void*)0);
	glEnableVertexAttribArray(1);
	glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, 4 * sizeof(float), (void*)(2 * sizeof(float)));
	return quadVAO;
}
unsigned int DrawTriangle() {
	float vertices[] = {
	 -0.5f, -0.5f, 0.0f,
	  0.5f, -0.5f, 0.0f,
	  0.0f,  0.5f, 0.0f
	};
	//VBO + VAO
	unsigned int VBO, VAO;
	glGenBuffers(1, &VBO);
	glGenVertexArrays(1, &VAO);

	glBindVertexArray(VAO);

	glBindBuffer(GL_ARRAY_BUFFER, VBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), (void*)0);
	glEnableVertexAttribArray(0);

	return VAO;
}
unsigned int PlaneVAO(){
	unsigned int planeVAO;
	float planeVertices[] = {
		// positions            // normals         // texcoords
		 25.0f, -0.5f,  25.0f,  0.0f, 1.0f, 0.0f,  25.0f,  0.0f,
		-25.0f, -0.5f,  25.0f,  0.0f, 1.0f, 0.0f,   0.0f,  0.0f,
		-25.0f, -0.5f, -25.0f,  0.0f, 1.0f, 0.0f,   0.0f, 25.0f,

		 25.0f, -0.5f,  25.0f,  0.0f, 1.0f, 0.0f,  25.0f,  0.0f,
		-25.0f, -0.5f, -25.0f,  0.0f, 1.0f, 0.0f,   0.0f, 25.0f,
		 25.0f, -0.5f, -25.0f,  0.0f, 1.0f, 0.0f,  25.0f, 25.0f
	};
	// plane VAO
	unsigned int planeVBO;
	glGenVertexArrays(1, &planeVAO);
	glGenBuffers(1, &planeVBO);
	glBindVertexArray(planeVAO);
	glBindBuffer(GL_ARRAY_BUFFER, planeVBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(planeVertices), planeVertices, GL_STATIC_DRAW);
	glEnableVertexAttribArray(0);
	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)0);
	glEnableVertexAttribArray(1);
	glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)(3 * sizeof(float)));
	glEnableVertexAttribArray(2);
	glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, 8 * sizeof(float), (void*)(6 * sizeof(float)));
	glBindVertexArray(0);
	return planeVAO;
}
//Default Skybox
unsigned int skyBoxVAO()
{
	float skyboxVertices[] = {
		// positions          
		-1.0f,  1.0f, -1.0f,
		-1.0f, -1.0f, -1.0f,
		 1.0f, -1.0f, -1.0f,
		 1.0f, -1.0f, -1.0f,
		 1.0f,  1.0f, -1.0f,
		-1.0f,  1.0f, -1.0f,

		-1.0f, -1.0f,  1.0f,
		-1.0f, -1.0f, -1.0f,
		-1.0f,  1.0f, -1.0f,
		-1.0f,  1.0f, -1.0f,
		-1.0f,  1.0f,  1.0f,
		-1.0f, -1.0f,  1.0f,

		 1.0f, -1.0f, -1.0f,
		 1.0f, -1.0f,  1.0f,
		 1.0f,  1.0f,  1.0f,
		 1.0f,  1.0f,  1.0f,
		 1.0f,  1.0f, -1.0f,
		 1.0f, -1.0f, -1.0f,

		-1.0f, -1.0f,  1.0f,
		-1.0f,  1.0f,  1.0f,
		 1.0f,  1.0f,  1.0f,
		 1.0f,  1.0f,  1.0f,
		 1.0f, -1.0f,  1.0f,
		-1.0f, -1.0f,  1.0f,

		-1.0f,  1.0f, -1.0f,
		 1.0f,  1.0f, -1.0f,
		 1.0f,  1.0f,  1.0f,
		 1.0f,  1.0f,  1.0f,
		-1.0f,  1.0f,  1.0f,
		-1.0f,  1.0f, -1.0f,

		-1.0f, -1.0f, -1.0f,
		-1.0f, -1.0f,  1.0f,
		 1.0f, -1.0f, -1.0f,
		 1.0f, -1.0f, -1.0f,
		-1.0f, -1.0f,  1.0f,
		 1.0f, -1.0f,  1.0f
	};
	unsigned int skyboxVAO, skyboxVBO;
	glGenVertexArrays(1, &skyboxVAO);
	glGenBuffers(1, &skyboxVBO);
	glBindVertexArray(skyboxVAO);
	glBindBuffer(GL_ARRAY_BUFFER, skyboxVBO);
	glBufferData(GL_ARRAY_BUFFER, sizeof(skyboxVertices), &skyboxVertices, GL_STATIC_DRAW);
	glEnableVertexAttribArray(0);
	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), (void*)0);
	return skyboxVAO;
}
unsigned int loadCubeMap(std::vector<std::string> faces)
{
	unsigned int textureID;
	glGenTextures(1, &textureID);
	glBindTexture(GL_TEXTURE_CUBE_MAP, textureID);

	int width, height, nrChannels;
	for (unsigned int i = 0; i < faces.size(); i++)
	{
		unsigned char* data = stbi_load(faces[i].c_str(), &width, &height, &nrChannels, 0);
		if (data)
		{
			glTexImage2D(GL_TEXTURE_CUBE_MAP_POSITIVE_X + i,
				0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, data);
			stbi_image_free(data);
		}
		else {
			cout << "Cube map texture failed to load at path: " << faces[i] << endl;
			stbi_image_free(data);
		}
	}
	glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
	glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
	glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);

	return textureID;
}
#pragma endregion