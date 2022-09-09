#if RENDERING_PIPE_LINE_URP

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRTCBlitRenderTextureFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public RenderTexture mTargetRenderTexture;
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Copy RenderTexture Pass");
        private RenderTargetIdentifier source { get; set; }

        private Material mBlitMaterial;

        public RenderTexture GetRenderTexture
        {
            get
            {
                return mTargetRenderTexture;
            }
        }

        public CustomRenderPass(Material blitmat)
        {
            mBlitMaterial = blitmat;
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (Application.isPlaying)
            {
                CommandBuffer buf = CommandBufferPool.Get();
                using (new ProfilingScope(buf, m_ProfilingSampler))
                {
                    int width = Screen.width;
                    int height = Screen.height;

                    source = renderingData.cameraData.renderer.cameraColorTarget;
                    if (mTargetRenderTexture == null || width != mTargetRenderTexture.width || height != mTargetRenderTexture.height)
                    {
                        if (mTargetRenderTexture != null)
                            GameObject.Destroy(mTargetRenderTexture);
                        mTargetRenderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
                    }
                    buf.Blit(null, mTargetRenderTexture, mBlitMaterial);
                }
                context.ExecuteCommandBuffer(buf);
                CommandBufferPool.Release(buf);
            }
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass m_ScriptablePass;
    private Material mBlitMaterial;

    public RenderTexture GetRenderTexture
    {
        get { return m_ScriptablePass.GetRenderTexture; }
    }

    /// <inheritdoc/>
    public override void Create()
    {
        Debug.Log("Create CustomRTCBlitRenderTextureFeature");
        mBlitMaterial = Resources.Load<Material>("Unlit_CaptureBlit_URP");
        m_ScriptablePass = new CustomRenderPass(mBlitMaterial);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRendering;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
#endif