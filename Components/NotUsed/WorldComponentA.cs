﻿namespace MineLib.PGL.Components.NotUsed
{
    // -- +X - East
    // -- -X - West
    // -- +Z - South
    // -- -Z - North
    // 4000 max
    // 16000 min

    /*
    public sealed class WorldComponentA : MineLibGameComponent
	{
#if DEBUG
        public static int Chunks;
	    public static int BuildedChunks;
        public static int DrawingOpaqueSections;
        public static int DrawingTransparentSections;
#endif
        //Clear Shader
        Effect Clear { get; set; }
        //GBuffer Shader
        Effect GBuffer { get; set; }

        RenderTargetBinding[] GBufferTargets { get; set; }

        //Fullscreen Quad
        FullscreenQuad fsq { get; set; }


        public Texture2D BasicTexture { get; set; }
        public Texture2D NormalTexture { get; set; }
        public Texture2D SpecularTexture { get; set; }

        SpriteBatch SpriteBatch { get; set; }



        readonly Minecraft _minecraft;
        readonly CameraComponentTC _camera;


        readonly List<ChunkVBO> _chunks = new List<ChunkVBO>();
        
        Task _builder;
        readonly CancellationTokenSource _cancellationToken;


        public WorldComponentA(Client game, CameraComponentTC camera, Minecraft minecraft) : base(game)
	    {
            _minecraft = minecraft;
            _camera = camera;

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            _cancellationToken = new CancellationTokenSource();

            var width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            var height = GraphicsDevice.PresentationParameters.BackBufferHeight;

            //Load Clear Shader
            Clear = Game.Content.Load<Effect>("Effects/Clear");
            //Clear.CurrentTechnique = Clear.Techniques[0];
            //Load GBuffer Shader
            GBuffer = Game.Content.Load<Effect>("Effects/GBuffer");
            //GBuffer.CurrentTechnique = GBuffer.Techniques[0];

            BasicTexture = Client.Blocks;
            NormalTexture = new Texture2D(GraphicsDevice, BasicTexture.Width, BasicTexture.Height);
            SpecularTexture = new Texture2D(GraphicsDevice, BasicTexture.Width, BasicTexture.Height);

            //Initialize GBuffer Targets Array
            GBufferTargets = new RenderTargetBinding[3];
            //Intialize Each Target of the GBuffer
            GBufferTargets[0] = new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8));
            GBufferTargets[1] = new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8));
            GBufferTargets[2] = new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Vector2, DepthFormat.Depth24Stencil8));

            //Create Fullscreen Quad
            fsq = new FullscreenQuad(GraphicsDevice);
	    }


        public void Build()
        {
            if (_minecraft != null && _minecraft.World != null && (_builder == null || _builder.IsCompleted))
            {
                var chunks = new Chunk[_minecraft.World.Chunks.Count];
                _minecraft.World.Chunks.CopyTo(chunks);
                _builder = Task.Factory.StartNew(() => BuildWorker(chunks), _cancellationToken.Token);
            }
        }

        private void BuildWorker(Chunk[] chunks)
		{
            _chunks.Clear();
            for (int i = 0; i < chunks.Length; i++)
            {
                var coords = chunks[i].Coordinates;

                var front   = FindChunk(chunks, coords + new Coordinates2D( 0,  1));
                var back    = FindChunk(chunks, coords + new Coordinates2D( 0, -1));

                var right   = FindChunk(chunks, coords + new Coordinates2D(-1,  0));
                var left    = FindChunk(chunks, coords + new Coordinates2D( 1,  0));


                if (_cancellationToken.IsCancellationRequested)
                    return;
                else
                    _chunks.Add(new ChunkVBO(GraphicsDevice, chunks[i], front, back, left, right));
            }
		}

	    private static Chunk FindChunk(Chunk[] chunks, Coordinates2D coords)
	    {
            for (int i = 0; i < chunks.Length; i++)
                if (chunks[i].Coordinates == coords)
                    return chunks[i];
            
            return null;
	    }

	    public void UpdateChunk(Chunk center, Chunk front, Chunk back, Chunk left, Chunk right)
	    {
            for (int i = 0; i < _chunks.Count; i++)
                if (_chunks[i].Coordinates2D == center.Coordinates)
                    _chunks[i] = new ChunkVBO(GraphicsDevice, center, front, back, left, right);
	    }

	    public void UpdateSection(int sectionIndex, Chunk center, Chunk front, Chunk back, Chunk left, Chunk right)
	    {
            for (int i = 0; i < _chunks.Count; i++)
                if (_chunks[i].Coordinates2D == center.Coordinates)
                    _chunks[i] = new ChunkVBO(GraphicsDevice, center, front, back, left, right);
	    }

	    public override void Update(GameTime gameTime)
		{
	        if (_minecraft != null && _minecraft.World != null)
	        {
	            if (_minecraft.World.Chunks.Count >= 441 && _minecraft.World.Chunks.Count > _chunks.Count)
	                Build();

#if DEBUG
                Chunks = _minecraft.World.Chunks.Count;
#endif
            }

#if DEBUG
            BuildedChunks = _chunks.Count;
#endif

            for (int i = 0; i < _chunks.Count; i++)
                if (_chunks[i] != null)
                    _chunks[i].Update();

	        if (_minecraft != null && _minecraft.World != null)
	        {
                var hours = (float) _minecraft.World.RealTime.Hours;
                //_solidBlockEffect.Parameters["TimeOfDay"].SetValue(hours);
                //_transparentBlockEffect.Parameters["TimeOfDay"].SetValue(hours);
	        }
		}

        public override void Draw(GameTime gameTime)
        {
            //Set States
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            //Clear GBuffer
            ClearGBuffer();
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            //Make GBuffer
            MakeGBuffer();

            GraphicsDevice.Clear(Color.CornflowerBlue);
            Debug();
        }

        public override void Dispose()
        {
            if (_cancellationToken != null)
                _cancellationToken.Cancel();

            if (_chunks != null)
            {
                foreach (var chunkVbo in _chunks)
                    chunkVbo.Dispose();

                _chunks.Clear();
            }
        }


        //GBuffer Creation
        private void MakeGBuffer()
        {
#if DEBUG
            DebugComponent.BoundingFrustumEnabled   = true;
            DrawingOpaqueSections                   = 0;
            DrawingTransparentSections              = 0;
#endif
            int vertices                            = 0;

            //Set Depth State
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Set up global GBuffer parameters
            _camera.ApplyTo(GBuffer);

            //Set WorldIT
            var t = Matrix.Transpose(Matrix.Invert(Matrix.Identity*_camera.View));
            GBuffer.Parameters["WorldViewIT"].SetValue(Matrix.Transpose(Matrix.Invert(Matrix.Identity * _camera.View)));
            //Set Albedo Texture
            GBuffer.Parameters["BaseTexture"].SetValue(BasicTexture);
            //Set Normal Texture
            //GBuffer.Parameters["NormalMap"].SetValue(NormalTexture);
            //Set Specular Texture
            GBuffer.Parameters["SpecularMap"].SetValue(SpecularTexture);
            //Apply Effect
            //GBuffer.CurrentTechnique.Passes[0].Apply();


            foreach (var pass in GBuffer.CurrentTechnique.Passes)
            {
                pass.Apply();

                for (int i = 0; i < _chunks.Count; i++)
                    if (_chunks[i] != null)
                    {
                        vertices += _chunks[i].TotalVerticesCount;
                        _chunks[i].DrawOpaque(_camera.BoundingFrustum);
                    }

                for (int i = 0; i < _chunks.Count; i++)
                    if (_chunks[i] != null)
                    {
                        vertices += _chunks[i].TotalVerticesCount;
                        _chunks[i].DrawTransparent(_camera.BoundingFrustum);
                    }
            }


            //Set RenderTargets off
            GraphicsDevice.SetRenderTargets(null);

#if DEBUG
            DebugComponent.Vertices                 = vertices;
#endif
        }

        //Clear GBuffer
        private void ClearGBuffer()
        {
            //Set to ReadOnly depth for now...
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            //Set GBuffer Render Targets
            GraphicsDevice.SetRenderTargets(GBufferTargets);
            //Set Clear Effect
            foreach (var pass in Clear.CurrentTechnique.Passes)
            {
                pass.Apply();

                //Draw
                fsq.Draw(GraphicsDevice);
            }
        }

        //Debug
        public void Debug()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //Begin SpriteBatch
            SpriteBatch.Begin();

            var width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            var height = GraphicsDevice.PresentationParameters.BackBufferHeight;

            Rectangle rect = new Rectangle(0, 0, width, height);

            //SpriteBatch.Draw((Texture2D)GBufferTargets[0].RenderTarget, rect, Color.White);
            SpriteBatch.Draw((Texture2D)GBufferTargets[1].RenderTarget, rect, Color.White);
            //SpriteBatch.Draw((Texture2D)GBufferTargets[2].RenderTarget, rect, Color.White);
            SpriteBatch.End();
        }
	}
    */
}
