using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WaveDistortionFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Material material;
    }

    public Settings settings = new Settings();
    private WaveDistortionPass pass;

    static readonly int WaveStrength = Shader.PropertyToID("_WaveStrength");

    public override void Create()
    {
        pass = new WaveDistortionPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null) return;
        if (!Application.isPlaying) return;
        if (settings.material.GetFloat(WaveStrength) <= 0.001f) return;

        renderer.EnqueuePass(pass);
    }

    class WaveDistortionPass : ScriptableRenderPass
    {
        private readonly Settings settings;
        private RTHandle tempRT;

        public WaveDistortionPass(Settings settings)
        {
            this.settings = settings;
            renderPassEvent = settings.renderPassEvent;
        }

        [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempRT, desc, name: "_WaveDistortionTemp");
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.material == null) return;

            var cmd = CommandBufferPool.Get("WaveDistortion");
            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            Blitter.BlitCameraTexture(cmd, source, tempRT);
            
            settings.material.SetTexture("_SourceTex", tempRT);
            
            Blitter.BlitCameraTexture(cmd, tempRT, source, settings.material, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }

        public void Dispose()
        {
            tempRT?.Release();
        }
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
    }
}