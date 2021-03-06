﻿using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine
{
    using Engine.Common;
    using Engine.Helpers;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Graphics class
    /// </summary>
    public class Graphics : IDisposable
    {
        /// <summary>
        /// On resized event
        /// </summary>
        public event EventHandler Resized;

#if DEBUG
        /// <summary>
        /// Debug device
        /// </summary>
        private DeviceDebug deviceDebug = null;
        /// <summary>
        /// Debug information queue
        /// </summary>
        private InfoQueue deviceDebugInfoQueue = null;
#endif

        /// <summary>
        /// Vertical sync enabled
        /// </summary>
        private readonly bool vsyncEnabled = false;
        /// <summary>
        /// Multisample count
        /// </summary>
        private readonly int msCount = 1;
        /// <summary>
        /// Multisample quality
        /// </summary>
        private readonly int msQuality = 0;
        /// <summary>
        /// Graphics device
        /// </summary>
        private Device3 device = null;
        /// <summary>
        /// Graphics inmmediate context
        /// </summary>
        private readonly DeviceContext3 deviceContext = null;
        /// <summary>
        /// Swap chain
        /// </summary>
        private SwapChain4 swapChain = null;
        /// <summary>
        /// Render target view
        /// </summary>
        private EngineRenderTargetView renderTargetView = null;
        /// <summary>
        /// Depth stencil view
        /// </summary>
        private EngineDepthStencilView depthStencilView = null;

        /// <summary>
        /// Current depth-stencil state
        /// </summary>
        private EngineDepthStencilState currentDepthStencilState = null;
        /// <summary>
        /// Current blend state
        /// </summary>
        private EngineBlendState currentBlendState = null;
        /// <summary>
        /// Current rasterizer state
        /// </summary>
        private EngineRasterizerState currentRasterizerState = null;

        /// <summary>
        /// Depth stencil state with z-buffer enabled for write
        /// </summary>
        private EngineDepthStencilState depthStencilzBufferEnabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer disabled for write
        /// </summary>
        private EngineDepthStencilState depthStencilzBufferDisabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer enabled for read
        /// </summary>
        private EngineDepthStencilState depthStencilRDzBufferEnabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer disabled for read
        /// </summary>
        private EngineDepthStencilState depthStencilRDzBufferDisabled = null;
        /// <summary>
        /// No depth, no stencil
        /// </summary>
        private EngineDepthStencilState depthStencilNone = null;
        /// <summary>
        /// Depth stencil state for shadow mapping
        /// </summary>
        private EngineDepthStencilState depthStencilShadowMapping = null;

        /// <summary>
        /// Default blend state
        /// </summary>
        private EngineBlendState blendDefault = null;
        /// <summary>
        /// Default alpha blend state
        /// </summary>
        private EngineBlendState blendDefaultAlpha = null;
        /// <summary>
        /// Blend state for transparent blending
        /// </summary>
        private EngineBlendState blendTransparent = null;
        /// <summary>
        /// Additive blend state
        /// </summary>
        private EngineBlendState blendAdditive = null;

        /// <summary>
        /// Default rasterizer
        /// </summary>
        private EngineRasterizerState rasterizerDefault = null;
        /// <summary>
        /// Wireframe rasterizer
        /// </summary>
        private EngineRasterizerState rasterizerWireframe = null;
        /// <summary>
        /// No-cull rasterizer
        /// </summary>
        private EngineRasterizerState rasterizerNoCull = null;
        /// <summary>
        /// Cull counter-clockwise face rasterizer
        /// </summary>
        private EngineRasterizerState rasterizerCullFrontFace = null;
        /// <summary>
        /// Shadow mapping rasterizer state
        /// </summary>
        private EngineRasterizerState rasterizerShadowMapping = null;

        /// <summary>
        /// Current vertex buffer first slot
        /// </summary>
        private int currentVertexBufferFirstSlot = -1;
        /// <summary>
        /// Current vertex buffer bindings
        /// </summary>
        private VertexBufferBinding[] currentVertexBufferBindings = null;
        /// <summary>
        /// Current index buffer reference
        /// </summary>
        private Buffer currentIndexBufferRef = null;
        /// <summary>
        /// Current index buffer format
        /// </summary>
        private Format currentIndexFormat = Format.Unknown;
        /// <summary>
        /// Current index buffer offset
        /// </summary>
        private int currentIndexOffset = -1;
        /// <summary>
        /// Current primitive topology set in input assembler
        /// </summary>
        private PrimitiveTopology currentIAPrimitiveTopology = PrimitiveTopology.Undefined;
        /// <summary>
        /// Current input layout set in input assembler
        /// </summary>
        private InputLayout currentIAInputLayout = null;
        /// <summary>
        /// Null shader resources for shader clearing
        /// </summary>
        private readonly ShaderResourceView[] nullSrv = new ShaderResourceView[CommonShaderStage.InputResourceSlotCount];

        /// <summary>
        /// Back buffer format
        /// </summary>
        protected Format BufferFormat = BackBufferFormats.R8G8B8A8_UNorm;
        /// <summary>
        /// Depth buffer format
        /// </summary>
        protected Format DepthFormat = DepthBufferFormats.D24_UNorm_S8_UInt;

        /// <summary>
        /// Device description
        /// </summary>
        public string DeviceDescription { get; private set; }

        /// <summary>
        /// Screen viewport
        /// </summary>
        public ViewportF Viewport { get; private set; }
        /// <summary>
        /// Gets the default render target
        /// </summary>
        public EngineRenderTargetView DefaultRenderTarget
        {
            get
            {
                return this.renderTargetView;
            }
        }
        /// <summary>
        /// Gets the default depth stencil buffer
        /// </summary>
        public EngineDepthStencilView DefaultDepthStencil
        {
            get
            {
                return this.depthStencilView;
            }
        }
        /// <summary>
        /// Gets if the device was created with multi-sampling active
        /// </summary>
        public bool MultiSampled
        {
            get
            {
                return this.msCount > 1;
            }
        }
        /// <summary>
        /// Current sample description
        /// </summary>
        public SampleDescription CurrentSampleDescription
        {
            get
            {
                return new SampleDescription(this.msCount, this.msQuality);
            }
        }
        /// <summary>
        /// Gets or sets the input assembler's primitive topology
        /// </summary>
        public PrimitiveTopology IAPrimitiveTopology
        {
            get
            {
                return this.currentIAPrimitiveTopology;
            }
            set
            {
                if (this.currentIAPrimitiveTopology != value)
                {
                    this.deviceContext.InputAssembler.PrimitiveTopology = value;
                    Counters.IAPrimitiveTopologySets++;

                    this.currentIAPrimitiveTopology = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the input assembler's input layout
        /// </summary>
        public InputLayout IAInputLayout
        {
            get
            {
                return this.currentIAInputLayout;
            }
            set
            {
                if (this.currentIAInputLayout != value)
                {
                    this.deviceContext.InputAssembler.InputLayout = value;
                    Counters.IAInputLayoutSets++;

                    this.currentIAInputLayout = value;
                }
            }
        }

        /// <summary>
        /// Finds mode description
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="refreshRate">Refresh date</param>
        /// <param name="mode">Returns found mode description</param>
        private static void FindModeDescription(Device3 device, Format format, int width, int height, int refreshRate, out ModeDescription1 mode)
        {
#if DEBUG
            using (var tmpFactory = new Factory2(true))
#else
            using (var tmpFactory = new Factory2())
#endif
            using (var factory = tmpFactory.QueryInterface<Factory5>())
            {
                using (var tmpAdapter = factory.GetAdapter1(0))
                using (var adapter = tmpAdapter.QueryInterface<Adapter4>())
                {
                    using (var tmpOutput = adapter.GetOutput(0))
                    using (var output = tmpOutput.QueryInterface<Output6>())
                    {
                        try
                        {
                            var displayModeList = output.GetDisplayModeList1(
                                format,
                                DisplayModeEnumerationFlags.Interlaced);

                            displayModeList = Array.FindAll(displayModeList, d => d.Width == width && d.Height == height);
                            if (displayModeList.Length > 0)
                            {
                                if (refreshRate > 0)
                                {
                                    Array.Sort(displayModeList, (d1, d2) =>
                                    {
                                        float f1 = (float)d1.RefreshRate.Numerator / (float)d1.RefreshRate.Denominator;
                                        float f2 = (float)d2.RefreshRate.Numerator / (float)d2.RefreshRate.Denominator;

                                        f1 = Math.Abs(refreshRate - f1);
                                        f2 = Math.Abs(refreshRate - f2);

                                        return f1.CompareTo(f2);
                                    });
                                }
                                else
                                {
                                    Array.Sort(displayModeList, (d1, d2) =>
                                    {
                                        float f1 = (float)d1.RefreshRate.Numerator / (float)d1.RefreshRate.Denominator;
                                        float f2 = (float)d2.RefreshRate.Numerator / (float)d2.RefreshRate.Denominator;

                                        return f2.CompareTo(f1);
                                    });
                                }

                                mode = displayModeList[0];

                                return;
                            }

                            ModeDescription1 desc = new ModeDescription1()
                            {
                                Width = width,
                                Height = height,
                                Format = format,
                            };
                            output.FindClosestMatchingMode1(
                                ref desc,
                                out mode,
                                device);

                            mode.Width = width;
                            mode.Height = height;

                            return;
                        }
                        catch
                        {
                            // Display mode not found
                        }
                    }
                }
            }

            mode = new ModeDescription1()
            {
                Width = width,
                Height = height,
                Format = format,
                RefreshRate = new Rational(0, 1),
                Scaling = DisplayModeScaling.Unspecified,
                ScanlineOrdering = DisplayModeScanlineOrder.Unspecified,
            };
        }
        /// <summary>
        /// Checks the multi-sample specified count
        /// </summary>
        /// <param name="device">Temporary device</param>
        /// <param name="format">Format</param>
        /// <param name="multiSampling">Multi-sample count</param>
        /// <param name="sampleCount">Sample count</param>
        /// <param name="maxQualityLevel">Maximum quality level</param>
        /// <returns>Returns true y the device supports MS for the specified format</returns>
        private static bool CheckMultisample(Device3 device, Format format, int multiSampling, out int sampleCount, out int maxQualityLevel)
        {
            sampleCount = 1;
            maxQualityLevel = 0;
            int maxQuality = device.CheckMultisampleQualityLevels1(format, multiSampling, CheckMultisampleQualityLevelsFlags.None);
            if (maxQuality > 0)
            {
                sampleCount = multiSampling;
                maxQualityLevel = maxQuality - 1;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="form">Game form</param>
        /// <param name="vsyncEnabled">Vertical sync enabled</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampling">Enable multisampling</param>
        public Graphics(EngineForm form, bool vsyncEnabled = false, int refreshRate = 0, int multiSampling = 0)
        {
            FindModeDescription(
                this.device,
                this.BufferFormat,
                form.RenderWidth,
                form.RenderHeight,
                refreshRate,
                out var displayMode);

            this.vsyncEnabled = vsyncEnabled && displayMode.RefreshRate != new Rational(0, 1);

#if DEBUG
            using (var tmpFactory = new Factory2(true))
#else
            using (var tmpFactory = new Factory2())
#endif
            using (var factory = tmpFactory.QueryInterface<Factory5>())
            {
                using (var tmpAdapter = factory.GetAdapter1(0))
                using (var adapter = tmpAdapter.QueryInterface<Adapter4>())
                {
                    this.DeviceDescription = string.Format("{0}", adapter.Description2.Description);

                    DeviceCreationFlags creationFlags = DeviceCreationFlags.None;

#if DEBUG
                    creationFlags |= DeviceCreationFlags.Debug;
#endif
                    using (var tmpDevice = new Device(adapter, creationFlags, FeatureLevel.Level_11_1, FeatureLevel.Level_11_0))
                    {
                        this.device = tmpDevice.QueryInterface<Device3>();
                    }
                }

                if (multiSampling != 0 && !CheckMultisample(this.device, this.BufferFormat, multiSampling, out this.msCount, out this.msQuality))
                {
                    throw new EngineException(string.Format("The specified multisampling value [{0}] is not supported for {1}", multiSampling, this.BufferFormat));
                }

                var desc = new SwapChainDescription1()
                {
                    BufferCount = 2,
                    Format = displayMode.Format,
                    Width = displayMode.Width,
                    Height = displayMode.Height,
                    Stereo = displayMode.Stereo,
                    SampleDescription = this.CurrentSampleDescription,
                    AlphaMode = AlphaMode.Ignore,
                    Scaling = Scaling.Stretch,
                    Usage = Usage.RenderTargetOutput,
                    SwapEffect = SwapEffect.Sequential,
                    Flags = SwapChainFlags.None,
                };
                var fsdesc = new SwapChainFullScreenDescription()
                {
                    RefreshRate = displayMode.RefreshRate,
                    Scaling = displayMode.Scaling,
                    ScanlineOrdering = displayMode.ScanlineOrdering,
                    Windowed = !form.IsFullscreen,
                };

                using (var tmpSwapChain = new SwapChain1(factory, this.device, form.Handle, ref desc, fsdesc))
                {
                    this.swapChain = tmpSwapChain.QueryInterface<SwapChain4>();
                }
            }

            this.deviceContext = this.device.ImmediateContext3;

            this.PrepareDevice(displayMode.Width, displayMode.Height, false);

#if DEBUG
            this.ConfigureDebugLayer();
#endif

            #region Alt + Enter

            using (var factory = this.swapChain.GetParent<Factory5>())
            {
                factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);
            }

            form.KeyUp += (sender, eventArgs) =>
            {
                if (eventArgs.Alt && (int)eventArgs.KeyCode == (int)Keys.Enter)
                {
                    this.swapChain.IsFullScreen = !this.swapChain.IsFullScreen;
                }
            };

            #endregion
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Graphics()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.swapChain?.IsFullScreen == true)
                {
                    this.swapChain.IsFullScreen = false;
                }

                this.swapChain?.Dispose();
                this.swapChain = null;

                this.DisposeResources();

                this.device?.Dispose();
                this.device = null;

#if DEBUG
                this.deviceDebugInfoQueue?.Dispose();
                this.deviceDebugInfoQueue = null;

                this.deviceDebug?.ReportLiveDeviceObjects(ReportingLevel.Detail);
                this.deviceDebug?.Dispose();
                this.deviceDebug = null;
#endif
            }
        }

        /// <summary>
        /// Prepare device
        /// </summary>
        /// <param name="width">Render width</param>
        /// <param name="height">Render height</param>
        /// <param name="resizing">Sets whether the render screen is resizing or not</param>
        public void PrepareDevice(int width, int height, bool resizing)
        {
            if (resizing)
            {
                this.DisposeResources();

                this.swapChain.ResizeBuffers(2, width, height, this.BufferFormat, SwapChainFlags.None);
            }

            #region Viewport

            this.Viewport = new ViewportF()
            {
                X = 0,
                Y = 0,
                Width = width,
                Height = height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            };

            #endregion

            #region Render Target

            using (var backBuffer = Resource.FromSwapChain<Resource>(this.swapChain, 0))
            {
                this.renderTargetView = new EngineRenderTargetView(new RenderTargetView1(this.device, backBuffer));
            }

            #endregion

            #region Depth Stencil Buffer and View

            this.CreateDepthStencil(this.DepthFormat, width, height, true, out this.depthStencilView);

            #endregion

            #region Set Defaults

            this.SetDefaultViewport();
            this.SetDefaultRenderTarget();

            this.SetDepthStencilZEnabled();
            this.SetRasterizerDefault();
            this.SetBlendDefault();

            #endregion

            if (resizing)
            {
                //Launch the "resized" event
                this.Resized?.Invoke(this, new EventArgs());
            }
        }

#if DEBUG
        /// <summary>
        /// Configure debug layer messages
        /// </summary>
        private void ConfigureDebugLayer()
        {
            this.deviceDebug = this.device.QueryInterface<DeviceDebug>();
            this.deviceDebugInfoQueue = this.deviceDebug.QueryInterface<InfoQueue>();

            var severityFilter = new InfoQueueFilter()
            {
                AllowList = new InfoQueueFilterDescription()
                {

                },
                DenyList = new InfoQueueFilterDescription()
                {
                    Severities = new MessageSeverity[]
                    {
                        MessageSeverity.Information,
                        MessageSeverity.Message,
                    },
                }
            };

            var idFilter = new InfoQueueFilter()
            {
                AllowList = new InfoQueueFilterDescription()
                {

                },
                DenyList = new InfoQueueFilterDescription()
                {
                    Ids = new MessageId[]
                    {
                        MessageId.MessageIdDeviceDrawRenderTargetViewNotSet,
                    },
                }
            };

            this.deviceDebugInfoQueue.AddStorageFilterEntries(severityFilter);
            this.deviceDebugInfoQueue.AddStorageFilterEntries(idFilter);
        }
#endif

        /// <summary>
        /// Begin frame
        /// </summary>
        public void Begin()
        {
            this.deviceContext.ClearDepthStencilView(
                this.depthStencilView.GetDepthStencil(),
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1.0f,
                0);

            this.deviceContext.ClearRenderTargetView(
                this.renderTargetView.GetRenderTarget(),
                GameEnvironment.Background);
        }
        /// <summary>
        /// End frame
        /// </summary>
        public void End()
        {
            if (this.vsyncEnabled)
            {
                this.swapChain.Present(1, PresentFlags.None);
            }
            else
            {
                this.swapChain.Present(0, PresentFlags.None);
            }
        }
        /// <summary>
        /// Sets the default viewport
        /// </summary>
        public void SetDefaultViewport()
        {
            this.SetViewport(this.Viewport);
        }
        /// <summary>
        /// Sets default render target
        /// </summary>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        public void SetDefaultRenderTarget(bool clearRT = true, bool clearDepth = true, bool clearStencil = true)
        {
            this.SetRenderTargets(
                this.renderTargetView, clearRT, GameEnvironment.Background,
                this.depthStencilView, clearDepth, clearStencil,
                false);
        }
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(Viewport viewport)
        {
            this.deviceContext.Rasterizer.SetViewport(viewport);
        }
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(ViewportF viewport)
        {
            this.deviceContext.Rasterizer.SetViewport(viewport);
        }
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        public void SetViewports(IEnumerable<Viewport> viewports)
        {
            var rawVpArray = viewports.Select(v => (SharpDX.Mathematics.Interop.RawViewportF)v).ToArray();

            this.deviceContext.Rasterizer.SetViewports(rawVpArray);
        }
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        public void SetViewports(IEnumerable<ViewportF> viewports)
        {
            var rawVpArray = viewports.Select(v => (SharpDX.Mathematics.Interop.RawViewportF)v).ToArray();

            this.deviceContext.Rasterizer.SetViewports(rawVpArray);
        }
        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="renderTarget">Render target</param>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clearRT">Indicates whether the target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="depthMap">Depth map</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        /// <param name="freeOMResources">Indicates whether the Output merger Shader Resources must be cleared</param>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor, EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil, bool freeOMResources)
        {
            if (freeOMResources)
            {
                this.ClearShaderResources();
            }

            var dsv = depthMap?.GetDepthStencil();
            var rtv = renderTargets?.GetRenderTargets();
            var rtvCount = renderTargets != null ? renderTargets.Count : 0;

            this.deviceContext.OutputMerger.SetTargets(dsv, rtvCount, rtv);

            if (clearRT && rtv != null && rtvCount > 0)
            {
                for (int i = 0; i < rtvCount; i++)
                {
                    this.deviceContext.ClearRenderTargetView(
                        rtv[i],
                        clearRTColor);
                }
            }

            if ((clearDepth || clearStencil) && dsv != null)
            {
                DepthStencilClearFlags clearDSFlags = 0;
                if (clearDepth) clearDSFlags |= DepthStencilClearFlags.Depth;
                if (clearStencil) clearDSFlags |= DepthStencilClearFlags.Stencil;

                this.deviceContext.ClearDepthStencilView(
                    dsv,
                    clearDSFlags,
                    1.0f, 0);
            }
        }
        /// <summary>
        /// Clear shader resources
        /// </summary>
        private void ClearShaderResources()
        {
            this.deviceContext.VertexShader.SetShaderResources(0, nullSrv);
            this.deviceContext.HullShader.SetShaderResources(0, nullSrv);
            this.deviceContext.DomainShader.SetShaderResources(0, nullSrv);
            this.deviceContext.GeometryShader.SetShaderResources(0, nullSrv);
            this.deviceContext.PixelShader.SetShaderResources(0, nullSrv);
        }
        /// <summary>
        /// Sets targets for stream output
        /// </summary>
        /// <param name="streamOutBinding">Stream output binding</param>
        public void SetStreamOutputTargets(StreamOutputBufferBinding[] streamOutBinding)
        {
            this.deviceContext.StreamOutput.SetTargets(streamOutBinding);
            Counters.SOTargetsSet++;
        }
        /// <summary>
        /// Clear depth / stencil buffer
        /// </summary>
        /// <param name="depthMap">Depth buffer</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        public void ClearDepthStencilBuffer(EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil)
        {
            if ((clearDepth || clearStencil) && depthMap != null)
            {
                DepthStencilClearFlags clearDSFlags = 0;
                if (clearDepth) clearDSFlags |= DepthStencilClearFlags.Depth;
                if (clearStencil) clearDSFlags |= DepthStencilClearFlags.Stencil;

                this.deviceContext.ClearDepthStencilView(
                    depthMap.GetDepthStencil(),
                    clearDSFlags,
                    1.0f, 0);
            }
        }
        /// <summary>
        /// Enables z-buffer for write
        /// </summary>
        public void SetDepthStencilZEnabled()
        {
            if (this.depthStencilzBufferEnabled == null)
            {
                this.depthStencilzBufferEnabled = EngineDepthStencilState.ZBufferEnabled(this);
            }

            this.SetDepthStencilState(this.depthStencilzBufferEnabled);
        }
        /// <summary>
        /// Disables z-buffer for write
        /// </summary>
        public void SetDepthStencilZDisabled()
        {
            if (this.depthStencilzBufferDisabled == null)
            {
                this.depthStencilzBufferDisabled = EngineDepthStencilState.ZBufferDisabled(this);
            }

            this.SetDepthStencilState(this.depthStencilzBufferDisabled);
        }
        /// <summary>
        /// Enables z-buffer for read
        /// </summary>
        public void SetDepthStencilRDZEnabled()
        {
            if (this.depthStencilRDzBufferEnabled == null)
            {
                this.depthStencilRDzBufferEnabled = EngineDepthStencilState.RDzBufferEnabled(this);
            }

            this.SetDepthStencilState(this.depthStencilRDzBufferEnabled);
        }
        /// <summary>
        /// Disables z-buffer for read
        /// </summary>
        public void SetDepthStencilRDZDisabled()
        {
            if (this.depthStencilRDzBufferDisabled == null)
            {
                this.depthStencilRDzBufferDisabled = EngineDepthStencilState.RDzBufferDisabled(this);
            }

            this.SetDepthStencilState(this.depthStencilRDzBufferDisabled);
        }
        /// <summary>
        /// Disables depth stencil
        /// </summary>
        public void SetDepthStencilNone()
        {
            if (this.depthStencilNone == null)
            {
                this.depthStencilNone = EngineDepthStencilState.None(this);
            }

            this.SetDepthStencilState(this.depthStencilNone);
        }
        /// <summary>
        /// Sets the depth state for shadow mapping
        /// </summary>
        public void SetDepthStencilShadowMapping()
        {
            if (this.depthStencilShadowMapping == null)
            {
                this.depthStencilShadowMapping = EngineDepthStencilState.ShadowMapping(this);
            }

            this.SetDepthStencilState(this.depthStencilShadowMapping);
        }
        /// <summary>
        /// Sets default blend state
        /// </summary>
        public void SetBlendDefault()
        {
            if (this.blendDefault == null)
            {
                this.blendDefault = EngineBlendState.Default(this);
            }

            this.SetBlendState(this.blendDefault);
        }
        /// <summary>
        /// Sets default alpha blend state
        /// </summary>
        public void SetBlendDefaultAlpha()
        {
            if (this.blendDefaultAlpha == null)
            {
                this.blendDefaultAlpha = EngineBlendState.DefaultAlpha(this);
            }

            this.SetBlendState(this.blendDefaultAlpha);
        }
        /// <summary>
        /// Sets transparent blend state
        /// </summary>
        public void SetBlendTransparent()
        {
            if (this.blendTransparent == null)
            {
                this.blendTransparent = EngineBlendState.Transparent(this);
            }

            this.SetBlendState(this.blendTransparent);
        }
        /// <summary>
        /// Sets additive blend state
        /// </summary>
        public void SetBlendAdditive()
        {
            if (this.blendAdditive == null)
            {
                this.blendAdditive = EngineBlendState.Additive(this);
            }

            this.SetBlendState(this.blendAdditive);
        }
        /// <summary>
        /// Sets default rasterizer
        /// </summary>
        public void SetRasterizerDefault()
        {
            if (this.rasterizerDefault == null)
            {
                this.rasterizerDefault = EngineRasterizerState.Default(this);
            }

            this.SetRasterizerState(this.rasterizerDefault);
        }
        /// <summary>
        /// Sets wireframe rasterizer
        /// </summary>
        public void SetRasterizerWireframe()
        {
            if (this.rasterizerWireframe == null)
            {
                this.rasterizerWireframe = EngineRasterizerState.Wireframe(this);
            }

            this.SetRasterizerState(this.rasterizerWireframe);
        }
        /// <summary>
        /// Sets no-cull rasterizer
        /// </summary>
        public void SetRasterizerCullNone()
        {
            if (this.rasterizerNoCull == null)
            {
                this.rasterizerNoCull = EngineRasterizerState.NoCull(this);
            }

            this.SetRasterizerState(this.rasterizerNoCull);
        }
        /// <summary>
        /// Sets cull counter-clockwise face rasterizer
        /// </summary>
        public void SetRasterizerCullFrontFace()
        {
            if (this.rasterizerCullFrontFace == null)
            {
                this.rasterizerCullFrontFace = EngineRasterizerState.CullFrontFace(this);
            }

            this.SetRasterizerState(this.rasterizerCullFrontFace);
        }
        /// <summary>
        /// Sets shadow mapping rasterizer state
        /// </summary>
        public void SetRasterizerShadowMapping()
        {
            if (this.rasterizerShadowMapping == null)
            {
                this.rasterizerShadowMapping = EngineRasterizerState.ShadowMapping(this);
            }

            this.SetRasterizerState(this.rasterizerShadowMapping);
        }
        /// <summary>
        /// Bind an array of vertex buffers to the input-assembler stage.
        /// </summary>
        /// <param name="firstSlot">The first input slot for binding</param>
        /// <param name="vertexBufferBindings">A reference to an array of VertexBufferBinding</param>
        public void IASetVertexBuffers(int firstSlot, params VertexBufferBinding[] vertexBufferBindings)
        {
            if (this.currentVertexBufferFirstSlot != firstSlot || this.currentVertexBufferBindings != vertexBufferBindings)
            {
                this.deviceContext.InputAssembler.SetVertexBuffers(firstSlot, vertexBufferBindings);
                Counters.IAVertexBuffersSets++;

                this.currentVertexBufferFirstSlot = firstSlot;
                this.currentVertexBufferBindings = vertexBufferBindings;
            }
        }
        /// <summary>
        /// Bind an index buffer to the input-assembler stage.
        /// </summary>
        /// <param name="indexBufferRef">A reference to an Buffer object</param>
        /// <param name="format">A SharpDX.DXGI.Format that specifies the format of the data in the index buffer</param>
        /// <param name="offset">Offset (in bytes) from the start of the index buffer to the first index to use</param>
        public void IASetIndexBuffer(Buffer indexBufferRef, Format format, int offset)
        {
            if (this.currentIndexBufferRef != indexBufferRef || this.currentIndexFormat != format || this.currentIndexOffset != offset)
            {
                this.deviceContext.InputAssembler.SetIndexBuffer(indexBufferRef, format, offset);
                Counters.IAIndexBufferSets++;

                this.currentIndexBufferRef = indexBufferRef;
                this.currentIndexFormat = format;
                this.currentIndexOffset = offset;
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        private void DisposeResources()
        {
            this.renderTargetView?.Dispose();
            this.renderTargetView = null;
            this.depthStencilView?.Dispose();
            this.depthStencilView = null;

            this.depthStencilzBufferEnabled?.Dispose();
            this.depthStencilzBufferEnabled = null;
            this.depthStencilzBufferDisabled?.Dispose();
            this.depthStencilzBufferDisabled = null;
            this.depthStencilRDzBufferEnabled?.Dispose();
            this.depthStencilRDzBufferEnabled = null;
            this.depthStencilRDzBufferDisabled?.Dispose();
            this.depthStencilRDzBufferDisabled = null;
            this.depthStencilNone?.Dispose();
            this.depthStencilNone = null;
            this.depthStencilShadowMapping?.Dispose();
            this.depthStencilShadowMapping = null;

            this.rasterizerDefault?.Dispose();
            this.rasterizerDefault = null;
            this.rasterizerWireframe?.Dispose();
            this.rasterizerWireframe = null;
            this.rasterizerNoCull?.Dispose();
            this.rasterizerNoCull = null;
            this.rasterizerCullFrontFace?.Dispose();
            this.rasterizerCullFrontFace = null;
            this.rasterizerShadowMapping?.Dispose();
            this.rasterizerShadowMapping = null;

            this.blendDefault?.Dispose();
            this.blendDefault = null;
            this.blendDefaultAlpha?.Dispose();
            this.blendDefaultAlpha = null;
            this.blendTransparent?.Dispose();
            this.blendTransparent = null;
            this.blendAdditive?.Dispose();
            this.blendAdditive = null;
        }

        /// <summary>
        /// Sets depth stencil state
        /// </summary>
        /// <param name="state">Depth stencil state</param>
        internal void SetDepthStencilState(EngineDepthStencilState state)
        {
            if (this.currentDepthStencilState != state)
            {
                this.device.ImmediateContext.OutputMerger.SetDepthStencilState(state.GetDepthStencilState(), state.StencilRef);
                this.device.ImmediateContext.OutputMerger.DepthStencilReference = state.StencilRef;

                this.currentDepthStencilState = state;

                Counters.DepthStencilStateChanges++;
            }
        }
        /// <summary>
        /// Stes blend state
        /// </summary>
        /// <param name="state">Blend state</param>
        internal void SetBlendState(EngineBlendState state)
        {
            if (this.currentBlendState != state)
            {
                this.device.ImmediateContext.OutputMerger.SetBlendState(state.GetBlendState(), state.BlendFactor, state.SampleMask);

                this.currentBlendState = state;

                Counters.BlendStateChanges++;
            }
        }
        /// <summary>
        /// Sets rasterizer state
        /// </summary>
        /// <param name="state">Rasterizer state</param>
        internal void SetRasterizerState(EngineRasterizerState state)
        {
            if (this.currentRasterizerState != state)
            {
                this.device.ImmediateContext.Rasterizer.State = state.GetRasterizerState();

                this.currentRasterizerState = state;

                Counters.RasterizerStateChanges++;
            }
        }

        /// <summary>
        /// Creates a new blend state
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="blendFactor">Blend factor</param>
        /// <param name="sampleMask">Sample mask</param>
        /// <returns>Returns a new blend state</returns>
        internal EngineBlendState CreateBlendState(BlendStateDescription1 description, Color4? blendFactor, int sampleMask)
        {
            return new EngineBlendState(new BlendState1(this.device, description), blendFactor, sampleMask);
        }
        /// <summary>
        /// Creates a new rasterizer state
        /// </summary>
        /// <param name="description">Description</param>
        /// <returns>Returns a new rasterizer state</returns>
        internal EngineRasterizerState CreateRasterizerState(RasterizerStateDescription2 description)
        {
            return new EngineRasterizerState(new RasterizerState2(this.device, description));
        }
        /// <summary>
        /// Creates a new depth stencil state
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="stencilRef">Stencil reference</param>
        /// <returns>Returns a new depth stencil state</returns>
        internal EngineDepthStencilState CreateDepthStencilState(DepthStencilStateDescription description, int stencilRef)
        {
            return new EngineDepthStencilState(new DepthStencilState(this.device, description), stencilRef);
        }

        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Vertex data collection</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateVertexBuffer(string name, IEnumerable<IVertexData> data, bool dynamic)
        {
            var vertexType = data.First().VertexType;

            switch (vertexType)
            {
                case VertexTypes.Billboard:
                    return CreateVertexBuffer(name, data.OfType<VertexBillboard>(), dynamic);
                case VertexTypes.CPUParticle:
                    return CreateVertexBuffer(name, data.OfType<VertexCpuParticle>(), dynamic);
                case VertexTypes.GPUParticle:
                    return CreateVertexBuffer(name, data.OfType<VertexGpuParticle>(), dynamic);
                case VertexTypes.Terrain:
                    return CreateVertexBuffer(name, data.OfType<VertexTerrain>(), dynamic);
                case VertexTypes.Position:
                    return CreateVertexBuffer(name, data.OfType<VertexPosition>(), dynamic);
                case VertexTypes.PositionColor:
                    return CreateVertexBuffer(name, data.OfType<VertexPositionColor>(), dynamic);
                case VertexTypes.PositionTexture:
                    return CreateVertexBuffer(name, data.OfType<VertexPositionTexture>(), dynamic);
                case VertexTypes.PositionNormalColor:
                    return CreateVertexBuffer(name, data.OfType<VertexPositionNormalColor>(), dynamic);
                case VertexTypes.PositionNormalTexture:
                    return CreateVertexBuffer(name, data.OfType<VertexPositionNormalTexture>(), dynamic);
                case VertexTypes.PositionNormalTextureTangent:
                    return CreateVertexBuffer(name, data.OfType<VertexPositionNormalTextureTangent>(), dynamic);
                case VertexTypes.PositionSkinned:
                    return CreateVertexBuffer(name, data.OfType<VertexSkinnedPosition>(), dynamic);
                case VertexTypes.PositionColorSkinned:
                    return CreateVertexBuffer(name, data.OfType<VertexSkinnedPositionColor>(), dynamic);
                case VertexTypes.PositionTextureSkinned:
                    return CreateVertexBuffer(name, data.OfType<VertexSkinnedPositionTexture>(), dynamic);
                case VertexTypes.PositionNormalColorSkinned:
                    return CreateVertexBuffer(name, data.OfType<VertexSkinnedPositionNormalColor>(), dynamic);
                case VertexTypes.PositionNormalTextureSkinned:
                    return CreateVertexBuffer(name, data.OfType<VertexSkinnedPositionNormalTexture>(), dynamic);
                case VertexTypes.PositionNormalTextureTangentSkinned:
                    return CreateVertexBuffer(name, data.OfType<VertexSkinnedPositionNormalTextureTangent>(), dynamic);
                default:
                    throw new EngineException(string.Format("Unknown vertex type: {0}", vertexType));
            }
        }
        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateVertexBuffer<T>(string name, IEnumerable<T> data, bool dynamic)
            where T : struct
        {
            return CreateBuffer(
                name,
                data,
                dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags.VertexBuffer,
                dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
        }
        /// <summary>
        /// Creates an index buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        internal Buffer CreateIndexBuffer<T>(string name, IEnumerable<T> data, bool dynamic)
            where T : struct
        {
            return CreateBuffer(
                name,
                data,
                dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags.IndexBuffer,
                dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
        }
        /// <summary>
        /// Creates a buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="length">Buffer length</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding</param>
        /// <param name="access">Cpu access</param>
        /// <returns>Returns created buffer</returns>
        internal Buffer CreateBuffer<T>(string name, int length, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * length;

            Counters.RegBuffer(typeof(T), name, (int)usage, (int)binding, sizeInBytes, length);

            var description = new BufferDescription()
            {
                Usage = usage,
                SizeInBytes = sizeInBytes,
                BindFlags = binding,
                CpuAccessFlags = access,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            return new Buffer(this.device, description);
        }
        /// <summary>
        /// Creates a buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding</param>
        /// <param name="access">Cpu access</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateBuffer<T>(string name, IEnumerable<T> data, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * data.Count();

            Counters.RegBuffer(typeof(T), name, (int)usage, (int)binding, sizeInBytes, data.Count());

            using (var dstr = new DataStream(sizeInBytes, true, true))
            {
                dstr.WriteRange(data.ToArray());
                dstr.Position = 0;

                var description = new BufferDescription()
                {
                    Usage = usage,
                    SizeInBytes = sizeInBytes,
                    BindFlags = binding,
                    CpuAccessFlags = access,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0,
                };

                return new Buffer(this.device, dstr, description);
            }
        }

        /// <summary>
        /// Creates a new Input Layout for a Shader
        /// </summary>
        /// <param name="shaderBytecode">Byte code</param>
        /// <param name="elements">Input elements</param>
        /// <returns>Returns a new Input Layout</returns>
        internal InputLayout CreateInputLayout(ShaderBytecode shaderBytecode, InputElement[] elements)
        {
            return new InputLayout(this.device, shaderBytecode, elements);
        }

        /// <summary>
        /// Creates a resource view from a texture description
        /// </summary>
        /// <param name="description">Texture description</param>
        /// <param name="tryMipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the new shader resource view</returns>
        internal ShaderResourceView1 CreateResource(TextureData description, bool tryMipAutogen)
        {
            bool mipAutogen = false;

            if (tryMipAutogen && description.MipMaps == 1)
            {
                var fmtSupport = this.device.CheckFormatSupport(description.Format);
                mipAutogen = fmtSupport.HasFlag(FormatSupport.MipAutogen);
            }

            if (mipAutogen)
            {
                ShaderResourceViewDescription1 desc;

                Texture2D1 texture;
                if (description.IsCubeMap)
                {
                    texture = this.CreateTexture2DCube(description.Width, description.Height, description.Format, 1, mipAutogen);
                    desc = new ShaderResourceViewDescription1()
                    {
                        Format = texture.Description.Format,
                        Dimension = ShaderResourceViewDimension.TextureCube,
                        TextureCube = new ShaderResourceViewDescription.TextureCubeResource()
                        {
                            MipLevels = -1,
                        }
                    };
                }
                else
                {
                    texture = this.CreateTexture2D(description.Width, description.Height, description.Format, 1, mipAutogen);
                    desc = new ShaderResourceViewDescription1()
                    {
                        Format = texture.Description.Format,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                        {
                            MipLevels = -1,
                        },
                    };
                }

                using (texture)
                {
                    var result = new ShaderResourceView1(this.device, texture, desc);

                    this.deviceContext.UpdateSubresource(description.GetDataBox(0, 0), texture, 0);

                    this.deviceContext.GenerateMips(result);

                    return result;
                }
            }
            else
            {
                var width = description.Width;
                var height = description.Height;
                var format = description.Format;
                var mipMaps = description.MipMaps;
                var arraySize = description.ArraySize;
                var data = description.GetDataBoxes();

                Texture2D1 texture;
                ShaderResourceViewDescription1 desc;

                if (description.IsCubeMap)
                {
                    texture = this.CreateTexture2DCube(width, height, format, mipMaps, 1, data);
                    desc = new ShaderResourceViewDescription1()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.TextureCube,
                        TextureCube = new ShaderResourceViewDescription.TextureCubeResource()
                        {
                            MipLevels = mipMaps,
                        }
                    };
                }
                else
                {
                    texture = this.CreateTexture2D(width, height, format, mipMaps, arraySize, data);
                    desc = new ShaderResourceViewDescription1()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                        {
                            MipLevels = mipMaps,
                        },
                    };
                }

                using (texture)
                {
                    return new ShaderResourceView1(this.device, texture, desc);
                }
            }
        }
        /// <summary>
        /// Creates a resource view from a texture description list
        /// </summary>
        /// <param name="descriptions">Texture description list</param>
        /// <param name="tryMipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the new shader resource view</returns>
        internal ShaderResourceView1 CreateResource(IEnumerable<TextureData> descriptions, bool tryMipAutogen)
        {
            var description = descriptions.First();
            int count = descriptions.Count();

            bool mipAutogen = false;

            if (tryMipAutogen && description.MipMaps == 1)
            {
                var fmtSupport = this.device.CheckFormatSupport(description.Format);
                mipAutogen = fmtSupport.HasFlag(FormatSupport.MipAutogen);
            }

            if (mipAutogen)
            {
                Texture2D1 textureArray;
                ShaderResourceViewDescription1 desc;

                if (description.IsCubeMap)
                {
                    textureArray = this.CreateTexture2DCube(description.Width, description.Height, description.Format, count, mipAutogen);
                    desc = new ShaderResourceViewDescription1()
                    {
                        Format = description.Format,
                        Dimension = ShaderResourceViewDimension.TextureCubeArray,
                        TextureCubeArray = new ShaderResourceViewDescription.TextureCubeArrayResource()
                        {
                            CubeCount = count,
                            MipLevels = -1,
                        }
                    };
                }
                else
                {
                    textureArray = this.CreateTexture2D(description.Width, description.Height, description.Format, count, mipAutogen);
                    desc = new ShaderResourceViewDescription1()
                    {
                        Format = description.Format,
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Texture2DArray = new ShaderResourceViewDescription1.Texture2DArrayResource1()
                        {
                            ArraySize = count,
                            MipLevels = -1,
                        },
                    };
                }

                using (textureArray)
                {
                    var result = new ShaderResourceView1(this.device, textureArray, desc);

                    int i = 0;
                    foreach (var currentDesc in descriptions)
                    {
                        var index = textureArray.CalculateSubResourceIndex(0, i++, out int mipSize);

                        this.deviceContext.UpdateSubresource(currentDesc.GetDataBox(0, 0), textureArray, index);
                    }

                    this.deviceContext.GenerateMips(result);

                    return result;
                }
            }
            else
            {
                var width = description.Width;
                var height = description.Height;
                var format = description.Format;
                var mipMaps = description.MipMaps;
                var arraySize = count;
                var data = new List<DataBox>();

                foreach (var currentDesc in descriptions)
                {
                    data.AddRange(currentDesc.GetDataBoxes());
                }

                Texture2D1 textureArray;
                ShaderResourceViewDescription1 desc;

                if (description.IsCubeMap)
                {
                    textureArray = this.CreateTexture2DCube(width, height, format, mipMaps, arraySize, data.ToArray());
                    desc = new ShaderResourceViewDescription1()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.TextureCube,
                        TextureCubeArray = new ShaderResourceViewDescription.TextureCubeArrayResource()
                        {
                            CubeCount = arraySize,
                            MipLevels = mipMaps,
                        },
                    };
                }
                else
                {
                    textureArray = this.CreateTexture2D(width, height, format, mipMaps, arraySize, data.ToArray());
                    desc = new ShaderResourceViewDescription1()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Texture2DArray = new ShaderResourceViewDescription1.Texture2DArrayResource1()
                        {
                            ArraySize = arraySize,
                            MipLevels = mipMaps,
                        },
                    };
                }

                using (textureArray)
                {
                    return new ShaderResourceView1(this.device, textureArray, desc);
                }
            }
        }
        /// <summary>
        /// Creates a texture filled with specified values
        /// </summary>
        /// <param name="size">Texture size</param>
        /// <param name="values">Color values</param>
        /// <returns>Returns created texture</returns>
        internal ShaderResourceView1 CreateTexture1D(int size, IEnumerable<Vector4> values)
        {
            try
            {
                Counters.Textures++;

                using (var str = DataStream.Create(values.ToArray(), false, false))
                {
                    using (var randTex = new Texture1D(
                        this.device,
                        new Texture1DDescription()
                        {
                            Format = Format.R32G32B32A32_Float,
                            Width = size,
                            ArraySize = 1,
                            MipLevels = 1,
                            Usage = ResourceUsage.Immutable,
                            BindFlags = BindFlags.ShaderResource,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                        },
                        str))
                    {
                        return new ShaderResourceView1(this.device, randTex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateTexture1D from value array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a texture filled with specified values
        /// </summary>
        /// <param name="size">Texture size</param>
        /// <param name="values">Color values</param>
        /// <returns>Returns created texture</returns>
        internal ShaderResourceView1 CreateTexture2D(int size, IEnumerable<Vector4> values)
        {
            try
            {
                Counters.Textures++;

                var tmp = new Vector4[size * size];
                Array.Copy(values.ToArray(), tmp, values.Count());

                using (var str = DataStream.Create(tmp, false, false))
                {
                    var dBox = new DataBox(str.DataPointer, size * FormatHelper.SizeOfInBytes(Format.R32G32B32A32_Float), 0);

                    using (var texture = new Texture2D1(
                        this.device,
                        new Texture2DDescription1()
                        {
                            Format = Format.R32G32B32A32_Float,
                            Width = size,
                            Height = size,
                            ArraySize = 1,
                            MipLevels = 1,
                            SampleDescription = new SampleDescription(1, 0),
                            Usage = ResourceUsage.Immutable,
                            BindFlags = BindFlags.ShaderResource,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                        },
                        new[] { dBox }))
                    {
                        return new ShaderResourceView1(this.device, texture);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateTexture2D from value array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates an empty Texture2D
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        /// <param name="arraySize">Size</param>
        /// <param name="generateMips">Generate mips for the texture</param>
        /// <returns>Returns the Texture2D</returns>
        private Texture2D1 CreateTexture2D(int width, int height, Format format, int arraySize, bool generateMips)
        {
            var description = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize,
                BindFlags = (generateMips) ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                MipLevels = (generateMips) ? 0 : 1,
                OptionFlags = (generateMips) ? ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            return new Texture2D1(this.device, description);
        }
        /// <summary>
        /// Creates a Texture2D
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        /// <param name="mipMaps">Mipmap count</param>
        /// <param name="arraySize">Array size</param>
        /// <param name="data">Initial data</param>
        /// <returns>Returns the Texture2D</returns>
        private Texture2D1 CreateTexture2D(int width, int height, Format format, int mipMaps, int arraySize, DataBox[] data)
        {
            var description = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                MipLevels = mipMaps,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            return new Texture2D1(this.device, description, data);
        }
        /// <summary>
        /// Creates a Texture2DCube
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        /// <param name="arraySize">Array size</param>
        /// <param name="generateMips">Generate mips for the texture</param>
        /// <returns>Returns the Texture2DCube</returns>
        private Texture2D1 CreateTexture2DCube(int width, int height, Format format, int arraySize, bool generateMips)
        {
            var description = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize * 6,
                BindFlags = (generateMips) ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                MipLevels = (generateMips) ? 0 : 1,
                OptionFlags = (generateMips) ? ResourceOptionFlags.TextureCube | ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            return new Texture2D1(this.device, description);
        }
        /// <summary>
        /// Creates a Texture2DCube
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        /// <param name="mipMaps">Mipmap count</param>
        /// <param name="arraySize">Array size</param>
        /// <param name="data">Initial data</param>
        /// <returns>Returns the Texture2DCube</returns>
        private Texture2D1 CreateTexture2DCube(int width, int height, Format format, int mipMaps, int arraySize, DataBox[] data)
        {
            var description = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize * 6,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                MipLevels = mipMaps,
                OptionFlags = ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            return new Texture2D1(this.device, description, data);
        }

        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        internal ShaderResourceView1 LoadTexture(string filename, bool mipAutogen)
        {
            try
            {
                Counters.Textures++;

                using (var resource = TextureData.ReadTexture(filename))
                {
                    return this.CreateResource(resource, mipAutogen);
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        internal ShaderResourceView1 LoadTexture(MemoryStream stream, bool mipAutogen)
        {
            try
            {
                Counters.Textures++;

                using (var resource = TextureData.ReadTexture(stream))
                {
                    return this.CreateResource(resource, mipAutogen);
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="filenames">Path file collection</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        internal ShaderResourceView1 LoadTextureArray(IEnumerable<string> filenames, bool mipAutogen)
        {
            try
            {
                var textureList = TextureData.ReadTexture(filenames);

                return LoadTextureArray(textureList, mipAutogen);
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="streams">Stream collection</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        internal ShaderResourceView1 LoadTextureArray(IEnumerable<MemoryStream> streams, bool mipAutogen)
        {
            try
            {
                var textureList = TextureData.ReadTexture(streams);

                return LoadTextureArray(textureList, mipAutogen);
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array in the graphics device
        /// </summary>
        /// <param name="textureList">Texture array</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        internal ShaderResourceView1 LoadTextureArray(IEnumerable<TextureData> textureList, bool mipAutogen)
        {
            Counters.Textures++;

            var resource = this.CreateResource(textureList, mipAutogen);

            foreach (var item in textureList)
            {
                item?.Dispose();
            }

            return resource;
        }
        /// <summary>
        /// Creates a random 1D texture
        /// </summary>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns created texture</returns>
        internal ShaderResourceView1 CreateRandomTexture(int size, float min, float max, int seed = 0)
        {
            try
            {
                Counters.Textures++;

                Random rnd = new Random(seed);

                var randomValues = new List<Vector4>();
                for (int i = 0; i < size; i++)
                {
                    randomValues.Add(rnd.NextVector4(new Vector4(min), new Vector4(max)));
                }

                return this.CreateTexture1D(size, randomValues.ToArray());
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRandomTexture Error. See inner exception for details", ex);
            }
        }

        /// <summary>
        /// Create depth stencil view
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="useSamples">Use samples if available</param>
        /// <param name="dsv">Resulting depth stencil view</param>
        internal void CreateDepthStencil(Format format, int width, int height, bool useSamples, out EngineDepthStencilView dsv)
        {
            bool multiSampled = false;
            SampleDescription sampleDescription = new SampleDescription(1, 0);
            if (useSamples)
            {
                multiSampled = this.MultiSampled;
                sampleDescription = this.CurrentSampleDescription;
            }

            using (var texture = new Texture2D1(
                this.device,
                new Texture2DDescription1()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = format,
                    SampleDescription = sampleDescription,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                }))
            {
                var description = new DepthStencilViewDescription()
                {
                    Format = format,
                    Dimension = multiSampled ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {

                    },
                    Texture2DMS = new DepthStencilViewDescription.Texture2DMultisampledResource()
                    {

                    },
                };

                dsv = new EngineDepthStencilView(new DepthStencilView(this.device, texture, description));
            }
        }
        /// <summary>
        /// Creates a new render tarjet and his texture
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="useSamples">Use samples if available</param>
        /// <param name="rtv">Render target</param>
        /// <param name="srv">Texture</param>
        internal void CreateRenderTargetTexture(Format format, int width, int height, bool useSamples, out EngineRenderTargetView rtv, out EngineShaderResourceView srv)
        {
            try
            {
                Counters.Textures++;

                bool multiSampled = false;
                SampleDescription sampleDescription = new SampleDescription(1, 0);
                if (useSamples)
                {
                    multiSampled = this.MultiSampled;
                    sampleDescription = this.CurrentSampleDescription;
                }

                CreateRenderTargetTexture(format, width, height, multiSampled, sampleDescription, out var rtv1, out var srv1);

                rtv = new EngineRenderTargetView(rtv1);
                srv = new EngineShaderResourceView(srv1);
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRenderTargetTexture Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a new multiple render target and his textures
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="arraySize">Render target list size</param>
        /// <param name="useSamples">Use samples if available</param>
        /// <param name="rtv">Render target</param>
        /// <param name="srv">Textures</param>
        internal void CreateRenderTargetTexture(Format format, int width, int height, int arraySize, bool useSamples, out EngineRenderTargetView rtv, out EngineShaderResourceView[] srv)
        {
            try
            {
                Counters.Textures++;

                bool multiSampled = false;
                SampleDescription sampleDescription = new SampleDescription(1, 0);
                if (useSamples)
                {
                    multiSampled = this.MultiSampled;
                    sampleDescription = this.CurrentSampleDescription;
                }

                rtv = new EngineRenderTargetView();
                srv = new EngineShaderResourceView[arraySize];

                for (int i = 0; i < arraySize; i++)
                {
                    CreateRenderTargetTexture(format, width, height, multiSampled, sampleDescription, out var rtv1, out var srv1);

                    rtv.Add(rtv1);
                    srv[i] = new EngineShaderResourceView(srv1);
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRenderTargetTexture Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a render target texture and a shader resource view for the texture
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="multiSampled">Create a multisampled texture</param>
        /// <param name="sampleDescription">Sample description</param>
        /// <param name="rtv">Resulting render target view</param>
        /// <param name="srv">Resulting shader resource view</param>
        private void CreateRenderTargetTexture(Format format, int width, int height, bool multiSampled, SampleDescription sampleDescription, out RenderTargetView1 rtv, out ShaderResourceView1 srv)
        {
            using (var texture = new Texture2D1(
                this.device,
                new Texture2DDescription1()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = format,
                    SampleDescription = sampleDescription,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                }))
            {
                var rtvDesc = new RenderTargetViewDescription1()
                {
                    Format = format,
                };
                var srvDesc = new ShaderResourceViewDescription1()
                {
                    Format = format,
                };

                if (multiSampled)
                {
                    rtvDesc.Dimension = RenderTargetViewDimension.Texture2DMultisampled;
                    rtvDesc.Texture2DMS = new RenderTargetViewDescription.Texture2DMultisampledResource();

                    srvDesc.Dimension = ShaderResourceViewDimension.Texture2DMultisampled;
                    srvDesc.Texture2DMS = new ShaderResourceViewDescription.Texture2DMultisampledResource();
                }
                else
                {
                    rtvDesc.Dimension = RenderTargetViewDimension.Texture2D;
                    rtvDesc.Texture2D = new RenderTargetViewDescription1.Texture2DResource();

                    srvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
                    srvDesc.Texture2D = new ShaderResourceViewDescription1.Texture2DResource1() { MipLevels = 1 };
                }

                rtv = new RenderTargetView1(this.device, texture, rtvDesc);
                srv = new ShaderResourceView1(this.device, texture, srvDesc);
            }
        }
        /// <summary>
        /// Creates a set of texture and depth stencil view for shadow mapping
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="dsv">Resulting Depth Stencil View</param>
        /// <param name="srv">Resulting Shader Resource View</param>
        internal void CreateShadowMapTextures(int width, int height, out EngineDepthStencilView dsv, out EngineShaderResourceView srv)
        {
            var depthMap = new Texture2D1(
                this.device,
                new Texture2DDescription1()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.R24G8_Typeless,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });

            using (depthMap)
            {
                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0,
                    },
                };
                dsv = new EngineDepthStencilView(new DepthStencilView(this.device, depthMap, dsDescription));

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0
                    },
                };
                srv = new EngineShaderResourceView(new ShaderResourceView1(this.device, depthMap, rvDescription));
            }
        }
        /// <summary>
        /// Creates a set of texture and depth stencil view for shadow mapping
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="mapCount">Per stencil view map count</param>
        /// <param name="dsv">Resulting Depth Stencil View</param>
        /// <param name="srv">Resulting Shader Resource View</param>
        internal void CreateShadowMapTextures(int width, int height, int mapCount, out EngineDepthStencilView dsv, out EngineShaderResourceView srv)
        {
            var depthMap = new Texture2D1(
                this.device,
                new Texture2DDescription1()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = mapCount,
                    Format = Format.R24G8_Typeless,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });

            using (depthMap)
            {
                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = DepthStencilViewDimension.Texture2DArray,
                    Texture2DArray = new DepthStencilViewDescription.Texture2DArrayResource()
                    {
                        ArraySize = mapCount,
                        FirstArraySlice = 0,
                        MipSlice = 0,
                    },
                };
                dsv = new EngineDepthStencilView(new DepthStencilView(this.device, depthMap, dsDescription));

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0
                    },
                };
                srv = new EngineShaderResourceView(new ShaderResourceView1(this.device, depthMap, rvDescription));
            }
        }
        /// <summary>
        /// Creates a set of texture and depth stencil view array for shadow mapping
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="mapCount">Per stencil view map count</param>
        /// <param name="arraySize">Array size</param>
        /// <param name="dsv">Resulting Depth Stencil View array</param>
        /// <param name="srv">Resulting Shader Resource View</param>
        internal void CreateShadowMapTextureArrays(int width, int height, int mapCount, int arraySize, out EngineDepthStencilView[] dsv, out EngineShaderResourceView srv)
        {
            var depthMap = new Texture2D1(
                this.device,
                new Texture2DDescription1()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = arraySize * mapCount,
                    Format = Format.R24G8_Typeless,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });

            using (depthMap)
            {
                dsv = new EngineDepthStencilView[arraySize];
                for (int i = 0; i < arraySize; i++)
                {
                    var dsDescription = new DepthStencilViewDescription
                    {
                        Flags = DepthStencilViewFlags.None,
                        Format = Format.D24_UNorm_S8_UInt,
                        Dimension = DepthStencilViewDimension.Texture2DArray,
                        Texture2DArray = new DepthStencilViewDescription.Texture2DArrayResource()
                        {
                            ArraySize = mapCount,
                            FirstArraySlice = i,
                            MipSlice = 0,
                        },
                    };
                    dsv[i] = new EngineDepthStencilView(new DepthStencilView(this.device, depthMap, dsDescription));
                }

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.Texture2DArray,
                    Texture2DArray = new ShaderResourceViewDescription1.Texture2DArrayResource1()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                        ArraySize = arraySize * mapCount,
                        FirstArraySlice = 0,
                        PlaneSlice = 0,
                    },
                };
                srv = new EngineShaderResourceView(new ShaderResourceView1(this.device, depthMap, rvDescription));
            }
        }
        /// <summary>
        /// Creates a cubic texture for shadow mapping
        /// </summary>
        /// <param name="width">Face width</param>
        /// <param name="height">Face height</param>
        /// <param name="dsv">Resulting Depth Stencil View</param>
        /// <param name="srv">Resulting Shader Resource View</param>
        internal void CreateCubicShadowMapTextures(int width, int height, out EngineDepthStencilView dsv, out EngineShaderResourceView srv)
        {
            var depthMap = new Texture2D1(
                this.device,
                new Texture2DDescription1()
                {
                    Width = width,
                    Height = height,
                    ArraySize = 6,
                    BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Format.R24G8_Typeless,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.TextureCube,
                    SampleDescription = new SampleDescription(1, 0),
                    TextureLayout = TextureLayout.Undefined,
                });

            using (depthMap)
            {
                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = DepthStencilViewDimension.Texture2DArray,
                    Texture2DArray = new DepthStencilViewDescription.Texture2DArrayResource()
                    {
                        ArraySize = 6,
                        FirstArraySlice = 0,
                        MipSlice = 0,
                    },
                };
                dsv = new EngineDepthStencilView(new DepthStencilView(this.device, depthMap, dsDescription));

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.TextureCube,
                    TextureCube = new ShaderResourceViewDescription.TextureCubeResource()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                    },
                };
                srv = new EngineShaderResourceView(new ShaderResourceView1(this.device, depthMap, rvDescription));
            }
        }
        /// <summary>
        /// Creates a cubic texture array for shadow mapping
        /// </summary>
        /// <param name="width">Face width</param>
        /// <param name="height">Face height</param>
        /// <param name="arraySize">Array size</param>
        /// <param name="dsv">Resulting Depth Stencil View array</param>
        /// <param name="srv">Resulting Shader Resource View</param>
        internal void CreateCubicShadowMapTextureArrays(int width, int height, int arraySize, out EngineDepthStencilView[] dsv, out EngineShaderResourceView srv)
        {
            var depthMap = new Texture2D1(
                this.device,
                new Texture2DDescription1()
                {
                    Width = width,
                    Height = height,
                    ArraySize = 6 * arraySize,
                    BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Format.R24G8_Typeless,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.TextureCube,
                    SampleDescription = new SampleDescription(1, 0),
                    TextureLayout = TextureLayout.Undefined,
                });

            using (depthMap)
            {
                dsv = new EngineDepthStencilView[arraySize];
                for (int i = 0; i < arraySize; i++)
                {
                    var dsDescription = new DepthStencilViewDescription
                    {
                        Flags = DepthStencilViewFlags.None,
                        Format = Format.D24_UNorm_S8_UInt,
                        Dimension = DepthStencilViewDimension.Texture2DArray,
                        Texture2DArray = new DepthStencilViewDescription.Texture2DArrayResource()
                        {
                            ArraySize = 6,
                            FirstArraySlice = i * 6,
                            MipSlice = 0,
                        },
                    };
                    dsv[i] = new EngineDepthStencilView(new DepthStencilView(this.device, depthMap, dsDescription));
                }

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.TextureCubeArray,
                    TextureCubeArray = new ShaderResourceViewDescription.TextureCubeArrayResource()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                        CubeCount = arraySize,
                        First2DArrayFace = 0,
                    },
                };
                srv = new EngineShaderResourceView(new ShaderResourceView1(this.device, depthMap, rvDescription));
            }
        }

        /// <summary>
        /// Loads vertex shader from file
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Retuns vertex shader description</returns>
        internal EngineVertexShader LoadVertexShader(
            string filename,
            string entryPoint,
            InputElement[] input,
            string profile)
        {
            return LoadVertexShader(
                File.ReadAllBytes(filename),
                entryPoint,
                input,
                profile);
        }
        /// <summary>
        /// Loads vertex shader from file
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Retuns vertex shader description</returns>
        internal EngineVertexShader LoadVertexShader(
            string filename,
            string entryPoint,
            InputElement[] input,
            string profile,
            out string compilationErrors)
        {
            return LoadVertexShader(
                File.ReadAllBytes(filename),
                entryPoint,
                input,
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads vertex shader from byte code
        /// </summary>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Retuns vertex shader description</returns>
        internal EngineVertexShader LoadVertexShader(
            byte[] byteCode,
            string entryPoint,
            InputElement[] input,
            string profile)
        {
            var res = LoadVertexShader(
                byteCode,
                entryPoint,
                input,
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Console.WriteLine(compilationErrors);
            }

            return res;
        }
        /// <summary>
        /// Loads vertex shader from byte code
        /// </summary>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Retuns vertex shader description</returns>
        internal EngineVertexShader LoadVertexShader(
            byte[] byteCode,
            string entryPoint,
            InputElement[] input,
            string profile,
            out string compilationErrors)
        {
            compilationErrors = null;
            using (ShaderIncludeManager includeManager = new ShaderIncludeManager())
            using (CompilationResult cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                InputLayout layout = new InputLayout(
                    this.device,
                    ShaderSignature.GetInputSignature(cmpResult.Bytecode),
                    input);

                VertexShader vertexShader = new VertexShader(
                    this.device,
                    cmpResult.Bytecode);

                return new EngineVertexShader(vertexShader, layout);
            }
        }
        /// <summary>
        /// Loads a pixel shader from file
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns pixel shader description</returns>
        internal EnginePixelShader LoadPixelShader(
            string filename,
            string entryPoint,
            string profile)
        {
            var res = LoadPixelShader(
                File.ReadAllBytes(filename),
                entryPoint,
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Console.WriteLine(compilationErrors);
            }

            return res;
        }
        /// <summary>
        /// Loads a pixel shader from file
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns pixel shader description</returns>
        internal EnginePixelShader LoadPixelShader(
            string filename,
            string entryPoint,
            string profile,
            out string compilationErrors)
        {
            return LoadPixelShader(
                File.ReadAllBytes(filename),
                entryPoint,
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a pixel shader from byte code
        /// </summary>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns pixel shader description</returns>
        internal EnginePixelShader LoadPixelShader(
            byte[] byteCode,
            string entryPoint,
            string profile)
        {
            var res = LoadPixelShader(
                byteCode,
                entryPoint,
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Console.WriteLine(compilationErrors);
            }

            return res;
        }
        /// <summary>
        /// Loads a pixel shader from byte code
        /// </summary>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns pixel shader description</returns>
        internal EnginePixelShader LoadPixelShader(
            byte[] byteCode,
            string entryPoint,
            string profile,
            out string compilationErrors)
        {
            compilationErrors = null;

            using (ShaderIncludeManager includeManager = new ShaderIncludeManager())
            using (CompilationResult cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new EnginePixelShader(new PixelShader(this.device, cmpResult.Bytecode));
            }
        }

        /// <summary>
        /// Loads an effect from byte code
        /// </summary>
        /// <param name="bytes">Byte code</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns loaded effect</returns>
        internal EngineEffect CompileEffect(byte[] bytes, string profile)
        {
            using (var includeManager = new ShaderIncludeManager())
            using (var cmpResult = ShaderBytecode.Compile(
                bytes,
                null,
                profile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                includeManager))
            {
                var effect = new Effect(
                    this.device,
                    cmpResult.Bytecode.Data,
                    EffectFlags.None);

                return new EngineEffect(effect);
            }
        }
        /// <summary>
        /// Loads an effect from pre-compiled file
        /// </summary>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded effect</returns>
        internal EngineEffect LoadEffect(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var effectCode = ShaderBytecode.FromStream(ms))
                {
                    var effect = new Effect(
                        this.device,
                        effectCode.Data,
                        EffectFlags.None);

                    return new EngineEffect(effect);
                }
            }
        }
        /// <summary>
        /// Apply effect pass
        /// </summary>
        /// <param name="technique"></param>
        /// <param name="index"></param>
        /// <param name="flags"></param>
        internal void EffectPassApply(EngineEffectTechnique technique, int index, int flags)
        {
            technique.GetPass(index).Apply(this.deviceContext, flags);
        }

        /// <summary>
        /// Creates a new Sampler state
        /// </summary>
        /// <param name="description">Sampler description</param>
        /// <returns>Returns the new sampler state</returns>
        internal EngineSamplerState CreateSamplerState(SamplerStateDescription description)
        {
            return new EngineSamplerState(new SamplerState(this.device, description));
        }

        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        internal void WriteDiscardBuffer<T>(Buffer buffer, IEnumerable<T> data)
            where T : struct
        {
            WriteDiscardBuffer(buffer, 0, data);
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        internal void WriteDiscardBuffer<T>(Buffer buffer, long offset, IEnumerable<T> data)
            where T : struct
        {
            if (data?.Any() == true)
            {
                this.deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
                using (stream)
                {
                    stream.Position = Marshal.SizeOf(default(T)) * offset;
                    stream.WriteRange(data.ToArray());
                }
                this.deviceContext.UnmapSubresource(buffer, 0);

                Counters.BufferWrites++;
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        internal void WriteNoOverwriteBuffer<T>(Buffer buffer, IEnumerable<T> data)
            where T : struct
        {
            WriteNoOverwriteBuffer(buffer, 0, data);
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        internal void WriteNoOverwriteBuffer<T>(Buffer buffer, long offset, IEnumerable<T> data)
            where T : struct
        {
            if (data?.Any() == true)
            {
                this.deviceContext.MapSubresource(buffer, MapMode.WriteNoOverwrite, MapFlags.None, out DataStream stream);
                using (stream)
                {
                    stream.Position = Marshal.SizeOf(default(T)) * offset;
                    stream.WriteRange(data.ToArray());
                }
                this.deviceContext.UnmapSubresource(buffer, 0);

                Counters.BufferWrites++;
            }
        }

        /// <summary>
        /// Reads an array of values from the specified buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns readed data</returns>
        internal T[] ReadBuffer<T>(Buffer buffer, int length)
            where T : struct
        {
            return ReadBuffer<T>(buffer, 0, length);
        }
        /// <summary>
        /// Reads an array of values from the specified buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset to read</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns readed data</returns>
        internal T[] ReadBuffer<T>(Buffer buffer, long offset, int length)
            where T : struct
        {
            Counters.BufferReads++;

            T[] data = new T[length];

            this.deviceContext.MapSubresource(buffer, MapMode.Read, MapFlags.None, out DataStream stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;

                for (int i = 0; i < length; i++)
                {
                    data[i] = stream.Read<T>();
                }
            }
            this.deviceContext.UnmapSubresource(buffer, 0);

            return data;
        }

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="vertexCount">Vertex count</param>
        /// <param name="startVertexLocation">Start vertex location</param>
        internal void Draw(int vertexCount, int startVertexLocation)
        {
            this.deviceContext.Draw(vertexCount, startVertexLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw indexed
        /// </summary>
        /// <param name="indexCount">Index count</param>
        /// <param name="startIndexLocation">Start vertex location</param>
        /// <param name="baseVertexLocation">Base vertex location</param>
        internal void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            this.deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw instanced
        /// </summary>
        /// <param name="vertexCountPerInstance">Vertex count per instance</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="startVertexLocation">Start vertex location</param>
        /// <param name="startInstanceLocation">Start instance count</param>
        internal void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation)
        {
            this.deviceContext.DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw indexed instanced
        /// </summary>
        /// <param name="indexCountPerInstance">Index count per instance</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="startIndexLocation">Start index location</param>
        /// <param name="baseVertexLocation">Base vertex location</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        internal void DrawIndexedInstanced(int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
        {
            this.deviceContext.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw auto
        /// </summary>
        internal void DrawAuto()
        {
            this.deviceContext.DrawAuto();

            Counters.DrawCallsPerFrame++;
        }
    }
}
