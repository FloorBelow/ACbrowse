using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using ACSharp;
using Force.Crc32;
using Num = System.Numerics;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ImGuiNET.SampleProgram.XNA
{
    /// <summary>
    /// Simple FNA + ImGui example
    /// </summary>
    public class AcBrowserGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private ImGuiRenderer _imGuiRenderer;

        private Texture2D _xnaTexture;
        private IntPtr _imGuiTexture;

        string[] forgenames;
        string[] forgepaths;
        Forge[] forges;
        ForgeFile[][][] files;
        Dictionary<uint, string> fileTypes;

        public AcBrowserGame()
        {
            Window.AllowUserResizing = true;
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.PreferMultiSampling = true;

            IsMouseVisible = true;

            forgepaths = Directory.EnumerateFiles(@"E:\Games\Ubisoft Game Launcher\games\Assassin's Creed II", "*.forge").ToArray<string>();
            forgenames = new string[forgepaths.Length]; for(int i = 0; i < forgenames.Length; i++) forgenames[i] = Path.GetFileName(forgepaths[i]);
            forges = new Forge[forgepaths.Length];
            files = new ForgeFile[forgepaths.Length][][];

            //forge = new Forge(@"E:\Games\Ubisoft Game Launcher\games\Assassin's Creed II\DataPC.forge", Games.AC2);
            //files = new ForgeFile[forge.datafileTable.Length][];
            //for(int i = 0; i < forge.datafileTable.Length; i++) { files[i] = forge.OpenDatafile(i, null, false); }
            fileTypes = new Dictionary<uint, string>();
            foreach(string line in File.ReadAllLines(@"F:\Extracted\AC\AC2FileTypes.txt")) {
                byte[] hashbytes = Encoding.ASCII.GetBytes(line);
                uint hash = Crc32Algorithm.Compute(hashbytes, 0, hashbytes.Length);
                fileTypes[hash] = line;
            }
        }

        protected override void Initialize()
        {
            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Texture loading example

			// First, load the texture as a Texture2D (can also be done using the XNA/FNA content pipeline)
			_xnaTexture = CreateTexture(GraphicsDevice, 300, 150, pixel =>
			{
				var red = (pixel % 300) / 2;
				return new Color(red, 1, 1);
			});

			// Then, bind it to an ImGui-friendly pointer, that we can use during regular ImGui.** calls (see below)
			_imGuiTexture = _imGuiRenderer.BindTexture(_xnaTexture);

            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(clear_color.X, clear_color.Y, clear_color.Z));

            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            // Draw our UI
            ImGuiLayout();

            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }

        // Direct port of the example at https://github.com/ocornut/imgui/blob/master/examples/sdl_opengl2_example/main.cpp
        private float f = 0.0f;

        private bool show_test_window = false;
        private bool show_another_window = false;
        private Num.Vector3 clear_color = new Num.Vector3(114f / 255f, 144f / 255f, 154f / 255f);
        private byte[] _textBuffer = new byte[100];

        protected virtual void ImGuiLayout()
        {
            // 1. Show a simple window
            // Tip: if we don't call ImGui.Begin()/ImGui.End() the widgets appears in a window automatically called "Debug"
            {
                for(int forge = 0; forge < forges.Length; forge++) {
                    if (ImGui.TreeNode(forgenames[forge])) {
                        if (forges[forge] is null) {
                            forges[forge] = new Forge(forgepaths[forge], Games.ac2);
                            files[forge] = new ForgeFile[forges[forge].datafileTable.Length][];
                        }
                        for (int dataFile = 0; dataFile < forges[forge].datafileTable.Length; dataFile++) {
                            if (ImGui.TreeNode(forges[forge].datafileTable[dataFile].name)) {
                                if (files[forge][dataFile] is null) files[forge][dataFile] = forges[forge].OpenDatafile(dataFile, null, false);
                                ImGui.BeginTable("files", 2);
                                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed);
                                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                                //ImGui.TableHeadersRow();
                                for (int file = 0; file < files[forge][dataFile].Length; file++) {
                                    ImGui.TableNextRow();
                                    if (fileTypes.ContainsKey(files[forge][dataFile][file].fileType)) {
                                        ImGui.TableSetColumnIndex(0);
                                        ImGui.Text(fileTypes[files[forge][dataFile][file].fileType]);
                                        ImGui.TableSetColumnIndex(1);
                                        ImGui.Text(files[forge][dataFile][file].name);
                                    }
                                }
                                ImGui.EndTable();
                                ImGui.TreePop();
                            }
                        }
                        ImGui.TreePop();
                    }

                }


            }


            // 2. Show another simple window, this time using an explicit Begin/End pair
            if (show_another_window)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(200, 100), ImGuiCond.FirstUseEver);
                ImGui.Begin("Another Window", ref show_another_window);
                ImGui.Text("Hello");
                ImGui.End();
            }

            // 3. Show the ImGui test window. Most of the sample code is in ImGui.ShowTestWindow()
            if (show_test_window)
            {
                ImGui.SetNextWindowPos(new Num.Vector2(650, 20), ImGuiCond.FirstUseEver);
                ImGui.ShowDemoWindow(ref show_test_window);
            }
        }

		public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
		{
			//initialize a texture
			var texture = new Texture2D(device, width, height);

			//the array holds the color for each pixel in the texture
			Color[] data = new Color[width * height];
			for(var pixel = 0; pixel < data.Length; pixel++)
			{
				//the function applies the color according to the specified pixel
				data[pixel] = paint( pixel );
			}

			//set the color
			texture.SetData( data );

			return texture;
		}
	}
}